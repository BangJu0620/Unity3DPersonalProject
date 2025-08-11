using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public LayerMask playerLayerMask;
    public float jumpPower;

    private void OnTriggerEnter(Collider other) // 점프대 발판을 올라가는게 아닌 옆에서 부딪혔을 때도 점프하는 것 방지
    {
        if ((playerLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            PlayerManager.Instance.Player.controller.Jump(jumpPower);
        }
    }
}
