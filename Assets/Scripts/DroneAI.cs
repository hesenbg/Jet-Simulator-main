using Fusion;
using UnityEngine;
using UnityEngine.Splines;

public class DroneAI : NetworkBehaviour
{
    [SerializeField] SplineContainer Trajectory;

    [SerializeField] float Speed;

    [SerializeField] Rigidbody Rigidbody;

    [Networked] int Index { get; set; }

    public override void FixedUpdateNetwork()
    {
        Spline spline = Trajectory.Spline;

        Vector3 target = Trajectory.transform.TransformPoint((Vector3)spline[Index].Position);

        Vector3 direction = target - Rigidbody.position;

        Vector3 position = Vector3.MoveTowards(Rigidbody.position, target, Speed * Runner.DeltaTime);

        if (direction.sqrMagnitude > 0f)
            Rigidbody.MoveRotation(Quaternion.LookRotation(direction));

        Rigidbody.MovePosition(position);

        if (position == target)
            Index = (Index + 1) % spline.Count;
    }
}