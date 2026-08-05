using Fusion;
using UnityEngine;
public class HealthManager : NetworkBehaviour
{
    [Networked]
    float CurrentHealth { get; set; }
    [SerializeField] float MaxHealth;
    [Networked]
    public bool IsDead { get; private set; }
    private bool _wasDead;

    [SerializeField] GameObject visuals;

    public void ResetHealth()
    {
        IsDead = false;

        CurrentHealth = MaxHealth;
    }

    public override void Spawned()
    {
        CurrentHealth = MaxHealth;
        _wasDead = false;
    }
    public void ApplyBallDamage()
    {
        CurrentHealth -= 50f;
    }
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (CurrentHealth <= 0 && !IsDead)
        {
            IsDead = true;
        }
    }
    public override void Render()
    {
        if (IsDead != _wasDead)
        {
            visuals.SetActive(!IsDead);
            _wasDead = IsDead;
        }
    }
}