using UnityEngine;

public class Flare : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 3f);
    }
}