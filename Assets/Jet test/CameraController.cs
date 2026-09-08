using UnityEngine;

public class JetCameraController : MonoBehaviour
{
    public Transform jetTarget;
    [SerializeField] Vector3 offset = new Vector3(0f, 2f, -8f);

    [Header("Position SOD Params")]
    [SerializeField] float posF = 2.5f;
    [SerializeField] float posZ = 0.9f;
    [SerializeField] float posR = 0.0f;

    [Header("Rotation Follow Speed")]
    [SerializeField] float rotationDamping = 10f;

    SODState posSodState;

    public void PlaceCamera()
    {
        Vector3 initialTargetPos = jetTarget.position + (jetTarget.rotation * offset);
        posSodState = SOD.SODCreate(posF, posZ, posR, initialTargetPos);
    }

    private void LateUpdate()
    {
        UpdateCam();
    }

    private void UpdateCam()
    {
        if (jetTarget == null) return;

        Vector3 targetEuler = jetTarget.eulerAngles;

        Quaternion targetRotation = Quaternion.Euler(targetEuler.x, targetEuler.y, 0f);

        Quaternion deltaRotation = targetRotation * Quaternion.Inverse(transform.rotation);

        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f) angle -= 360f;

        if (!float.IsNaN(axis.x) && angle != 0f)
        {
            transform.RotateAround(jetTarget.position, axis, angle); 
        }

        Vector3 targetPosition = jetTarget.position + (Quaternion.Euler(targetEuler) * offset);

        transform.position = targetPosition;
    }


    private void OnValidate()
    {
        if (jetTarget != null)
        {
            Vector3 initialTargetPos = jetTarget.position + (jetTarget.rotation * offset);
            posSodState = SOD.SODCreate(posF, posZ, posR, initialTargetPos);
        }
    }
}