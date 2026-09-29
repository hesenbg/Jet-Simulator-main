using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum Side { Red, Blue, Neutral}

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

    public  GamePhase CurrentPhase;

    [Header("Events")]
    public Action<Transform> OnLocalPlayerJoined;

    public Action OnLocalPlayerLeft;

    public Action<Transform> OnPlayerJoined;

    public Action<Transform> OnPlayerLeft;

    public Action<List<SessionInfo>> OnSessionListUpdated;

    public Volume GetGForceVolume => GForcePostProcces;


    public void AddPlayerInstance(Transform player)
    {
        PlayerInstances.Add(player);
    }


    public NetworkedPlayer GetLocalNetworkedPlayer()
    {
        LocalPlayer.gameObject.TryGetComponent<NetworkedPlayer>(out NetworkedPlayer player);

        return player;
    }

    public bool CheckReadyAll()
    {
        foreach(Transform player in PlayerInstances)
        {
            if(player.gameObject.TryGetComponent<NetworkedPlayer>(out NetworkedPlayer Networked))
            {
                if(!Networked.IsReady)
                    return false;
            }
        }
        return true;
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