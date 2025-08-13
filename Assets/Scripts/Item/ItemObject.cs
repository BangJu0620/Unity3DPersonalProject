using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IInteractable  // ������ �ִ� ��ȣ�ۿ� ��ü�� �����ϸ� �ɵ�
{
    public string GetInteractPrompt();
    public void OnInteract();
}


public class ItemObject : MonoBehaviour, IInteractable
{
    public ItemData data;

    public string GetInteractPrompt()   // UI�� �������
    {
        string str = $"{data.displayName}\n{data.description}";
        if(data.type == ItemType.Consumable)
        {
            str += "\n[E] Ű�� ���� ����ϱ�";
        }
        else if(data.type == ItemType.Equipable)
        {
            str += "\n[E] Ű�� ���� �����ϱ�";
        }
        return str;
    }

    public void OnInteract()    // ��ȣ�ۿ�
    {
        //PlayerManager.Instance.Player.itemData = data;
        if(data.type == ItemType.Consumable)
        {
            StartCoroutine(UseConsumable());
        }
        else if(data.type == ItemType.Equipable)
        {
            Debug.Log("���� �õ�");
            // �����ϱ�
            Equip();
        }
        
        // �κ��丮�� ������ �߰��ϴ� �뵵
        //PlayerManager.Instance.Player.addItem?.Invoke();
        //Destroy(gameObject);
    }

    void Equip()
    {
        if(PlayerManager.Instance.Player.equipData != null)
        {
            Debug.Log("���� null Ȯ��");
            // ���� ������ ������
            DropItem(PlayerManager.Instance.Player.equipData);
        }

        // ������ �����ϱ�
        EquipItem();
    }

    void EquipItem()    // �� ��� ����
    {
        Debug.Log("EquipItem");
        //SetActiveItemEffect(true, data.type);
        SetActiveEquipEffect(true, data);
        PlayerManager.Instance.Player.equipData = data;
        Instantiate(data.equipPrefab, PlayerManager.Instance.Player.equipPosition);
        Destroy(gameObject);
    }

    void DropItem(ItemData itemData)    // ���� ��� ����Ʈ��
    {
        Debug.Log("DropItem");
        //SetActiveItemEffect(false, itemData.type);
        SetActiveEquipEffect(false, itemData);
        Instantiate(itemData.dropPrefab, PlayerManager.Instance.Player.dropPosition.position, Quaternion.identity);
        //PlayerManager.Instance.Player.equipData = null;
        PlayerManager.Instance.Player.equip.UnEquip();
    }

    IEnumerator UseConsumable()
    {
        SetActiveItemEffect(true);

        yield return new WaitForSeconds(data.duration);

        SetActiveItemEffect(false);
    }

    void SetActiveItemEffect(bool isActive)  // ������ ȿ�� Ȱ��ȭ
    {
        int num;
        if (isActive) num = 1;
        else num = -1;
        Debug.Log($"num: {num}");

        if(data.type == ItemType.Consumable)
        {
            for (int i = 0; i < data.consumables.Length; i++)
            {
                switch (data.consumables[i].type)
                {
                    case EffectType.Speed:
                        PlayerManager.Instance.Player.controller.moveSpeed += data.consumables[i].value * num;
                        break;
                    case EffectType.JumpPower:
                        PlayerManager.Instance.Player.controller.jumpPower += data.consumables[i].value * num;
                        break;
                    case EffectType.DoubleJump:
                        PlayerManager.Instance.Player.controller.isDoubleJump = isActive;
                        break;
                    case EffectType.Invincibility:
                        PlayerManager.Instance.Player.condition.isInvincibility = isActive;
                        break;
                }
            }
        }
    }

    void SetActiveEquipEffect(bool isActive, ItemData itemData)
    {
        int num;
        if (isActive) num = 1;
        else num = -1;

        if (itemData.type == ItemType.Equipable)
        {
            for (int i = 0; i < itemData.equipables.Length; i++)
            {
                switch (itemData.equipables[i].type)
                {
                    case EffectType.Speed:
                        Debug.Log($"Speed, num: {num}");
                        PlayerManager.Instance.Player.controller.moveSpeed += itemData.equipables[i].value * num;
                        break;
                    case EffectType.JumpPower:
                        Debug.Log($"Jump, num: {num}");
                        PlayerManager.Instance.Player.controller.jumpPower += itemData.equipables[i].value * num;
                        break;
                }
            }
        }
    }
}
