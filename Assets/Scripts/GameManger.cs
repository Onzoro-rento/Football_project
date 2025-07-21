// GameManager.cs
// 役割：ゲーム全体の流れ（状態遷移）、ルール、各システム間の連携を管理する司令塔

using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // ゲームの状態を定義
    public enum GameState { Initializing, Ready, Countdown, Active, GameOver, Replaying }
    public static GameState CurrentState { get; private set; }

    [Header("ゲーム設定")]
    [Tooltip("クリアまでに必要なリフティング回数")]
    [SerializeField] private int totalLiftsToClear = 20;
    [Tooltip("物理挙動が適用される間隔（例: 5回に1回）")]
    [SerializeField] private int physicsLiftInterval = 5;
    [Tooltip("ゲーム全体の速度（0.4で40%の速度）")]
    [SerializeField] private float gameSpeed = 0.4f;

    [Header("オブジェクト参照")]
    [SerializeField] private Transform rightControllerAnchor;
    [SerializeField] private GameObject shoeObject;

    [Header("ボール初期位置オフセット")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 0.50f, 0.30f);

    // --- システム参照 ---
    [Header("システム参照")]
    [SerializeField] private BallController ballController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ReplaySystem replaySystem;

    // --- 内部状態 ---
    private int liftCount = 0;
    private Rigidbody shoeRb;

    void Start()
    {
        if (shoeObject != null) shoeRb = shoeObject.GetComponent<Rigidbody>();

        // 各システムが参照されているかチェック
        if (ballController == null || uiManager == null || replaySystem == null)
        {
            Debug.LogError("必要なシステムコンポーネントが設定されていません！");
            return;
        }

        // イベントの購読設定
        ballController.OnKicked += HandleKick;
        ballController.OnHitGround += HandleGameOver;

        Time.timeScale = gameSpeed;
        CurrentState = GameState.Initializing;
    }

    void OnDestroy()
    {
        // 念のためイベント購読を解除
        if (ballController != null)
        {
            ballController.OnKicked -= HandleKick;
            ballController.OnHitGround -= HandleGameOver;
        }
        Time.timeScale = 1.0f;
    }

    void Update()
    {
        // ゲーム開始トリガー
        if (CurrentState == GameState.Initializing)
        {
            if (OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) || OVRInput.IsControllerConnected(OVRInput.Controller.LTouch))
            {
                ResetGame();
            }
            return;
        }

        // リセットボタン
        if ((CurrentState == GameState.GameOver || CurrentState == GameState.Active) && OVRInput.GetDown(OVRInput.Button.One))
        {
            ResetGame();
        }
    }

    private void ResetGame()
    {
        StopAllCoroutines();
        Time.timeScale = gameSpeed;

        CurrentState = GameState.Ready;
        liftCount = 0;

        ballController.SetKickable(true);
        ballController.ResetPosition(rightControllerAnchor, positionOffset, Quaternion.Euler(45, 0, 0));

        uiManager.HideCountdown();
        uiManager.HideReplayActivator();
        uiManager.UpdateStatusText("ボールを蹴ってスタート");

        replaySystem.StopLogging();

        Debug.Log("ゲームをリセットしました。");
    }

    // BallControllerのOnKickedイベントによって呼び出される
    private void HandleKick(Collision collision)
    {
        // 最初のキックはカウントダウンを開始
        if (CurrentState == GameState.Ready)
        {
            StartCoroutine(StartCountdown());
            return;
        }

        if (CurrentState != GameState.Active) return;

        liftCount++;
        uiManager.UpdateStatusText($"Lifting: {liftCount}");

        if (liftCount >= totalLiftsToClear)
        {
            HandleGameClear();
            return;
        }

        // アシストか物理キックかを判定
        if (liftCount % physicsLiftInterval == 0)
        {
            StartCoroutine(ballController.PerformPhysicsKick(shoeRb, gameSpeed));
        }
        else
        {
            ballController.PerformAssistKick();
        }
    }

    private IEnumerator StartCountdown()
    {
        CurrentState = GameState.Countdown;

        int count = 3;
        while (count > 0)
        {
            uiManager.ShowCountdown(count.ToString());
            yield return new WaitForSecondsRealtime(1.0f);
            count--;
        }

        uiManager.ShowCountdown("GO!");
        StartGame();
        yield return new WaitForSecondsRealtime(1.0f);
        uiManager.HideCountdown();
    }

    void StartGame()
    {
        CurrentState = GameState.Active;
        ballController.ActivatePhysics(); // ボールの物理演算を開始
        replaySystem.StartLogging(gameSpeed); // リプレイの記録を開始
        uiManager.UpdateStatusText($"Lifting: {liftCount}");
    }

    // BallControllerのOnHitGroundイベントによって呼び出される
    private void HandleGameOver()
    {
        if (CurrentState != GameState.Active && CurrentState != GameState.Countdown) return;

        CurrentState = GameState.GameOver;
        replaySystem.StopLogging();
        ballController.DeactivatePhysics();

        uiManager.UpdateStatusText($"ゲームオーバー\n回数: {liftCount}\nAボタンでリセット");
        uiManager.ShowReplayActivator(ballController.transform.position);

        Debug.Log("ゲームオーバー！");
    }

    // リプレイ開始のトリガー（ReplayActivatorのOnClickイベントなどから呼ぶ）
    public void StartReplay()
    {
        if (CurrentState != GameState.GameOver) return;
        CurrentState = GameState.Replaying;
        uiManager.UpdateStatusText("リプレイ再生中...");
        StartCoroutine(PlayReplayCoroutine());
    }

    private IEnumerator PlayReplayCoroutine()
    {
        // ReplaySystemの再生コルーチンを開始し、それが終わるまで待機する
        yield return StartCoroutine(replaySystem.Play());

        // 再生が終了したらゲームオーバー状態に戻す
        CurrentState = GameState.GameOver;
        uiManager.UpdateStatusText($"ゲームオーバー\n回数: {liftCount}\nAボタンでリセット");
    }

    private void HandleGameClear()
    {
        CurrentState = GameState.GameOver; // ゲームクリア後も一種の終了状態
        replaySystem.StopLogging();
        ballController.DeactivatePhysics();
        uiManager.UpdateStatusText($"クリア！\n合計 {liftCount} 回");
        Debug.Log("ゲームクリア！");
    }
}