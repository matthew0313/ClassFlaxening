using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }
    public SessionManager() => Instance = this;
    ISession m_activeSession;
    public ISession activeSession
    {
        get => m_activeSession;
        private set
        {
            m_activeSession = value;
            Debug.Log($"Active Session Changed: {value.Name}");
            onSessionChange?.Invoke(m_activeSession);
        }
    }
    public Action<ISession> onSessionChange;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    public event Action onSignIn;
    public event Action<string> onPlayerNameChange;
    public event Action onSessionMoveStart, onSessionMoveFail;
    public bool sessionMoving { get; private set; } = false;
    public bool signedIn { get; private set; } = false;
    public string playerName
    {
        get
        {
            string tmp = AuthenticationService.Instance.PlayerName;
            tmp = tmp.Substring(0, tmp.IndexOf('#'));
            return tmp;
        }
    }
    public async void SetPlayerName(string playerName)
    {
        Debug.Log(playerName);
        await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
        onPlayerNameChange?.Invoke(playerName);
    }
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Player: {AuthenticationService.Instance.PlayerName}");
            onSignIn?.Invoke();
            signedIn = true;
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }

    public const string playerNamePropertyKey = "playerName";
    async Task<Dictionary<string, PlayerProperty>> GetPlayerProperties()
    {
        string playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        PlayerProperty playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Public);
        return new Dictionary<string, PlayerProperty>()
        {
            { playerNamePropertyKey, playerNameProperty }
        };
    }
    public const int maxPlayers = 4;
    private async Task<SessionOptions> GetSessionOptions()
    {
        Dictionary<string, PlayerProperty> playerProperties = await GetPlayerProperties();

        SessionOptions options = new()
        {
            MaxPlayers = maxPlayers,
            IsLocked = false,
            IsPrivate = false,
            PlayerProperties = playerProperties
        };
        return options.WithRelayNetwork();
    }
    public async void StartSession()
    {
        sessionMoving = true;
        onSessionMoveStart?.Invoke();
        try 
        {
            activeSession = await MultiplayerService.Instance.CreateSessionAsync(await GetSessionOptions());
        }
        catch
        {
            onSessionMoveFail?.Invoke();
        }
        finally
        {
            sessionMoving = false;
        }
    }
    public async void JoinOrStartSession(string sessionId)
    {
        sessionMoving = true;
        onSessionMoveStart?.Invoke();
        try
        {
            activeSession = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionId, await GetSessionOptions());
        }
        catch
        {
            onSessionMoveFail?.Invoke();
        }
        finally
        {
            sessionMoving = false;
        }
    }
    public async void JoinSessionById(string sessionId)
    {
        sessionMoving = true;
        onSessionMoveStart?.Invoke();
        try
        {
            activeSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
        }
        catch
        {
            onSessionMoveFail?.Invoke();
        }
        finally
        {
            sessionMoving = false;
        }
    }
    public async void JoinSessionByCode(string sessionCode)
    {
        sessionMoving = true;
        onSessionMoveStart?.Invoke();
        try
        {
            activeSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);
        }
        catch
        {
            onSessionMoveFail?.Invoke();
        }
        finally
        {
            sessionMoving = false;
        }
    }
    public async void KickPlayer(string playerId)
    {
        if (!activeSession.IsHost) return;
        await activeSession.AsHost().RemovePlayerAsync(playerId);
    }
    public async Task<IList<ISessionInfo>> QuerySessions()
    {
        var sessionQueryOptions = new QuerySessionsOptions()
        {
            SortOptions = new List<SortOption>()
            {
                new SortOption(SortOrder.Descending, SortField.AvailableSlots)
            }
        };
        var results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
        return results.Sessions;
    }
    public async Task LeaveSession()
    {
        if(activeSession != null)
        {
            sessionMoving = true;
            onSessionMoveStart?.Invoke();
            try
            {
                await activeSession.LeaveAsync();
            }
            catch { }
            finally
            {
                sessionMoving = false;
                activeSession = null;
            }
        }
    }
}
