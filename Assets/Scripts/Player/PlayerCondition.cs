using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakePhysicalDamage(int damage);
}

public class PlayerCondition : MonoBehaviour, IDamageable
{
    public UICondition uiCondition;

    public Condition health;
    public Condition stamina;

    public bool isInvincibility = false;

    public event Action onTakeDamage;
    public event Action onDrainStamina; // 지속적으로 사용하는 스태미나

    // 무적 테스트용
    //private void Start()
    //{
    //    InvokeRepeating("Damage", 0, 1);
    //}

    void Update()
    {
        if (!PlayerManager.Instance.Player.controller.isDashing)
        {
            stamina.Add(stamina.passiveValue * Time.deltaTime);
        }

        //onDrainStamina?.Invoke();

        if (health.curValue <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        health.Add(amount);
    }

    public void Die()
    {
        Debug.Log("죽었다!");
    }

    // 무적 테스트용
    //void Damage()   
    //{
    //    TakePhysicalDamage(10);
    //}

    public void TakePhysicalDamage(int damage)
    {
        if (isInvincibility)
        {
            return;
        }
        health.Subtract(damage);
        onTakeDamage?.Invoke();
    }

    public bool UseStamina(float amount)    // 후에 생길 벽타기에 스태미나 달게 하기
    {
        if (stamina.curValue - amount < 0f)
        {
            return false;
        }

        stamina.Subtract(amount);
        return true;
    }

    public bool DrainStamina(float amount)
    {
        if(stamina.curValue - amount * Time.deltaTime < 0f)
        {
            return false;
        }

        stamina.Subtract(amount * Time.deltaTime);
        return true;
    }
}
