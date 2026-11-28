using UnityEngine;
using UnityEngine.Rendering;

public class JetData : MonoBehaviour
{
    [Header("Settings")]
    public float TimeBetweenChanges;
    public float CurrentTimeBetweenChanges;
    public float InterpolationSpeed;
    [SerializeField] private float BaseMinYlevel;
    [SerializeField] private bool IsEnabled = true;

    [Header("Detected Values")]
    public Vector3 DetectedSpeed;
    public float DetectedAOA;
    public Vector3 SpeedChangeAmount;
    public float AOAChangeAmount;

    public Volume GForcePostProccesing;

    private JetPhysics jetPhysics;
    private CombatSystem Combat;
    private Vector3 currentSpeed;
    private float currentAOA;
    private float gForce;

    public JetState CurrentJetState => jetPhysics.State;
    public float GetSpeed => jetPhysics != null && jetPhysics.rb != null ? jetPhysics.rb.linearVelocity.magnitude : 0f;
    public float GetGForce => gForce;
    public float GetAltitute => transform.position.y - BaseMinYlevel;
    public float GetThrust => jetPhysics.Thrust;
    public float GetAOA => jetPhysics != null && jetPhysics.rb != null ? jetPhysics.AOA : 0f;
    public float GetRollValue => jetPhysics.Roll;
    public float GetPitchValue => jetPhysics.Pitch;
    public FlareState GetFlareState => Combat.CurrentFlareState;

    private void Start()
    {
        jetPhysics = GetComponentInParent<JetPhysics>();

        Combat = GetComponentInParent<CombatSystem>();

        CurrentTimeBetweenChanges = 0f;

        if (jetPhysics.HasStateAuthority)
        {
            GForcePostProccesing = GameEvent_Data.Instance.GetGForceVolume;
        }
        else
        {
            IsEnabled = false;
        }
    }

    private void Update()
    {
        if (!IsEnabled)
            return;

        currentSpeed = jetPhysics.rb.linearVelocity;
        currentAOA = jetPhysics.AOA;

        CalculateGForce();
        CalculateChanges();
    }

    private void CalculateGForce()
    {
        float acceleration = SpeedChangeAmount.magnitude / TimeBetweenChanges;
        gForce = acceleration / 10f + 1f;

        GForcePostProccesing.weight = Mathf.Lerp(
            GForcePostProccesing.weight,
            gForce / 10f,
            InterpolationSpeed * Time.deltaTime);
    }

    private void CalculateChanges()
    {
        if (CurrentTimeBetweenChanges < TimeBetweenChanges)
        {
            CurrentTimeBetweenChanges += Time.deltaTime;
            return;
        }

        SpeedChangeAmount = currentSpeed - DetectedSpeed;
        AOAChangeAmount = currentAOA - DetectedAOA;

        DetectedSpeed = currentSpeed;
        DetectedAOA = currentAOA;

        CurrentTimeBetweenChanges = 0f;
    }
}