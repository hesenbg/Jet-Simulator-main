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

    }

    private void Update()
    {
        UpdateJetConditions();
    }
}