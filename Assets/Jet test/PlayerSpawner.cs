using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    [SerializeField] GameObject SpawnPointParent;

    [SerializeField] Transform[] SpawnPoints;

    [Networked] int index {  get; set; }

    private void Start()
    {
        index = 0;
        SpawnPoints = SpawnPointParent.GetComponentsInChildren<Transform>();
    }

    void IPlayerJoined.PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            int spawnIndex = player.PlayerId % SpawnPoints.Length;
            Runner.Spawn(PlayerPrefab, SpawnPoints[spawnIndex].position, Quaternion.identity, player);
        }
    }
}