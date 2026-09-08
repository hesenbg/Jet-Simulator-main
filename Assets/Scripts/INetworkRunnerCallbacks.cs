using Fusion;
using System;
using UnityEngine;

public enum WeaponType {Missle, MachineGun }

public enum JetState { Ground, Air }

[Serializable]
public struct NetworkInputData : INetworkInput
{
    // jet inputs
    public NetworkBool ThrustUp;
    public NetworkBool ThrustDown;

    public NetworkBool YawRight;
    public NetworkBool YawLeft;

    public float Pitch;

    public float Roll;    
}