using System;
using UnityEngine;

public enum GamePhase { Session, Lobby, InGame, Dead, EndGame }

public class GameManager : MonoBehaviour
{
    [SerializeField] SessionLogic SessionUI;

    [SerializeField] LobbyLogic LobbyUI;

    [SerializeField] JetUI JetUI;

    [SerializeField] GameObject DeadScreen;

    private void Start()
    {
        GameEvent_Data.Instance.CurrentPhase = GamePhase.Session;

        GameEvent_Data.Instance.OnLocalPlayerJoined += OnPlayerJoined;

        GameEvent_Data.Instance.OnPlayerDespawn += OnPlayerDespawned;

        GameEvent_Data.Instance.OnPlayerSpawned += OnPlayerSpawned;
    }

    private void OnPlayerSpawned(Transform transform, bool arg2)
    {
        if (!arg2)
            return;

        if (GameEvent_Data.Instance.CurrentPhase == GamePhase.Lobby)
            return;

        GameEvent_Data.Instance.CurrentPhase = GamePhase.InGame;
    }

    private void OnPlayerDespawned(Transform transform, bool arg2)
    {
        if (!arg2)
            return;
        GameEvent_Data.Instance.CurrentPhase = GamePhase.Dead;
    }

    private void OnPlayerJoined(Transform local)
    {
        GameEvent_Data.Instance.CurrentPhase = GamePhase.Lobby;
    }

    private void Update()
    {

        if (LobbyUI == null || LobbyUI.Object == null || !LobbyUI.Object.IsValid)
            return;

        if (GameEvent_Data.Instance.CurrentPhase == GamePhase.Lobby && LobbyUI.IsAllReady)
        {
            GameEvent_Data.Instance.CurrentPhase = GamePhase.InGame;
        }

        switch (GameEvent_Data.Instance.CurrentPhase)
        {
            case GamePhase.Session:
                Session();
                break;
            case GamePhase.Lobby:
                Lobby();
                break;
            case GamePhase.InGame:
                InGame();
                break;
            case GamePhase.Dead:
                Dead();
                break;
            case GamePhase.EndGame:
                EndGame();
                break;
        }
    }

    private void Lobby()
    {
        SessionUI.DisableUI();
        LobbyUI.EnableUI();
    }

    private void InGame()
    {
        DeadScreen.SetActive(false);
        LobbyUI.DisableUI();
        LobbyUI.DisableSessionJoin();
        JetUI.EnableUI();
    }

    private void Dead()
    {
        JetUI.DisableUI();
        DeadScreen.SetActive(true);
    }

    private void EndGame()
    {

    }

    private void Session()
    {
        SessionUI.EnableUI();
    }
}