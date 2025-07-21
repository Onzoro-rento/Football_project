// BallController.cs
// �����F�{�[�����g�̕��������i�ՓˁA�L�b�N�j���Ǘ����A�C�x���g�𔭍s����

using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class BallController : MonoBehaviour
{
    // --- �C�x���g��` ---
    public event Action<Collision> OnKicked;        // �{�[�����R��ꂽ���ɔ��s
    public event Action OnHitGround;                 // �{�[�����n�ʂɐڐG�������ɔ��s

    [Header("�ݒ�")]
    [SerializeField] private string shoeTag = "Shoe";
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float collisionCooldown = 0.1f;
    [SerializeField] private AudioClip collisionSound;
    [SerializeField] private float assistedLiftHeight = 0.5f;

    // --- �R���|�[�l���g�Q�� ---
    private Rigidbody rb;
    private AudioSource audioSource;

    // --- ������� ---
    private float lastCollisionTime = -1f;
    private bool isKickable = false; // �Q�[�����A�N�e�B�u�ȏ�Ԃ�

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        rb.isKinematic = true; // �ŏ��͕������Z�𖳌���
    }

    /// <summary>
    /// �O������{�[���̃L�b�N�ۏ�Ԃ�ݒ肵�܂�
    /// </summary>
    public void SetKickable(bool status)
    {
        isKickable = status;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isKickable) return;

        // �n�ʂɏՓ�
        if (collision.gameObject.CompareTag(groundTag))
        {
            OnHitGround?.Invoke();
            return;
        }

        // �C�ɏՓ�
        if (collision.gameObject.CompareTag(shoeTag))
        {
            // �N�[���_�E���`�F�b�N
            if (Time.time < lastCollisionTime + collisionCooldown) return;
            lastCollisionTime = Time.time;

            if (audioSource != null && collisionSound != null)
            {
                audioSource.PlayOneShot(collisionSound);
            }

            // �C�x���g�𔭍s���āA������GameManager�Ɉς˂�
            OnKicked?.Invoke(collision);
        }
    }

    /// <summary>
    /// �A�V�X�g�L�b�N�����s���܂�
    /// </summary>
    public void PerformAssistKick()
    {
        Debug.Log($"�A�V�X�g�I �㏸����: {assistedLiftHeight}m");
        float requiredVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * assistedLiftHeight);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = new Vector3(0, requiredVelocity, 0);
    }

    /// <summary>
    /// �����x�[�X�̃L�b�N�����s���܂�
    /// </summary>
    public IEnumerator PerformPhysicsKick(Rigidbody shoeRb, float gameSpeed)
    {
        Debug.Log("���������L�b�N�I");
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
    /// �{�[���̈ʒu�ƕ�����Ԃ����Z�b�g���܂�
    /// </summary>
    public void ResetPosition(Transform anchor, Vector3 offset, Quaternion rotationOffset)
    {
        rb.isKinematic = false; // �ꎞ�I��Kinematic���������Ĉʒu��ݒ�
        rb.useGravity = false;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = anchor.position + anchor.TransformDirection(offset);
        transform.rotation = anchor.rotation * rotationOffset;
    }

    /// <summary>
    /// �������Z���J�n���܂�
    /// </summary>
    public void ActivatePhysics()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    /// <summary>
    /// �������Z���~���܂�
    /// </summary>
    public void DeactivatePhysics()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}