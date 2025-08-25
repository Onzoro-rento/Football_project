// GameManager.cs

using UnityEngine;

using System.Collections;

using System;
using UnityEngine.SceneManagement; // ★ 追加: シーン遷移に必要



public class GameManager : MonoBehaviour

{

    public enum GameState { Initializing, Ready, Countdown, Active, GameOver, Replaying }

    public static GameState CurrentState { get; private set; }



    [Header("ゲーム設定")]

    [Tooltip("クリアまでに必要なリフティング回数")]

    [SerializeField] private int totalLiftsToClear = 20;



    [Tooltip("落下フェーズのタイムスケール（例: 0.5 で50%の速度）")]

    [SerializeField] private float fallTimeScale = 0.5f;



    // ★削除: 手動物理演算が不要になったため、以下の変数を削除

    // [SerializeField] private float maxKickHeight = 1.2f;

    // [SerializeField] private float riseSpeedFactor = 0.4f;



    [Tooltip("ゲーム開始時の打ち上げの高さ(m)")]

    [SerializeField] private float initialLaunchHeight = 0.8f;

    [SerializeField] private Vector3 ReplayActivatorOffset = new Vector3(0f, 0.1f, 0.5f);

    [Tooltip("アシストキックでボールが到達する高さ(m)")]
    [SerializeField] private float assistedKickHeight = 0.8f;

    [Header("ボール初期位置")]

    [Tooltip("ボールの初期位置（ワールド座標）")]

    [SerializeField] private Vector3 initialBallPosition = new Vector3(0f, 0.7f, 0.3f);

    [Tooltip("ボールの初期回転")]

    [SerializeField] private Quaternion initialBallRotation = Quaternion.Euler(45, 0, 0);
    [Header("フィードバック色設定")]
    [Tooltip("次のキックがアシストモードであることを示す色")]
    [SerializeField] public Color assistKickColor = Color.cyan;
    [Tooltip("次のキックが通常モードであることを示す色")]
    [SerializeField] public Color normalKickColor = Color.yellow;


    [Header("システム参照")]

    [SerializeField] private BallController ballController;

    [SerializeField] private UIManager uiManager;

    [SerializeField] private ReplaySystem replaySystem;

    [SerializeField] private ShoeController shoeController;

    private Vector3 initialGravity;





    private int liftCount = 0;



    void Start()

    {

        if (ballController == null) { Debug.LogError("BallControllerが設定されていません！"); return; }

        ballController.OnKicked += HandleKick;

        ballController.OnHitGround += HandleGameOver;

        CurrentState = GameState.Initializing;
        fallTimeScale = PlayerPrefs.GetFloat("SpeedMultiplier", 1.0f);
        // ★ゲーム開始時に本来の重力を記憶
        Debug.Log($"ゲーム開始時の重力: {Physics.gravity}");

        initialGravity = Physics.gravity;

    }



    void OnDestroy()

    {

        if (ballController != null)

        {

            ballController.OnKicked -= HandleKick;

            ballController.OnHitGround -= HandleGameOver;

        }

    }



    void Update()

    {

        if (CurrentState == GameState.Initializing)

        {

            if (Input.anyKeyDown || (OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) || OVRInput.IsControllerConnected(OVRInput.Controller.LTouch)))

            {

                ResetGame();

            }

            return;

        }



        if ((CurrentState == GameState.GameOver || CurrentState == GameState.Active) && (Input.GetKeyDown(KeyCode.R) || OVRInput.GetDown(OVRInput.Button.One)))

