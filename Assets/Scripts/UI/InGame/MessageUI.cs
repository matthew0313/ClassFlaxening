using DG.Tweening;
using MEC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Collections;

public class MessageUI : MonoBehaviour
{
    [SerializeField] MessageUIElement elementPrefab;
    [SerializeField] Transform elementAnchor;
    [SerializeField] int maxMessages = 50;
    [SerializeField] TMP_InputField inputField;
    readonly List<MessageUIElement> elements = new();
    private void Start()
    {
        MessageManager.Instance.onMessageReceive += OnMessageReceive;
        inputField.onSubmit.AddListener((string message) =>
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            MessageManager.Instance.SendMessageRpc($"{Player.localPlayer.playerName.Value}: {message}");
            inputField.text = string.Empty;
            EventSystem.current.SetSelectedGameObject(null);
        });
    }
    private void Update()
    {
        if (InputManager.Instance.OpenChatInput() && (EventSystem.current.currentSelectedGameObject == null || !EventSystem.current.currentSelectedGameObject.TryGetComponent<TMP_InputField>(out _)))
        {
            inputField.Select();
        }
    }
    private void OnMessageReceive(string message)
    {
        MessageUIElement element;
        if(elements.Count >= maxMessages)
        {
            element = elements[elements.Count - 1];
            elements.RemoveAt(elements.Count - 1);
        }
        else
        {
            element = Instantiate(elementPrefab, elementAnchor);
            element.gameObject.SetActive(true);
        }
        element.Set(message);
        elements.Insert(0, element);
    }
}