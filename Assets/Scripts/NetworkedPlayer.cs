using Fusion;
public class NetworkedPlayer : NetworkBehaviour
{
    public Side PlayerSide { get;  set; }
    public bool IsReady { get; }
    public string PlayerName { get; private set; }
}