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

    public float Thrust {set; get; }
    public float Yaw { set; get; }
    public float Pitch { set; get; }
    public float Roll { set; get; }

    public float networkDeltaTime { set; get; }

    [Header("Vectors")]
    [SerializeField] Vector3 ThrustVector;
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

    NetworkedPlayer player;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        AOA = 0f;

        if (!HasStateAuthority)
            return;

        JetCameraController controller = Camera.main.gameObject.GetComponent<JetCameraController>();

        controller.jetTarget = transform;

        player = gameObject.GetComponent<NetworkedPlayer>();


        controller.PlaceCamera();
    }

    private void Update()
    {
        ThrustMag = ThrustVector.magnitude;
        LiftMag = Lift.magnitude;
        DragMag = Drag.magnitude;

        currentAngularVelocity = rb.angularVelocity;

        AngularMag = rb.angularVelocity.magnitude;
    }

    public override void Spawned()
    {
        GameEvent_Data.Instance.AddPlayerInstance(transform);

        GameEvent_Data.Instance.OnPlayerJoined.Invoke(transform);

        if (HasStateAuthority)
        {
            GameEvent_Data.Instance.LocalPlayerParams = GetComponent<JetData>();

            GameEvent_Data.Instance.OnLocalPlayerJoined.Invoke(transform);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        GameEvent_Data.Instance.OnPlayerLeft.Invoke(transform);
    }


    public void SetIputs(float Thrust, float Yaw, float Pitch, float Roll)
    {
        this.Thrust = Thrust;
        this.Yaw = Yaw;
        this.Pitch = Pitch;
        this.Roll = Roll;
    }
    public override void FixedUpdateNetwork()
    {

        if (!HasStateAuthority)
            return;

        networkDeltaTime = Runner.DeltaTime;

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
            AOA = Vector3.Angle(transform.forward, ThrustVector) / 90f;
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
        ThrustVector = transform.forward * Data.ThrustForceAmount * Thrust;

        rb.AddForce(ThrustVector);
    }

    private void CalculateDragForce()
    {
        Drag = -rb.linearVelocity.normalized * (Mathf.Pow(rb.linearVelocity.magnitude, 1) / 1) * Data.SurfaceAreaExposed * DragCoefficient * Data.AirDensity;

        rb.AddForce(Drag);
    }

    private void CalculateLiftForce()
    {
        Lift = transform.up * Data.WingSurfaceArea * ThrustVector.magnitude * LiftCoefficient * Data.AirDensity;

        rb.AddForce(Lift);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(transform.position, ThrustVector * 3);

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