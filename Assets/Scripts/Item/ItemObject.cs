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

    public string GetInteractPrompt()
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

    public void OnInteract()
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
        
        //PlayerManager.Instance.Player.addItem?.Invoke();
        //Destroy(gameObject);
    }

    void Equip()
    {
        if(PlayerManager.Instance.Player.equipData != null)
        {
            Debug.Log("장착 null인가요");
            // 장착 아이템 버리기
            DropItem(PlayerManager.Instance.Player.equipData);
        }

        // 아이템 장착하기
        EquipItem();
    }

    void EquipItem()
    {
        Debug.Log("EquipItem");
        PlayerManager.Instance.Player.equipData = data;
        PlayerManager.Instance.Player.equip = data.equipPrefab.GetComponent<Equipment>();
        Instantiate(data.equipPrefab, PlayerManager.Instance.Player.equipPosition);
        Destroy(gameObject);
        SetActiveItemEffect(true, ItemType.Equipable);
    }

    void DropItem(ItemData itemData)
    {
        Debug.Log("DropItem");
        Instantiate(itemData.dropPrefab, PlayerManager.Instance.Player.dropPosition.position, Quaternion.identity);
        PlayerManager.Instance.Player.equipData = null;
        PlayerManager.Instance.Player.equip.UnEquip();
        SetActiveItemEffect(false, ItemType.Equipable);
    }

    IEnumerator UseConsumable()
    {
        SetActiveItemEffect(true, ItemType.Consumable);

        yield return new WaitForSeconds(data.duration);

        SetActiveItemEffect(false, ItemType.Consumable);
    }

    void SetActiveItemEffect(bool isActive, ItemType itemType)
    {
        int num;
        if (isActive) num = 1;
        else num = -1;

        if(itemType == ItemType.Consumable)
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
        else if(itemType == ItemType.Equipable)
        {
            for (int i = 0; i < data.equipables.Length; i++)
            {
                switch (data.equipables[i].type)
                {
                    case EffectType.Speed:
                        PlayerManager.Instance.Player.controller.moveSpeed += data.equipables[i].value * num;
                        break;
                    case EffectType.JumpPower:
                        PlayerManager.Instance.Player.controller.jumpPower += data.equipables[i].value * num;
                        break;
                }
            }
        }
    }
}
