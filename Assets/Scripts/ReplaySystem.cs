// ReplaySystem.cs
// 役割：ボールと靴の位置・回転を記録し、再生する

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// このファイル内で完結するため、構造体はここに置く
public struct TransformData
{
    public float timestamp;
    public Vector3 position;
    public Quaternion rotation;
}

public class ReplaySystem : MonoBehaviour
{
    [Header("リプレイ設定")]
    [SerializeField] private bool smoothReplay = true;

    [Header("対象オブジェクト")]
    [SerializeField] private GameObject ballObject;
    [SerializeField] private GameObject shoeObject;

    // --- コンポーネント参照 ---
    private Renderer ballRenderer;
    private ShoeController shoeController;
    private Rigidbody shoeRb;

    // --- 内部状態 ---
    private List<TransformData> ballLog = new List<TransformData>();
    private List<TransformData> shoeLog = new List<TransformData>();
    private bool isLogging = false;

    // ★★★ 変更点: 記録時の速度(gameSpeed)は不要なため削除 ★★★

    void Awake()
    {
        if (ballObject != null) ballRenderer = ballObject.GetComponent<Renderer>();
        if (shoeObject != null)
        {
            shoeController = shoeObject.GetComponent<ShoeController>();
            shoeRb = shoeObject.GetComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        if (isLogging)
        {
            LogTransforms();
        }
    }

    // ★★★ 変更点: 引数(currentSpeed)を削除 ★★★
    public void StartLogging()
    {
        ballLog.Clear();
        shoeLog.Clear();
        isLogging = true;
        Debug.Log("リプレイロギング開始");
    }

    public void StopLogging()
    {
        isLogging = false;
        Debug.Log("リプレイロギング停止");
    }

    // ★★★ 変更点: 引数で再生速度(playbackSpeed)を受け取るように変更 ★★★
    public IEnumerator Play(float playbackSpeed)
    {
        if (ballLog.Count == 0)
        {
            Debug.LogWarning("リプレイデータがありません");
            yield break;
        }

        // 0以下の速度が指定された場合は等速に戻す
        if (playbackSpeed <= 0)
        {
            playbackSpeed = 1.0f;
        }

        Debug.Log($"リプレイ再生開始 (再生速度: {playbackSpeed})");
        SetReplayMode(true);

        yield return new WaitForSecondsRealtime(1.5f);

        // --- 再生ロジックを刷新 ---
        float startTime = ballLog[0].timestamp;
        float replayDuration = ballLog.Last().timestamp - startTime;

        // ★★★ 変更点: 記録時の速度ではなく、指定された再生速度で再生時間を計算 ★★★
        float replayPlaybackDuration = replayDuration / playbackSpeed;
        float replayTimer = 0f;

        while (replayTimer <= replayPlaybackDuration + float.Epsilon)
        {
            replayTimer += Time.unscaledDeltaTime;

            // ★★★ 変更点: 再生タイマーに再生速度を掛けて、記録データ上の正しい時刻を算出 ★★★
            float tstamp = startTime + Mathf.Min(replayTimer * playbackSpeed, replayDuration);

            // 補間再生
            if (smoothReplay)
            {
                ApplyInterpolatedTransform(ballLog, ballObject.transform, tstamp);
                if (shoeObject != null) ApplyInterpolatedTransform(shoeLog, shoeObject.transform, tstamp);
            }
            // フレーム毎の再生
            else
            {
                ApplyTransformAtTimestamp(ballLog, ballObject.transform, tstamp);
                if (shoeObject != null) ApplyTransformAtTimestamp(shoeLog, shoeObject.transform, tstamp);
            }

            yield return null;
        }

        // 最終フレームにきっちり合わせる
        ballObject.transform.position = ballLog.Last().position;
        ballObject.transform.rotation = ballLog.Last().rotation;
        if (shoeObject != null && shoeLog.Any())
        {
            shoeObject.transform.position = shoeLog.Last().position;
            shoeObject.transform.rotation = shoeLog.Last().rotation;
        }

        Debug.Log("リプレイ終了");
        SetReplayMode(false);
    }

    /// <summary>
    /// リプレイモードの開始/終了を切り替えます
    /// </summary>
    private void SetReplayMode(bool isReplaying)
    {
        if (isReplaying)
        {
            if (shoeController != null) shoeController.followController = false;
            if (shoeRb != null)
            {
                shoeRb.isKinematic = true;
                shoeRb.velocity = Vector3.zero;
                shoeRb.angularVelocity = Vector3.zero;
            }
            SetBallMaterialAlpha(0.3f);
        }
        else
        {
            if (shoeController != null) shoeController.followController = true;
            if (shoeRb != null) shoeRb.isKinematic = true;
            SetBallMaterialAlpha(1.0f);
        }
    }

    // --- 以下、ヘルパーメソッド (変更なし) ---
    private void LogTransforms()
    {
        float currentTime = Time.time;
        ballLog.Add(new TransformData { timestamp = currentTime, position = ballObject.transform.position, rotation = ballObject.transform.rotation });
        if (shoeObject != null)
        {
            shoeLog.Add(new TransformData { timestamp = currentTime, position = shoeObject.transform.position, rotation = shoeObject.transform.rotation });
        }
    }

    private void ApplyInterpolatedTransform(List<TransformData> log, Transform target, float timestamp)
    {
        if (log.Count <= 1) return;
        int i0 = GetIndexForTimestamp(log, timestamp);
        int i1 = (i0 < log.Count - 1) ? i0 + 1 : i0;
        var a = log[i0];
        var b = log[i1];
        float dt = b.timestamp - a.timestamp;
        float t = dt > Mathf.Epsilon ? (timestamp - a.timestamp) / dt : 0f;
        target.position = Vector3.Lerp(a.position, b.position, t);
        target.rotation = Quaternion.Slerp(a.rotation, b.rotation, t);
    }

    private void ApplyTransformAtTimestamp(List<TransformData> log, Transform target, float timestamp)
    {
        int index = GetIndexForTimestamp(log, timestamp);
        if (index >= 0)
        {
            target.position = log[index].position;
            target.rotation = log[index].rotation;
        }
    }

    private int GetIndexForTimestamp(List<TransformData> log, float timestamp)
    {
        if (log.Count == 0) return -1;
        for (int i = 0; i < log.Count - 1; i++)
        {
            if (log[i + 1].timestamp >= timestamp)
            {
                return i;
            }
        }
        return log.Count - 1;
    }

    private void SetBallMaterialAlpha(float alpha)
    {
        if (ballRenderer == null) return;
        var mat = ballRenderer.material;
        if (alpha < 1.0f)
        {
            mat.SetFloat("_Mode", 2); mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha); mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha); mat.SetInt("_ZWrite", 0); mat.DisableKeyword("_ALPHATEST_ON"); mat.EnableKeyword("_ALPHABLEND_ON"); mat.DisableKeyword("_ALPHAPREMULTIPLY_ON"); mat.renderQueue = 3000;
        }
        else
        {
            mat.SetFloat("_Mode", 0); mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One); mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero); mat.SetInt("_ZWrite", 1); mat.DisableKeyword("_ALPHATEST_ON"); mat.DisableKeyword("_ALPHABLEND_ON"); mat.DisableKeyword("_ALPHAPREMULTIPLY_ON"); mat.renderQueue = -1;
        }
        Color c = mat.color; c.a = alpha; mat.color = c;
    }
}