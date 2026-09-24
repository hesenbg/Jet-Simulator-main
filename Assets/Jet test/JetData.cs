using System;
using UnityEngine;
using UnityEngine.Rendering;

public class JetData : MonoBehaviour
{
    float JetAltitute;

    float GForce;

    public float GForceMultipiler;

    public Vector3 CurrentSpeed;

    public float CurrentAOA;

    JetPhysics Physics;

    [Header("Settings")]
    public float TimeBetweenChanges;
    public float CurrentTimeBetweenChanges;

    public Vector3 DetectedSpeed;
    public float DetectedAOA;

    public Vector3 SpeedChangeAmount;
    public float AOAChangeAmount;

    public Volume GForcePostProccesing;

    public float InterpolationSpeed;

    [SerializeField] float BaseMinYlevel;

    [SerializeField] bool IsEnabled = true;

    private void Start()
    {
        Physics = GetComponent<JetPhysics>();

        CurrentTimeBetweenChanges = 0;

        if (Physics.HasStateAuthority)
        {
            GForcePostProccesing = GameEvent_Data.Instance.GetGForceVolume;
        }
        else
        {
            IsEnabled = false;
        }
    }
    private void CalculateChanges()
    {
        if (CurrentTimeBetweenChanges < TimeBetweenChanges)
        {
            CurrentTimeBetweenChanges += Time.deltaTime;
        }
        else
        {
            SpeedChangeAmount = CurrentSpeed - DetectedSpeed;

            AOAChangeAmount = CurrentAOA - DetectedAOA;

            DetectedSpeed = CurrentSpeed;

            DetectedAOA = CurrentAOA;

            CurrentTimeBetweenChanges = 0;
        }
    }





    private void Update()
    {

        if (!IsEnabled)
            return;
        CurrentSpeed = Physics.rb.linearVelocity;

        CurrentAOA = Physics.AOA;

        CalculateGForce();

        CalculateChanges();

    }

    private void CalculateGForce()
    {
        float acceleration = SpeedChangeAmount.magnitude / TimeBetweenChanges;
        GForce = (acceleration / 10f) + 1f;

        GForcePostProccesing.weight = Mathf.Lerp(GForcePostProccesing.weight, GForce / 10f, InterpolationSpeed * Time.deltaTime);
    }

    public float GetSpeed => Physics.rb.linearVelocity.magnitude;

    public float GetGForce => GForce;

    public float GetAltitute => transform.position.y - BaseMinYlevel;

    public float GetThrust => Physics.ThrustInput;

    public float GetRollValue => Physics.Roll;

    public float GetPitchValue => Physics.Pitch;
}
