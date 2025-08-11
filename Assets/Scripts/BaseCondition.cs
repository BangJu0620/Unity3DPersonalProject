using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakePhysicalDamage(int damage);
}

public class BaseCondition : MonoBehaviour
{
    public Condition health;
}
