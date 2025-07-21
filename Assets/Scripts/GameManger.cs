// GameManager.cs
// �����F�Q�[���S�̗̂���i��ԑJ�ځj�A���[���A�e�V�X�e���Ԃ̘A�g���Ǘ�����i�ߓ�

using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // �Q�[���̏�Ԃ��`
    public enum GameState { Initializing, Ready, Countdown, Active, GameOver, Replaying }
    public static GameState CurrentState { get; private set; }

    [Header("�Q�[���ݒ�")]
    [Tooltip("�N���A�܂łɕK�v�ȃ��t�e�B���O��")]
    [SerializeField] private int totalLiftsToClear = 20;
    [Tooltip("�����������K�p�����Ԋu�i��: 5���1��j")]
    [SerializeField] private int physicsLiftInterval = 5;
    [Tooltip("�Q�[���S�̂̑��x�i0.4��40%�̑��x�j")]
    [SerializeField] private float gameSpeed = 0.4f;

    [Header("�I�u�W�F�N�g�Q��")]
    [SerializeField] private Transform rightControllerAnchor;
    [SerializeField] private GameObject shoeObject;

    [Header("�{�[�������ʒu�I�t�Z�b�g")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 0.50f, 0.30f);

    // --- �V�X�e���Q�� ---
    [Header("�V�X�e���Q��")]
    [SerializeField] private BallController ballController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ReplaySystem replaySystem;

    // --- ������� ---
    private int liftCount = 0;
    private Rigidbody shoeRb;

    void Start()
    {
        if (shoeObject != null) shoeRb = shoeObject.GetComponent<Rigidbody>();

        // �e�V�X�e�����Q�Ƃ���Ă��邩�`�F�b�N
        if (ballController == null || uiManager == null || replaySystem == null)
        {
            Debug.LogError("�K�v�ȃV�X�e���R���|�[�l���g���ݒ肳��Ă��܂���I");
            return;
        }

        // �C�x���g�̍w�ǐݒ�
        ballController.OnKicked += HandleKick;
        ballController.OnHitGround += HandleGameOver;

        Time.timeScale = gameSpeed;
        CurrentState = GameState.Initializing;
    }

    void OnDestroy()
    {
        // �O�̂��߃C�x���g�w�ǂ�����
        if (ballController != null)
        {
            ballController.OnKicked -= HandleKick;
            ballController.OnHitGround -= HandleGameOver;
        }
        Time.timeScale = 1.0f;
    }

    void Update()
    {
        // �Q�[���J�n�g���K�[
        if (CurrentState == GameState.Initializing)
        {
            if (OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) || OVRInput.IsControllerConnected(OVRInput.Controller.LTouch))
            {
                ResetGame();
            }
            return;
        }

        // ���Z�b�g�{�^��
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
        uiManager.UpdateStatusText("�{�[�����R���ăX�^�[�g");

        replaySystem.StopLogging();

        Debug.Log("�Q�[�������Z�b�g���܂����B");
    }

    // BallController��OnKicked�C�x���g�ɂ���ČĂяo�����
    private void HandleKick(Collision collision)
    {
        // �ŏ��̃L�b�N�̓J�E���g�_�E�����J�n
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

        // �A�V�X�g�������L�b�N���𔻒�
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
        ballController.ActivatePhysics(); // �{�[���̕������Z���J�n
        replaySystem.StartLogging(gameSpeed); // ���v���C�̋L�^���J�n
        uiManager.UpdateStatusText($"Lifting: {liftCount}");
    }

    // BallController��OnHitGround�C�x���g�ɂ���ČĂяo�����
    private void HandleGameOver()
    {
        if (CurrentState != GameState.Active && CurrentState != GameState.Countdown) return;

        CurrentState = GameState.GameOver;
        replaySystem.StopLogging();
        ballController.DeactivatePhysics();

        uiManager.UpdateStatusText($"�Q�[���I�[�o�[\n��: {liftCount}\nA�{�^���Ń��Z�b�g");
        uiManager.ShowReplayActivator(ballController.transform.position);

        Debug.Log("�Q�[���I�[�o�[�I");
    }

    // ���v���C�J�n�̃g���K�[�iReplayActivator��OnClick�C�x���g�Ȃǂ���Ăԁj
    public void StartReplay()
    {
        if (CurrentState != GameState.GameOver) return;
        CurrentState = GameState.Replaying;
        uiManager.UpdateStatusText("���v���C�Đ���...");
        StartCoroutine(PlayReplayCoroutine());
    }

    private IEnumerator PlayReplayCoroutine()
    {
        // ReplaySystem�̍Đ��R���[�`�����J�n���A���ꂪ�I���܂őҋ@����
        yield return StartCoroutine(replaySystem.Play());

        // �Đ����I��������Q�[���I�[�o�[��Ԃɖ߂�
        CurrentState = GameState.GameOver;
        uiManager.UpdateStatusText($"�Q�[���I�[�o�[\n��: {liftCount}\nA�{�^���Ń��Z�b�g");
    }

    private void HandleGameClear()
    {
        CurrentState = GameState.GameOver; // �Q�[���N���A������̏I�����
        replaySystem.StopLogging();
        ballController.DeactivatePhysics();
        uiManager.UpdateStatusText($"�N���A�I\n���v {liftCount} ��");
        Debug.Log("�Q�[���N���A�I");
    }
}