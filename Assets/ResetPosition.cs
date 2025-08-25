using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    // Start is called before the first frame update
    // �y�ݒ荀��1�z���Z�b�g�������{�[���̃I�u�W�F�N�g
    public Transform ballTransform;

    // �y�ݒ荀��2�z��ƂȂ�C�̃I�u�W�F�N�g
    public Transform shoeTransform;

    // �y�ݒ荀��3�z�C����ǂꂭ�炢�O�ɏo�����̋���
    public float forwardOffset = 0.5f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Three))
        {
            // �{�[���ƌC�̃I�u�W�F�N�g���ݒ肳��Ă��邩�O�̂��ߊm�F
            if (ballTransform != null && shoeTransform != null)
            {
                // 1. �C�̌��݈ʒu���擾
                Vector3 shoePos = shoeTransform.position;

                // 2. �C�������Ă���O���̕������擾
                Vector3 shoeForward = shoeTransform.forward;

                // 3. �{�[���̐V�����ʒu���v�Z�i�C�̈ʒu + �C�̑O�� �~ �����j
                Vector3 newBallPosition = shoePos + shoeForward * forwardOffset;

                // 4. �{�[���̈ʒu���X�V
                ballTransform.position = newBallPosition;

                // 5. (���܂�) �{�[���̓��������S�Ɏ~�߂�ƁA����Y��Ƀ��Z�b�g����܂�
                Rigidbody ballRb = ballTransform.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    ballRb.velocity = Vector3.zero;        // �ړ����x�����Z�b�g
                    ballRb.angularVelocity = Vector3.zero; // ��]���x�����Z�b�g
                }
            }
        }
    }
}

