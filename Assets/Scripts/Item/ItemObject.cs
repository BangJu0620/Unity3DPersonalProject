using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
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
        return str;
    }

    public void OnInteract()
    {
        PlayerManager.Instance.Player.itemData = data;
        StartCoroutine(UseConsumable());
        
        //PlayerManager.Instance.Player.addItem?.Invoke();
        //Destroy(gameObject);
    }

    IEnumerator UseConsumable()
    {
        SetActiveItemEffect(true);

        yield return new WaitForSeconds(data.duration);

        SetActiveItemEffect(false);
    }

    void SetActiveItemEffect(bool isActive)
    {
        int num;
        if (isActive) num = 1;
        else num = -1;

        for (int i = 0; i < data.consumables.Length; i++)
        {
            switch (data.consumables[i].type)
            {
                case ConsumableType.Speed:
                    PlayerManager.Instance.Player.controller.moveSpeed += data.consumables[i].value * num;
                    break;
                case ConsumableType.JumpPower:
                    PlayerManager.Instance.Player.controller.jumpPower += data.consumables[i].value * num;
                    break;
            }
        }
    }
}
