// BallController.cs

using UnityEngine;

using System;

using FMODUnity;
using System.Collections;
using Oculus.Interaction.PoseDetection;



[RequireComponent(typeof(Rigidbody))]

public class BallController : MonoBehaviour

{

    public event Action<Collision> OnKicked;

    public event Action OnHitGround;



    [Header("設定")]

    [SerializeField] private string shoeTag = "Shoe";


    [SerializeField] private float collisionCooldown = 0.1f;

    //[SerializeField] private EventReference kickSound;

    //[SerializeField] private EventReference kickSoundA;


    // 放物運動制御用の変数
    private bool isManualControl = false;
    private Vector3 initialPosition;
    private Vector3 initialVelocity;
    private Vector3 initialAngularVelocity;
    private float launchTime;
    private float currentTimeScale = 1.0f;
   
    private bool hasReachedPeak = false;
    private Vector3 originalGravity;
    // 落下時の重力スケール
    
    public AudioSource audioSource; // FMODのサウンドを再生するためのAudioSource

    public AudioClip jumpSound;
    public AudioClip firstKickSound;   // 最初のキック音

    // ★★★ ここからが色変更のための追加箇所 ★★★
    [Header("フィードバック設定")]
    [Tooltip("弱いキック（失敗）だった場合に点滅させる色")]
    [SerializeField] private Color weakKickFlashColor = Color.red;
    [Tooltip("色が点滅する時間（秒）")]
    [SerializeField] private float flashDuration = 0.5f;

    private Material ballMaterial;
    private Color baseColor; // 予測フィードバックで使われる基本色
    private Coroutine flashColorCoroutine;


    private Rigidbody rb;

    private float lastCollisionTime = -1f;



    // ★削除: 手動物理演算のための変数をすべて削除

    // private enum BallState { ... }

    // private BallState currentState;

    // private Vector3 launchPosition;

    // ...など



    void Awake()

    {

        rb = GetComponent<Rigidbody>();
        originalGravity = Physics.gravity;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            ballMaterial = renderer.material;
            baseColor = ballMaterial.color;
        }

