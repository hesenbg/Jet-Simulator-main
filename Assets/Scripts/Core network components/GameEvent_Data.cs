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

    [SerializeField] private RadarDetectable LockedJet;

    public GameManager Manager {  get; private set; }

    public JetData JetData {  get;  set; }

    [SerializeField] JetParameters Params;

    [Header("Data")]

    public List<Transform> PlayerInstances;

    public List<RadarDetectable> radarDetectables;

    public  GamePhase CurrentPhase;

    [Header("Events")]
    public Action<Transform> OnLocalPlayerJoined;

    public Action OnLocalPlayerLeft;

    public Action<Transform,bool> OnPlayerSpawned;

    public Action<Transform,bool> OnPlayerDespawn;

    public Action<List<SessionInfo>> OnSessionListUpdated;

    public Action<Transform> OnPlayerRespawned;

    public Volume GetGForceVolume => GForcePostProcces;

    public void AddRadarDetectable(RadarDetectable radarDetectable)
    {
        radarDetectables.Add(radarDetectable);
    }

    public RadarDetectable GetLockedJet => LockedJet;

    public void SetLockedJet(RadarDetectable jet) { LockedJet = jet; } 

    public void RemoveRadarDetectable(RadarDetectable radarDetectable)
    {
        radarDetectables.Remove(radarDetectable);
    }

    public void AddPlayerInstance(Transform player)
    {
        PlayerInstances.Add(player);
    }

    public Transform GetLocalTransform()
    {
        return NetworkSpawner.GetLocalTransform();
    }

    public JetParameters GetJetParameters() => Params;

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

        Manager = GetComponent<GameManager>();
    }
}