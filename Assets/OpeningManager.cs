using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningManager : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("�Ǘ�����UI�I�u�W�F�N�g")]
    public GameObject tutorialUI; // ����UI��Canvas��Panel

    [Header("�o��������{�[���̃I�u�W�F�N�g")]
    public GameObject ball; // �{�[���̌��f�[�^
    void Start()
    {
        ball.SetActive(false); // ������Ԃł̓{�[�����\���ɂ���
        tutorialUI.SetActive(false); // ����UI��\������
        StartCoroutine(ShowUIAfterDelay(3.0f)); // 3�b���UI��\������
    }
    private IEnumerator ShowUIAfterDelay(float delay)
    {
        // �w��b�������҂�
        yield return new WaitForSeconds(delay);

        // UI��\������
        if (tutorialUI != null)
        {
            tutorialUI.SetActive(true);
            Debug.Log("UI��\�����܂����B");
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
}
