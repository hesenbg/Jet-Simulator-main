using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    [SerializeField] GameObject JetIconParent;

    [SerializeField] GameObject JetIconPrefab;

    [SerializeField] float Scale;

    [SerializeField] float MaxDistanceBetweenIcons;

    [SerializeField] Dictionary<Transform, RectTransform> JetIcons = new Dictionary<Transform, RectTransform>();

    private void Start()
    {
        Spawner.Instance.PlayerLeft += OnPlayerLeft;

        Spawner.Instance.PlayerJoined += OnPlayerJoined;
    }

    private void OnPlayerLeft(Transform transform)
    {
        RectTransform JetIcon = JetIcons[transform];

        Destroy(JetIcon);

        JetIcons.Remove(transform);
    }

    private void OnPlayerJoined(Transform transform)
    {
        RectTransform JetIconTransform = Instantiate(JetIconPrefab, JetIconParent.transform).GetComponent<RectTransform>();

        Debug.Log("added");

        JetIcons.Add(transform, JetIconTransform);
    }

    private void Update()
    {
        UpdateJetIcons();
    }

    private void UpdateJetIcons()
    {
        foreach(Transform tr in Spawner.Instance.PlayerInstances)
        {
            JetIcons[tr].TryGetComponent<RectTransform>(out RectTransform rt);

            Transform local = Spawner.Instance.LocalTransform;

            Vector3 relativePos = new Vector3((tr.position.x - local.position.x)/Scale, (tr.position.z - local.position.z) / Scale,0f );

            rt.localPosition = relativePos;
        }
    }
}
