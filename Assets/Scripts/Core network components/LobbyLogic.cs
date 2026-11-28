using Fusion;
using System;
using UnityEngine;

public struct PlayerPropeties : INetworkStruct
{
    public NetworkString<_32> Name;

    public NetworkBool IsReady;
}

public class LobbyLogic : NetworkBehaviour
{
    public event Action<int> OnIndexReceived;

    [Networked, Capacity(3)]
    public NetworkDictionary<PlayerRef, PlayerPropeties> RedSide => default;

    [Networked, Capacity(3)]
    public NetworkDictionary<PlayerRef, PlayerPropeties> BlueSide => default;

    [Networked] public NetworkBool IsAllReady { get; set; }
    [Networked] public int PlayerIndex { get; set; }
    [Networked] private NetworkBool IsEligibleToStart { get; set; }

    [SerializeField] private Canvas canvas;

    public override void Spawned()
    {
        if (HasStateAuthority)
            PlayerIndex = 0;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        IsAllReady = CheckAllReady();
        IsEligibleToStart = CheckLobbyEligible();
    }

    public void EnableUI() => canvas.gameObject.SetActive(true);

    public void DisableUI() => canvas.gameObject.SetActive(false);

    public void SetReady() => RPC_SetReady();

    public void DisableSessionJoin()
    {
        Runner.SessionInfo.IsOpen = false;
        Runner.SessionInfo.IsVisible = false;
    }

    public void SetRedSide() => RPC_SetRedSide(SessionLogic.PlayerName);

    public void SetBlueSide() => RPC_SetBlueSide(SessionLogic.PlayerName);

    public void SetPlayerSides()
    {
        foreach (Transform player in GameEvent_Data.Instance.PlayerInstances)
        {
            if (!player.TryGetComponent(out NetworkedPlayer networked) || !networked.Object.HasStateAuthority)
                continue;

            PlayerRef playerRef = networked.Object.InputAuthority;

            if (RedSide.ContainsKey(playerRef))
                networked.PlayerSide = Side.Red;
            else if (BlueSide.ContainsKey(playerRef))
                networked.PlayerSide = Side.Blue;
        }
    }

    private bool CheckLobbyEligible()
    {
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            if (!RedSide.ContainsKey(player) && !BlueSide.ContainsKey(player))
                return false;
        }

        return true;
    }

    private bool CheckAllReady()
    {
        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            if (RedSide.TryGet(player, out PlayerPropeties props) || BlueSide.TryGet(player, out props))
            {
                if (!props.IsReady)
                    return false;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetIndex(RpcInfo info = default) => RPC_ReceiveIndex(info.Source, PlayerIndex++);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReceiveIndex([RpcTarget] PlayerRef target, int index) => OnIndexReceived?.Invoke(index);

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetRedSide(NetworkString<_32> playerName, RpcInfo info = default)
    {
        if (RedSide.ContainsKey(info.Source) || RedSide.Count >= RedSide.Capacity)
            return;

        BlueSide.Remove(info.Source);
        RedSide.Add(info.Source, new PlayerPropeties { Name = playerName });
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetBlueSide(NetworkString<_32> playerName, RpcInfo info = default)
    {
        if (BlueSide.ContainsKey(info.Source) || BlueSide.Count >= BlueSide.Capacity)
            return;

        RedSide.Remove(info.Source);
        BlueSide.Add(info.Source, new PlayerPropeties { Name = playerName });
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetReady(RpcInfo info = default)
    {
        if (!IsEligibleToStart)
            return;

        if (RedSide.TryGet(info.Source, out PlayerPropeties props))
        {
            props.IsReady = true;
            RedSide.Set(info.Source, props);
        }
        else if (BlueSide.TryGet(info.Source, out props))
        {
            props.IsReady = true;
            BlueSide.Set(info.Source, props);
        }
    }
}