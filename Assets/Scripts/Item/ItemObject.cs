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
        //PlayerManager.Instance.Player.addItem?.Invoke();
        Destroy(gameObject);
    }
}
