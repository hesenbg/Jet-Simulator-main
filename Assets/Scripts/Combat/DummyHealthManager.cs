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
            GameEvent_Data.Instance.RemoveRadarDetectable(GetComponent<RadarDetectable>());
            Runner.Despawn(Object);
        }
    }
}
