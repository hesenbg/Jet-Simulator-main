using Fusion;
using UnityEngine;

public enum MissleState { Search, Chase }

public class Missle : NetworkBehaviour
{
    [SerializeField] VisionCone cone;
    [SerializeField] Rigidbody rb;
    [SerializeField] float speed;
    [SerializeField] float turnSpeed = 180f;

    [SerializeField] Transform Parent;

    [SerializeField] Transform target;

    [Networked] NetworkId TargetNetworkID { get; set; }

    TickTimer NoTargetTimer;

    [SerializeField] private Vector3 InitialTargetLocation;

    public MissleState state;

    public void SetTarget(NetworkId targetID) => TargetNetworkID = targetID;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();

        cone = GetComponent<VisionCone>();

        NoTargetTimer = TickTimer.CreateFromSeconds(Runner, 3f);
    }

    public override void FixedUpdateNetwork()
    {

        if (target == null && Runner.TryFindObject(TargetNetworkID, out NetworkObject targetObject))
            target = targetObject.transform;

        if (target == null)
            target = cone.GetClosest();

        if (target == null)
        {
            if (NoTargetTimer.Expired(Runner))
                Runner.Despawn(Object);
            return;
        }

        Chase();
    }

    private void Chase()
    {
        Vector3 direction = target.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        rb.linearVelocity = transform.forward * speed;
    }

    private bool hasHit;

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit || !HasStateAuthority)
            return;

        if (other.transform.IsChildOf(GameEvent_Data.Instance.GetLocalNetworkedPlayer().transform))
            return;

        HealthManager targetHm = other.GetComponentInParent<HealthManager>();

        if (targetHm == null)
            return;

        if (targetHm.Object == null || !targetHm.Object.IsValid)
            return;

        hasHit = true;
        targetHm.RPC_ApplyLethalDamage();
        Runner.Despawn(Object);
    }
}