using Fusion;
using System;
using UnityEngine;

public enum LockState { NoLock, InsideLockZone, Locking, Locked }

public class CombatSystem : NetworkBehaviour
{
    [SerializeField] public CombatData CombatData;

    [SerializeField] RadarDetectable LocalJetRadarData;


    Action OnFire;

    float LockProgress = 0;

    Vector2 ScreenCenter => new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

    private void UpdateLockedJet(Camera cam) // finds closest possible jet to the center of the screen
    {
        RadarDetectable closest = null;
        float MinScreenDist = float.MaxValue;

        foreach (var jet in GameEvent_Data.Instance.radarDetectables)
        {
            if (jet.transform == transform)
                return;

            Vector3 screenPos = cam.WorldToScreenPoint(jet.transform.position);

            //if (screenPos.z <= 0) continue;

            float ScreenDist = Vector2.Distance(screenPos, ScreenCenter);
            float WorldDistance = Vector3.Distance(transform.position, jet.transform.position);

            Debug.Log(ScreenDist);

            if (ScreenDist <= MinScreenDist && WorldDistance < CombatData.MaxLockDistance)
            {
                closest = jet;
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

        GameEvent_Data.Instance.LockedJet = closest;
    }

    private void TryLockJet(RadarDetectable jet, Camera cam)  // checks if it is lockable and locks it 
    {
        Debug.Log(jet.State);

        Vector3 screenPos = cam.WorldToScreenPoint(jet.transform.position);

        float distance = Vector2.Distance(screenPos, ScreenCenter);

        float JetRCSintersectValue = 1 - distance / (CombatData.RadarDetectionSize + jet.Size);

        if (screenPos.z <= 0 || JetRCSintersectValue <= 0)
        {
            jet.SetLockState(LockState.NoLock);
            LockProgress = 0;
            return;
        }

        LockProgress = Mathf.Min(1, LockProgress + JetRCSintersectValue * CombatData.LockSpeed * Runner.DeltaTime);

        jet.SetLockState(LockProgress >= 1 ? LockState.Locked : LockState.Locking);
    }

    public override void FixedUpdateNetwork()
    {
        Camera cam = Camera.main;

        UpdateLockedJet(cam);

        TryLockJet(GameEvent_Data.Instance.LockedJet, cam);
    }
}