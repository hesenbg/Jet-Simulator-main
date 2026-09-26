using Fusion;
using System.Collections.Generic;

public class LobbyLogic : NetworkBehaviour
{
    [Networked] public List<NetworkedPlayer> RedSide { get; set; }

    [Networked] public List<NetworkedPlayer> BlueSide { get; set; }

    [Networked] bool IsEligibleToStart { get; set; }

    public override void FixedUpdateNetwork()
    {

    }

    public void SetRedSide(NetworkedPlayer player)
    {
        RedSide.Add(player);
        player.PlayerSide = Side.Red;
    }

    public void SetBlueSide(NetworkedPlayer player)
    {
        BlueSide.Add(player);
        player.PlayerSide = Side.BLue;
    }
}