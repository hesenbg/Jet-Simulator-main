using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class LobbyUI : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] LobbyLogic lobby;

    [Header("UI Components")]
    [SerializeField] RectTransform RedTeamNameParent;
    [SerializeField] RectTransform BlueTeamNameParent;

    [SerializeField] List<TextMeshProUGUI> RedTeamList;
    [SerializeField] List<TextMeshProUGUI> BlueTeamList;

    bool HasGameStarted;

    private void Start()
    {
        lobby = GetComponent<LobbyLogic>();

        GameEvent_Data.Instance.OnLocalPlayerJoined += OnJoined;

        RedTeamList.Clear();
        RedTeamList.AddRange(RedTeamNameParent.GetComponentsInChildren<TextMeshProUGUI>(true));

        BlueTeamList.Clear();
        BlueTeamList.AddRange(BlueTeamNameParent.GetComponentsInChildren<TextMeshProUGUI>(true));
    }

    private void OnJoined(Transform transform)
    {
        HasGameStarted = true;
    }


    private void UpdateSides()
    {
        foreach (var text in BlueTeamList)
            text.text = string.Empty;

        foreach (var text in RedTeamList)
            text.text = string.Empty;

        int i = 0;
        foreach (var name in lobby.BlueSide)
        {
            BlueTeamList[i].text = name.ToString();
            i++;
        }

        int y = 0;
        foreach (var name in lobby.RedSide)
        {
            RedTeamList[y].text = name.ToString();
            y++;
        }
    }

    private void FixedUpdate()
    {
        if (HasGameStarted)
        {
            UpdateSides();
        }

    }
}