// VelocityTracker.cs
// �����F�A�^�b�`���ꂽ�I�u�W�F�N�g�̑��x��ǐՁE�ێ�����B

using UnityEngine;

public class VelocityTracker : MonoBehaviour
{
    private Vector3 lastPosition;
    private Vector3 currentVelocity;

    void Start()
    {
        // Rigidbody�������Kinematic�ɂ��Ă������Ƃ𐄏�
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        // FixedUpdate�Ԃ̈ړ��ʂƎ��Ԃ��瑬�x���v�Z
        currentVelocity = (transform.position - lastPosition) / Time.fixedUnscaledDeltaTime;
        lastPosition = transform.position;
    }

    /// <summary>
    /// ���݂̑��x���擾���܂��B
    /// </summary>
    public Vector3 GetVelocity()
    {
        return currentVelocity;
    }
}