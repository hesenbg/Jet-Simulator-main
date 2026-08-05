using System;
using UnityEngine;
using UnityEngine.InputSystem;


public enum JetState { Ground, Air }

[Serializable]
public struct JetInput
{
    public float ThrustValue;
    public float Yaw;
    public float Roll;
    public float Pitch;
}
