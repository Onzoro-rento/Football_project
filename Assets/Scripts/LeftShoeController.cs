using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftShoeController : MonoBehaviour
{
    [Header("�Ǐ]����Ώۂ̃R���g���[���[�A���J�[")]
    public Transform targetControllerAnchor;
    // Start is called before the first frame update]
    public GameObject leftShoe;
    public float yOffset = -0.09f;
    public float zOffset = 0.15f; // X�������̃I�t�Z�b�g�i�K�v�ɉ����Ē����j
    public float xOffset = 0.11f; // Z�������̃I�t�Z�b�g�i�K�v�ɉ����Ē����j
    public float x_rotationOffset = 35f;
    public float y_rotationOffset = 180f;
    public float z_rotationOffset = 65f;
    [Header("�ݒ�")]
    [Tooltip("�{�[���̃^�O��")]
    [SerializeField] private string ballTag = "Ball";
    [Tooltip("�R���g���[���[�ւ̒Ǐ]��L���ɂ��邩")]
    public bool followController = true;
    [Tooltip("�U���q�̈ʒu������Transform�BFMOD�̃p�����[�^���ɍ��킹�Ă��������B")]
    [SerializeField] private List<Transform> vibratorPositions;
    [Header("�G�o�t�B�[�h�o�b�N�ݒ�")]
    [Tooltip("�Đ�����FMOD�̐G�o�C�x���g�̃��X�g�BvibratorPositions�̏��Ԃƈ�v�����Ă��������B")]
    [SerializeField] private List<EventReference> kickHapticEvents;

    private Rigidbody rb;
    private Vector3 previousPosition;
    private Vector3 currentVelocity;
    public Vector3 CurrentVelocity => currentVelocity;

    void Awake()
    {
        // --- �� �C���_ 2�FRigidbody���擾���AKinematic�ɐݒ� ---
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        // �����I�ȗ͂��󂯂Ȃ��悤��Kinematic�i�^���w�I�j�ɐݒ�
        // ����ɂ��A���̃I�u�W�F�N�g�Ƃ̏Փ˂͌��m���邪�A�d�͂⑼�̗͂̉e���͎󂯂Ȃ�

    }

    // --- �� �C���_ 3�FLateUpdate����FixedUpdate�ɏ������ړ� ---
    // �������Z�Ɋ֘A����ړ���FixedUpdate�ōs��
    void FixedUpdate()
    {
        if (followController && targetControllerAnchor != null)
        {
            // --- �� �C���_ 4�FMovePosition��MoveRotation���g�p ---
            // transform.position�𒼐ڕύX����̂ł͂Ȃ��ARigidbody����Ĉړ�������
            Vector3 targetPosition = targetControllerAnchor.position + new Vector3(xOffset, yOffset, zOffset);


            //currentVelocity = (targetPosition - previousPosition) / Time.fixedDeltaTime;
            previousPosition = targetPosition;
            rb.MovePosition(targetPosition);

            // ��]�����l��Rigidbody����čs��
            Quaternion targetRotation = targetControllerAnchor.rotation * Quaternion.Euler(x_rotationOffset, y_rotationOffset, z_rotationOffset);
            rb.MoveRotation(targetRotation);
        }
        //else
        //{
        //    currentVelocity = Vector3.zero;
        //}
    }
    private void OnCollisionEnter(Collision collision)
    {
        // �Փ˂������肪�{�[���łȂ���Ή������Ȃ�
        if (!collision.gameObject.CompareTag(ballTag))
        {
            return;
        }

        // �G�o�C�x���g��Unity�̃C���X�y�N�^�[��Őݒ肳��Ă��Ȃ���Ώ����𒆒f
        if (kickHapticEvents == null || kickHapticEvents.Count == 0)
        {
            // ���̃��b�Z�[�W�̓G���[�ł͂Ȃ��̂ŁA�x��(Warning)�Ƃ��ĕ\��
            Debug.LogWarning("Kick Haptic Event���ݒ肳��Ă��܂���B");
            return;
        }

        // �ŏ��̏Փ˓_�����[���h���W�Ŏ擾
        Vector3 contactPoint = collision.contacts[0].point;

        // --- �������炪�G�o�t�B�[�h�o�b�N�̎�v���W�b�N ---

        // 1. �Փ˓_�ɍł��߂��U���q�}�[�J�[��XZ���ʏ�Ō�����
        int closestVibratorIndex = -1;
        float minDistance = float.MaxValue;

        // �Փ˓_��XZ���W��Vector2�Ƃ��Ď��o���Ă���
        Vector2 contactPointXZ = new Vector2(contactPoint.x, contactPoint.z);

        for (int i = 0; i < vibratorPositions.Count; i++)
        {
            // �e�U���q��XZ���W��Vector2�Ƃ��Ď��o��
            Vector3 vibratorPos = vibratorPositions[i].position;
            Vector2 vibratorPointXZ = new Vector2(vibratorPos.x, vibratorPos.z);

            // Vector2.Distance��XZ���ʏ�̋������v�Z
            float distance = Vector2.Distance(contactPointXZ, vibratorPointXZ);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestVibratorIndex = i;
            }
        }

        // 2. �ł��߂��U���q�����������ꍇ�AFMOD�C�x���g���Đ�
        if (closestVibratorIndex != -1)
        {
            Debug.Log($"�{�[���� {vibratorPositions[closestVibratorIndex].name} �̋߂��Ƀq�b�g�B�U���qID: {closestVibratorIndex} ���Đ����܂��B");


            // �C���f�b�N�X���g���čĐ�����C�x���g�����X�g����I��
            EventReference selectedEvent = kickHapticEvents[closestVibratorIndex];
            // �C�x���g�̔����ʒu���Փ˓_�ɐݒ�i3D�T�E���h�Ƃ��ĈӖ������j
            Debug.Log($"�{�[���� {vibratorPositions[closestVibratorIndex].name} �̋߂��Ƀq�b�g�B�C�x���g���X�g�� {closestVibratorIndex} �Ԗڂ��Đ����܂��B");

            // �I�������C�x���g���A�Փ˓_�ōĐ� (PlayOneShot�̓p�����[�^�ݒ�s�v�ȏꍇ�ɕ֗�)
            RuntimeManager.PlayOneShot(selectedEvent, contactPoint);
        }
    }
}
