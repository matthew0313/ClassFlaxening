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
using Unity.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class MessageManager : NetworkBehaviour
{
    public static MessageManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    public event Action<string> onMessageReceive;
    [Rpc(SendTo.ClientsAndHost, AllowTargetOverride = true)]
    public void SendMessageRpc(FixedString4096Bytes message, RpcParams rpcParams = new()) => onMessageReceive?.Invoke(message.ToString());
}