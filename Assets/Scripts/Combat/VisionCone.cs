using Fusion;
using UnityEngine;
public class VisionCone : NetworkBehaviour
{
    [SerializeField] float Angle;
    [SerializeField] float Radius;
    [SerializeField] LayerMask JetTarget;
    [SerializeField] LayerMask Obstacles;

    private readonly Collider[] colliders = new Collider[32];
    private int count;

    public override void FixedUpdateNetwork()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, Radius, colliders, JetTarget);        
    }

    private bool IsTargetTrackable(Transform targetTransform)
    {
        if (targetTransform == null || !targetTransform.gameObject.activeInHierarchy)
            return false;

        RadarDetectable rd = targetTransform.GetComponentInParent<RadarDetectable>();
        if (rd == null)
            rd = targetTransform.GetComponentInChildren<RadarDetectable>();

        if (rd != null && (rd.Stealth <= 0f || Mathf.Approximately(rd.Stealth, 0f)))
            return false;

        return true;
    }

    public Transform GetClosest()
    {
        Transform closest = null;
        float minSqrDist = float.MaxValue;

        NetworkedPlayer localPlayer = GameEvent_Data.Instance != null ? GameEvent_Data.Instance.GetLocalNetworkedPlayer() : null;

        for (int i = 0; i < count; i++)
        {
            if (colliders[i] == null) continue;

            if (!IsTargetTrackable(colliders[i].transform)) continue;

            NetworkedPlayer targetJet = colliders[i].GetComponentInParent<NetworkedPlayer>();
            if (targetJet == null)
                targetJet = colliders[i].GetComponentInChildren<NetworkedPlayer>();

            if (targetJet != null && localPlayer != null && localPlayer.PlayerSide == targetJet.PlayerSide)
                continue;

            Vector3 targetPos = colliders[i].transform.position;
            Vector3 toTarget = targetPos - transform.position;

            if (Vector3.Angle(transform.forward, toTarget) > Angle * 0.5f) continue;

            float sqrDist = toTarget.sqrMagnitude;
            if (sqrDist >= minSqrDist) continue;

            if (Physics.Linecast(transform.position, targetPos, Obstacles)) continue;

            minSqrDist = sqrDist;
            closest = colliders[i].transform;
        }

        return closest;
    }

    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        float half = Angle * 0.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, Radius);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(pos, Quaternion.AngleAxis(half, transform.up) * transform.forward * Radius);
        Gizmos.DrawRay(pos, Quaternion.AngleAxis(-half, transform.up) * transform.forward * Radius);
        Gizmos.DrawRay(pos, Quaternion.AngleAxis(half, transform.right) * transform.forward * Radius);
        Gizmos.DrawRay(pos, Quaternion.AngleAxis(-half, transform.right) * transform.forward * Radius);

        if (!Application.isPlaying) return;

        Transform closest = GetClosest();
        if (closest == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, closest.position);
    }
}