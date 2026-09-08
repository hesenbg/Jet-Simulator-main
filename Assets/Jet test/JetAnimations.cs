using UnityEngine;
public class JetAnimations : MonoBehaviour
{
    [Header("Stabilizators")]
    [SerializeField] GameObject LeftElevator;
    [SerializeField] GameObject RightElevator;
    [SerializeField] GameObject LeftVerticalStabilizator;
    [SerializeField] GameObject RightVerticalStabilizator;
    [SerializeField] GameObject RightFlaperon;
    [SerializeField] GameObject LeftFlaperon;
    [Header("Settings")]
    [SerializeField] float MaxAngle = 15f;
    [SerializeField] float RotationSpeed = 5f;
    JetPhysics phsyics;

    Quaternion baseLeftElevator, baseRightElevator;
    Quaternion baseLeftVStab, baseRightVStab;
    Quaternion baseLeftFlaperon, baseRightFlaperon;

    private void Start()
    {
        phsyics = GetComponent<JetPhysics>();

        baseLeftElevator = LeftElevator.transform.localRotation;
        baseRightElevator = RightElevator.transform.localRotation;
        baseLeftVStab = LeftVerticalStabilizator.transform.localRotation;
        baseRightVStab = RightVerticalStabilizator.transform.localRotation;
        baseLeftFlaperon = LeftFlaperon.transform.localRotation;
        baseRightFlaperon = RightFlaperon.transform.localRotation;
    }
     
    private void Update()
    {
        float t = RotationSpeed * Time.deltaTime;
        
        Quaternion elevatorTarget = Quaternion.Euler(phsyics.Pitch * MaxAngle, 0f, 0f);
        LeftElevator.transform.localRotation = Quaternion.Slerp(LeftElevator.transform.localRotation, baseLeftElevator * elevatorTarget, t);
        RightElevator.transform.localRotation = Quaternion.Slerp(RightElevator.transform.localRotation, baseRightElevator * elevatorTarget, t);
        
        Quaternion rudderTarget = Quaternion.Euler(0f, phsyics.Yaw * MaxAngle, 0f);
        LeftVerticalStabilizator.transform.localRotation = Quaternion.Slerp(LeftVerticalStabilizator.transform.localRotation, baseLeftVStab * rudderTarget, t);
        RightVerticalStabilizator.transform.localRotation = Quaternion.Slerp(RightVerticalStabilizator.transform.localRotation, baseRightVStab * rudderTarget, t);
        
        float rollAngle = phsyics.Roll * MaxAngle;
        Quaternion leftFlaperonTarget = Quaternion.Euler(rollAngle, 0f, 0f);
        Quaternion rightFlaperonTarget = Quaternion.Euler(-rollAngle, 0f, 0f);
        LeftFlaperon.transform.localRotation = Quaternion.Slerp(LeftFlaperon.transform.localRotation, baseLeftFlaperon * leftFlaperonTarget, t);
        RightFlaperon.transform.localRotation = Quaternion.Slerp(RightFlaperon.transform.localRotation, baseRightFlaperon * rightFlaperonTarget, t);
    }
}