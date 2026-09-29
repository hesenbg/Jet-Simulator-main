using Fusion;
using UnityEngine;
public class InputData : ScriptableObject
{
    
}
public class InputManager : NetworkBehaviour
{
    public static InputManager instance;

    [Header("Input Keys")]
    [SerializeField] KeyCode ThrustForwardKey;
    [SerializeField] KeyCode ThrustBackwardsKey;

    [SerializeField] KeyCode YawRightKey;
    [SerializeField] KeyCode YawLeftKey;

    [SerializeField] int PitchAxisDirection;

    [SerializeField] int RollAxisDirection;

    [Header("Input Values")]

    public bool ThrustForward;

    public bool ThrustBackward;

    public bool YawRight;

    public bool YawLeft;

    public float PitchAxis;

    public float RollAxis;
    private bool mouseLocked;

    private void Awake()
    {
        instance = this;
    }

    public override void FixedUpdateNetwork()
    {
        
    }

    private void Update()
    {
        if (GameEvent_Data.Instance.CurrentPhase != GamePhase.InGame)
            return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            mouseLocked = !mouseLocked;

            Cursor.lockState = mouseLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !mouseLocked;
        }
    }

    public void GetJetPhysicsInput(float IncreaseRate, float dt, float currentThrust, float currentYaw, float yawThreshold,
        out float thrustInput, out float yaw, out float roll, out float pitch)
    {
        ThrustForward = Input.GetKey(ThrustForwardKey);
        ThrustBackward = Input.GetKey(ThrustBackwardsKey);
        YawRight = Input.GetKey(YawRightKey);
        YawLeft = Input.GetKey(YawLeftKey);

        if (ThrustForward)
            currentThrust += IncreaseRate * dt;
        else if (ThrustBackward)
            currentThrust -= IncreaseRate * dt;
        else
            currentThrust = Mathf.MoveTowards(currentThrust, 0f, IncreaseRate * dt);

        thrustInput = Mathf.Clamp01(currentThrust);

        if (YawRight)
            currentYaw += IncreaseRate * dt;
        else if (YawLeft)
            currentYaw -= IncreaseRate * dt;
        else
            currentYaw = Mathf.MoveTowards(currentYaw, 0f, IncreaseRate * dt);

        yaw = Mathf.Clamp(currentYaw, -yawThreshold, yawThreshold);

        RollAxis = Input.GetAxisRaw("Mouse X");
        PitchAxis = Input.GetAxisRaw("Mouse Y");

        roll = RollAxis;
        pitch = PitchAxis;
    }
}