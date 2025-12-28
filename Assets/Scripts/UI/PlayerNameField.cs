using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class PlayerNameField : MonoBehaviour
{
    TMP_InputField inputField;
    private void OnEnable()
    {
        if (inputField == null) inputField = GetComponent<TMP_InputField>();
        SessionManager.Instance.onPlayerNameChange += OnPlayerNameChange;
        inputField.onEndEdit.AddListener(SessionManager.Instance.SetPlayerName);
        OnPlayerNameChange(SessionManager.Instance.playerName);
    }
    private void OnDisable()
    {
        SessionManager.Instance.onPlayerNameChange -= OnPlayerNameChange;
        inputField.onEndEdit.RemoveListener(SessionManager.Instance.SetPlayerName);
    }
    private void OnPlayerNameChange(string playerName)
    {
        inputField.text = playerName;
    }
}