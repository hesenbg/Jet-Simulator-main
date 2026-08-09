using UnityEngine;
using UnityEngine.Rendering;

public class JetData : MonoBehaviour
{
    public float JetAltitute;

    public float GForce;

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

    private void Start()
    {
        Physics = GetComponent<JetPhysics>();

        CurrentTimeBetweenChanges = 0;
    }

    private void Update()
    {
        CurrentSpeed = Physics.rb.linearVelocity;

        CurrentAOA = Physics.AOA;

        CalculateGForce();

        CalculateChanges();

        CalculateAltitute();
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

    private void CalculateAltitute()
    {
        JetAltitute = transform.position.y;
    }


    private void CalculateGForce()
    {
        float acceleration = SpeedChangeAmount.magnitude / TimeBetweenChanges;
        GForce = (acceleration / 10f) + 1f;

        //GForcePostProccesing.weight = Mathf.Lerp(GForcePostProccesing.weight, GForce / 10f, InterpolationSpeed * Time.deltaTime);
    }
}
