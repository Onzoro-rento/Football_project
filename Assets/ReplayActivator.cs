using UnityEngine;

public class ReplayActivator : MonoBehaviour
{
    public GameManager gameController;

    private void OnTriggerEnter(Collider other)
    {
        // �v���C���[�̔���i�K�v�ɉ����ďC���j
        Debug.Log($"�g���K�[�ɐڐG�I�����: {other.name}, �^�O��: {other.tag}");
        if (other.CompareTag("Shoe") && gameController != null && GameManager.CurrentState == GameManager.GameState.GameOver)
        {
            gameController.StartReplay();
            gameObject.SetActive(false); // ��x�Đ�������Cube���\���ɂ��� (�C��)
        }
    }
}