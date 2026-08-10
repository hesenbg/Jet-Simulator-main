using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class JetPhysics : NetworkBehaviour
{
    [Header("Components")]
    public Rigidbody rb { get; private set; }

    [Header("Parameters")]
    [SerializeField] JetParameters Data;

    [Header("Inputs")]
    [Networked] float ThrustInput { get; set; }
    [Networked] float Yaw { get; set; }
    [Networked] float Roll { get; set; }
    [Networked] float Pitch { get; set; }

    [Header("Debug - Networked Values")]
    public float LocalThrustInput;
    public float LocalYaw;
    public float LocalPitch;
    public float LocalRoll;

    [Header("Forces")]
    [SerializeField] Vector3 Thrust;
    [SerializeField] float ThrustMag;
    [SerializeField] Vector3 Lift;
    [SerializeField] float LiftMag;
    [SerializeField] Vector3 Drag;
    [SerializeField] float DragMag;
    [SerializeField] float WeightMag;

    [Header("Aerodynamics")]
    public float AOA;
    [SerializeField] float LiftCoefficient;
    [SerializeField] float DragCoefficient;

    [Header("Runtime Working Values")]
    public float ThrustMinThreshold;
    public float IncreaseRate {  get; private set; }
    float LiftAOA_0;
    float DragAOA_0;

    [Header("State machine")]
    public JetState State;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        AOA = 0f;

        if (!Object.HasInputAuthority)
            return;

        JetCameraController  controller = Camera.main.gameObject.GetComponent<JetCameraController>();

        controller.jetTarget = transform;

        controller.PlaceCamera();
    }

    private void Update()
    {
        ThrustMag = Thrust.magnitude;
        LiftMag = Lift.magnitude;
        DragMag = Drag.magnitude;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Spawner.Instance.LocalTransform = transform;
        }

        Spawner.Instance.PlayerJoined.Invoke(this.transform);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            float dt = Runner.DeltaTime;

            if (data.ThrustUp)
                ThrustInput += dt * IncreaseRate;
            else if (data.ThrustDown)
                ThrustInput -= dt * IncreaseRate;
            else
                ThrustInput = Mathf.Lerp(ThrustInput, ThrustMinThreshold, dt * IncreaseRate);

            ThrustInput = Mathf.Clamp01(ThrustInput);

            if (data.YawRight)
                Yaw += dt * IncreaseRate;
            else if (data.YawLeft)
                Yaw -= dt * IncreaseRate;
            else
                Yaw = Mathf.MoveTowards(Yaw, 0f, dt * IncreaseRate);
            Yaw = Mathf.Clamp(Yaw, -1f, 1f);

            Roll = data.Roll;
            Pitch = data.Pitch;

            LocalThrustInput = ThrustInput;
            LocalYaw = Yaw;
            LocalPitch = Pitch;
            LocalRoll = Roll;

            if (!Object.HasInputAuthority)
            {
                Cursor.lockState = data.IsMouseLoced ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = data.IsMouseLoced;
            }

            Cursor.lockState = data.IsMouseLoced ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = data.IsMouseLoced;
        }

        StateMachine();
        CalculateThrustForce();
        CalculateDragForce();
        CalculateLiftForce();
        ClampSpeed();
        ApplyManeuver();
        CalculateAOA();
        Weight();
    }

    private void StateMachine()
    {
        if (Physics.Raycast(transform.position, -transform.up, Data.AirThresholdDistance))
            State = JetState.Ground;
        else
            State = JetState.Air;

        switch (State)
        {
            case JetState.Ground:
                JetGroundState();
                break;
            case JetState.Air:
                JetAirState();
                break;
        }
    }

    private void JetGroundState()
    {
        ThrustMinThreshold = Data.GroundThrustThreshold;
        DragAOA_0 = Data.GroundBaseDragCoefficient;
        LiftAOA_0 = Data.GroundBaseLiftCoefficient;
        IncreaseRate = Data.GroundIncreaseRate;
        AOA = 0f;
    }

    private void JetAirState()
    {
        ThrustMinThreshold = Data.AirThrustThreshold;
        IncreaseRate = Data.AirIncreaseRate;
        DragAOA_0 = Data.AirBaseDragCoefficient;
        LiftAOA_0 = Data.AirBaseLiftCoefficient;
    }

    private void ApplyManeuver()
    {
        rb.AddTorque(transform.up * Yaw);

        rb.AddTorque(transform.right * Pitch);

        rb.AddTorque(transform.forward * Roll);
    }

    private void CalculateAOA()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            AOA = Vector3.Angle(transform.forward, rb.linearVelocity.normalized) / 90f;
        }

        LiftCoefficient = Data.LiftCoefficientCurve.Evaluate(AOA) + LiftAOA_0;

        DragCoefficient = Data.DragCoefficientCurve.Evaluate(AOA) + DragAOA_0;
    }

    private void Weight()
    {
        rb.AddForce(Vector3.down * Data.WeightAmplifier);

        WeightMag = rb.mass * Physics.gravity.magnitude * Data.WeightAmplifier;
    }

    private void ClampSpeed()
    {
        if (rb.linearVelocity.magnitude > Data.MaxVelocity)
        {
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, Data.MaxVelocity);
        }
    }

    private void CalculateThrustForce()
    {
        Thrust = transform.forward * Data.ThrustForceAmount * ThrustInput;

        rb.AddForce(Thrust);
    }

    private void CalculateDragForce()
    {
        Drag = -rb.linearVelocity.normalized * (Mathf.Pow(rb.linearVelocity.magnitude, 1) / 1) * Data.SurfaceAreaExposed * DragCoefficient * Data.AirDensity;

        rb.AddForce(Drag);
    }

    private void CalculateLiftForce()
    {
        Lift = transform.up * Data.WingSurfaceArea * Thrust.magnitude * LiftCoefficient * Data.AirDensity;

        rb.AddForce(Lift);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(transform.position, Thrust * 3);

        Gizmos.color = Color.blue;

        Gizmos.DrawRay(transform.position, Drag * 3);

        Gizmos.color = Color.red;

        Gizmos.DrawRay(transform.position, Lift * 3);

        Gizmos.color = Color.yellow;

        if (!Application.isPlaying)
            return;

        Gizmos.DrawRay(transform.position, rb.linearVelocity);
    }
}