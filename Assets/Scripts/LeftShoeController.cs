using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftShoeController : MonoBehaviour
{
    // Start is called before the first frame update]
    public GameObject leftShoe;
    public float yOffset = -0.09f;
    public float zOffset = 0.15f; // X�������̃I�t�Z�b�g�i�K�v�ɉ����Ē����j
    public float xOffset = 0.11f; // Z�������̃I�t�Z�b�g�i�K�v�ɉ����Ē����j
    void Start()
    {
        
}

    // Update is called once per frame
    void Update()
    {
        // rightShoe �̈ʒu�� Y �������̃I�t�Z�b�g��������
        transform.position = leftShoe.transform.position + new Vector3(xOffset, yOffset, zOffset);

        // �E���̉�]�ɁAY��������180�x��]��������
        transform.rotation = leftShoe.transform.rotation * Quaternion.Euler(35, 180, 65);
    }
}
