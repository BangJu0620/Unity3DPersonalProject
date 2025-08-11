using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public LayerMask playerLayerMask;
    public float jumpPower;

    private void OnTriggerEnter(Collider other) // ������ ������ �ö󰡴°� �ƴ� ������ �ε����� ���� �����ϴ� �� ����
    {
        if ((playerLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            PlayerManager.Instance.Player.controller.Jump(jumpPower);
        }
    }
}
