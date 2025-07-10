using UnityEngine;

public class ReplayActivator : MonoBehaviour
{
    public GameController gameController;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーの判定（必要に応じて修正）
        if (other.CompareTag("Shoe") && gameController != null && GameController.CurrentState == GameController.GameState.GameOver)
        {
            gameController.StartReplay();
            gameObject.SetActive(false); // 一度再生したらCubeを非表示にする (任意)
        }
    }
}