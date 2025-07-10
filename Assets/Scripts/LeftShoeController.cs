using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftShoeController : MonoBehaviour
{
    // Start is called before the first frame update]
    public GameObject leftShoe;
    public float yOffset = -0.09f;
    public float zOffset = 0.15f; // X軸方向のオフセット（必要に応じて調整）
    public float xOffset = 0.11f; // Z軸方向のオフセット（必要に応じて調整）
    void Start()
    {
        
}

    // Update is called once per frame
    void Update()
    {
        // rightShoe の位置に Y 軸方向のオフセットを加える
        transform.position = leftShoe.transform.position + new Vector3(xOffset, yOffset, zOffset);

        // 右足の回転に、Y軸方向で180度回転を加える
        transform.rotation = leftShoe.transform.rotation * Quaternion.Euler(35, 180, 65);
    }
}
