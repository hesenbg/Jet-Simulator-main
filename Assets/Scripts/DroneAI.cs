using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class DroneAI : NetworkBehaviour
{
    [SerializeField] SplineContainer Trajectory;

    [SerializeField] List<Vector3> TravelPoints;

    [SerializeField] float Speed;

    [SerializeField] Rigidbody Rigidbody;

    [Networked] public Vector3 TargetPos { get; set; }

    [Networked] int Index { get; set; }

    public override void FixedUpdateNetwork()
    {
        Spline spline = Trajectory.Spline;

        if (spline.Count != 4)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 point = Vector3.Lerp(transform.position, TargetPos, i / 3f);
                spline.Add(new BezierKnot(Trajectory.transform.InverseTransformPoint(point)), TangentMode.Linear);
            }

            Index = 0;
        }

        Vector3 target = Trajectory.transform.TransformPoint((Vector3)spline[Index].Position);

        Vector3 direction = target - Rigidbody.position;

        Vector3 position = Vector3.MoveTowards(Rigidbody.position, target, Speed * Runner.DeltaTime);

        if (direction.sqrMagnitude > 0f)
            Rigidbody.MoveRotation(Quaternion.LookRotation(direction));

        Rigidbody.MovePosition(position);

        if (position != target)
            return;

        if (Index < spline.Count - 1)
        {
            Index++;
        }
        else
        {
            spline.Clear();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        for (int i = 0; i < TravelPoints.Count; i++)
        {
            Vector3 point = transform.TransformPoint(TravelPoints[i]);

            Gizmos.DrawSphere(point, 15f);

            if (i > 0)
                Gizmos.DrawLine(transform.TransformPoint(TravelPoints[i - 1]), point);
        }
    }
}