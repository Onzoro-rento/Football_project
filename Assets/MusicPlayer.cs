using UnityEngine;
using FMODUnity; // FMOD for Unity�̖��O��Ԃ��g�p���邽�߂ɕK�v

public class FMODKeyPlayer : MonoBehaviour
{
    // Unity��Inspector����Đ�������FMOD�C�x���g��ݒ肷�邽�߂̕ϐ�
    [SerializeField]
    private EventReference soundA; // A�L�[�ōĐ�����T�E���h

    [SerializeField]
    private EventReference soundB; // B�L�[�ōĐ�����T�E���h

    [SerializeField]
    private EventReference soundC; // C�L�[�ōĐ�����T�E���h

    void Update()
    {
        // "A"�L�[�������ꂽ�u�Ԃ����m
        if (Input.GetKeyDown(KeyCode.A))
        {
            // soundA���ݒ肳��Ă���΍Đ�
            if (!soundA.IsNull)
            {
                RuntimeManager.PlayOneShot(soundA, transform.position);
            }
        }

        // "B"�L�[�������ꂽ�u�Ԃ����m
        if (Input.GetKeyDown(KeyCode.B))
        {
            // soundB���ݒ肳��Ă���΍Đ�
            if (!soundB.IsNull)
            {
                RuntimeManager.PlayOneShot(soundB, transform.position);
            }
        }

        // "C"�L�[�������ꂽ�u�Ԃ����m
        if (Input.GetKeyDown(KeyCode.C))
        {
            // soundC���ݒ肳��Ă���΍Đ�
            if (!soundC.IsNull)
            {
                RuntimeManager.PlayOneShot(soundC, transform.position);
            }
        }
    }
}