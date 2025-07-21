using UnityEngine;
using OVR; // OVRManager���g�p���邽�߂ɕK�v

public class RefreshRateSetter : MonoBehaviour
{
    void Start()
    {
        // OVRManager�����݂��A�L���ȏꍇ�Ƀ��t���b�V�����[�g��ݒ�
        if (OVRManager.display != null)
        {
            float targetRefreshRate = 72f; // �ڕW�̃��t���b�V�����[�g
            OVRManager.display.displayFrequency = targetRefreshRate;
            Debug.Log($"Display refresh rate set to: {OVRManager.display.displayFrequency}Hz");

            // ���p�\�ȃ��t���b�V�����[�g���m�F�������ꍇ�i�I�v�V�����j
            // foreach (float freq in OVRManager.display.displayFrequenciesAvailable)
            // {
            //     Debug.Log($"Available refresh rate: {freq}Hz");
            // }
        }
        else
        {
            Debug.LogWarning("OVRManager.display is not available. Make sure OVRManager is in the scene.");
        }
    }
}