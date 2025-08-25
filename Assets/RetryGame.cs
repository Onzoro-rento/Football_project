using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RetryGame : MonoBehaviour
{
    [Header("ボタンの見た目設定")]
    public Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.7f);
    private Image buttonImage;
    private Color originalColor;
    [SerializeField] private GameManager gameManager;

    // ★ 追加: ボタンが一度押されたかを管理するフラグ
    private bool isPressed = false;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        originalColor = buttonImage.color;
    }

    // ★ OnEnableでリセットするように変更
    // OnEnableは、オブジェクトがアクティブになったときに呼ばれる
    private void OnEnable()
    {
        isPressed = false;
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // まだ押されておらず、 Shoeタグのオブジェクトが触れた場合
        if (!isPressed && other.gameObject.CompareTag("Shoe"))
        {
            Debug.Log("Shoeがリトライボタンに触れました。ゲームをリセットします。");

            isPressed = true; // 連続で呼ばれるのを防ぐ
            buttonImage.color = pressedColor; // 触れた時の色に変更

            // ★★★ アクションをOnTriggerEnterで実行 ★★★
            gameManager.RetryGame();
        }
    }

    // OnTriggerExitは色を戻すためだけに使っても良いが、
    // 今回はリセットと同時にパネルが消えるため、実質不要になる
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Shoe"))
        {
            // パネルが消えるので、この処理はほとんどの場合、実行されない
            // isPressed = false; // OnEnableでリセットするので不要
            // buttonImage.color = originalColor;
        }
    }
}