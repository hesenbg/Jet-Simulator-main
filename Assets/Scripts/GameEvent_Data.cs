using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum Side { Red, BLue, Neutral}

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

    public NetworkedPlayer LocalNetworkedPlayer { get; set; }

    [Header("Data")]
    public Transform LocalPlayer;

    public List<Transform> PlayerInstances;

    [Header("Events")]
    public Action<Transform> OnLocalPlayerJoined;

    public Action OnLocalPlayerLeft;

    public Action<Transform> OnPlayerJoined;

    public Action<Transform> OnPlayerLeft;

    public Action<List<SessionInfo>> OnSessionListUpdated;

    public Volume GetGForceVolume => GForcePostProcces;

    public void SpawnLocalPlayer()
    {
        NetworkSpawner.SpawnPlayer();
    }

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