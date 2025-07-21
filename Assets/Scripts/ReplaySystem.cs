// ReplaySystem.cs
// �����F�{�[���ƌC�̈ʒu�E��]���L�^���A�Đ�����

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// ���̃t�@�C�����Ŋ������邽�߁A�\���̂͂����ɒu��
public struct TransformData
{
    public float timestamp;
    public Vector3 position;
    public Quaternion rotation;
}

public class ReplaySystem : MonoBehaviour
{
    [Header("���v���C�ݒ�")]
    [SerializeField] private bool smoothReplay = true;

    [Header("�ΏۃI�u�W�F�N�g")]
    [SerializeField] private GameObject ballObject;
    [SerializeField] private GameObject shoeObject;

    // --- �R���|�[�l���g�Q�� ---
    private Renderer ballRenderer;
    private ShoeController shoeController;
    private Rigidbody shoeRb;

    // --- ������� ---
    private List<TransformData> ballLog = new List<TransformData>();
    private List<TransformData> shoeLog = new List<TransformData>();
    private bool isLogging = false;
    private float gameSpeed = 1.0f;

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

    public void StartLogging(float currentSpeed)
    {
        gameSpeed = currentSpeed;
        ballLog.Clear();
        shoeLog.Clear();
        isLogging = true;
        Debug.Log("���v���C���M���O�J�n");
    }

    public void StopLogging()
    {
        isLogging = false;
        Debug.Log("���v���C���M���O��~");
    }

    public IEnumerator Play()
    {
        if (ballLog.Count == 0)
        {
            Debug.LogWarning("���v���C�f�[�^������܂���");
            yield break;
        }

        Debug.Log("���v���C�Đ��J�n");
        SetReplayMode(true);

        yield return new WaitForSecondsRealtime(1.5f);

        // --- �Đ����W�b�N�i���̃R�[�h����قڗ��p�j ---
        float startTime = ballLog[0].timestamp;
        float replayDuration = ballLog.Last().timestamp - startTime;
        float replayPlaybackDuration = replayDuration / gameSpeed;
        float replayTimer = 0f;

        while (replayTimer <= replayPlaybackDuration + float.Epsilon)
        {
            replayTimer += Time.unscaledDeltaTime;
            float tstamp = startTime + Mathf.Min(replayTimer * gameSpeed, replayDuration);

            // ��ԍĐ�
            if (smoothReplay)
            {
                ApplyInterpolatedTransform(ballLog, ballObject.transform, tstamp);
                if (shoeObject != null) ApplyInterpolatedTransform(shoeLog, shoeObject.transform, tstamp);
            }
            // �t���[�����̍Đ�
            else
            {
                ApplyTransformAtTimestamp(ballLog, ballObject.transform, tstamp);
                if (shoeObject != null) ApplyTransformAtTimestamp(shoeLog, shoeObject.transform, tstamp);
            }

            yield return null;
        }

        // �ŏI�t���[���ɂ������荇�킹��
        ballObject.transform.position = ballLog.Last().position;
        ballObject.transform.rotation = ballLog.Last().rotation;
        if (shoeObject != null && shoeLog.Any())
        {
            shoeObject.transform.position = shoeLog.Last().position;
            shoeObject.transform.rotation = shoeLog.Last().rotation;
        }

        Debug.Log("���v���C�I��");
        SetReplayMode(false);
    }

    /// <summary>
    /// ���v���C���[�h�̊J�n/�I����؂�ւ��܂�
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
            if (shoeRb != null) shoeRb.isKinematic = true; // �O�̂���Kinematic�ɖ߂�
            SetBallMaterialAlpha(1.0f);
        }
    }

    // --- �ȉ��A�w���p�[���\�b�h ---
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
            mat.SetFloat("_Mode", 2); // Fade
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
        else
        {
            mat.SetFloat("_Mode", 0); // Opaque
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
        }
        Color c = mat.color;
        c.a = alpha;
        mat.color = c;
    }
}