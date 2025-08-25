using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Image�R���|�[�l���g���������߂ɕK�v
public class UIButton : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ball;
    public GameObject tutorialUI;
    [Header("�{�^���̌����ڐݒ�")]
    public Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.7f);
    private Image buttonImage;      // �{�^����Image�R���|�[�l���g��ۑ�����ϐ�
    private Color originalColor;    // ���̐F��ۑ����Ă����ϐ�
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
        Debug.Log("OnTriggerEnter���Ă΂�܂����B");
        Debug.Log("�g���K�[�ɐN�������I�u�W�F�N�g��: " + other.gameObject.name + " / �^�O��: '" + other.gameObject.tag + "'");
        if (other.gameObject.CompareTag("Shoe"))
        {   
            Debug.Log("Shoe���{�^���ɐG��܂����B");
            buttonImage.color = pressedColor; // �ݒ肵���u�G�ꂽ���̐F�v�ɕύX
            
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit���Ă΂�܂����B");
        Debug.Log("�g���K�[�ɐN�������I�u�W�F�N�g��: " + other.gameObject.name + " / �^�O��: '" + other.gameObject.tag + "'");
        if (other.gameObject.CompareTag("Shoe"))
        {
            ball.SetActive(true);
            tutorialUI.SetActive(false);
            
            buttonImage.color = originalColor; // �L�����Ă��������̐F�ɖ߂�
            
        }
    }
}

