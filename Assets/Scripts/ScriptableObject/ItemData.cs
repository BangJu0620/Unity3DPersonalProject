using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Equipable,  // ��������
    Consumable, // �Һ񰡴�
}

public enum EffectType  // ������� �� �� �÷��ٰ���
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
    public EffectType type; // � �� �÷��ִ���
    public float value;         // �󸶳� �÷��ִ���
}

// ScriptableObject�� ���� �� ������ ����� ���� �޴�â���ٰ� �߰�
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
    public float duration;      // ���ӽð�

    [Header("Equip")]
    public GameObject equipPrefab;
    public ItemDataEffect[] equipables;
}
