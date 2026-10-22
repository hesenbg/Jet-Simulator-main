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

    public RadarDetectable LockedJet;

    public JetData JetData {  get;  set; }

    [Header("Data")]

    public List<Transform> PlayerInstances;

    public List<RadarDetectable> radarDetectables;

    public  GamePhase CurrentPhase;

    [Header("Events")]
    public Action<Transform> OnLocalPlayerJoined;

    public Action OnLocalPlayerLeft;

    public Action<Transform,bool> OnPlayerJoined;

    public Action<Transform,bool> OnPlayerLeft;

    public Action<List<SessionInfo>> OnSessionListUpdated;

    public Volume GetGForceVolume => GForcePostProcces;

    public void AddRadarDetectable(RadarDetectable radarDetectable)
    {
        radarDetectables.Add(radarDetectable);
    }

    public void AddPlayerInstance(Transform player)
    {
        PlayerInstances.Add(player);
    }

    public Transform GetLocalTransform()
    {
        return NetworkSpawner.GetLocalTransform();
    }

    public NetworkedPlayer GetLocalNetworkedPlayer()
    {
        GetLocalTransform().gameObject.TryGetComponent<NetworkedPlayer>(out NetworkedPlayer player);

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

    private void Start()
    {
        NetworkSpawner.LocalPlayerJoined += local => OnLocalPlayerJoined?.Invoke(local);
        NetworkSpawner.LocalPlayerLeft += () => OnLocalPlayerLeft?.Invoke();
    }
}