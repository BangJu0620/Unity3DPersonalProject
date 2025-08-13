using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Equipable,  // 장착가능
    Consumable, // 소비가능
}

public enum EffectType  // 사용했을 때 뭘 늘려줄건지
{
    Health,
    Speed,
    JumpPower,
    DoubleJump,
    Invincibility
}

[Serializable]
public class ItemDataEffect
{
    public EffectType type; // 어떤 걸 늘려주는지
    public float value;         // 얼마나 늘려주는지
}

// ScriptableObject를 만들 때 빠르기 만들기 위해 메뉴창에다가 추가
[CreateAssetMenu(fileName = "Item", menuName = "New Item")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string displayName;
    public string description;
    public ItemType type;
    public Sprite icon;
    public GameObject dropPrefab;

    [Header("Consumable")]
    public ItemDataEffect[] consumables;
    public float duration;      // 지속시간

    [Header("Equip")]
    public GameObject equipPrefab;
    public ItemDataEffect[] equipables;
}
