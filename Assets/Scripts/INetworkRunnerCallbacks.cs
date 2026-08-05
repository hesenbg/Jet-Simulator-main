using Fusion;
using UnityEngine;

public enum WeaponType {Missle, MachineGun }

public struct NetworkInputData : INetworkInput
{
    public float Thrust;
    public float Yaw;
    public float Roll;
    public float Pitch;

    NetworkBool IsFiring;

    WeaponType Weapon;
}