using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour, Fusion.INetworkRunnerCallbacks
{

    public static Spawner Instance;

    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    public Action<Transform> PlayerJoined;

    public Action<Transform> PlayerLeft;

    public Transform LocalTransform;

    [SerializeField] Transform[] SpawnPoints;

    [SerializeField] GameObject SpawnPointParent;

    [SerializeField] int SpawnPointIdex = 0;

    public List<Transform> PlayerInstances { get; private set; } = new List<Transform>();

    void Fusion.INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }

    void Fusion.INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    void Fusion.INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    void Fusion.INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    void Fusion.INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    void Fusion.INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    private bool mouseButton0;
    private bool mouseButton1;

    private void Awake()
    {
        Instance = this;

        PlayerJoined += OnPlayerJoined;
    }

    private void Start()
    {
        SpawnPoints = SpawnPointParent.GetComponentsInChildren<Transform>();
    }

    private void OnPlayerJoined(Transform transform)
    {
        PlayerInstances.Add(transform);

        SpawnPointIdex++;
    }


    private void Update()
    {
        mouseButton0 = mouseButton0 | Input.GetMouseButton(0);

        mouseButton1 = mouseButton1 | Input.GetMouseButton(1);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        data.ThrustUp = Input.GetKey(KeyCode.W);
        data.ThrustDown = Input.GetKey(KeyCode.S);

        data.YawRight = Input.GetKey(KeyCode.D);
        data.YawLeft = Input.GetKey(KeyCode.A);

        data.Roll = Input.GetAxisRaw("Mouse X");
        data.Pitch = Input.GetAxisRaw("Mouse Y");

        input.Set(data);
    }

    void Fusion.INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    void Fusion.INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    void Fusion.INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    void Fusion.INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, SpawnPoints[SpawnPointIdex].position, SpawnPoints[SpawnPointIdex].rotation, player);

            SpawnPointIdex++;

            _spawnedCharacters.Add(player, networkPlayerObject);

            runner.SetPlayerObject(player, networkPlayerObject);

            PlayerJoined.Invoke(networkPlayerObject.transform);
        }
    }

    void Fusion.INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
            PlayerLeft.Invoke(networkObject.transform);

        }
    }

    void Fusion.INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    void Fusion.INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }

    void Fusion.INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }

    void Fusion.INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }

    void Fusion.INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    void Fusion.INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }


    private NetworkRunner _runner;

    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    private void OnGUI()
    {
        if (_runner == null)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.fontSize = 30;

            if (GUI.Button(new Rect(0, 0, 400, 80), "Host", style))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(0, 80, 400, 80), "Join", style))
            {
                StartGame(GameMode.Client);
            }
        }
    }
}