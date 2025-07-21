using UnityEngine;

public class ShoeController : MonoBehaviour
{
    [Header("追従する対象のコントローラーアンカー")]
    public Transform targetControllerAnchor;

    [Header("位置・回転のオフセット")]
    public float xOffset = -0.05f;
    public float yOffset = -0.09f;
    public float zOffset = 0.05f;
    public float x_rotationOffset = 65f;
    public float y_rotationOffset = 180f;
    public float z_rotationOffset = -60f;

    [Tooltip("コントローラーへの追従を有効にするか")]
    public bool followController = true;

    void LateUpdate()   
    {
        // followControllerがtrueの場合のみ、コントローラーの位置に追従する
        if (followController && targetControllerAnchor != null)
        {
            transform.position = targetControllerAnchor.position + targetControllerAnchor.TransformDirection(new Vector3(xOffset, yOffset, zOffset));
            transform.rotation = targetControllerAnchor.rotation * Quaternion.Euler(x_rotationOffset, y_rotationOffset, z_rotationOffset);
        }
    }
}