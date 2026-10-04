using Fusion;
using System;
using UnityEngine;
public class InputData : ScriptableObject
{
    
}
public class InputManager : NetworkBehaviour
{
    public static InputManager instance;

    [Header("References")]
    [SerializeField] JetPhysics jetMovement;
    [SerializeField] JetParameters jetParameters;

    [Header("Input Keys")]
    [SerializeField] KeyCode ThrustForwardKey;
    [SerializeField] KeyCode ThrustBackwardsKey;

    [SerializeField] KeyCode YawRightKey;
    [SerializeField] KeyCode YawLeftKey;

    [Header("Input Values")]
    private bool mouseLocked;

    private float Yaw;
    private float Pitch;
    private float Roll;
    private float Thrust;

    [Header("Private Vars")]
    private float currentThrust;
    private float currentYaw;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GameEvent_Data.Instance.OnLocalPlayerJoined += PlayerJoined;
    }

    public override void Spawned()
    {
        GameEvent_Data.Instance.OnLocalPlayerJoined += PlayerJoined;
    }

    private void PlayerJoined(Transform transform)
    {
        jetMovement = transform.gameObject.GetComponent<JetPhysics>();
    }

    public override void FixedUpdateNetwork()
    {
        if(GameEvent_Data.Instance.CurrentPhase == GamePhase.InGame)
        {
            UpdateJetMovementInputs(Runner.DeltaTime);
            jetMovement.SetIputs(Thrust, Yaw, Pitch, Roll);
        }

    }

    private void UpdateJetMovementInputs(float dt)
    {
        if (Input.GetKey(ThrustForwardKey))
            currentThrust += jetMovement.IncreaseRate * dt;
        else if (Input.GetKey(ThrustBackwardsKey))
            currentThrust -= jetMovement.IncreaseRate * dt;
        else
            currentThrust = Mathf.MoveTowards(currentThrust,jetMovement.ThrustMinThreshold , jetMovement.IncreaseRate * dt);

        Thrust = Mathf.Clamp01(currentThrust);

        if (Input.GetKey(YawRightKey))
            currentYaw += jetMovement.IncreaseRate * dt;
        else if (Input.GetKey(YawLeftKey))
            currentYaw -= jetMovement.IncreaseRate * dt;
        else
            currentYaw = Mathf.MoveTowards(currentYaw, 0f, jetMovement.IncreaseRate * dt);

        Yaw = Mathf.Clamp(currentYaw, -jetParameters.YawThreshold, jetParameters.YawThreshold);

        Roll = Input.GetAxisRaw("Mouse X");
        Pitch = Input.GetAxisRaw("Mouse Y");
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
}