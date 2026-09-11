using Fusion;
using UnityEngine;
public enum JetState {  Air, Ground}
public class JetPhysics : NetworkBehaviour
{
    [Header("Components")]
    public Rigidbody rb { get; private set; }
    [SerializeField] GameObject MeshObject;

    [Header("Parameters")]
    [SerializeField] JetParameters Data;

    [Header("Inputs")]

    public bool HasInput;

    [Header("Debug - Networked Values")]

    public Vector3 currentAngularVelocity;

    public float AngularMag;

    public float ThrustInput;
    public float Yaw;
    public float Pitch;
    public float Roll;

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
    public float IncreaseRate { get; private set; }
    float LiftAOA_0;
    float DragAOA_0;

    [Header("State machine")]
    public JetState State;

    bool mouseLocked = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        AOA = 0f;

        if (!HasStateAuthority)
            return;

        JetCameraController controller = Camera.main.gameObject.GetComponent<JetCameraController>();

        controller.jetTarget = transform;

        controller.PlaceCamera();

        Cursor.lockState = mouseLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !mouseLocked;
    }

    private void Update()
    {
        ThrustMag = Thrust.magnitude;
        LiftMag = Lift.magnitude;
        DragMag = Drag.magnitude;

        currentAngularVelocity = rb.angularVelocity;

        AngularMag = rb.angularVelocity.magnitude;


        if (HasStateAuthority)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            mouseLocked = !mouseLocked;

            Cursor.lockState = mouseLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !mouseLocked;
        }
    }

    public override void FixedUpdateNetwork()
    {
        float dt = Runner.DeltaTime;

        if (HasStateAuthority)
        {
            // thrust
            if (Input.GetKey(KeyCode.W))
            {
                ThrustInput += IncreaseRate * dt;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                ThrustInput -= IncreaseRate * dt;
            }
            else
            {
                ThrustInput = Mathf.MoveTowards(ThrustInput, ThrustMinThreshold, IncreaseRate * dt);
            }
            ThrustInput = Mathf.Clamp01(ThrustInput);

            // yaw
            if (Input.GetKey(KeyCode.D))
                Yaw += IncreaseRate * dt;
            else if (Input.GetKey(KeyCode.A))
                Yaw -= IncreaseRate * dt;
            else
            {
                Yaw = Mathf.MoveTowards(Yaw, 0,IncreaseRate * dt);
            }

            Yaw = Mathf.Clamp(Yaw, -Data.YawThreshold, Data.YawThreshold);

            // others
            Roll = Input.GetAxis("Mouse X");


            Pitch = Input.GetAxis("Mouse Y");
        }
        

        if (rb != null)
        {
            CalculateThrustForce();
            CalculateDragForce();
            CalculateLiftForce();
            ClampSpeed();
            CalculateAOA();
            Weight();
            ApplyManeuver();
        }

        StateMachine();
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
        Vector3 accelerationTorque =
            transform.up * Yaw * Data.YawAMP +
            transform.right * Pitch * Data.PitchAMP +
            transform.forward * Roll * Data.RollAMP;

        rb.AddTorque(accelerationTorque, ForceMode.Acceleration);
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