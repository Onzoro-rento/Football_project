using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionDetect : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    private bool hasHitTarget = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    // �������R�[�h
    private void OnCollisionEnter(Collision collision)
    { // �� OnCollisionEnter ���\�b�h�̊J�n

        if (hasHitTarget) return;

        // 'collision' �͂��̃��\�b�h�̓����Ȃ̂ŁA���Ȃ��g����
        DifficultyTarget target = collision.gameObject.GetComponent<DifficultyTarget>();

        if (target != null)
        {
            hasHitTarget = true;

            if (rb != null)
            {
                rb.isKinematic = true;
            }

            PlayerPrefs.SetFloat("SpeedMultiplier", target.multiplier);
            PlayerPrefs.Save();
            Debug.Log("�{�� " + target.multiplier + " ��ۑ����܂����B2�b��ɃV�[�������[�h���܂��B");

            StartCoroutine(LoadSceneAfterDelay(1.0f, "Training_new"));
        }

    } // �� OnCollisionEnter ���\�b�h�̏I���B���ׂĂ̏����������ɂ���B
    private IEnumerator LoadSceneAfterDelay(float delay, string sceneName)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
