using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackToHome : MonoBehaviour
{
    [Header("�{�^���̌����ڐݒ�")]
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
        Debug.Log("OnTriggerEnter���Ă΂�܂����B");
        Debug.Log("�g���K�[�ɐN�������I�u�W�F�N�g��: " + other.gameObject.name + " / �^�O��: '" + other.gameObject.tag + "'");

        // �� �ǉ�: ���v���C���܂��̓Q�[���A�N�e�B�u���̓{�^���𖳌�������
        if (GameManager.CurrentState == GameManager.GameState.Replaying || GameManager.CurrentState == GameManager.GameState.Active)
        {
            Debug.Log("���v���C���܂��̓Q�[���A�N�e�B�u���̂��߁A�{�^������͖����ł��B");
            return;
        }

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

        // �� �ǉ�: ���v���C���܂��̓Q�[���A�N�e�B�u���̓{�^���𖳌�������
        if (GameManager.CurrentState == GameManager.GameState.Replaying || GameManager.CurrentState == GameManager.GameState.Active)
        {
            Debug.Log("���v���C���܂��̓Q�[���A�N�e�B�u���̂��߁A�{�^������͖����ł��B");
            return;
        }

        if (other.gameObject.CompareTag("Shoe"))
        {
            gameManager.GoToHomeScene(); // �Q�[�������Z�b�g���郁�\�b�h���Ăяo��
            buttonImage.color = originalColor; // �L�����Ă��������̐F�ɖ߂�
        }
    }
}