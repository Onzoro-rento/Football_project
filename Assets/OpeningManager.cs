using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("管理するUIオブジェクト")]
    public GameObject tutorialUI; // 説明UIのCanvasやPanel

    [Header("出現させるボールのオブジェクト")]
    public GameObject ball; // ボールの元データ
    void Start()
    {
        ball.SetActive(false); // 初期状態ではボールを非表示にする
        tutorialUI.SetActive(false); // 説明UIを表示する
        StartCoroutine(ShowUIAfterDelay(3.0f)); // 3秒後にUIを表示する
    }
    private IEnumerator ShowUIAfterDelay(float delay)
    {
        // 指定秒数だけ待つ
        yield return new WaitForSeconds(delay);

        // UIを表示する
        if (tutorialUI != null)
        {
            tutorialUI.SetActive(true);
            Debug.Log("UIを表示しました。");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
}
