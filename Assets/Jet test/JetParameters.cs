using UnityEngine;

[CreateAssetMenu(fileName = "JetParameters", menuName = "Jet/Jet Parameters")]
public class JetParameters : ScriptableObject
{
    [Header("Thrust Settings")]
    public float ThrustForceAmount = 10f;
    public float MaxVelocity;

    [Header("Lift/Drag - Aerodynamics")]
    public float WingSurfaceArea;
    public float SurfaceAreaExposed;

    [Header("Lift Coefficient")]
    public AnimationCurve LiftCoefficientCurve;

    [Header("Drag Coefficient")]
    public AnimationCurve DragCoefficientCurve;

    [Header("Control")]
    public float YawThreshold;
    public float AirDensity;
    public float WeightAmplifier;
    public float InputDeadzone;

    [Header("Mounuver Amplifiers")]
    public float RollAMP;
    public float PitchAMP;
    public float YawAMP;

    public float AngularAccel;

    [Header("State Thresholds")]
    [Header("Air")]
    public float AirThresholdDistance;
    public float AirThrustThreshold;
    public float AirIncreaseRate;
    public float AirBaseDragCoefficient;
    public float AirBaseLiftCoefficient;
    [Header("Ground")]
    public float GroundThrustThreshold;
    public float GroundIncreaseRate;
    public float GroundBaseDragCoefficient;
    public float GroundBaseLiftCoefficient;
}


