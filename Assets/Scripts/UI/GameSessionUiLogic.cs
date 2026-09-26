using Fusion;
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

    [SerializeField] string SessionName;

    private void Start()
    {
        if (NetworkSpawner != null)
        {
            NetworkSpawner.OnSessionListUpdated += RefreshSessionUIlist;
        }
    }

    private void OnDestroy()
    {
        if (NetworkSpawner != null)
        {
            NetworkSpawner.OnSessionListUpdated -= RefreshSessionUIlist;
        }
    }

    public void EnableUI()
    {
        gameObject.SetActive(true);
    }

    public void DisableUI()
    {
        gameObject.SetActive(false);
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