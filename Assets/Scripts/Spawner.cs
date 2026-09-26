using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour, Fusion.INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    [Header("Events")]

    public Action<Transform> LocalPlayerJoined;

    public Action LocalPlayerLeft;

    public Action<List<SessionInfo>> OnSessionListUpdated;

    [Header("Instances")]
    public Transform LocalPlayerTransform;

    private PlayerRef LocalPlayerRef;

    [SerializeField] Transform[] SpawnPoints;

    [SerializeField] GameObject SpawnPointParent;


    private NetworkRunner _runner;

    private List<SessionInfo> Sessions = new List<SessionInfo>();



    private void Awake()
    {
    }

    private void OnDestroy()
    {

        if (_runner != null)
        {
            _runner.RemoveCallbacks(this);
        }
    }

    private void Start()
    {
        if (SpawnPointParent != null)
        {
            var children = new List<Transform>();
            foreach (Transform child in SpawnPointParent.transform)
                children.Add(child);
            SpawnPoints = children.ToArray();
        }

        JoinSessionList();
    }


    private void EnsureRunner()
    {
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            _runner.AddCallbacks(this);
        }
    }

    public async void JoinSessionList()
    {
        EnsureRunner();

        var result = await _runner.JoinSessionLobby(SessionLobby.Shared);
        if (!result.Ok)
        {
            Debug.LogError($"Failed to join lobby: {result.ShutdownReason}");
        }
    }

    public async void CreateSession(string roomName)
    {
        if (Sessions.Exists(s => s.Name == roomName))
        {
            Debug.LogError($"Session '{roomName}' already exists");
            return;
        }

        EnsureRunner();

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            Scene = scene,
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"Failed to create session: {result.ShutdownReason}");
            Destroy(sceneManager);
            Destroy(_runner);
            _runner = null;
            JoinSessionList();
        }
    }

    public async void JoinSession(string roomName)
    {
        EnsureRunner();

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void SpawnPlayer()
    {
        if (_runner == null || LocalPlayerRef != _runner.LocalPlayer) return;

        int spawnIndex = SpawnPoints != null && SpawnPoints.Length > 0 ? LocalPlayerRef.PlayerId % SpawnPoints.Length : 0;
        Vector3 position = SpawnPoints != null && SpawnPoints.Length > 0 ? SpawnPoints[spawnIndex].position : Vector3.zero;
        Quaternion rotation = SpawnPoints != null && SpawnPoints.Length > 0 ? SpawnPoints[spawnIndex].rotation : Quaternion.identity;

        NetworkObject networkPlayerObject = _runner.Spawn(
            playerPrefab,
            position,
            rotation,
            LocalPlayerRef);

        var rb = networkPlayerObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = position;
            rb.rotation = rotation;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        LocalPlayerTransform = networkPlayerObject.transform;
        LocalPlayerJoined?.Invoke(LocalPlayerTransform);

        spawnedCharacters.Add(LocalPlayerRef, networkPlayerObject);
        _runner.SetPlayerObject(LocalPlayerRef, networkPlayerObject);
    }

    void Fusion.INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Sessions = sessionList;
        OnSessionListUpdated?.Invoke(sessionList);
    }

    void Fusion.INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        LocalPlayerRef = player;
    }

    void Fusion.INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            spawnedCharacters.Remove(player);

            if (player == runner.LocalPlayer)
            {
                LocalPlayerLeft?.Invoke();
            }
        }
    }

    void Fusion.INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
    void Fusion.INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    void Fusion.INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    void Fusion.INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    void Fusion.INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    void Fusion.INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    void Fusion.INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    void Fusion.INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void Fusion.INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void Fusion.INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    void Fusion.INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
    void Fusion.INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
    void Fusion.INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
    void Fusion.INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
}