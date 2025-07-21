// UIManager.cs
// �����FUI�v�f�̕\���E��\���E�e�L�X�g�X�V��S������

using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI�v�f")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject replayActivatorObject;

    void Start()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (replayActivatorObject != null) replayActivatorObject.SetActive(false);
        UpdateStatusText("");
    }

    public void UpdateStatusText(string text)
    {
        if (statusText != null) statusText.text = text;
    }

    public void ShowCountdown(string text)
    {
        if (countdownText == null) return;
        countdownText.gameObject.SetActive(true);
        countdownText.text = text;
    }

    public void HideCountdown()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    public void ShowReplayActivator(Vector3 position)
    {
        if (replayActivatorObject == null) return;
        replayActivatorObject.transform.position = position + new Vector3(0, 1.2f, 0);
        replayActivatorObject.SetActive(true);
    }

    public void HideReplayActivator()
    {
        if (replayActivatorObject != null) replayActivatorObject.SetActive(false);
    }
}