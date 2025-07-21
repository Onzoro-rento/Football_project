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
    [SerializeField]
    private EventReference soundD; // D�L�[�ōĐ�����T�E���h
    [SerializeField]
    private EventReference soundE; // E�L�[�ōĐ�����T�E���h
    [SerializeField]
    private EventReference soundF; // F�L�[�ōĐ�����T�E���h
    [SerializeField]
    private EventReference soundG; // G�L�[�ōĐ�����T�E���h
    [SerializeField]
    private EventReference soundH; // F�L�[�ōĐ�����T�E���h

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
        if (Input.GetKeyDown(KeyCode.D))
        {
            // soundC���ݒ肳��Ă���΍Đ�
            if (!soundD.IsNull)
            {
                RuntimeManager.PlayOneShot(soundD, transform.position);
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            // soundC���ݒ肳��Ă���΍Đ�
            if (!soundE.IsNull)
            {
                RuntimeManager.PlayOneShot(soundE, transform.position);
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            // soundC���ݒ肳��Ă���΍Đ�
            if (!soundF.IsNull)
            {
                RuntimeManager.PlayOneShot(soundF, transform.position);
            }
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            // soundC���ݒ肳��Ă���΍Đ�
            if (!soundG.IsNull)
            {
                RuntimeManager.PlayOneShot(soundG, transform.position);
            }
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            // soundC���ݒ肳��Ă���΍Đ�
            if (!soundH.IsNull)
            {
                RuntimeManager.PlayOneShot(soundH, transform.position);
            }
        }
    }
}