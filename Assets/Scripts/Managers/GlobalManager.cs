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

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Instance = Instantiate(Resources.Load<GlobalManager>("GlobalManager"));
        DontDestroyOnLoad(Instance.gameObject);
    }
    [Header("Device")]
    [SerializeField] bool overrideDeviceType = false;
    [SerializeField] DeviceType deviceTypeOverride = DeviceType.Desktop;
    public DeviceType deviceType => overrideDeviceType ? deviceTypeOverride : SystemInfo.deviceType;
}