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

    // --- Private Variables ---
    private Rigidbody rb;
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
            // 何もせず、Unityの物理演算（調整された重力下）に任せる
        }
        else
        {
            Debug.Log($"Lift {liftCount}: アシスト！ 上昇高さ: {assistedLiftHeight}m");
            // --- CHANGED: アシスト挙動の変更 ---
            // 目標の高さに到達するために必要な初速を計算 v = sqrt(2 * g * h)
            float requiredVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * assistedLiftHeight);

            // 現在の速度をリセット
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // 計算した初速を真上に与える
            rb.velocity = new Vector3(0, requiredVelocity, 0);
            // --- END CHANGED ---
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
        rb.isKinematic = true;
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
        yield return new WaitForSecondsRealtime(1.5f); // CHANGED

        CurrentState = GameState.Replaying;
        Debug.Log("リプレイ再生開始");

        if (shoeController != null) shoeController.followController = false;
        if (statusText != null) statusText.text = "リプレイ再生中...";

        // --- 【リプレイ開始：ボールの透明度を上げる】 ---
        if (ballRenderer != null)
        {
            Material ballMaterial = ballRenderer.material;

            // ★ 透明化設定（Standard Shader用）
            ballMaterial.SetFloat("_Mode", 3); // 3 = Transparent
            ballMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            ballMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            ballMaterial.SetInt("_ZWrite", 0);
            ballMaterial.DisableKeyword("_ALPHATEST_ON");
            ballMaterial.EnableKeyword("_ALPHABLEND_ON");
            ballMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            ballMaterial.renderQueue = 3000;

            // ★ 透明度変更（例：30%）
            Color currentColor = ballMaterial.color;
            currentColor.a = 0.3f;
            ballMaterial.color = currentColor;

            // Optional: MaterialPropertyBlock でも反映（必須ではない）
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_Color", currentColor);
            ballRenderer.SetPropertyBlock(propertyBlock);
        }

        // --- 【透明度変更 終了】 ---

        if (ballLog.Count > 0)
        {
            transform.position = ballLog.First().position; // .First() を使用 (System.Linq を using 必要)
            transform.rotation = ballLog.First().rotation; // .First() を使用
        }
        if (shoeLog.Count > 0 && shoeObject != null)
        {
            shoeObject.transform.position = shoeLog.First().position; // .First() を使用
            shoeObject.transform.rotation = shoeLog.First().rotation; // .First() を使用
        }

        if (ballLog.Count == 0)
        {
            Debug.Log("リプレイデータがありません。");
            CurrentState = GameState.GameOver;
            yield break;
        }

        float startTime = ballLog.First().timestamp; // .First() を使用
        float replayTimer = 0f;
        float replayDuration = ballLog.Last().timestamp - startTime; // .Last() を使用

        while (replayTimer <= replayDuration)
        {
            replayTimer += Time.unscaledDeltaTime; // CHANGED: タイムスケールに関係なく再生
            float currentTimestamp = startTime + (replayTimer * gameSpeed); // 保存された時間軸に合わせる

            int ballIndex = GetIndexForTimestamp(ballLog, currentTimestamp);
            int shoeIndex = GetIndexForTimestamp(shoeLog, currentTimestamp);

            if (ballIndex != -1)
            {
                transform.position = ballLog.ElementAt(ballIndex).position; // .ElementAt() を使用
                transform.rotation = ballLog.ElementAt(ballIndex).rotation; // .ElementAt() を使用
            }
            if (shoeIndex != -1 && shoeObject != null)
            {
                shoeObject.transform.position = shoeLog.ElementAt(shoeIndex).position; // .ElementAt() を使用
                shoeObject.transform.rotation = shoeLog.ElementAt(shoeIndex).rotation; // .ElementAt() を使用
            }

            yield return null;
        }

        Debug.Log("リプレイ終了");
        if (ballRenderer != null)
        {
            Material ballMaterial = ballRenderer.material;

            // ★ 不透明設定（元に戻す）
            ballMaterial.SetFloat("_Mode", 0); // 0 = Opaque
            ballMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            ballMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            ballMaterial.SetInt("_ZWrite", 1);
            ballMaterial.DisableKeyword("_ALPHATEST_ON");
            ballMaterial.DisableKeyword("_ALPHABLEND_ON");
            ballMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            ballMaterial.renderQueue = -1;

            // ★ 透明度を100%に戻す
            Color currentColor = ballMaterial.color;
            currentColor.a = 1f;
            ballMaterial.color = currentColor;

            // MaterialPropertyBlock を使うなら再適用
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_Color", currentColor);
            ballRenderer.SetPropertyBlock(propertyBlock);
        }

        CurrentState = GameState.GameOver;
        if (shoeController != null) shoeController.followController = true;

        // --- 【リプレイ終了：ボールの透明度を元に戻す】 ---
        if (ballRenderer != null)
        {
            Material ballMaterial = ballRenderer.material;
            Color currentColor = ballMaterial.color;
            currentColor.a = 1f; // 透明度を元に戻す
                                 // MaterialPropertyBlock を再度適用して変更を反映
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_Color", currentColor);
            ballRenderer.SetPropertyBlock(propertyBlock);
        }
        // --- 【透明度復元 終了】 ---

        if (statusText != null)
        {
            statusText.text = $"ゲームオーバー\n回数: {liftCount}\nAボタンでリセット";
        }
    }

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