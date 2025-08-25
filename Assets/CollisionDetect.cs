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
    // 正しいコード
    private void OnCollisionEnter(Collision collision)
    { // ← OnCollisionEnter メソッドの開始

        if (hasHitTarget) return;

        // 'collision' はこのメソッドの内側なので、問題なく使える
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
            Debug.Log("倍率 " + target.multiplier + " を保存しました。2秒後にシーンをロードします。");

            StartCoroutine(LoadSceneAfterDelay(1.0f, "Training_new"));
        }

    } // ← OnCollisionEnter メソッドの終了。すべての処理が内側にある。
    private IEnumerator LoadSceneAfterDelay(float delay, string sceneName)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
