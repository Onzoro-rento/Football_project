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
    [SerializeField]
    private EventReference soundD; // Dキーで再生するサウンド
    [SerializeField]
    private EventReference soundE; // Eキーで再生するサウンド
    [SerializeField]
    private EventReference soundF; // Fキーで再生するサウンド
    [SerializeField]
    private EventReference soundG; // Gキーで再生するサウンド
    [SerializeField]
    private EventReference soundH; // Fキーで再生するサウンド

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
        if (Input.GetKeyDown(KeyCode.D))
        {
            // soundCが設定されていれば再生
            if (!soundD.IsNull)
            {
                RuntimeManager.PlayOneShot(soundD, transform.position);
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            // soundCが設定されていれば再生
            if (!soundE.IsNull)
            {
                RuntimeManager.PlayOneShot(soundE, transform.position);
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            // soundCが設定されていれば再生
            if (!soundF.IsNull)
            {
                RuntimeManager.PlayOneShot(soundF, transform.position);
            }
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            // soundCが設定されていれば再生
            if (!soundG.IsNull)
            {
                RuntimeManager.PlayOneShot(soundG, transform.position);
            }
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            // soundCが設定されていれば再生
            if (!soundH.IsNull)
            {
                RuntimeManager.PlayOneShot(soundH, transform.position);
            }
        }
    }
}