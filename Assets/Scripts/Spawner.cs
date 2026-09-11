using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour, Fusion.INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public Action<Transform> PlayerJoined;
    public Action<Transform> PlayerLeft;

    public Transform LocalTransform;

    [SerializeField] Transform[] SpawnPoints;
    [SerializeField] GameObject SpawnPointParent;

    [SerializeField] GameSessionUiLogic SessionUI;

    public List<Transform> PlayerInstances { get; private set; } = new List<Transform>();

    private NetworkRunner _runner;

    public Action<List<SessionInfo>> OnSessionListUpdated;

    private void Awake()
    {
        PlayerJoined += OnPlayerJoined;
    }

    private void Start()
    {
        if (SpawnPointParent != null)
        {
            SpawnPoints = SpawnPointParent.GetComponentsInChildren<Transform>();
        }

        JoinSessionList();
    }

    private void OnPlayerJoined(Transform transform)
    {
        PlayerInstances.Add(transform);
    }

    public async void JoinSessionList()
    {
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }

        _runner.AddCallbacks(this);

        var result = await _runner.JoinSessionLobby(SessionLobby.Shared);
        if (!result.Ok)
        {
            Debug.LogError($"Failed to join lobby: {result.ShutdownReason}");
        }
    }

    public async void CreateSession(string roomName)
    {
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }

        _runner.AddCallbacks(this);

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public async void JoinSession(string roomName)
    {
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
        }

        _runner.AddCallbacks(this);

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    void Fusion.INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("list updated");
        OnSessionListUpdated?.Invoke(sessionList);
    }

    void Fusion.INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {

            SessionUI.DisableUI();

            int spawnIndex = SpawnPoints != null && SpawnPoints.Length > 0 ? player.PlayerId % SpawnPoints.Length : 0;
            Vector3 position = SpawnPoints != null && SpawnPoints.Length > 0 ? SpawnPoints[spawnIndex].position : Vector3.zero;
            Quaternion rotation = SpawnPoints != null && SpawnPoints.Length > 0 ? SpawnPoints[spawnIndex].rotation : Quaternion.identity;

            NetworkObject networkPlayerObject = runner.Spawn(
                _playerPrefab,
                position,
                rotation,
                player);

            var rb = networkPlayerObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position = position;
                rb.rotation = rotation;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            spawnedCharacters.Add(player, networkPlayerObject);
            runner.SetPlayerObject(player, networkPlayerObject);
            PlayerJoined?.Invoke(networkPlayerObject.transform);
        }
    }

    void Fusion.INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

        SessionUI.EnableUI();

        if (spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            spawnedCharacters.Remove(player);
            PlayerLeft?.Invoke(networkObject.transform);
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