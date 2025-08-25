// UIManager.cs
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI statusText;
    //[SerializeField] private GameObject replayActivatorObject; // ★ 不要になるのでコメントアウトまたは削除

    // ★ 追加: 新しいUIパネルへの参照
    [SerializeField] private GameObject gameClearPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultScoreText_Over;  // ゲームオーバー時の最終スコア
    [SerializeField] private TextMeshProUGUI resultScoreText_Clear; // ゲームクリア時の最終スコア
    void Start()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        //if (replayActivatorObject != null) replayActivatorObject.SetActive(false); // ★ 不要

        // ★ 追加: ゲーム開始時にパネルを非表示にする
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateStatusText("");
    }
    // ★ 追加: ゲーム終了時の最終スコアを更新するメソッド
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

    // ★ replayActivatorObject関連のメソッドは不要になるのでコメントアウトまたは削除
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

    // ★ 追加: 新しいパネルを制御するメソッド群
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