using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable    // 추후에 데미지를 입는 NPC나 몬스터에 상속해주면 될듯
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
    //public event Action onDrainStamina; // 지속적으로 사용하는 스태미나, 이벤트를 업데이트에 넣고 써도 되나?

    // 무적 테스트용
    //private void Start()
    //{
    //    InvokeRepeating("Damage", 0, 1);
    //}

    void Update()
    {
        if (!PlayerManager.Instance.Player.controller.isDashing && !PlayerManager.Instance.Player.controller.isHanging) // 달리기나 벽 타기 중이 아니라면
        {
            stamina.Add(stamina.passiveValue * Time.deltaTime); // 스태미나 회복
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

    public bool UseStamina(float amount)    // 단발성 스태미나 소모, 점프나 공격
    {
        if (stamina.curValue - amount < 0f)
        {
            return false;
        }

        stamina.Subtract(amount);
        return true;
    }

    public bool DrainStamina(float amount)  // 지속성 스태미나 소모, 달리기나 벽 타기
    {
        if(stamina.curValue - amount * Time.deltaTime < 0f)
        {
            return false;
        }

        stamina.Subtract(amount * Time.deltaTime);
        return true;
    }
}
