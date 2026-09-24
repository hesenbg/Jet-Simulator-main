using TMPro;
using UnityEngine;

public class JetConditionUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI JetSpeed;

    [SerializeField] TextMeshProUGUI JetAltitute;

    [SerializeField] TextMeshProUGUI GForce;


    private void Update()
    {
        if (GameEvent_Data.Instance.LocalPlayer == null)
            return;

        JetSpeed.text = Mathf.RoundToInt(GameEvent_Data.Instance.LocalPlayerParams.GetSpeed).ToString();

        JetAltitute.text = Mathf.RoundToInt(GameEvent_Data.Instance.LocalPlayerParams.GetAltitute).ToString();

        GForce.text = GameEvent_Data.Instance.LocalPlayerParams.GetGForce.ToString("F1");
    }
}