        DeactivatePhysics();

    }



    // ★削除: 手動物理演算を行っていたFixedUpdateを削除

    // void FixedUpdate() { ... }
    void FixedUpdate()
    {
        if (!rb.isKinematic || isManualControl)
        {
            // もしボールのY座標が0以下になったら
            if (transform.position.y <= 0)
            {
                
                DeactivatePhysics();  // ボールの物理挙動を停止
                OnHitGround?.Invoke(); // GameManagerにゲームオーバーを通知
                return; // 処理を終了
            }
        }
        if (isManualControl && !hasReachedPeak)
        {
            // 手動で放物運動を計算
            

            float t = (Time.time - launchTime) * currentTimeScale;
            // 放物運動の公式
            Vector3 newPosition = new Vector3(
                initialPosition.x + initialVelocity.x * t,
                initialPosition.y + initialVelocity.y * t + 0.5f * Physics.gravity.y * t * t,
                initialPosition.z + initialVelocity.z * t
            );
            transform.position = newPosition;
            transform.Rotate(initialAngularVelocity * Mathf.Rad2Deg * Time.deltaTime * currentTimeScale, Space.World);
            // 最高点に達したかチェック
            float verticalVelocity = initialVelocity.y + Physics.gravity.y * t;
            if (verticalVelocity <= 0 && initialVelocity.y > 0)
            {
                // 最高点に達したら通常の物理演算に戻す
                hasReachedPeak = true;
                Physics.gravity = Physics.gravity * currentTimeScale*currentTimeScale;
                SwitchToPhysicsMode(newPosition, new Vector3(initialVelocity.x*currentTimeScale, verticalVelocity, initialVelocity.z*currentTimeScale),initialAngularVelocity*currentTimeScale);
            }
        }
    }


    void OnCollisionEnter(Collision collision)

    {

        // ★修正: ボールが物理的にアクティブでない場合は衝突判定しない

        if (rb.isKinematic && !isManualControl) return;







        if (collision.gameObject.CompareTag(shoeTag))
        {
            // ★ 追加: 手動制御中にキックされたら、まず物理モードに戻す]
            if (isManualControl) { return; }
            //if (isManualControl)
            //{
            //    //// 現在の状態で物理演算に切り替え
            //    //SwitchToPhysicsMode(transform.position, GetCurrentVelocity(), rb.angularVelocity);
            //    isManualControl = false;
            //    rb.isKinematic = false;
            //    rb.useGravity = true;
            //}

            if (Time.time < lastCollisionTime + collisionCooldown) return;
            lastCollisionTime = Time.time;

            // (効果音の再生処理は変更なし)
            if (GameManager.CurrentState == GameManager.GameState.Ready)
            {
                if (firstKickSound != null) audioSource.PlayOneShot(firstKickSound);
            }
            else
            {
                if (jumpSound != null) audioSource.PlayOneShot(jumpSound);
            }

            Physics.gravity = originalGravity;
            hasReachedPeak = false;
            OnKicked?.Invoke(collision);
        }

    }
    // 通常の物理演算モードに切り替え
    private void SwitchToPhysicsMode(Vector3 position, Vector3 velocity,Vector3 angularVelocity)
    {
        
        isManualControl = false;
        transform.position = position;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.velocity = velocity;
        rb.angularVelocity = angularVelocity;
    }


    // 手動制御による打ち上げ
    public void LaunchWithManualControl(float timeScale)
    {
        // 古い呼び出し方を削除し、新しいコルーチンを開始する
        StartCoroutine(ActivateManualControlAfterDelay(timeScale));
    }

    // ★★★ 新しいコルーチン ★★★
    private IEnumerator ActivateManualControlAfterDelay(float timeScale)
    {
        // 現在の物理フレームが終了し、衝突計算が完了するのを待つ
        yield return new WaitForFixedUpdate();

        // 物理エンジンによる反発が完了した、この時点での速度と角速度が「正しい初速」になる
        Vector3 currentVelocity = rb.velocity;
        Vector3 currentAngularVelocity = rb.angularVelocity;
        Debug.Log($"初速度：{currentVelocity}");
        // 速度が十分に上向きでない場合は、手動制御に移行しない（弱すぎるキックなど）
        // この閾値はお好みで調整してください
        if (currentVelocity.y < 0.0f)
        {
            // 上昇しないので、そのままUnityの物理に任せる
            // isKinematic = false のままなので、自然に落下する

            currentVelocity.y = 2.0f;
        }

        // --- ここからが手動制御への切り替え処理 ---
        isManualControl = true;
        hasReachedPeak = false;
        currentTimeScale = timeScale;
        initialPosition = transform.position; // 1フレーム後の位置を初期位置とする
        initialVelocity = currentVelocity;    // 1フレーム後の正しい速度を初期速度とする
        initialAngularVelocity = currentAngularVelocity;
        launchTime = Time.time;

        // 物理計算が完了した後にKinematicにする
        rb.isKinematic = true;
        rb.useGravity = false;
    }
    public void Launch(Vector3 velocity, float timeScale)
    {
        // Launchメソッドは主にInitialKickで使われるので、初速を直接設定する
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.velocity = velocity;

        // その後、スローモーションの手動制御に切り替える
        LaunchWithManualControl(timeScale);
    }




    // ★削除: StartSlowRiseとStartFreeFallは不要になったため削除

    // public void StartSlowRise(...) { ... }

    // public void StartFreeFall() { ... }

    // BallController.cs に追加

/// <summary>
/// アシスト付きの打ち上げを開始する（GameManagerから呼ばれる）
/// </summary>
/// <param name="height">到達させたい高さ</param>
/// <param name="timeScale">スローモーションのスケール</param>
public void LaunchAssisted(float height, float timeScale)
{
    StartCoroutine(AssistedLaunchCoroutine(height, timeScale));
}

