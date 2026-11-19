using Fusion;
using UnityEngine;

public enum MissleState { Search, Chase }

public class Missile : NetworkBehaviour
{
    [SerializeField] VisionCone cone;
    [SerializeField] Rigidbody rb;
    [SerializeField] float speed = 30f;
    [SerializeField] float turnSpeed = 180f;
    [SerializeField] float navigationConstant = 4f;

    [SerializeField] Transform Parent;

    [SerializeField] Transform target;

    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioClip audioClip;

    [Networked] NetworkId TargetNetworkID { get; set; }

    TickTimer NoTargetTimer;

    [SerializeField] private Vector3 InitialTargetLocation;

    public MissleState state;

    private Vector3 previousTargetPosition;
    private bool hasPreviousTargetPosition;
    private bool hasHit;

    public void SetTarget(NetworkId targetID) => TargetNetworkID = targetID;

    public override void Spawned()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (cone == null)
            cone = GetComponent<VisionCone>();

        NoTargetTimer = TickTimer.CreateFromSeconds(Runner, 3f);

        if (audioSource != null && audioClip != null)
            audioSource.PlayOneShot(audioClip, 1f);

        if (rb != null)
            rb.linearVelocity = transform.forward * speed;
    }

    public override void FixedUpdateNetwork()
    {
        if (target == null && Runner.TryFindObject(TargetNetworkID, out NetworkObject targetObject))
        {
            target = targetObject.transform;
            hasPreviousTargetPosition = false;
        }

        if (target == null)
        {
            Transform closest = cone.GetClosest();
            if (closest != null)
            {
                target = closest;
                hasPreviousTargetPosition = false;
            }
        }

        if (target == null)
        {
            state = MissleState.Search;
            hasPreviousTargetPosition = false;
            rb.linearVelocity = transform.forward * speed;

            if (NoTargetTimer.Expired(Runner))
                Runner.Despawn(Object);
            return;
        }

        state = MissleState.Chase;
        Chase();
    }

    private void Chase()
    {
        float dt = Runner.DeltaTime;
        if (dt <= 0f)
            return;

        Vector3 targetPosition = target.position;
        Vector3 targetVelocity = GetTargetVelocity(targetPosition, dt);

        Vector3 missilePosition = rb.position;
        Vector3 missileVelocity = rb.linearVelocity.sqrMagnitude > 0.01f ? rb.linearVelocity : transform.forward * speed;

        Vector3 los = targetPosition - missilePosition;
        float distanceSqr = los.sqrMagnitude;

        if (distanceSqr < 0.0001f)
            return;

        float distance = Mathf.Sqrt(distanceSqr);
        Vector3 losDirection = los / distance;

        Vector3 relativeVelocity = targetVelocity - missileVelocity;
        float closingVelocity = -Vector3.Dot(relativeVelocity, losDirection);

        Vector3 desiredDirection;

        if (closingVelocity > 0f)
        {
            Vector3 losRotationRate = Vector3.Cross(los, relativeVelocity) / distanceSqr;
            Vector3 lateralAcceleration = navigationConstant * closingVelocity * Vector3.Cross(losRotationRate, losDirection);
            Vector3 nextVelocity = missileVelocity + lateralAcceleration * dt;
            desiredDirection = nextVelocity.normalized;
        }
        else
        {
            desiredDirection = losDirection;
        }

        float maxRadiansDelta = turnSpeed * Mathf.Deg2Rad * dt;
        Vector3 steeredDirection = Vector3.RotateTowards(transform.forward, desiredDirection, maxRadiansDelta, 0f).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(steeredDirection);
        transform.rotation = targetRotation;
        rb.rotation = targetRotation;
        rb.linearVelocity = steeredDirection * speed;
    }

    private Vector3 GetTargetVelocity(Vector3 currentTargetPosition, float dt)
    {
        if (target.TryGetComponent<Rigidbody>(out Rigidbody targetRb))
        {
            previousTargetPosition = currentTargetPosition;
            hasPreviousTargetPosition = true;
            return targetRb.linearVelocity;
        }

        if (!hasPreviousTargetPosition)
        {
            previousTargetPosition = currentTargetPosition;
            hasPreviousTargetPosition = true;
            return Vector3.zero;
        }

        Vector3 estimatedVelocity = (currentTargetPosition - previousTargetPosition) / dt;
        previousTargetPosition = currentTargetPosition;
        return estimatedVelocity;
    }

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