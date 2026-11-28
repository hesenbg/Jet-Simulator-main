using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum Side { Red, Blue, Neutral }

public class GameEvent_Data : MonoBehaviour
{
    public static GameEvent_Data Instance;

    [Header("Components")]
    [SerializeField] private Spawner NetworkSpawner;
    [SerializeField] private Volume GForcePostProcces;
    [SerializeField] public LobbyLogic lobby;
    [SerializeField] private RadarDetectable LockedJet;
    [SerializeField] private JetParameters Params;

    [Header("Data")]
    public List<Transform> PlayerInstances;
    public List<RadarDetectable> radarDetectables;
    public GamePhase CurrentPhase;

    [Header("Events")]
    public Action OnLocalPlayerJoined;
    public Action OnLocalPlayerLeft;
    public Action<Transform, bool> OnPlayerSpawned;
    public Action<Transform, bool> OnPlayerDespawn;
    public Action<List<SessionInfo>> OnSessionListUpdated;
    public Action<Transform> OnPlayerRespawned;

    public GameManager Manager { get; private set; }
    public JetData JetData { get; set; }
    public Volume GetGForceVolume => GForcePostProcces;
    public RadarDetectable GetLockedJet => LockedJet;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        NetworkSpawner.LocalPlayerLeft += () => OnLocalPlayerLeft?.Invoke();

        Manager = GetComponent<GameManager>();
    }

    public void AddRadarDetectable(RadarDetectable radarDetectable) => radarDetectables.Add(radarDetectable);

    public void RemoveRadarDetectable(RadarDetectable radarDetectable) => radarDetectables.Remove(radarDetectable);

    public void SetLockedJet(RadarDetectable jet) => LockedJet = jet;

    public void AddPlayerInstance(Transform player) => PlayerInstances.Add(player);

    public Transform GetLocalTransform() => NetworkSpawner.GetLocalTransform();

    public void SpawnPlayer() => NetworkSpawner.SpawnPlayer();

    public void SetPlayers() => lobby.SetPlayerSides();

    public JetParameters GetJetParameters() => Params;

    public void InvokeLocalPlayerJoined() => OnLocalPlayerJoined.Invoke();

    public NetworkedPlayer GetLocalNetworkedPlayer()
    {
        GetLocalTransform().TryGetComponent(out NetworkedPlayer player);
        return player;
    }

    public bool CheckReadyAll()
    {
        if(PlayerInstances.Count == 0) return false;

        foreach (Transform player in PlayerInstances)
        {
            if (player.TryGetComponent(out NetworkedPlayer networked))
            {
                if (!networked.IsReady)
                    return false;
                else
                    continue;
            }
            else
                return false;
        }

        return true;
    }
}