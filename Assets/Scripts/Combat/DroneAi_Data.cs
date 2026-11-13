using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
[CreateAssetMenu(fileName = "DroneAi_Data")]

public class DroneAi_Data : ScriptableObject
{
    [SerializeField] List<Vector3> travelPoints;

    [SerializeField] SplineContainer TravelSpline;

    [SerializeField] float speed;

    public List<Vector3> TravelPoints => travelPoints;

    public float Speed => speed;
}
