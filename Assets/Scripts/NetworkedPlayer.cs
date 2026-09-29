using Fusion;
public class NetworkedPlayer : NetworkBehaviour
{
    [Networked] public Side PlayerSide { get; set; }

    [Networked] public string PlayerName { get; set; }

    [Networked] public bool IsReady { get; set; }
}