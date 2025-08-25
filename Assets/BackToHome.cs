using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackToHome : MonoBehaviour
{
    [Header("ボタンの見た目設定")]
    public Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.7f);
    private Image buttonImage;
    private Color originalColor;
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        originalColor = buttonImage.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnterが呼ばれました。");
        Debug.Log("トリガーに侵入したオブジェクト名: " + other.gameObject.name + " / タグ名: '" + other.gameObject.tag + "'");

        // ★ 追加: リプレイ中またはゲームアクティブ中はボタンを無効化する
        if (GameManager.CurrentState == GameManager.GameState.Replaying || GameManager.CurrentState == GameManager.GameState.Active)
        {
            Debug.Log("リプレイ中またはゲームアクティブ中のため、ボタン操作は無効です。");
            return;
        }

        if (other.gameObject.CompareTag("Shoe"))
        {
            Debug.Log("Shoeがボタンに触れました。");
            buttonImage.color = pressedColor; // 設定した「触れた時の色」に変更
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExitが呼ばれました。");
        Debug.Log("トリガーに侵入したオブジェクト名: " + other.gameObject.name + " / タグ名: '" + other.gameObject.tag + "'");

        // ★ 追加: リプレイ中またはゲームアクティブ中はボタンを無効化する
        if (GameManager.CurrentState == GameManager.GameState.Replaying || GameManager.CurrentState == GameManager.GameState.Active)
        {
            Debug.Log("リプレイ中またはゲームアクティブ中のため、ボタン操作は無効です。");
            return;
        }

        if (other.gameObject.CompareTag("Shoe"))
        {
            gameManager.GoToHomeScene(); // ゲームをリセットするメソッドを呼び出す
            buttonImage.color = originalColor; // 記憶しておいた元の色に戻す
        }
    }
}