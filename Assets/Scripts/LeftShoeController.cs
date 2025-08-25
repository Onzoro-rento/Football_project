using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftShoeController : MonoBehaviour
{
    [Header("追従する対象のコントローラーアンカー")]
    public Transform targetControllerAnchor;
    // Start is called before the first frame update]
    public GameObject leftShoe;
    public float yOffset = -0.09f;
    public float zOffset = 0.15f; // X軸方向のオフセット（必要に応じて調整）
    public float xOffset = 0.11f; // Z軸方向のオフセット（必要に応じて調整）
    public float x_rotationOffset = 35f;
    public float y_rotationOffset = 180f;
    public float z_rotationOffset = 65f;
    [Header("設定")]
    [Tooltip("ボールのタグ名")]
    [SerializeField] private string ballTag = "Ball";
    [Tooltip("コントローラーへの追従を有効にするか")]
    public bool followController = true;
    [Tooltip("振動子の位置を示すTransform。FMODのパラメータ順に合わせてください。")]
    [SerializeField] private List<Transform> vibratorPositions;
    [Header("触覚フィードバック設定")]
    [Tooltip("再生するFMODの触覚イベントのリスト。vibratorPositionsの順番と一致させてください。")]
    [SerializeField] private List<EventReference> kickHapticEvents;

    private Rigidbody rb;
    private Vector3 previousPosition;
    private Vector3 currentVelocity;
    public Vector3 CurrentVelocity => currentVelocity;

    void Awake()
    {
        // --- ★ 修正点 2：Rigidbodyを取得し、Kinematicに設定 ---
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        // 物理的な力を受けないようにKinematic（運動学的）に設定
        // これにより、他のオブジェクトとの衝突は検知するが、重力や他の力の影響は受けない

    }

    // --- ★ 修正点 3：LateUpdateからFixedUpdateに処理を移動 ---
    // 物理演算に関連する移動はFixedUpdateで行う
    void FixedUpdate()
    {
        if (followController && targetControllerAnchor != null)
        {
            // --- ★ 修正点 4：MovePositionとMoveRotationを使用 ---
            // transform.positionを直接変更するのではなく、Rigidbodyを介して移動させる
            Vector3 targetPosition = targetControllerAnchor.position + new Vector3(xOffset, yOffset, zOffset);


            //currentVelocity = (targetPosition - previousPosition) / Time.fixedDeltaTime;
            previousPosition = targetPosition;
            rb.MovePosition(targetPosition);

            // 回転も同様にRigidbodyを介して行う
            Quaternion targetRotation = targetControllerAnchor.rotation * Quaternion.Euler(x_rotationOffset, y_rotationOffset, z_rotationOffset);
            rb.MoveRotation(targetRotation);
        }
        //else
        //{
        //    currentVelocity = Vector3.zero;
        //}
    }
    private void OnCollisionEnter(Collision collision)
    {
        // 衝突した相手がボールでなければ何もしない
        if (!collision.gameObject.CompareTag(ballTag))
        {
            return;
        }

        // 触覚イベントがUnityのインスペクター上で設定されていなければ処理を中断
        if (kickHapticEvents == null || kickHapticEvents.Count == 0)
        {
            // このメッセージはエラーではないので、警告(Warning)として表示
            Debug.LogWarning("Kick Haptic Eventが設定されていません。");
            return;
        }

        // 最初の衝突点をワールド座標で取得
        Vector3 contactPoint = collision.contacts[0].point;

        // --- ここからが触覚フィードバックの主要ロジック ---

        // 1. 衝突点に最も近い振動子マーカーをXZ平面上で見つける
        int closestVibratorIndex = -1;
        float minDistance = float.MaxValue;

        // 衝突点のXZ座標をVector2として取り出しておく
        Vector2 contactPointXZ = new Vector2(contactPoint.x, contactPoint.z);

        for (int i = 0; i < vibratorPositions.Count; i++)
        {
            // 各振動子のXZ座標をVector2として取り出す
            Vector3 vibratorPos = vibratorPositions[i].position;
            Vector2 vibratorPointXZ = new Vector2(vibratorPos.x, vibratorPos.z);

            // Vector2.DistanceでXZ平面上の距離を計算
            float distance = Vector2.Distance(contactPointXZ, vibratorPointXZ);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestVibratorIndex = i;
            }
        }

        // 2. 最も近い振動子が見つかった場合、FMODイベントを再生
        if (closestVibratorIndex != -1)
        {
            Debug.Log($"ボールが {vibratorPositions[closestVibratorIndex].name} の近くにヒット。振動子ID: {closestVibratorIndex} を再生します。");


            // インデックスを使って再生するイベントをリストから選択
            EventReference selectedEvent = kickHapticEvents[closestVibratorIndex];
            // イベントの発生位置を衝突点に設定（3Dサウンドとして意味を持つ）
            Debug.Log($"ボールが {vibratorPositions[closestVibratorIndex].name} の近くにヒット。イベントリストの {closestVibratorIndex} 番目を再生します。");

            // 選択したイベントを、衝突点で再生 (PlayOneShotはパラメータ設定不要な場合に便利)
            RuntimeManager.PlayOneShot(selectedEvent, contactPoint);
        }
    }
}
