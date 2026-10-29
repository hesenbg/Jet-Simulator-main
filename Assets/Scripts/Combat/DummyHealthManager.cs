using UnityEngine;

public class DummyHealthManager : HealthManager
{
    public override void OnDamage(float Damage, Vector3 pos)
    {
        
    }

    public override void OnDeath()
    {
        
        GameEvent_Data.Instance.RemoveRadarDetectable(GetComponent<RadarDetectable>());

        if (Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}
