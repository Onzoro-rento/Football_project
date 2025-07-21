using UnityEngine;
using OVR; // OVRManagerを使用するために必要

public class RefreshRateSetter : MonoBehaviour
{
    void Start()
    {
        // OVRManagerが存在し、有効な場合にリフレッシュレートを設定
        if (OVRManager.display != null)
        {
            float targetRefreshRate = 72f; // 目標のリフレッシュレート
            OVRManager.display.displayFrequency = targetRefreshRate;
            Debug.Log($"Display refresh rate set to: {OVRManager.display.displayFrequency}Hz");

            // 利用可能なリフレッシュレートを確認したい場合（オプション）
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