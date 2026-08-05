using UnityEngine;
using UnityEditor;

public class JetPhysics : MonoBehaviour
{

    [Header("Components")]
    public Rigidbody rb {  get; private set; }

    [Header("Inputs")]
    public JetInput input;

    [Header("Forces")]
    [SerializeField] Vector3 Thrust;
    [SerializeField] float ThrustMag;
    [SerializeField] Vector3 Lift;
    [SerializeField] float LiftMag;
    [SerializeField] Vector3 Drag;
    [SerializeField] float DragMag;
    [SerializeField] float WeightMag;

    [Header("Thrust Settings")]
    [SerializeField] float ThrustForceAmount = 10f;
    [SerializeField] float ThrustMinThreshold = 0.4f;
    [SerializeField] float IncreaseRate = 0.4f;
    [SerializeField] float MaxVelocity;

    [Header("Lift/Drag - Aerodynamics")]
    public float AOA;
    [SerializeField] float WingSurfaceArea;
    [SerializeField] float SurfaceAreaExposed;
    [SerializeField] AnimationCurve ExposedSurfaceAreaRatio;

    [Header("Lift Coefficient")]
    [SerializeField] float LiftAOA_0;
    [SerializeField] AnimationCurve LiftCoefficientCurve;
    [SerializeField] float LiftCoefficient;

    [Header("Drag Coefficient")]
    [SerializeField] float DragAOA_0;
    [SerializeField] AnimationCurve DragCoefficientCurve;
    [SerializeField] float DragCoefficient;

    [Header("Control")]
    [SerializeField] float YawThreshold;
    [SerializeField] float AirDensity;
    [SerializeField] float WeightAmplifier;

    [Header("State machine")]
    public JetState State;

    public float AirThresholdDistance;

    public float GroundThrustThreshold;
    public float AirThrustThreshold;

    public float GroundIncreaseRate;
    public float AirIncreaseRate;

    public float GroundBaseDragCoefficient;
    public float AirBaseDragCoefficient;

    public float GroundBaseLiftCoefficient;
    public float AirBaseLiftCoefficient;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        AOA = 0f;
    }

    private void Update()
    {
        GetInputs();

        ThrustMag = Thrust.magnitude;
        LiftMag = Lift.magnitude;   
        DragMag = Drag.magnitude;

        StateMachine();
    }

    private void FixedUpdate()
    {
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

        if (Physics.Raycast(transform.position, Vector3.down, AirThresholdDistance))
            State = JetState.Ground;
        else
            State = JetState.Air;

        // apply states
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
        ThrustMinThreshold = GroundThrustThreshold;
        DragAOA_0 = GroundBaseDragCoefficient;
        LiftAOA_0 = GroundBaseLiftCoefficient;
        IncreaseRate = GroundIncreaseRate;
        AOA = 0f;
    }

    private void JetAirState()
    {
        ThrustMinThreshold = AirThrustThreshold;
        IncreaseRate = AirIncreaseRate;
        DragAOA_0 = AirBaseDragCoefficient;
        LiftAOA_0 = AirBaseLiftCoefficient;
    }

    private void ApplyManeuver()
    {
        rb.AddTorque(transform.up * input.Yaw);

        rb.AddTorque(transform.right * input.Pitch);

        rb.AddTorque(transform.forward * input.Roll);
    }

    private void CalculateAOA()
    {
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            AOA = Vector3.Angle(transform.forward, rb.linearVelocity.normalized)/90f;
        }

        LiftCoefficient = LiftCoefficientCurve.Evaluate(AOA) + LiftAOA_0;

        DragCoefficient = DragCoefficientCurve.Evaluate(AOA) + DragAOA_0;
    }

    private void Weight()
    {
        rb.AddForce(Vector3.down * WeightAmplifier);

        WeightMag = rb.mass * Physics.gravity.magnitude * WeightAmplifier;
    }


    private void ClampSpeed()
    {
        if (rb.linearVelocity.magnitude > MaxVelocity)
        {
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, MaxVelocity);
        }
    }

    private void CalculateThrustForce()
    {
        Thrust = transform.forward * ThrustForceAmount * input.ThrustValue;

        rb.AddForce(Thrust);
    }

    private void CalculateDragForce()
    {
        Drag = -rb.linearVelocity.normalized * (Mathf.Pow(rb.linearVelocity.magnitude,1)/1) * SurfaceAreaExposed* DragCoefficient*AirDensity;

        rb.AddForce(Drag);
    }

    private void CalculateLiftForce()
    {
        Lift = transform.up * WingSurfaceArea* Thrust.magnitude * LiftCoefficient * AirDensity;

        rb.AddForce(Lift);
    }

    private void GetInputs()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }


        // thrust
        if (Input.GetKey(KeyCode.W))
        {
            input.ThrustValue += IncreaseRate * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            input.ThrustValue -= IncreaseRate * Time.deltaTime;
        }
        else
        {
            input.ThrustValue = Mathf.MoveTowards(input.ThrustValue, ThrustMinThreshold, IncreaseRate * Time.deltaTime);
        }
        input.ThrustValue = Mathf.Clamp01(input.ThrustValue);

        // yaw
        if (Input.GetKey(KeyCode.D))
            input.Yaw += IncreaseRate * Time.deltaTime;
        else if (Input.GetKey(KeyCode.A))
            input.Yaw -= IncreaseRate * Time.deltaTime;
        else
        {
            input.Yaw = Mathf.MoveTowards(input.Yaw, 0f, IncreaseRate * Time.deltaTime);
        }

        input.Yaw = Mathf.Clamp(input.Yaw,-YawThreshold,YawThreshold);

        // others
        input.Roll = Input.GetAxis("Mouse X");


        input.Pitch = Input.GetAxis("Mouse Y");
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(transform.position, Thrust*3);

        Gizmos.color = Color.blue;

        Gizmos.DrawRay(transform.position, Drag*3);

        Gizmos.color = Color.red;

        Gizmos.DrawRay(transform.position, Lift*3);

        Gizmos.color = Color.yellow;

        if (!Application.isPlaying)
            return;
        
        Gizmos.DrawRay(transform.position,rb.linearVelocity);

    }
}
