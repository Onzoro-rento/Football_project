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
        // �Փ˂�������̃Q�[���I�u�W�F�N�g�̃^�O�� "Shoe" ���ǂ����𔻒�
        if (collision.gameObject.tag == "Shoe")
        {
            Debug.Log("Ball��Shoe�ɏՓ˂��܂����B���ʉ����Đ����܂��B");
            // ���ʉ���AudioSource���ݒ肳��Ă���΍Đ�
            if (audioSource.clip != null && audioSource != null)
            {
                // PlayOneShot�Ō��ʉ����Đ�
                audioSource.PlayOneShot(audioSource.clip);
                
            }
        }
    }
}
