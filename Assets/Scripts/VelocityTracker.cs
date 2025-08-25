// VelocityTracker.cs
// 役割：アタッチされたオブジェクトの速度を追跡・保持する。

using UnityEngine;

public class VelocityTracker : MonoBehaviour
{
    private Vector3 lastPosition;
    private Vector3 currentVelocity;

    void Start()
    {
        // RigidbodyがあればKinematicにしておくことを推奨
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        // FixedUpdate間の移動量と時間から速度を計算
        currentVelocity = (transform.position - lastPosition) / Time.fixedUnscaledDeltaTime;
        lastPosition = transform.position;
    }

    /// <summary>
    /// 現在の速度を取得します。
    /// </summary>
    public Vector3 GetVelocity()
    {
        return currentVelocity;
    }
}