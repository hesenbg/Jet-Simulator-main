using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameSessionUiLogic uiLogic;

    private void Start()
    {
        GameEvent_Data.Instance.OnLocalPlayerJoined += OnPlayerJoined;

        GameEvent_Data.Instance.OnLocalPlayerLeft += OnPlayerLeft;
    }

    private void OnPlayerJoined(Transform local)
    {
        uiLogic.DisableUI();
    }

    private void OnPlayerLeft()
    {
        uiLogic.EnableUI();
    }

    private void Update()
    {
        
    }
}