/// <summary>
/// アシスト打ち上げを実行するコルーチン
/// </summary>
private IEnumerator AssistedLaunchCoroutine(float height, float timeScale)
{
    // 現在の物理フレームが終了し、衝突計算が完了するのを待つ
    yield return new WaitForFixedUpdate();

    // --- ここからが手動制御への切り替え処理 ---
    
    // アシスト：横方向の速度は0にし、指定された高さに到達するための真上の速度を計算
    // v = sqrt(2 * g * h)
    float requiredVelocityY = Mathf.Sqrt(2f * Mathf.Abs(originalGravity.y) * height);
    Vector3 assistedVelocity = new Vector3(0, requiredVelocityY, 0);

    isManualControl = true;
    hasReachedPeak = false;
    currentTimeScale = timeScale;
    initialPosition = transform.position;
    initialVelocity = assistedVelocity; // ★計算したアシスト速度を初速として設定
    initialAngularVelocity = rb.angularVelocity; // 回転はキックの影響を維持
    launchTime = Time.time;

    // 物理計算が完了した後にKinematicにする
    rb.isKinematic = true;
    rb.useGravity = false;
}

    public void SetReadyToKick()

    {

        isManualControl = false;
        hasReachedPeak = false;
        Physics.gravity = originalGravity;
        rb.isKinematic = false;

        rb.useGravity = false; // 最初のタッチを待つ間は重力を無効化

        rb.velocity = Vector3.zero;

        rb.angularVelocity = Vector3.zero;

    }



    public void ResetPosition(Vector3 worldPosition, Quaternion worldRotation)

    {

        DeactivatePhysics();

        transform.position = worldPosition;

        transform.rotation = worldRotation;

    }



    public void DeactivatePhysics()

    {
        isManualControl = false;
        hasReachedPeak = false;

        Time.timeScale = 1.0f;

        rb.velocity = Vector3.zero;

        rb.angularVelocity = Vector3.zero;
        Physics.gravity = originalGravity;
        rb.isKinematic = true; // ボールの物理挙動を完全に停止

    }

    public void Freeze()

    {

        rb.isKinematic = true; // ボールの物理挙動を完全に停止

        rb.useGravity = false; // 重力も無効化

    }

    public Vector3 GetCurrentVelocity()
    {
        if (isManualControl && !hasReachedPeak)
        {
            float t = (Time.time - launchTime) * currentTimeScale;
            return new Vector3(
                initialVelocity.x,
                initialVelocity.y + Physics.gravity.y * t,
                initialVelocity.z
            );
        }
        return rb.velocity;
    }
    public void SetBaseColor(Color color)
    {
        this.baseColor = color;
        // 点滅中でなければ、すぐに色を適用する
        if (flashColorCoroutine == null)
        {
            ballMaterial.color = this.baseColor;
        }
    }

    /// <summary>
    /// 事後フィードバック用にボールの色を一時的に点滅させる
    /// </summary>
    private void FlashColor(Color flashColor, float duration)
    {
        if (ballMaterial == null) return;

        // 既に実行中の点滅があれば停止
        if (flashColorCoroutine != null)
        {
            StopCoroutine(flashColorCoroutine);
        }
        flashColorCoroutine = StartCoroutine(FlashColorCoroutine(flashColor, duration));
    }

    /// <summary>
    /// 色を一定時間変更し、基本色に戻すコルーチン
    /// </summary>
    private IEnumerator FlashColorCoroutine(Color newColor, float duration)
    {
        ballMaterial.color = newColor;
        yield return new WaitForSeconds(duration);
        ballMaterial.color = baseColor; // 元の色ではなく、記憶している「基本色」に戻す
        flashColorCoroutine = null; // コルーチンが終了したことを示す
    }
}

