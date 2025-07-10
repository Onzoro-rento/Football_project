using UnityEngine;

public class ReplayActivator : MonoBehaviour
{
    public GameController gameController;

    private void OnTriggerEnter(Collider other)
    {
        // �v���C���[�̔���i�K�v�ɉ����ďC���j
        if (other.CompareTag("Shoe") && gameController != null && GameController.CurrentState == GameController.GameState.GameOver)
        {
            gameController.StartReplay();
            gameObject.SetActive(false); // ��x�Đ�������Cube���\���ɂ��� (�C��)
        }
    }
}