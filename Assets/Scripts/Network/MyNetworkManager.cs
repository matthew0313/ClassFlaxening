using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

[RequireComponent(typeof(NetworkManager))]
public class MyNetworkManager : MonoBehaviour
{
    public static MyNetworkManager Instance { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        Instance = Instantiate(Resources.Load<MyNetworkManager>("MyNetworkManager"));
        DontDestroyOnLoad(Instance.gameObject);
    }
}