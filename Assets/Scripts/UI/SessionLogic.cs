using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionLogic : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI SessionName;

    [SerializeField] Button SessionStartButton;

    [SerializeField] TextMeshProUGUI PlayerCount;

    public void ApplySettings(string sessionName, string playerCount, out Button StartButton) 
    {
        StartButton = SessionStartButton;

        SessionName.text = sessionName;

        PlayerCount.text = playerCount;
    }
}
