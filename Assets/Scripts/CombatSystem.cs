using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public struct RivalJetData
{
    public RivalJetData(float stealth, HealthManager hm, Transform jet, bool locked)
    {
        IsLocked = locked;

        Stealth = stealth;

        transform = jet;

        healthManager = hm;
    }

    public bool IsLocked { get; private set; }

    public Transform transform { get; set; }
    public float Stealth {  get;  set; }
    public HealthManager healthManager { get;  set; }
}

public enum LockState {NoLock,InsideLockZone, Locking, Locked }

public class CombatSystem : NetworkBehaviour
{
    public List<RivalJetData> JetDatas { get; private set; }

    [SerializeField] public CombatData CombatData;

    Action OnFire;

    Side LocalPlayerSide;

    private void Start()
    {

    }

    public override void Spawned()
    {
        GameEvent_Data.Instance.OnPlayerJoined += OnPlayerJoined;
    }

    private void OnPlayerJoined(Transform local, bool IsLocal)
    {
        if (local)
        {
            foreach (var insatnce in GameEvent_Data.Instance.PlayerInstances)
            {
                local.gameObject.TryGetComponent<HealthManager>(out HealthManager HmInstance);
                insatnce.gameObject.TryGetComponent<CombatSystem>(out CombatSystem CombatInstance);

                JetDatas.Add(new RivalJetData(CombatInstance.CombatData.Stealth, HmInstance, local,false));
            }
            return;
        }

        local.gameObject.TryGetComponent<NetworkedPlayer>(out NetworkedPlayer network);
        local.gameObject.TryGetComponent<CombatSystem>(out CombatSystem combat);
        local.gameObject.TryGetComponent<HealthManager>(out HealthManager hm);

        if (network.PlayerSide!= LocalPlayerSide)
            JetDatas.Add(new RivalJetData(combat.CombatData.Stealth,hm, local, false));
    }

    private void UpdateLockedJet()
    {
        float MinScreenDist = float.MaxValue;

        float MinWorldDist = float .MaxValue;

        foreach(var jet in JetDatas)
        {
            float ScreenDist = Vector2.Distance(Camera.main.WorldToScreenPoint(jet.transform.position), new Vector3(Screen.width/2,Screen.height/2));

            float WorldDistance = Vector3.Distance(transform.position,jet.transform.position);

            if (ScreenDist <= MinScreenDist && WorldDistance <= MinWorldDist )
            {
                GameEvent_Data.Instance.LockedJet.transform = jet.transform;

                MinWorldDist = WorldDistance;

                MinScreenDist = ScreenDist;
            }
        }
    }

    private void TryLockJet(Transform jet)
    {

    }

    public override void FixedUpdateNetwork()
    {
        UpdateLockedJet();


    }

    private void Update()
    {
        
    }
}