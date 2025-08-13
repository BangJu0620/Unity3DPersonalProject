using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IInteractable  // 도전에 있는 상호작용 물체에 적용하면 될듯
{
    public string GetInteractPrompt();
    public void OnInteract();
}


public class ItemObject : MonoBehaviour, IInteractable
{
    public ItemData data;

    public string GetInteractPrompt()   // UI에 설명띄우기
    {
        string str = $"{data.displayName}\n{data.description}";
        if(data.type == ItemType.Consumable)
        {
            str += "\n[E] 키를 눌러 사용하기";
        }
        else if(data.type == ItemType.Equipable)
        {
            str += "\n[E] 키를 눌러 장착하기";
        }
        return str;
    }

    public void OnInteract()    // 상호작용
    {
        //PlayerManager.Instance.Player.itemData = data;
        if(data.type == ItemType.Consumable)
        {
            StartCoroutine(UseConsumable());
        }
        else if(data.type == ItemType.Equipable)
        {
            Debug.Log("장착 시도");
            // 장착하기
            Equip();
        }
        
        // 인벤토리에 아이템 추가하는 용도
        //PlayerManager.Instance.Player.addItem?.Invoke();
        //Destroy(gameObject);
    }

    void Equip()
    {
        if(PlayerManager.Instance.Player.equipData != null)
        {
            Debug.Log("장착 null 확인");
            // 장착 아이템 버리기
            DropItem(PlayerManager.Instance.Player.equipData);
        }

        // 아이템 장착하기
        EquipItem();
    }

    void EquipItem()    // 새 장비를 장착
    {
        Debug.Log("EquipItem");
        //SetActiveItemEffect(true, data.type);
        SetActiveEquipEffect(true, data);
        PlayerManager.Instance.Player.equipData = data;
        Instantiate(data.equipPrefab, PlayerManager.Instance.Player.equipPosition);
        Destroy(gameObject);
    }

    void DropItem(ItemData itemData)    // 기존 장비를 떨어트림
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

    void SetActiveItemEffect(bool isActive)  // 아이템 효과 활성화
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
