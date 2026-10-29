using Fusion;
using System;
using UnityEngine;

public abstract class HealthManager : NetworkBehaviour
{
    [SerializeField] float MaxHealth;

    public float CurrentHealth { get; private set; }

    protected float BaseDamageMultipiler = 1f;

    protected Action OnHealthZero;

    protected Action<float, Vector3> OnDamageDone;

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ApplyDamage(float damage, float Multipiler, Vector3 pos)
    {
        var totalDamage = damage * Multipiler * BaseDamageMultipiler;

        CurrentHealth -= totalDamage;
        OnDamageDone?.Invoke(totalDamage, pos);

        CheckDie();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ApplyLethalDamage()
    {
        CurrentHealth = 0;
        CheckDie();
    }

    bool CheckDie()
    {
        if (CurrentHealth <= 0f)
        {
            OnHealthZero?.Invoke();
            OnHealthZero = null;
            return true;
        }
        return false;
    }

    public void IncreaseHealth(float Health)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + Health, 0f, MaxHealth);
    }

    abstract public void OnDeath();

    abstract public void OnDamage(float Damage, Vector3 pos);


    public override void Spawned()
    {
        CurrentHealth = MaxHealth;
        OnHealthZero += OnDeath;
        OnDamageDone += OnDamage;
    }
}   