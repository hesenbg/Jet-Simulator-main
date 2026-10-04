using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class LobbyLogic : NetworkBehaviour
{
    [Networked, Capacity(3)]
    public NetworkLinkedList<NetworkString<_32>> RedSide => default;

    [Networked, Capacity(3)]
    public NetworkLinkedList<NetworkString<_32>> BlueSide => default;

    [Networked]  bool IsEligibleToStart { get; set; }

    [Networked] public bool IsAllReady { get; set; }

    [SerializeField] Canvas canvas;

    public override void FixedUpdateNetwork()
    {
        CheckEligible();
    }

    public void EnableUI()
    {
        canvas.gameObject.SetActive(true);
    }

    public void DisableUI()
    {
        canvas.gameObject.SetActive(false);
    }

    public void SetReady()
    {
        GameEvent_Data.Instance.GetLocalNetworkedPlayer().IsReady = true;
    }

    private void CheckEligible()
    {
        IsAllReady = GameEvent_Data.Instance.CheckReadyAll();
    }

    public void SetRedSide()
    {
        var player = GameEvent_Data.Instance.GetLocalNetworkedPlayer();
        player.PlayerSide = Side.Red;
        RPC_SetRedSide(player.PlayerName);
    }

    public void SetBlueSide()
    {
        var player = GameEvent_Data.Instance.GetLocalNetworkedPlayer();
        player.PlayerSide = Side.Blue;
        RPC_SetBlueSide(player.PlayerName);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetRedSide(NetworkString<_32> playerName)
    {
        RedSide.Add(playerName);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetBlueSide(NetworkString<_32> playerName)
    {
        BlueSide.Add(playerName);
    }
}