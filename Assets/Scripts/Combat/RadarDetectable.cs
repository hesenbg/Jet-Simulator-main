using Fusion;
using System;
using UnityEngine;

[Serializable]
public class RadarDetectable : MonoBehaviour
{
    [SerializeField] float stealth;

    [SerializeField] float size;

    [SerializeField] LockState currentLockState;

    [SerializeField] float lockProgress = 0;

    public LockState State => currentLockState;

    public float Stealth => stealth;

    public float LockProgress => lockProgress;                                          

    public float Size => size;

    public void SetSteath(float Stealth) => stealth = Stealth;

    public float SetLockProgress(float value) => lockProgress = value;

    public void SetLockState(LockState state) => currentLockState = state;


    private void Start()
    {
        GameEvent_Data.Instance.AddRadarDetectable(this);        
    }
}