using TMPro;
using UnityEngine;

public class JetUI : MonoBehaviour
{
    [Header("Conditions")]
    [SerializeField] TextMeshProUGUI JetSpeed;

    [SerializeField] TextMeshProUGUI JetAltitute;

    [SerializeField] TextMeshProUGUI GForce;

    [Header("Lock system")]
    [SerializeField] RectTransform CenterLock;

    [SerializeField] RectTransform TargetLock;

    [Header("Ally List")]
    [SerializeField] RectTransform parent;

    private void UpdateJetConditions()
    {
        if (GameEvent_Data.Instance.LocalPlayerParams == null)
            return;

        JetSpeed.text = Mathf.RoundToInt(GameEvent_Data.Instance.LocalPlayerParams.GetSpeed).ToString();

        JetAltitute.text = Mathf.RoundToInt(GameEvent_Data.Instance.LocalPlayerParams.GetAltitute).ToString();

        GForce.text = GameEvent_Data.Instance.LocalPlayerParams.GetGForce.ToString("F1");
    }


    private void UpdateLockSystemUI()
    {
        if (GameEvent_Data.Instance.LockedJet == null)
            return;
        TargetLock.gameObject.SetActive(GameEvent_Data.Instance.LockedJet.State != LockState.NoLock);

        if (GameEvent_Data.Instance.LockedJet.State == LockState.NoLock)
            return;

        TargetLock.position = Camera.main.WorldToScreenPoint(GameEvent_Data.Instance.LockedJet.transform.position);
    }

    private void Update()
    {
        UpdateLockSystemUI();

        UpdateJetConditions();
    }
}