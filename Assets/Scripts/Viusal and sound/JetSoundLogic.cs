using UnityEngine;

public class JetSoundLogic : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] AudioSource EngineThrust;

    [SerializeField] AudioSource AfterBurnerIgnition;

    [SerializeField] AudioSource FrameAerodynamics;

    [Header("SFXs")]
    [SerializeField] AudioClip AfterBurner;

    [Header("Settings")]
    [SerializeField] float MaxEngineThrustValue;

    [SerializeField] float EnginePitchRange;

    [SerializeField] float MonuverPitchEffectivness;

    [SerializeField] float MonuverRollEffectivness;

    [SerializeField] float InterpolationSpeed;

    float EngineBasePitch;
    float EngineLevel;
    float AerodynamicsLevel;
    Quaternion LastRotation;
    bool HasAfterburnerStartCalled = false;

    private void Awake()
    {
        EngineBasePitch = EngineThrust.pitch;
        LastRotation = transform.rotation;
    }

    private void OnAfterburnerStart()
    {
        AfterBurnerIgnition.PlayOneShot(AfterBurner);
    }

    private void Update()
    {
        float Step = InterpolationSpeed * Time.deltaTime;
        AfterburnerState State = GameEvent_Data.Instance.GetJetParameters().CurrentState;

        EngineLevel = Mathf.MoveTowards(EngineLevel, GameEvent_Data.Instance.JetData.GetThrust, Step);
        EngineThrust.volume = EngineLevel * MaxEngineThrustValue;
        EngineThrust.pitch = EngineBasePitch + EngineLevel * EnginePitchRange;

        Vector3 Delta = (Quaternion.Inverse(LastRotation) * transform.rotation).eulerAngles;
        LastRotation = transform.rotation;
        float RateScale = 1f / Mathf.Max(Time.deltaTime, 0.0001f);
        float PitchRate = Mathf.Abs(Mathf.DeltaAngle(0f, Delta.x)) * RateScale;
        float RollRate = Mathf.Abs(Mathf.DeltaAngle(0f, Delta.z)) * RateScale;
        float AerodynamicsTarget = Mathf.Clamp01(PitchRate * MonuverPitchEffectivness + RollRate * MonuverRollEffectivness);
        AerodynamicsLevel = Mathf.MoveTowards(AerodynamicsLevel, AerodynamicsTarget, Step);
        FrameAerodynamics.volume = AerodynamicsLevel;

        if (State == AfterburnerState.Using && !HasAfterburnerStartCalled)
        {
            OnAfterburnerStart();
            HasAfterburnerStartCalled = true;
        }

        if (State == AfterburnerState.Ready)
        {
            HasAfterburnerStartCalled = false;
        }
    }
}