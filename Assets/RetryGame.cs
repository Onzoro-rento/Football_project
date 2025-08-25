using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RetryGame : MonoBehaviour
{
    [Header("�{�^���̌����ڐݒ�")]
    public Color pressedColor = new Color(0.8f, 0.8f, 0.8f, 0.7f);
    private Image buttonImage;
    private Color originalColor;
    [SerializeField] private GameManager gameManager;

    // �� �ǉ�: �{�^������x�����ꂽ�����Ǘ�����t���O
    private bool isPressed = false;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        originalColor = buttonImage.color;
    }

    // �� OnEnable�Ń��Z�b�g����悤�ɕύX
    // OnEnable�́A�I�u�W�F�N�g���A�N�e�B�u�ɂȂ����Ƃ��ɌĂ΂��
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
        // �܂�������Ă��炸�A Shoe�^�O�̃I�u�W�F�N�g���G�ꂽ�ꍇ
        if (!isPressed && other.gameObject.CompareTag("Shoe"))
        {
            Debug.Log("Shoe�����g���C�{�^���ɐG��܂����B�Q�[�������Z�b�g���܂��B");

            isPressed = true; // �A���ŌĂ΂��̂�h��
            buttonImage.color = pressedColor; // �G�ꂽ���̐F�ɕύX

            // ������ �A�N�V������OnTriggerEnter�Ŏ��s ������
            gameManager.RetryGame();
        }
    }

    // OnTriggerExit�͐F��߂����߂����Ɏg���Ă��ǂ����A
    // ����̓��Z�b�g�Ɠ����Ƀp�l���������邽�߁A�����s�v�ɂȂ�
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Shoe"))
        {
            // �p�l����������̂ŁA���̏����͂قƂ�ǂ̏ꍇ�A���s����Ȃ�
            // isPressed = false; // OnEnable�Ń��Z�b�g����̂ŕs�v
            // buttonImage.color = originalColor;
        }
    }
}