//using UnityEngine;



//public class LiftingPhysicsController : MonoBehaviour

//{

//    // --- 調整用パラメータ ---

//    [Header("速度倍率 (f)")]

//    [Range(0.1f, 1.0f)]

//    public float speedScale = 0.4f; // 論文で言うf倍速



//    // --- 内部状態変数 ---

//    private Rigidbody rb;

//    private bool isFlyingManual = false; // 手動での飛行中かどうかのフラグ



    

//    private Vector3 p0; // 初期位置 (Initial Position)

//    private Vector3 v0; // 初期速度 (Initial Velocity)

//    private float flightTime; // 飛行時間 (t)



//    void Start()

//    {

//        rb = GetComponent<Rigidbody>();

//    }



//    void OnCollisionEnter(Collision collision)

//    {

//        // 足に蹴られた時、かつ手動飛行中でない時に計算を開始

//        if (collision.gameObject.CompareTag("Shoe") && !isFlyingManual)

//        {

//            // 1. キックの瞬間の状態を記録

//            p0 = transform.position; // 初期位置 P₀

//            v0 = collision.relativeVelocity; // キックによる初期速度 V₀



//            // 2. 飛行状態に移行

//            flightTime = 0f; // 飛行時間をリセット

//            isFlyingManual = true; // 手動飛行フラグをON

//            rb.isKinematic = true; // Unityの物理エンジンを無効化

//        }

//    }



//    void Update()

//    {

//        // 手動飛行中の場合のみ、位置を計算して更新

//        if (isFlyingManual)

//        {

//            // 3. 速度倍率を考慮して飛行時間を進める

//            flightTime += Time.deltaTime * speedScale;



           

//            float g = Physics.gravity.y; // Unityの重力加速度 (-9.8 m/s^2)

//            float t = flightTime;



//            float x = p0.x + v0.x * t;

//            float y = p0.y + v0.y * t + 0.5f * g * t * t;

//            float z = p0.z + v0.z * t;



//            transform.position = new Vector3(x, y, z);



           
//        }

//    }
//    public void CancelManualFlight()
//    {
//        isFlyingManual = false;
//        rb.isKinematic = false; // 物理エンジンを再度有効化
//    }

//}