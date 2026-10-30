using Fusion;
using System;
using UnityEngine;
public enum LockState { NoLock, InsideLockZone, Locking, Locked }
public class CombatSystem : NetworkBehaviour
{
    [SerializeField] public CombatData CombatData;

    [SerializeField] RadarDetectable LocalJetRadarData;

    [SerializeField] NetworkPrefabRef misslePrefab;

    [SerializeField] Transform missleBay;

    [SerializeField] RadarDetectable LockedJet;

    Action OnShoot;

    float LockProgress = 0;

    Vector2 ScreenCenter => new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

    Vector3 pos;

    public override void Spawned()
    {
        OnShoot += onShoot;
    }

    private void onShoot()
    {
        if (GameEvent_Data.Instance.LockedJet == null)
            return;

        if(GameEvent_Data.Instance.LockedJet.State == LockState.Locked)
        {
            NetworkObject Object = Runner.Spawn(misslePrefab, missleBay.position, Quaternion.LookRotation(missleBay.forward));

            NetworkId targetId = GameEvent_Data.Instance.LockedJet.transform.GetComponentInParent<NetworkObject>().Id;

            if (Object.TryGetComponent<Missle>(out Missle Missle))
            {
                Missle.SetTarget(targetId);
            }

        }
    }

    public void FireOnShoot()
    {
        OnShoot.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, (pos-transform.position).normalized * CombatData.MaxLockDistance);
    }

    private void UpdateLockedJet(Camera cam) // finds closest possible jet to the center of the screen
    {
        RadarDetectable closest = null;
        float MinScreenDist = float.MaxValue;

        foreach (RadarDetectable target in GameEvent_Data.Instance.radarDetectables)
        {
            pos = target.transform.position;

            if (target == LocalJetRadarData)
                continue;


            Vector3 screenPos = cam.WorldToScreenPoint(target.transform.position);

            //if (screenPos.z <= 0) continue;

            float ScreenDist = Vector2.Distance(screenPos, ScreenCenter);
            float WorldDistance = Vector3.Distance(transform.position, target.transform.position);
 

            if (ScreenDist <= MinScreenDist && WorldDistance < CombatData.MaxLockDistance && ScreenDist < CombatData.RadarDetectionSize)
            {
                closest = target;
                MinScreenDist = ScreenDist;
            }
        }

        RadarDetectable previous = GameEvent_Data.Instance.LockedJet;

        if (previous != closest) // if closest jet diffferent than the prev one, then start locking again
        {
            LockProgress = 0;

            if (previous != null)
                previous.SetLockState(LockState.NoLock);
        }

        LockedJet = closest;
    }

    private void TryLockJet(RadarDetectable jet, Camera cam)
    {
        if(jet== null)  
            return;

        Vector3 screenPos = cam.WorldToScreenPoint(jet.transform.position);

        float distance = Vector2.Distance(screenPos, ScreenCenter);

        float JetRCSintersectValue = 1 - distance / (CombatData.RadarDetectionSize + jet.Size);

        if (screenPos.z <= 0 || JetRCSintersectValue <= 0)   // z pos is for checking behind the screen
        {
            jet.SetLockState(LockState.NoLock);
            LockProgress = 0;
            return;
        }

        Vector3 direction = jet.transform.position - transform.position;

        if (!Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, CombatData.MaxLockDistance)
            || hit.collider.GetComponentInParent<RadarDetectable>() != jet)   
        {
            jet.SetLockState(LockState.NoLock);
            LockProgress = 0;
            return;
        }

        LockProgress = Mathf.Min(1, LockProgress + JetRCSintersectValue * CombatData.LockSpeed * Runner.DeltaTime);

        jet.SetLockState(LockProgress >= 1 ? LockState.Locked : LockState.Locking);

        if(jet.State != LockState.NoLock)
        {
            GameEvent_Data.Instance.LockedJet = jet;
        }

    }

    public override void FixedUpdateNetwork()
    {
        Camera cam = Camera.main;

        UpdateLockedJet(cam);

        TryLockJet(LockedJet, cam);
    }
}