        {

            ResetGame();

        }

    }



    public void ResetGame()

    {

        Time.timeScale = 1.0f;

        StopAllCoroutines();

        CurrentState = GameState.Ready;

        liftCount = 0;

        Physics.gravity = initialGravity;
        Debug.Log($"ゲーム開始時の重力: {fallTimeScale}");


        ballController.ResetPosition(initialBallPosition, initialBallRotation);

        ballController.SetReadyToKick();

        if (ballController != null)
        {
            ballController.SetBaseColor(assistKickColor);
        }


        if (uiManager != null)

        {

            uiManager.HideCountdown();

            // ★ 追加: リセット時に必ずパネルを非表示にする
            uiManager.HideGameClearPanel();
            uiManager.HideGameOverPanel();

            uiManager.UpdateStatusText("ボールを蹴ってスタート");

        }

        if (replaySystem != null) replaySystem.StopLogging();

        

    }



    // ★修正: キック処理をゲーム状態で分岐させる

    private void HandleKick(Collision collision)

    {

        if (CurrentState == GameState.Ready)

        {

            // 初回キックの処理

            CurrentState = GameState.Countdown;

            StartCoroutine(InitialKickSequence());

        }

        else if (CurrentState == GameState.Active)

        {

            // 2回目以降のリフティング処理

            // 物理演算には介入せず、回数を数えるだけ

            

            liftCount++;
            bool isAssistKick = (liftCount % 5 != 0);
            if (isAssistKick)
            {
                // アシストキック: ボールを一定の高さまで上げる
                Debug.Log($"Assist Kick! Count: {liftCount}");
                ballController.LaunchAssisted(assistedKickHeight, fallTimeScale);
            }
            else
            {
                // 通常キック: 実際のキックの物理演算を反映
                Debug.Log($"Normal Kick! Count: {liftCount}");
                ballController.LaunchWithManualControl(fallTimeScale);
            }


            int nextLiftCount = liftCount + 1;

            if (nextLiftCount > totalLiftsToClear)
            {
                // ゲームクリア後のため、デフォルトの色に戻す（任意）
                ballController.SetBaseColor(Color.white);
            }
            else
            {
                bool isNextKickAssist = (nextLiftCount % 5 != 0);
                if (isNextKickAssist)
                {
                    ballController.SetBaseColor(assistKickColor);
                }
                else
                {
                    ballController.SetBaseColor(normalKickColor);
                }
            }

            if (uiManager != null) uiManager.UpdateStatusText($"Lifting: {liftCount}");



            if (liftCount >= totalLiftsToClear)

            {

                HandleGameClear();

            }

        }

    }



    private IEnumerator InitialKickSequence()

    {

        // カウントダウン中はボールを物理的に固定

        ballController.GetComponent<Rigidbody>().isKinematic = true;



        int count = 3;

        while (count > 0)

        {

            if (uiManager != null) uiManager.ShowCountdown(count.ToString());

            yield return new WaitForSecondsRealtime(1.0f);

            count--;

        }

        if (uiManager != null) uiManager.ShowCountdown("GO!");

        yield return new WaitForSecondsRealtime(0.5f);

        if (uiManager != null) uiManager.HideCountdown();



        CurrentState = GameState.Active;



        // ★修正: Unityの物理法則に基づいた初速を計算

        // v = sqrt(2 * g * h)

        //Time.timeScale = fallTimeScale;

        //Physics.gravity = initialGravity / fallTimeScale;

        float requiredVelocityY = Mathf.Sqrt(2f * Mathf.Abs(initialGravity.y) * initialLaunchHeight);

        Vector3 initialLaunchVelocity = new Vector3(0, requiredVelocityY, 0);



        // ★修正: BallControllerの新しいLaunchメソッドを呼び出す

        ballController.Launch(initialLaunchVelocity, fallTimeScale);



        if (replaySystem != null) replaySystem.StartLogging();

        if (uiManager != null) uiManager.UpdateStatusText($"Lifting: {liftCount}");

    }



    // ★削除: LiftingKickSequenceは不要になったため、まるごと削除

    // private IEnumerator LiftingKickSequence() { ... }



    private void HandleGameOver()

    {

        if (CurrentState != GameState.Active && CurrentState != GameState.Countdown) return;

        Time.timeScale = 1.0f;

        Physics.gravity = initialGravity;

        CurrentState = GameState.GameOver;

        if (replaySystem != null) replaySystem.StopLogging();

        ballController.DeactivatePhysics();

        if (uiManager != null)

        {
            uiManager.UpdateResultScore(liftCount);
            uiManager.ShowGameOverPanel();

        }

        Debug.Log("ゲームオーバー！");

    }



    public void StartReplay()

    {

        if (CurrentState != GameState.GameOver || replaySystem == null) return;

        CurrentState = GameState.Replaying;

        if (uiManager != null) uiManager.UpdateStatusText("リプレイ再生中...");

        StartCoroutine(PlayReplayCoroutine(0.5f));

    }



    private IEnumerator PlayReplayCoroutine(float speed)

    {

        if (replaySystem != null) yield return StartCoroutine(replaySystem.Play(speed));

        CurrentState = GameState.GameOver;

        if (uiManager != null)
        {
            uiManager.UpdateStatusText(""); // テキストをリセット
            uiManager.ShowGameOverPanel(); // ★ 追加: リプレイ終了後、再度パネルを表示
        }

    }



    private void HandleGameClear()

    {

        Time.timeScale = 1.0f;

        CurrentState = GameState.GameOver;
    
        if (replaySystem != null) replaySystem.StopLogging();

        ballController.DeactivatePhysics();

        if (uiManager != null)
        {
            uiManager.UpdateResultScore(liftCount);
            uiManager.ShowGameClearPanel();

            
        }

    }
    public void RetryGame()
    {
        ResetGame();
        
    }

    /// <summary>
    /// ホームへ戻るボタンから呼び出されるメソッド
    /// </summary>
    public void GoToHomeScene()
    {
        Time.timeScale = 1.0f; // タイムスケールを必ず元に戻す
        SceneManager.LoadScene("Opening");
    }

}