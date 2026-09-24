using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    [SerializeField] GameObject JetIconParent;

    [SerializeField] GameObject JetIconPrefab;

    [SerializeField] float Scale;

    [SerializeField] float MaxDistanceBetweenIcons;

    [SerializeField] Dictionary<Transform, RectTransform> JetIcons = new Dictionary<Transform, RectTransform>();

    public float AngleOffset;

    private void Start()
    {
        GameEvent_Data.Instance.OnPlayerLeft += OnPlayerLeft;

        GameEvent_Data.Instance.OnPlayerJoined = OnPlayerJoined;
    }

    private void OnPlayerLeft(Transform transform)
    {
        RectTransform JetIcon = JetIcons[transform];

        Destroy(JetIcon);

        JetIcons.Remove(transform);
    }

    private void OnPlayerJoined(Transform transform)
    {
        if (JetIcons.ContainsKey(transform))
            return;

        RectTransform JetIconTransform = Instantiate(JetIconPrefab, JetIconParent.transform).GetComponent<RectTransform>();
        JetIcons.Add(transform, JetIconTransform);
    }

    private void Update()
    {
        UpdateJetIcons();
    }

    private void UpdateJetIcons()
    {
        Transform local = GameEvent_Data.Instance.LocalPlayer;
        foreach (Transform tr in GameEvent_Data.Instance.PlayerInstances)
        {
            RectTransform rt = JetIcons[tr];
            Vector2 offset = new Vector2(
                (tr.position.x - local.position.x) / Scale,
                (tr.position.z - local.position.z) / Scale
            );
            if (offset.magnitude > MaxDistanceBetweenIcons)
                offset = offset.normalized * MaxDistanceBetweenIcons;
        
            rt.localPosition = offset;
            rt.localRotation = Quaternion.Euler(0, 0, -tr.eulerAngles.y);
            rt.localRotation = Quaternion.Euler(0, 0, -tr.eulerAngles.y + AngleOffset);
        }
    }
}
