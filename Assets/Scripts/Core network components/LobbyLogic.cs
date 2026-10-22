using Fusion;
using System;
using UnityEngine;

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
        CheckAllReady();

        IsEligibleToStart = CheckLobbyEligible();
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
        if (!IsEligibleToStart)
            return;
        GameEvent_Data.Instance.GetLocalNetworkedPlayer().IsReady = true;
    }

    private bool CheckLobbyEligible()
    {
        foreach (Transform player in GameEvent_Data.Instance.PlayerInstances)
        {
            if (player.gameObject.TryGetComponent<NetworkedPlayer>(out NetworkedPlayer Networked))
            {
                if (Networked.PlayerSide == Side.Neutral)
                    return false;
            }
        }
        return true;
    }

    private void CheckAllReady()
    {
        IsAllReady = GameEvent_Data.Instance.CheckReadyAll();
    }

    public void SetRedSide()
    {
        var player = GameEvent_Data.Instance.GetLocalNetworkedPlayer();

        if (player.PlayerSide == Side.Red)
            return;
        else if (player.PlayerSide== Side.Blue)
        {
            RPC_DelBlueSide(player.PlayerName);
        }
        player.PlayerSide = Side.Red;

        RPC_SetRedSide(player.PlayerName);
    }

    public void SetBlueSide()
    {
        var player = GameEvent_Data.Instance.GetLocalNetworkedPlayer();

        if (player.PlayerSide == Side.Blue)
            return;
        else if (player.PlayerSide == Side.Red)
        {
            RPC_DelRedSide(player.PlayerName);
        }
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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_DelRedSide(NetworkString<_32> playerName)
    {
        RedSide.Remove(playerName);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_DelBlueSide(NetworkString<_32> playerName)
    {
        BlueSide.Remove(playerName);
    }
}