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
        if (jetTarget == null) return;

        Quaternion targetRotation = Quaternion.LookRotation(jetTarget.forward, jetTarget.up);
        Quaternion newRotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, 0f);
        Quaternion deltaRotation = newRotation * Quaternion.Inverse(transform.rotation);

        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;

        if (!float.IsNaN(axis.x) && angle != 0f)
        {
            transform.RotateAround(jetTarget.position, axis, angle);
        } 

        Vector3 targetPosition = jetTarget.position + (jetTarget.rotation * offset);
        transform.position = SOD.SODUpdate(ref posSodState, Time.deltaTime, targetPosition);
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