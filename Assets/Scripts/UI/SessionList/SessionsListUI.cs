using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionsListUI : MonoBehaviour
{
    [SerializeField] SessionsListUIElement elementPrefab;
    [SerializeField] Transform elementAnchor;
    readonly List<SessionsListUIElement> elements = new List<SessionsListUIElement>();
    public async void Refresh()
    {
        var sessions = await SessionManager.Instance.QuerySessions();
        int i = 0;
        for(i = 0; i < sessions.Count; i++)
        {
            if (elements.Count <= i) elements.Add(Instantiate(elementPrefab, elementAnchor));
            elements[i].Show();
            elements[i].Set(sessions[i]);
        }
        for(; i < elements.Count; i++)
        {
            elements[i].Hide();
        }
    }
}