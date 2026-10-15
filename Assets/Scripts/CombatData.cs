using UnityEngine;

[CreateAssetMenu(fileName = "JetCombatData")]

public class CombatData : ScriptableObject
{
    public float Stealth;

    public float LockSpeed;

    public int MaxMissleCount;

    public float ReloadSpeed;

    public float Size; // both x and y so we can detect in radar

    public float MaxLockDistance;

    public float LockDetectThreshold; // x and y distance of radar lock to start see enemy jets
}
