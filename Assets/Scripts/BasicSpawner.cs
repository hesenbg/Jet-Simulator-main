using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, Fusion.INetworkRunnerCallbacks
{

    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField] GameObject Manager;

    void Fusion.INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }

    void Fusion.INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    void Fusion.INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    void Fusion.INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    void Fusion.INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    void Fusion.INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    private bool mouseButton0;
    private bool mouseButton1;

    private void Update()
    {
        mouseButton0 = mouseButton0 | Input.GetMouseButton(0);

        mouseButton1 = mouseButton1 | Input.GetMouseButton(1);

    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        float Thrust = 0;
        float Yaw = 0;
        float Roll = 0;
        float Pitch = 0;

        if (Input.GetKey(KeyCode.W))
            Thrust += 0.2f * _runner.DeltaTime;
        if (Input.GetKey(KeyCode.S))
            Thrust -= 0.2f * _runner.DeltaTime;
        Thrust = Mathf.Clamp01(Thrust);

        if (Input.GetKey(KeyCode.D))
            Yaw += 0.2f * _runner.DeltaTime;
        if (Input.GetKey(KeyCode.A))
            Yaw -= 0.2f * _runner.DeltaTime;
        Yaw = Mathf.Clamp01(Yaw);

        Roll = Input.GetAxis("Mouse X");

        
        Pitch = Input.GetAxis("Mouse Y");

        data.Thrust = Thrust;
        data.Yaw = Yaw;
        data.Roll = Roll;
        data.Pitch = Pitch;

        input.Set(data);
    }

    void Fusion.INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    void Fusion.INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    void Fusion.INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    void Fusion.INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3, 2, 0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            networkPlayerObject.transform.SetParent(Manager.transform);
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    void Fusion.INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
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