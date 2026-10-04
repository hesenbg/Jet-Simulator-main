using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSessionUiLogic : MonoBehaviour
{
    [SerializeField] private GameObject SessionUIPrefab;

    [SerializeField] private Transform SessionUI_Parent;

    [SerializeField] private Spawner NetworkSpawner;

    [SerializeField] private float HeightDifferenceBetweenSessionUI = 60f;

    [SerializeField] private Canvas canvas;

    [SerializeField] string SessionName;

    [SerializeField] string PlayerName;
    
    private void Start()
    {

        GameEvent_Data.Instance.OnLocalPlayerJoined += OnJoined;

        if (NetworkSpawner != null)
        {
            NetworkSpawner.OnSessionListUpdated += RefreshSessionUIlist;
        }
    }

    private void OnJoined(Transform transform)
    {
        transform.gameObject.TryGetComponent<NetworkedPlayer>(out NetworkedPlayer player);

        player.PlayerName = PlayerName;
    }

    private void OnDestroy()
    {
        if (NetworkSpawner != null)
        {
            NetworkSpawner.OnSessionListUpdated -= RefreshSessionUIlist;
        }
    }

    public void AddPlayerName(string Name)
    {
        PlayerName = Name;
    }

    public void EnableUI()
    {
        canvas.gameObject.SetActive(true);
    }

    public void DisableUI()
    {
        canvas.gameObject.SetActive(false);
    }

    public void CreateSession()
    {
        StartCoroutine(CreateSessionDelayed());
    }

    private IEnumerator CreateSessionDelayed()
    {
        yield return new WaitForSeconds(3f);
        NetworkSpawner.CreateSession(SessionName);
    }

    public void AddNewSession(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogWarning("Session name cannot be empty.");
            return;
        }

        SessionName = name;
    }

    public void RefreshSessionUIlist(List<SessionInfo> list)
    {
        foreach (Transform child in SessionUI_Parent)
        {
            Destroy(child.gameObject);
        }

        int index = 0;

        foreach (SessionInfo session in list)
        {
            if (session.IsVisible && session.IsOpen)
            {
                GameObject sessionItem = Instantiate(SessionUIPrefab, SessionUI_Parent);

                if (sessionItem.TryGetComponent<RectTransform>(out RectTransform rectTransform))
                {
                    rectTransform.anchoredPosition = new Vector2(0f, index * HeightDifferenceBetweenSessionUI);
                }

                if (!sessionItem.TryGetComponent<SessionLogic>(out SessionLogic logic))
                {
                    index++;
                    continue;
                }

                string roomName = session.Name;
                string playerCountInfo = $"{session.PlayerCount}/{session.MaxPlayers}";

                logic.ApplySettings(roomName, playerCountInfo, out Button startButton);

                startButton.onClick.AddListener(() =>
                {
                    NetworkSpawner.JoinSession(roomName);
                });

                index++;
            }
        }
    }
}