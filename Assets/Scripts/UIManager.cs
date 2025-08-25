// UIManager.cs
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI�v�f")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI statusText;
    //[SerializeField] private GameObject replayActivatorObject; // �� �s�v�ɂȂ�̂ŃR�����g�A�E�g�܂��͍폜

    // �� �ǉ�: �V����UI�p�l���ւ̎Q��
    [SerializeField] private GameObject gameClearPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultScoreText_Over;  // �Q�[���I�[�o�[���̍ŏI�X�R�A
    [SerializeField] private TextMeshProUGUI resultScoreText_Clear; // �Q�[���N���A���̍ŏI�X�R�A
    void Start()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        //if (replayActivatorObject != null) replayActivatorObject.SetActive(false); // �� �s�v

        // �� �ǉ�: �Q�[���J�n���Ƀp�l�����\���ɂ���
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateStatusText("");
    }
    // �� �ǉ�: �Q�[���I�����̍ŏI�X�R�A���X�V���郁�\�b�h
    public void UpdateResultScore(int count)
    {
        string resultMessage = $"Final Score: {count}";

        if (resultScoreText_Over != null)
        {
            resultScoreText_Over.text = resultMessage;
        }
        if (resultScoreText_Clear != null)
        {
            resultScoreText_Clear.text = resultMessage;
        }
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

    // �� replayActivatorObject�֘A�̃��\�b�h�͕s�v�ɂȂ�̂ŃR�����g�A�E�g�܂��͍폜
    /*
    public void ShowReplayActivator(Vector3 position)
    {
        // ...
    }

    public void HideReplayActivator()
    {
        // ...
    }
    */

    // �� �ǉ�: �V�����p�l���𐧌䂷�郁�\�b�h�Q
    public void ShowGameClearPanel()
    {
        if (gameClearPanel != null) gameClearPanel.SetActive(true);
    }

    public void HideGameClearPanel()
    {
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void HideGameOverPanel()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }
}