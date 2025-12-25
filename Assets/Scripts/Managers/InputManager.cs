using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using MEC;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine.SceneManagement;




#if UNITY_EDITOR
using UnityEditor;
#endif

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Instance = new GameObject().AddComponent<InputManager>();
        DontDestroyOnLoad(Instance.gameObject);
    }
    PlayerInput playerInput;
    private void Awake()
    {
        playerInput = new PlayerInput();
    }
    private void OnEnable() => playerInput.Enable();
    private void OnDisable() => playerInput.Disable();
    private void OnDestroy() => playerInput.Dispose();
    public bool UIEscapeInput() => playerInput.UI.Escape.triggered;
    public bool OpenChatInput() => playerInput.InGame.OpenChat.triggered;
}