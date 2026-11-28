using System;
using System.Collections;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [Header("References")]
    [SerializeField] JetPhysics jetMovement;
    [SerializeField] JetParameters jetParameters;
    [SerializeField] CombatData combatData;
    [SerializeField] CombatSystem combatSystem;

    [Header("Input Keys")]
    [SerializeField] KeyCode ThrustForwardKey;
    [SerializeField] KeyCode ThrustBackwardsKey;

    [SerializeField] KeyCode YawRightKey;
    [SerializeField] KeyCode YawLeftKey;

    [SerializeField] KeyCode AfterBurner;

    [SerializeField] KeyCode Flare;

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
    private float currentFlareCooldown;
    private int currentMissileCount;
    private float currentMissileReloadTime;
    private bool isReloadingMissiles;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GameEvent_Data.Instance.OnPlayerSpawned += PlayerJoined;

        jetParameters.CurrentAfterburnerState = AfterburnerState.Ready;

        ResetCombatState();
    }

    private void PlayerJoined(Transform Jet, bool IsStateAth)
    {

        if (!IsStateAth)
            return;

        jetMovement = Jet.gameObject.GetComponent<JetPhysics>();

        combatSystem = Jet.gameObject.GetComponent<CombatSystem>();

        if (combatData == null && combatSystem != null)
            combatData = combatSystem.CombatData;

        ResetCombatState();
    }


    private void ResetCombatState()
    {
        if (combatData == null)
            return;

        currentMissileCount = combatData.MaxMissleCount;
        currentMissileReloadTime = 0f;
        isReloadingMissiles = false;
        currentFlareCooldown = 0f;
    }  
     
    public void Update()
    {
        UpdateScreenLock();

        if (GameEvent_Data.Instance.CurrentPhase != GamePhase.InGame)
            return;

        if(jetParameters.CurrentAfterburnerState == AfterburnerState.Cooldown)
        {
            jetParameters.CurrentAfterburnerCooldownDuration += Time.deltaTime;
            if(jetParameters.CurrentAfterburnerCooldownDuration >= jetParameters.AfterburnerCooldown)
            {
                jetParameters.CurrentAfterburnerState = AfterburnerState.Ready;
            }
        }


        if (combatSystem != null && combatData != null)
        {
            if (combatSystem.CurrentFlareState == FlareState.Cooldown)
            {
                currentFlareCooldown += Time.deltaTime;
                if (currentFlareCooldown >= combatData.FlareCooldown)
                {
                    combatSystem.CurrentFlareState = FlareState.Ready;
                }
            }

            if (isReloadingMissiles)
            {
                currentMissileReloadTime += Time.deltaTime;
                if (currentMissileReloadTime >= combatData.ReloadSpeed)
                {
                    currentMissileCount = combatData.MaxMissleCount;
                    currentMissileReloadTime = 0f;
                    isReloadingMissiles = false;
                }
            }
        }

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
        if (Input.GetKeyDown(FireKey) && !isReloadingMissiles && currentMissileCount > 0)
        {
            if (combatSystem.FireOnShoot())
            {
                currentMissileCount--;
                if (currentMissileCount <= 0)
                {
                    isReloadingMissiles = true;
                    currentMissileReloadTime = 0f;
                }
            }
        }
    }

    IEnumerator AfterburnerPhase()
    {
        jetParameters.CurrentAfterburnerState = AfterburnerState.Using;
        yield return new WaitForSeconds(jetParameters.AfterburnerDuration);
        jetParameters.CurrentAfterburnerState = AfterburnerState.Cooldown;
        jetParameters.CurrentAfterburnerCooldownDuration = 0;
    }

    IEnumerator FlarePhase()
    {
        combatSystem.CurrentFlareState = FlareState.Using;
        yield return new WaitForSeconds(combatData.FlareDuration);
        combatSystem.CurrentFlareState = FlareState.Cooldown;
        currentFlareCooldown = 0f;
    }

    private void UpdateJetMovementInputs(float dt)
    {
        if (Input.GetKeyDown(AfterBurner) && jetParameters.CurrentAfterburnerState == AfterburnerState.Ready)
        {
            jetParameters.CurrentAfterburnerState = AfterburnerState.Using;
            StartCoroutine(AfterburnerPhase());
        }

        // flare
        if (combatSystem != null && combatData != null && Input.GetKeyDown(Flare) && combatSystem.CurrentFlareState == FlareState.Ready)
        {
            combatSystem.CurrentFlareState = FlareState.Using;
            StartCoroutine(FlarePhase());
        }

        if (Input.GetKey(ThrustForwardKey))
            currentThrust += jetMovement.IncreaseRate * dt;
        else if (Input.GetKey(ThrustBackwardsKey))
            currentThrust -= jetMovement.IncreaseRate * dt;
        else
            currentThrust = Mathf.MoveTowards(currentThrust, jetMovement.ThrustMinThreshold, jetMovement.IncreaseRate * dt);

        Thrust = Mathf.Clamp01(currentThrust);

        if (Input.GetKey(YawRightKey))
            currentYaw += jetMovement.IncreaseRate * dt;
        else if (Input.GetKey(YawLeftKey))
            currentYaw -= jetMovement.IncreaseRate * dt;
        else
            currentYaw = Mathf.MoveTowards(currentYaw, 0f, jetMovement.IncreaseRate * dt);

        Yaw = Mathf.Clamp(currentYaw, -jetParameters.YawThreshold, jetParameters.YawThreshold);

        Roll = Input.GetAxis("Mouse X");
        Pitch = Input.GetAxis("Mouse Y");

        jetMovement.SetIputs(Thrust, Yaw, Pitch, Roll);
    }
}