using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JetUI : MonoBehaviour
{
    [Header("Conditions")]
    [SerializeField] TextMeshProUGUI JetSpeed;

    [SerializeField] TextMeshProUGUI JetAltitute;

    [SerializeField] TextMeshProUGUI GForce;

    [SerializeField] TextMeshProUGUI AOA;

    [SerializeField] TextMeshProUGUI AfterburnerReady;

    [SerializeField] TextMeshProUGUI FlaresReady;

    [Header("Lock system")]
    [SerializeField] RectTransform CenterLock;

    [SerializeField] RectTransform TargetLock;

    [SerializeField] List<Sprite> LockAnimation;

    [SerializeField] Color BaseTargetLockColor;

    [SerializeField] Color LockedTargetColor;

    [SerializeField] Image TargetLockImage;

    [Header("Ally List")]
    [SerializeField] RectTransform parent;

    public void EnableUI()
    {
        gameObject.SetActive(true);
    }

    public void DisableUI()
    {
        gameObject.SetActive(false);
    }

    private void UpdateJetConditions()
    {
        if (GameEvent_Data.Instance.JetData == null)
            return;

        JetSpeed.text = Mathf.RoundToInt(GameEvent_Data.Instance.JetData.GetSpeed).ToString() + " km/h";

        JetAltitute.text = Mathf.RoundToInt(GameEvent_Data.Instance.JetData.GetAltitute).ToString() + " m";

        GForce.text = GameEvent_Data.Instance.JetData.GetGForce.ToString("F1") + " G";

        AOA.text = GameEvent_Data.Instance.JetData.GetAOA.ToString("F1") + "°";

        AfterburnerReady.text = GameEvent_Data.Instance.GetJetParameters().CurrentAfterburnerState.ToString();
    }



    private void UpdateLockSystemUI()
    {
        if (GameEvent_Data.Instance.GetLockedJet == null)
        {
            TargetLock.gameObject.SetActive(false);
            return;
        }

        if (GameEvent_Data.Instance.GetLockedJet.State == LockState.NoLock || GameEvent_Data.Instance.GetLockedJet == null)
        {
            TargetLockImage.color = BaseTargetLockColor;
            return;
        }
        else if (GameEvent_Data.Instance.GetLockedJet.State == LockState.Locked)
        {
            TargetLockImage.color = LockedTargetColor;
        }

        TargetLockImage.color = GameEvent_Data.Instance.GetLockedJet.State == LockState.Locked ? LockedTargetColor : BaseTargetLockColor;

        LockState state = GameEvent_Data.Instance.GetLockedJet.State;

        if (state == LockState.Locked ||  state == LockState.Locking)
            TargetLock.gameObject.SetActive(true);

        else
        {
            TargetLock.gameObject.SetActive(false);
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(GameEvent_Data.Instance.GetLockedJet.transform.position);

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