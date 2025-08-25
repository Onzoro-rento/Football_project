using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Imageコンポーネントを扱うために必要
public class UIButton : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ball;
    public GameObject tutorialUI;
    [Header("ボタンの見た目設定")]
    public Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.7f);
    private Image buttonImage;      // ボタンのImageコンポーネントを保存する変数
    private Color originalColor;    // 元の色を保存しておく変数
    void Start()
    {
        buttonImage = GetComponent<Image>();
        originalColor = buttonImage.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnterが呼ばれました。");
        Debug.Log("トリガーに侵入したオブジェクト名: " + other.gameObject.name + " / タグ名: '" + other.gameObject.tag + "'");
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
        if (other.gameObject.CompareTag("Shoe"))
        {
            ball.SetActive(true);
            tutorialUI.SetActive(false);
            
            buttonImage.color = originalColor; // 記憶しておいた元の色に戻す
            
        }
    }
}

