using System;
using Unity.VisualScripting;
using UnityEngine;

public enum GamePhase {Session, Lobby, InGame }

public class GameManager : MonoBehaviour
{
    [SerializeField] GameSessionUiLogic uiLogic;

    [SerializeField] LobbyLogic LobbyLogic;

    private void Start()
    {
        GameEvent_Data.Instance.CurrentPhase = GamePhase.Session;

        GameEvent_Data.Instance.OnLocalPlayerJoined += OnPlayerJoined;

        GameEvent_Data.Instance.OnLocalPlayerLeft += OnPlayerLeft;
    }

    private void OnPlayerJoined(Transform local)
    {
        uiLogic.DisableUI();
        LobbyLogic.gameObject.SetActive(true);
        GameEvent_Data.Instance.CurrentPhase = GamePhase.Lobby;
    }

    private void OnPlayerLeft()
    {
        uiLogic.EnableUI();
    }

    private void Update()
    {
        if (LobbyLogic == null || LobbyLogic.Object == null || !LobbyLogic.Object.IsValid)
            return;

        if (LobbyLogic.IsAllReady)
        {
            LobbyLogic.gameObject.SetActive(false);
            GameEvent_Data.Instance.CurrentPhase = GamePhase.InGame;
        }
    }
}
