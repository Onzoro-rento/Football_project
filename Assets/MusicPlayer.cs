using UnityEngine;
using FMODUnity; // FMOD for Unityの名前空間を使用するために必要

public class FMODKeyPlayer : MonoBehaviour
{
    // UnityのInspectorから再生したいFMODイベントを設定するための変数
    [SerializeField]
    private EventReference soundA; // Aキーで再生するサウンド

    [SerializeField]
    private EventReference soundB; // Bキーで再生するサウンド

    [SerializeField]
    private EventReference soundC; // Cキーで再生するサウンド

    void Update()
    {
        // "A"キーが押された瞬間を検知
        if (Input.GetKeyDown(KeyCode.A))
        {
            // soundAが設定されていれば再生
            if (!soundA.IsNull)
            {
                RuntimeManager.PlayOneShot(soundA, transform.position);
            }
        }

        // "B"キーが押された瞬間を検知
        if (Input.GetKeyDown(KeyCode.B))
        {
            // soundBが設定されていれば再生
            if (!soundB.IsNull)
            {
                RuntimeManager.PlayOneShot(soundB, transform.position);
            }
        }

        // "C"キーが押された瞬間を検知
        if (Input.GetKeyDown(KeyCode.C))
        {
            // soundCが設定されていれば再生
            if (!soundC.IsNull)
            {
                RuntimeManager.PlayOneShot(soundC, transform.position);
            }
        }
    }
}