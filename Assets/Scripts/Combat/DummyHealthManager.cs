using Fusion;
using UnityEngine;

public class DummyHealthManager : HealthManager
{
    public override void OnDamage(float Damage, Vector3 pos)
    {
        
    }

    public override void OnDeath()
    {
        if (Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        GameEvent_Data.Instance.RemoveRadarDetectable(GetComponent<RadarDetectable>());
    }
}