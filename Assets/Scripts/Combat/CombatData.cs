using UnityEngine;
[CreateAssetMenu(fileName = "JetCombatData")]
public class CombatData : ScriptableObject
{
    [SerializeField] float lockSpeed;

    [SerializeField] int maxMissleCount;

    [SerializeField] float reloadSpeed;

    [SerializeField] float maxLockDistance;

    [SerializeField] float radarDetectionSize;

    public float LockSpeed => lockSpeed;

    public int MaxMissleCount => maxMissleCount;

    public float ReloadSpeed => reloadSpeed;

    public float RadarDetectionSize => radarDetectionSize;

    public float MaxLockDistance => maxLockDistance;
}
