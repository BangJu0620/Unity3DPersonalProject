using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{

    private void Awake()
    {
        PlayerManager.Instance.Player.equip = this;
    }

    public void UnEquip()
    {
        Destroy(gameObject);
    }
}
