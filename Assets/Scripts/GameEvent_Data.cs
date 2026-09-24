using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class GameEvent_Data : MonoBehaviour
{
    public static GameEvent_Data Instance;
    private void Awake()
    {
        Instance = this;
    }

    [Header("Components")]
    [SerializeField] Spawner NetworkSpawner;

    [SerializeField] Volume GForcePostProcces;

    public JetData LocalPlayerParams {  get; private set; }

    [Header("Data")]
    public Transform LocalPlayer;

    public List<Transform> PlayerInstances;

    [Header("Events")]
    public Action<Transform> OnLocalPlayerJoined;

    public Action OnLocalPlayerLeft;

    public Action<Transform> OnPlayerJoined;

    public Action<Transform> OnPlayerLeft;

    public Action<List<SessionInfo>> OnSessionListUpdated;


    public bool IsStateAthority(Transform player)
    {
        return player == LocalPlayer;
    }

    public Volume GetGForceVolume => GForcePostProcces;


    public void AddPlayerInstance(Transform player)
    {
        PlayerInstances.Add(player);
    }

    public void AddLocalPlayerInstance(Transform Local)
    {
        LocalPlayer = Local;

        LocalPlayerParams = Local.gameObject.GetComponent<JetData>();
    }

    private void Start()
    {
        NetworkSpawner.LocalPlayerLeft = OnLocalPlayerLeft;

        NetworkSpawner.LocalPlayerJoined = OnLocalPlayerJoined;

        NetworkSpawner.LocalPlayerJoined += AddLocalPlayerInstance;

    }
}