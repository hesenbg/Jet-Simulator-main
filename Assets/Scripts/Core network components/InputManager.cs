using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [Header("References")]
    [SerializeField] JetPhysics jetMovement;
    [SerializeField] JetParameters jetParameters;
    [SerializeField] CombatSystem combatSystem;

    [Header("Input Keys")]
    [SerializeField] KeyCode ThrustForwardKey;
    [SerializeField] KeyCode ThrustBackwardsKey;

    [SerializeField] KeyCode YawRightKey;
    [SerializeField] KeyCode YawLeftKey;

    [SerializeField] KeyCode FireKey;

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

    private void PlayerJoined(Transform transform)
    {
        jetMovement = transform.gameObject.GetComponent<JetPhysics>();

        combatSystem = transform.gameObject.GetComponent<CombatSystem>();
    }  
     
    public void Update()
    {
        if (GameEvent_Data.Instance.CurrentPhase != GamePhase.InGame)
            return;

        UpdateScreenLock();

        UpdateWeaponInputs();

        UpdateJetMovementInputs(Time.deltaTime);
    }

    private void UpdateScreenLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            mouseLocked = !mouseLocked;

            Cursor.lockState = mouseLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !mouseLocked;
        }
    }

    private void UpdateWeaponInputs()
    {
        if (Input.GetKeyDown(FireKey))
        {
            combatSystem.FireOnShoot();
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

        jetMovement.SetIputs(Thrust, Yaw, Pitch, Roll);

    }
}