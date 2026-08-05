using UnityEngine;

public class JetParameters : MonoBehaviour
{
    public float JetAltitute;

    public float GForce;

    public float CurrentSpeed;

    public float CurrentAOA;

    JetPhysics Physics;

    [Header("Settings")]
    public float TimeBetweenChanges;
    float CurrentTimeBetweenChanges;

    float DetectedSpeed;
    public float SpeedChangeAmount;

    float DetectedAOA;
    public float AOAChangeAmount;

    private void Start()
    {
        Physics = GetComponent<JetPhysics>();
    }

    private void Update()
    {

        CurrentSpeed = Physics.rb.linearVelocity.magnitude;

        CurrentAOA = Physics.AOA;

        CalculateGForce();

    }


    private void CalculateGForce()
    {
        if (CurrentTimeBetweenChanges < TimeBetweenChanges)
        {
            CurrentTimeBetweenChanges += Time.deltaTime;
        }
        else
        {
            DetectedSpeed = CurrentSpeed;

            DetectedAOA = CurrentAOA;

            SpeedChangeAmount = CurrentSpeed - DetectedSpeed;

            AOAChangeAmount = CurrentAOA - DetectedAOA;
        }

    }
}
