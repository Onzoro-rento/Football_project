using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

// リプレイデータ用の構造体
public struct TransformData
{
    public float timestamp;
    public Vector3 position;
    public Quaternion rotation;
}

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class GameController : MonoBehaviour
{
    // ゲームの状態を定義
    public enum GameState { Initializing, Ready, Countdown, Active, GameOver, Replaying }
    public static GameState CurrentState { get; private set; }

    [Header("ゲーム設定")]
    [Tooltip("クリアまでに必要なリフティング回数")]
    public int totalLiftsToClear = 20;
    [Tooltip("物理挙動が適用される間隔（例: 5回に1回）")]
    public int physicsLiftInterval = 5;
    [Tooltip("ゲーム全体の速度（0.4で40%の速度）")]
    public float gameSpeed = 0.4f;
    [Tooltip("地面として認識するオブジェクトのタグ")]
    public string groundTag = "Ground";

    [Header("リフティング挙動")]
    [Tooltip("アシスト時にボールが上昇する高さ(メートル)")]
    public float assistedLiftHeight = 0.5f; // CHANGED: 固定の力ではなく高さで指定

    [Header("オブジェクト参照")]
    public Transform leftControllerAnchor;
    public Transform rightControllerAnchor;
    [Tooltip("靴のゲームオブジェクトをここにドラッグ＆ドロップ")]
    public GameObject shoeObject;

    [Header("ボールの初期位置オフセット")]
    public Vector3 positionOffset = new Vector3(0f, 0.50f, 0.30f);

    [Header("UI設定")]
    public TextMeshProUGUI countdownText;
    [Tooltip("リフティング回数やステータスを表示するテキスト")]
    public TextMeshProUGUI statusText;

    [Header("効果音設定")]
    public AudioClip collisionSound;
    public GameObject replayActivatorPrefab;
    [Header("リプレイ設定")]
    [Tooltip("リプレイ時に補間再生を行うか")]
    public bool smoothReplay = true;  // ※InspectorでON/OFF可
    // --- Private Variables ---
    private Rigidbody rb;
    private Rigidbody shoeRb;
    private Renderer ballRenderer;
    private AudioSource audioSource;
    private Coroutine countdownCoroutine;
    private ShoeController shoeController;
    private int liftCount = 0;

    // リプレイ用データ
    private List<TransformData> ballLog = new List<TransformData>();
    private List<TransformData> shoeLog = new List<TransformData>();
    private bool isLogging = false;

    // 物理挙動調整用
    private Vector3 originalGravity; // NEW: 元の重力を保存する変数
    [Header("当たり判定設定")]
    [SerializeField] private float collisionCooldown = 0.1f; // 衝突後の無敵時間（Inspectorから調整可能）
    private float lastCollisionTime = -1f; // 最後に衝突した時間

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ballRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        if (shoeObject != null)
        {
            shoeController = shoeObject.GetComponent<ShoeController>();
            // ▼▼▼【追加】▼▼▼
            // ゲーム開始時に必ずコントローラー追従を有効にする
            shoeRb = shoeObject.GetComponent<Rigidbody>();
            shoeController.followController = true;
            // ▲▲▲【追加】▲▲▲
        }
        else
        {
            Debug.LogError("Shoe Objectが設定されていません！");
        }

        

        // ゲーム速度を設定
        Time.timeScale = gameSpeed;

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (statusText != null) statusText.text = "";

        CurrentState = GameState.Initializing;
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnDestroy()
    {
        
        Time.timeScale = 1.0f; // タイムスケールも元に戻す
    }


    void Update()
    {
        if (CurrentState == GameState.Initializing)
        {
            if (OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) || OVRInput.IsControllerConnected(OVRInput.Controller.LTouch))
            {
                ResetGame();
            }
            return;
        }

        if ((CurrentState == GameState.GameOver || CurrentState == GameState.Active) && OVRInput.GetDown(OVRInput.Button.One))
        {
            ResetGame();
        }
    }

    void FixedUpdate()
    {
        if (isLogging)
        {
            LogTransforms();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // CHANGED: 衝突した相手の情報をログに出力してデバッグしやすくする
        Debug.Log($"ボールが衝突: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag(groundTag))
        {
            if (CurrentState == GameState.Active || CurrentState == GameState.Countdown) // カウントダウン中もゲームオーバーにする
            {
                HandleGameOver();
            }
            return;
        }

        if (!collision.gameObject.CompareTag("Shoe"))
        {
            return;
        }
        if (Time.time < lastCollisionTime + collisionCooldown)
        {
            return; // クールダウン中は処理をしない
        }
        lastCollisionTime = Time.time; // 有効な衝突として時間を記録

        if (CurrentState == GameState.Ready || CurrentState == GameState.Active)
        {
            HandleKick(collision);
        }
    }

    private void HandleKick(Collision collision)
    {
        if (audioSource != null && collisionSound != null)
        {
            audioSource.PlayOneShot(collisionSound);
        }

        if (CurrentState == GameState.Ready)
        {
            if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
            countdownCoroutine = StartCoroutine(StartCountdown());
            return;
        }

        liftCount++;
        UpdateStatusText();

        if (liftCount >= totalLiftsToClear)
        {
            HandleGameClear();
        }

        // 5回目の物理挙動か、それ以外のアシストかを判定
        if (liftCount % physicsLiftInterval == 0)
        {
            Debug.Log($"Lift {liftCount}: 物理挙動！");
            StartCoroutine(HandlePhysicsKick(collision));
            // ★★★何もしないことで、Unityの物理エンジンによる自然な衝突計算がそのまま適用されます★★★
        }
        else
        {
            Debug.Log($"Lift {liftCount}: アシスト！ 上昇高さ: {assistedLiftHeight}m");

            // --- ▼▼▼ アシストキックのコードを、elseブロックの中に完全に閉じ込める ▼▼▼ ---
            // 目標の高さに到達するために必要な初速を計算 v = sqrt(2 * g * h)
            float requiredVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * assistedLiftHeight);

            // 現在の速度をリセット
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // 計算した初速を真上に与える
            rb.velocity = new Vector3(0, requiredVelocity, 0);
            // --- ▲▲▲ アシストキックのコードを、elseブロックの中に完全に閉じ込める ▲▲▲ ---
        }
    }
    private IEnumerator HandlePhysicsKick(Collision collision)
{
    // 衝突時の靴の速度を物理演算に反映させるため、一時的にKinematicを解除
    if (shoeRb != null)
    {
        shoeRb.isKinematic = false;
    }

    // 1. 時間を1倍速に戻す
    Time.timeScale = 1.0f;

    // 2. 物理エンジンが衝突を再計算するのを1フレーム待つ
    yield return new WaitForFixedUpdate();

    // 3. 1倍速で計算されたボールの速度を、0.4倍速の世界の速度に変換して保存
    Vector3 scaledVelocity = rb.velocity * gameSpeed;
    Vector3 scaledAngularVelocity = rb.angularVelocity * gameSpeed;
    
    // 4. 時間を0.4倍速に戻す
    Time.timeScale = gameSpeed;

    // 5. 変換した速度をボールに再設定
    rb.velocity = scaledVelocity;
    rb.angularVelocity = scaledAngularVelocity;

    // 靴を元の状態に戻す
    if (shoeRb != null)
    {
        shoeRb.isKinematic = true;
    }
}

    private IEnumerator StartCountdown()
    {
        CurrentState = GameState.Countdown;
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        int count = 3;
        while (count > 0)
        {
            if (countdownText != null) countdownText.text = count.ToString();
            // CHANGED: Time.timeScaleの影響を受けない待機に変更
            yield return new WaitForSecondsRealtime(1.0f);
            count--;
        }

        if (countdownText != null) countdownText.text = "GO!";
        StartGame();
        yield return new WaitForSecondsRealtime(1.0f); // CHANGED
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    void StartGame()
    {
        CurrentState = GameState.Active;
        rb.isKinematic = false;
        rb.useGravity = true;

        ballLog.Clear();
        shoeLog.Clear();
        isLogging = true;
    }

    void ResetGame()
    {
        StopAllCoroutines();
        Time.timeScale = gameSpeed;

        CurrentState = GameState.Ready;
        rb.isKinematic = false;
        rb.useGravity = false;
        liftCount = 0;

        if (shoeController != null)
        {
            shoeController.followController = true;
        }

        UpdateStatusText();
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        RepositionBall();

        isLogging = false;
        ballLog.Clear();
        shoeLog.Clear();
        if (replayActivatorPrefab != null)
        {
            replayActivatorPrefab.SetActive(false);
        }
        // --- ▲▲▲【処理を追加】▲▲▲ ---

      

        Debug.Log("ゲームをリセットしました。ボールを蹴って開始してください。");
    }

    void RepositionBall()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Transform targetCtrl = rightControllerAnchor;
        if (targetCtrl == null) return;

        transform.position = targetCtrl.position + targetCtrl.TransformDirection(positionOffset);
        transform.rotation = targetCtrl.rotation * Quaternion.Euler(45, 0, 0);
    }

    private void HandleGameOver()
    {
        CurrentState = GameState.GameOver;
        isLogging = false;
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (statusText != null)
        {
            statusText.text = $"ゲームオーバー\n回数: {liftCount}\nAボタンでリセット";
        }
        Debug.Log("ゲームオーバー！リプレイを開始するにはリプレイキューブに触れてください。");

        // --- ▼▼▼【処理を変更】▼▼▼ ---
        // プレハブを生成するのではなく、シーン上のオブジェクトを表示する
        if (replayActivatorPrefab != null)
        {
            // 表示したい位置に移動させてから表示する
            replayActivatorPrefab.transform.position = transform.position + new Vector3(0, 1.2f, 0); // ボールが落ちた位置の少し上に出現
            replayActivatorPrefab.SetActive(true);
        }
        else
        {
            Debug.LogError("Replay Activator Prefab が設定されていません！");
        }
        // --- ▲▲▲【処理を変更】▲▲▲ ---
    }
    public void StartReplay()
    {
        StartCoroutine(PlayReplay());
    }

    private void HandleGameClear()
    {
        if (statusText != null)
        {
            statusText.text = $"クリア！\n合計 {liftCount} 回";
        }
        Debug.Log("ゲームクリア！");
    }


    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = $"Lifting: {liftCount}";
        }
    }

    // --- Replay System ---
    private void LogTransforms()
    {
        float currentTime = Time.time;
        ballLog.Add(new TransformData
        {
            timestamp = currentTime,
            position = transform.position,
            rotation = transform.rotation
        });

        if (shoeObject != null)
        {
            shoeLog.Add(new TransformData
            {
                timestamp = currentTime,
                position = shoeObject.transform.position,
                rotation = shoeObject.transform.rotation
            });
        }
    }

    // リプレイのコードは変更なし

    private IEnumerator PlayReplay()
    {
        // リプレイ開始前の待機
        yield return new WaitForSecondsRealtime(1.5f);
        CurrentState = GameState.Replaying;
        Debug.Log("リプレイ再生開始");
        shoeController.followController = false;
        //statusText.text = "リプレイ再生中...";
        // --- 透明化設定（Fadeモード） ---
        if (ballRenderer != null)
        {
            var mat = ballRenderer.material;
            mat.SetFloat("_Mode", 2);  // 2 = Fade
            mat.SetFloat("_Mode", 2);  // 2 = Fade
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            // アルファだけを変更
            Color c = mat.color;
            c.a = 0.3f;
            mat.color = c;
        }
        // --- 透明化終了 ---
        // 最初の位置合わせ
        if (ballLog.Count > 0)
        {
            transform.position = ballLog[0].position;
            transform.rotation = ballLog[0].rotation;
        }
        if (shoeLog.Count > 0 && shoeObject != null)
        {
            shoeObject.transform.position = shoeLog[0].position;
            shoeObject.transform.rotation = shoeLog[0].rotation;
        }
        if (ballLog.Count == 0)
        {
            Debug.LogWarning("リプレイデータがありません");
            CurrentState = GameState.GameOver;
            yield break;
        }
        float startTime = ballLog[0].timestamp;
        float replayTimer = 0f;
        float replayDuration = ballLog.Last().timestamp - startTime;
        while (replayTimer <= replayDuration)
        {
            replayTimer += Time.unscaledDeltaTime;
            float tstamp = startTime + replayTimer * gameSpeed;
            if (smoothReplay)
            {
                // --- ボール補間 ---
                if (ballLog.Count > 1)
                {
                    int i0 = GetIndexForTimestamp(ballLog, tstamp);
                    int i1 = Mathf.Min(i0 + 1, ballLog.Count - 1);
                    var a = ballLog[i0];
                    var b = ballLog[i1];
                    float dt = b.timestamp - a.timestamp;
                    float t = dt > Mathf.Epsilon ? (tstamp - a.timestamp) / dt : 0f;
                    transform.position = Vector3.Lerp(a.position, b.position, t);
                    transform.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);
                }
                // --- 靴補間 ---
                if (shoeLog.Count > 1 && shoeObject != null)
                {
                    int i0 = GetIndexForTimestamp(shoeLog, tstamp);
                    int i1 = Mathf.Min(i0 + 1, shoeLog.Count - 1);
                    var a = shoeLog[i0];
                    var b = shoeLog[i1];
                    float dt = b.timestamp - a.timestamp;
                    float t = dt > Mathf.Epsilon ? (tstamp - a.timestamp) / dt : 0f;
                    shoeObject.transform.position = Vector3.Lerp(a.position, b.position, t);
                    shoeObject.transform.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);
                }
            }
            else
            {
                // 元のインデックス再生（ジャンプ）
                int bi = GetIndexForTimestamp(ballLog, tstamp);
                if (bi >= 0)
                {
                    transform.position = ballLog[bi].position;
                    transform.rotation = ballLog[bi].rotation;
                }
                int si = GetIndexForTimestamp(shoeLog, tstamp);
                if (si >= 0 && shoeObject != null)
                {
                    shoeObject.transform.position = shoeLog[si].position;
                    shoeObject.transform.rotation = shoeLog[si].rotation;
                }
            }
            yield return null;
        }
        Debug.Log("リプレイ終了");
        // --- 不透明化に戻す ---
        if (ballRenderer != null)
        {
            var mat = ballRenderer.material;
            mat.SetFloat("_Mode", 0);  // Opaque
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
            Color c = mat.color; c.a = 1f; mat.color = c;
        }
        // --- 不透明化終了 ---
        CurrentState = GameState.GameOver;
        shoeController.followController = true;
        //statusText.text = $"ゲームオーバー\n回数: {liftCount}\nAボタンでリセット";
    }
    // 既存の GetIndexForTimestamp() はそのまま使用します


private int GetIndexForTimestamp(List<TransformData> log, float timestamp)
    {
        if (log.Count == 0) return -1;
        for (int i = 0; i < log.Count; i++)
        {
            if (log[i].timestamp >= timestamp)
            {
                return i;
            }
        }
        return log.Count - 1;
    }

}
