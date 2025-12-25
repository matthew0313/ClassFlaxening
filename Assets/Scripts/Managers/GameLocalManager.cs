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

public class GameLocalManager : MonoBehaviour
{
    public static GameLocalManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
    }
    private void OnDisable()
    {
        NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
    }
    private void OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
    {
        if(data.EventType == ConnectionEvent.ClientDisconnected)
        {
            SceneManager.LoadScene("Title");
        }
    }
}