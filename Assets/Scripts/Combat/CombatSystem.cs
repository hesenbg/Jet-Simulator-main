using Fusion;
using System;
using UnityEngine;
public enum LockState { NoLock, Locking, Locked }
public enum FlareState { Ready, Using, Cooldown }

public class CombatSystem : NetworkBehaviour
{
    [SerializeField] public CombatData CombatData;

    [SerializeField] RadarDetectable LocalJetRadarData;

    [SerializeField] NetworkPrefabRef misslePrefab;

    [SerializeField] Transform missleBay;

    [SerializeField] RadarDetectable LockedJet;

    [Networked] public FlareState CurrentFlareState {  get;  set; }

    Action OnShoot;

    float BaseStealthValue;

    float LockProgress = 0;

    Vector2 ScreenCenter => new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);


    public override void Spawned()
    {
        BaseStealthValue = LocalJetRadarData.Stealth;
        OnShoot += onShoot;
    }

    private void onShoot()
    {
        if (GameEvent_Data.Instance.GetLockedJet == null)
            return;

        if (GameEvent_Data.Instance.GetLockedJet.State == LockState.Locked)
        {
            NetworkObject Object = Runner.Spawn(misslePrefab, missleBay.position, Quaternion.LookRotation(missleBay.forward));

            NetworkId targetId = GameEvent_Data.Instance.GetLockedJet.transform.GetComponentInParent<NetworkObject>().Id;

            if (Object.TryGetComponent<Missile>(out Missile Missle))
            {
                Missle.SetTarget(targetId);
            }

        }
    }

    public bool FireOnShoot()
    {
        if (GameEvent_Data.Instance.GetLockedJet == null || GameEvent_Data.Instance.GetLockedJet.State != LockState.Locked)
            return false;

        OnShoot?.Invoke();
        return true;
    }


    private void UpdateLockedJet(Camera cam) // finds closest possible jet to the center of the screen
    {
        RadarDetectable closest = null;
        float MinScreenDist = float.MaxValue;

        foreach (RadarDetectable target in GameEvent_Data.Instance.radarDetectables)
        {
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

        RadarDetectable previous = GameEvent_Data.Instance.GetLockedJet;

        if (previous != closest) // if closest jet diffferent than the prev one, then start locking again
        {
            LockProgress = 0;

            if (previous != null)
            {
                previous.SetLockProgress(0);
                previous.SetLockState(LockState.NoLock);
            }
        }

        LockedJet = closest;
    }

    private void TryLockJet(RadarDetectable jet, Camera cam)
    {
        if (jet == null)
        {
            GameEvent_Data.Instance.SetLockedJet(null);
            return;
        }

        Vector3 screenPos = cam.WorldToScreenPoint(jet.transform.position);

        float distance = Vector2.Distance(screenPos, ScreenCenter);

        float JetRCSintersectValue = 1 - distance / (CombatData.RadarDetectionSize + jet.Size);

        if (screenPos.z <= 0 || JetRCSintersectValue <= 0)   // z pos is for checking behind the screen
        {
            jet.SetLockState(LockState.NoLock);
            GameEvent_Data.Instance.SetLockedJet(null);
            LockProgress = 0;
            return;
        }

        Vector3 direction = jet.transform.position - transform.position;

        if (!Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit, CombatData.MaxLockDistance)
            || hit.collider.GetComponentInParent<RadarDetectable>() != jet)
        {
            jet.SetLockState(LockState.NoLock);
            GameEvent_Data.Instance.SetLockedJet(null);
            LockProgress = 0;
            return;
        }

        LockProgress = Mathf.Min(1, LockProgress + JetRCSintersectValue * CombatData.LockSpeed * Runner.DeltaTime);

        jet.SetLockState(LockProgress >= 1 ? LockState.Locked : LockState.Locking);

        jet.SetLockProgress(LockProgress);               

        if (jet.State != LockState.NoLock)
        {
            GameEvent_Data.Instance.SetLockedJet(jet);
        }
    }

    private void UpdateVisibility()
    {
        LocalJetRadarData.SetSteath(CurrentFlareState == FlareState.Using ? 0f : BaseStealthValue);
    }

    public override void FixedUpdateNetwork()
    {
        Camera cam = Camera.main;

        UpdateVisibility();

        UpdateLockedJet(cam);

        TryLockJet(LockedJet, cam);
    }
}