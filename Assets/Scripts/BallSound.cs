using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSound : MonoBehaviour
{
    // Start is called before the first frame update
    AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        // 衝突した相手のゲームオブジェクトのタグが "Shoe" かどうかを判定
        if (collision.gameObject.tag == "Shoe")
        {
            Debug.Log("BallがShoeに衝突しました。効果音を再生します。");
            // 効果音とAudioSourceが設定されていれば再生
            if (audioSource.clip != null && audioSource != null)
            {
                // PlayOneShotで効果音を再生
                audioSource.PlayOneShot(audioSource.clip);
                
            }
        }
    }
}
