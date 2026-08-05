using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{

    [SerializeField] float Sensitivity;

    [Networked] public bool SpawnedProjectile { get; set; }

    private Camera localCamera;
    private bool cursorLocked = true;
    private float pitch;

    private HealthManager hm;


    public enum PlayerState {Controllable,Dead }

    private void Awake()
    {
        hm= GetComponentInChildren<HealthManager>();
    }

    private void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.Tab))
        {
            cursorLocked = !cursorLocked;
            Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            localCamera = Camera.main;
            localCamera.transform.SetParent(transform);
            localCamera.transform.localPosition = new Vector3(0, 0.55f, 0);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }


    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data) && !hm.IsDead )
        {

        }
    }

    public void CameraController(Vector3 Direction)
    {
        transform.Rotate(Vector3.up * Direction.x * Sensitivity);
        if (!Object.HasInputAuthority) return;
        pitch -= Direction.y * Sensitivity;
        localCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}