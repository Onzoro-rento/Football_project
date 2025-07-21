using UnityEngine;

public class ReplayActivator : MonoBehaviour
{
    public GameManager gameController;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーの判定（必要に応じて修正）
        Debug.Log($"トリガーに接触！相手は: {other.name}, タグは: {other.tag}");
        if (other.CompareTag("Shoe") && gameController != null && GameManager.CurrentState == GameManager.GameState.GameOver)
        {
            gameController.StartReplay();
            gameObject.SetActive(false); // 一度再生したらCubeを非表示にする (任意)
        }
    }
}