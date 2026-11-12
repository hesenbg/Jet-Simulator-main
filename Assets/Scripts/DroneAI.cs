using Fusion;
using UnityEngine;
using UnityEngine.Splines;

public class DroneAI : NetworkBehaviour
{
    [SerializeField] Rigidbody Rigidbody;

    [SerializeField] SplineContainer TravelPoints;

    [SerializeField] float Speed;

    [SerializeField] int gizmoSegments = 64;

    [Networked] float Distance { get; set; }

    float length;

    public override void Spawned()
    {
        length = TravelPoints.CalculateLength();
    }

    public override void FixedUpdateNetwork()
    {
        if (length <= 0f)
            return;

        Distance = Mathf.Repeat(Distance + Speed * Runner.DeltaTime, length);

        float t = Distance / length;

        Vector3 position = TravelPoints.EvaluatePosition(t);
        Vector3 tangent = TravelPoints.EvaluateTangent(t);

        if (tangent.sqrMagnitude > 0f)
            Rigidbody.MoveRotation(Quaternion.LookRotation(tangent));

        Rigidbody.MovePosition(position);
    }

    void OnDrawGizmos()
    {

    }
}