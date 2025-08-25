using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    // Start is called before the first frame update
    // 【設定項目1】リセットしたいボールのオブジェクト
    public Transform ballTransform;

    // 【設定項目2】基準となる靴のオブジェクト
    public Transform shoeTransform;

    // 【設定項目3】靴からどれくらい前に出すかの距離
    public float forwardOffset = 0.5f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Three))
        {
            // ボールと靴のオブジェクトが設定されているか念のため確認
            if (ballTransform != null && shoeTransform != null)
            {
                // 1. 靴の現在位置を取得
                Vector3 shoePos = shoeTransform.position;

                // 2. 靴が向いている前方の方向を取得
                Vector3 shoeForward = shoeTransform.forward;

                // 3. ボールの新しい位置を計算（靴の位置 + 靴の前方 × 距離）
                Vector3 newBallPosition = shoePos + shoeForward * forwardOffset;

                // 4. ボールの位置を更新
                ballTransform.position = newBallPosition;

                // 5. (おまけ) ボールの動きを完全に止めると、より綺麗にリセットされます
                Rigidbody ballRb = ballTransform.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    ballRb.velocity = Vector3.zero;        // 移動速度をリセット
                    ballRb.angularVelocity = Vector3.zero; // 回転速度をリセット
                }
            }
        }
    }
}

