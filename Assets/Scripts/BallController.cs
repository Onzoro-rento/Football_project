// BallController.cs
// 役割：ボール自身の物理挙動（衝突、キック）を管理し、イベントを発行する

using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class BallController : MonoBehaviour
{
    // --- イベント定義 ---
    public event Action<Collision> OnKicked;        // ボールが蹴られた時に発行
    public event Action OnHitGround;                 // ボールが地面に接触した時に発行

    [Header("設定")]
    [SerializeField] private string shoeTag = "Shoe";
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float collisionCooldown = 0.1f;
    [SerializeField] private AudioClip collisionSound;
    [SerializeField] private float assistedLiftHeight = 0.5f;

    // --- コンポーネント参照 ---
    private Rigidbody rb;
    private AudioSource audioSource;

    // --- 内部状態 ---
    private float lastCollisionTime = -1f;
    private bool isKickable = false; // ゲームがアクティブな状態か

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        rb.isKinematic = true; // 最初は物理演算を無効化
    }

    /// <summary>
    /// 外部からボールのキック可否状態を設定します
    /// </summary>
    public void SetKickable(bool status)
    {
        isKickable = status;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isKickable) return;

        // 地面に衝突
        if (collision.gameObject.CompareTag(groundTag))
        {
            OnHitGround?.Invoke();
            return;
        }

        // 靴に衝突
        if (collision.gameObject.CompareTag(shoeTag))
        {
            // クールダウンチェック
            if (Time.time < lastCollisionTime + collisionCooldown) return;
            lastCollisionTime = Time.time;

            if (audioSource != null && collisionSound != null)
            {
                audioSource.PlayOneShot(collisionSound);
            }

            // イベントを発行して、処理をGameManagerに委ねる
            OnKicked?.Invoke(collision);
        }
    }

    /// <summary>
    /// アシストキックを実行します
    /// </summary>
    public void PerformAssistKick()
    {
        Debug.Log($"アシスト！ 上昇高さ: {assistedLiftHeight}m");
        float requiredVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * assistedLiftHeight);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = new Vector3(0, requiredVelocity, 0);
    }

    /// <summary>
    /// 物理ベースのキックを実行します
    /// </summary>
    public IEnumerator PerformPhysicsKick(Rigidbody shoeRb, float gameSpeed)
    {
        Debug.Log("物理挙動キック！");
        if (shoeRb != null) shoeRb.isKinematic = false;

        Time.timeScale = 1.0f;
        yield return new WaitForFixedUpdate();

        Vector3 scaledVelocity = rb.velocity * gameSpeed;
        Vector3 scaledAngularVelocity = rb.angularVelocity * gameSpeed;

        Time.timeScale = gameSpeed;

        rb.velocity = scaledVelocity;
        rb.angularVelocity = scaledAngularVelocity;

        if (shoeRb != null) shoeRb.isKinematic = true;
    }

    /// <summary>
    /// ボールの位置と物理状態をリセットします
    /// </summary>
    public void ResetPosition(Transform anchor, Vector3 offset, Quaternion rotationOffset)
    {
        rb.isKinematic = false; // 一時的にKinematicを解除して位置を設定
        rb.useGravity = false;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = anchor.position + anchor.TransformDirection(offset);
        transform.rotation = anchor.rotation * rotationOffset;
    }

    /// <summary>
    /// 物理演算を開始します
    /// </summary>
    public void ActivatePhysics()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    /// <summary>
    /// 物理演算を停止します
    /// </summary>
    public void DeactivatePhysics()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}