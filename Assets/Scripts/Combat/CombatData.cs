using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "JetCombatData")]
public class CombatData : ScriptableObject
{
    [SerializeField] float lockSpeed;

    [SerializeField] int maxMissleCount;

    [SerializeField] float reloadSpeed;

    [SerializeField] float maxLockDistance;

    [SerializeField] float radarDetectionSize;

    [SerializeField, FormerlySerializedAs("FlareCooldown")] float flareCooldown;

    [SerializeField, FormerlySerializedAs("TimeBetweenFlareBursts")] float timeBetweenFlareBursts;

    [SerializeField, FormerlySerializedAs("NumberOfBurst")] int numberOfBurst;

    public float LockSpeed => lockSpeed;

    public int MaxMissleCount => maxMissleCount;

    public float ReloadSpeed => reloadSpeed;

    public float RadarDetectionSize => radarDetectionSize;

    public float MaxLockDistance => maxLockDistance;

    public float FlareDuration => timeBetweenFlareBursts * numberOfBurst;

    public float FlareCooldown => flareCooldown;

    public void SetFlareCooldown(float cooldown) => flareCooldown = cooldown;

    public float TimeBetweenFlareBursts => timeBetweenFlareBursts;

    public int NumberOfBurst => numberOfBurst;
}