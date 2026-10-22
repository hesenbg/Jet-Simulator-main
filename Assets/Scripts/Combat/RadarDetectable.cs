using Fusion;
using System;
using UnityEngine;

[RequireComponent (typeof(HealthManager))]
[RequireComponent (typeof(NetworkObject))]
[RequireComponent (typeof(HealthManager))]

[Serializable]
public class RadarDetectable : MonoBehaviour
{
    [SerializeField] float stealth;

    [SerializeField] float size;

    [SerializeField] LockState currentLockState;

    HealthManager healthManager;

    public LockState State => currentLockState;

    public float Stealth => stealth;

    public float Size => size;

    public void SetLockState(LockState state) => currentLockState = state;

    private void Awake()
    {
        healthManager = GetComponent<HealthManager>();
    }

    private void Start()
    {
        GameEvent_Data.Instance.AddRadarDetectable(this);        
    }

    private void Update()
    {
    } 
}