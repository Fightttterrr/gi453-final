using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Settings")]
    public int damage;
    public float cooldownTime;
    public float attackDuration = 0.2f;
    protected float nextAttackTime = 0f;

    public virtual void Attack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }
        nextAttackTime = Time.time + cooldownTime;


        PerformAttack();
    }


    protected virtual void PerformAttack()
    {
        Debug.Log("Weapon Action!");
    }

    public virtual void Turn(bool isRight) { }
}