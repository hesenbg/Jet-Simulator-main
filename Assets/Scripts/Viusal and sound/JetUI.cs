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
        if (GameEvent_Data.Instance.JetData == null)
            return;

        JetSpeed.text = Mathf.RoundToInt(GameEvent_Data.Instance.JetData.GetSpeed).ToString();

        JetAltitute.text = Mathf.RoundToInt(GameEvent_Data.Instance.JetData.GetAltitute).ToString();

        GForce.text = GameEvent_Data.Instance.JetData.GetGForce.ToString("F1");
    }



    private void UpdateLockSystemUI()
    {
        if (GameEvent_Data.Instance.LockedJet == null)
        {
            TargetLock.gameObject.SetActive(false);
            return;
        }

        LockState state = GameEvent_Data.Instance.LockedJet.State;

        if (state == LockState.Locked)
            TargetLock.gameObject.SetActive(true);

        else
        {
            TargetLock.gameObject.SetActive(false);
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(GameEvent_Data.Instance.LockedJet.transform.position);

        screenPos.z = 0;

        TargetLock.gameObject.SetActive(true);
        TargetLock.position = screenPos;
    }

    private void Update()
    {
        UpdateLockSystemUI();

        UpdateJetConditions();
    }
}