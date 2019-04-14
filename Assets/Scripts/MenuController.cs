#pragma warning disable 649
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace TimeTreasure
{
    public enum MenuPage
    {
        OfflineOnlineMenuPage,
        SignInSignUpMenuPage,
        SignInPage,
        SignUpPage,
        SignUpGuestPage,
        CreateJoinGameMenu,
        CreateGamePage,
        JoinGamePage,
        StartGameMenuPage,
    }

    public class MenuController : MonoBehaviour
    {
        public static MenuController m_Instance { get; private set; }

        [SerializeField]
        private Button m_avatarSignOutButton;
        [SerializeField]
        private TextMeshProUGUI m_playerNameText;

        [SerializeField]
        private TextMeshProUGUI m_gameNameText;
        [SerializeField]
        private TMP_InputField m_gameNameTextInput, m_playerNameTextInput;
        [SerializeField]
        private GameObject m_offlineOnlineMenuPage, m_signInSignUpMenuPage, m_signInPage, m_signUpPage, m_signUpGuestPage, m_createJoinGameMenu, m_createGamePage, m_joinGamePage, m_startGameMenuPage;
        [SerializeField]
        private GameObject m_addPlayerPanel;
        [SerializeField]
        private Button m_backButton, m_offlineButton, m_onlineButton, m_gotoSignInPageButton, m_gotoSignUpPageButton, m_gotoSignUpGuestPageButton, m_gotoCreatePageButton, m_gotoJoinGamePageButton;
        [SerializeField]
        private Button m_createNewGameButton, m_addPlayerButton, m_startGameButton;
        [SerializeField]
        private Game m_gamePrefab;
        [SerializeField]
        private Transform m_gameContainer;
        [SerializeField]
        private List<JoinedPlayerInfo> m_joinedPlayerInfos = new List<JoinedPlayerInfo>();

        private GameObject m_currentlyActivePage;
        private List<Game> m_allCreatedgames = new List<Game>();
        private Stack<GameObject> m_stackOfGameobjectForBack = new Stack<GameObject>();

        private void Awake()
        {
            if (m_Instance != null)
            {
                DestroyImmediate(this);
                return;
            }
            m_Instance = this;
        }

        private void Start()
        {
            ShowMenuPage(MenuPage.OfflineOnlineMenuPage);
            GameManager.m_Instance.ResetGame();
            m_playerNameTextInput.text = "";
            m_playerNameText.text = "";
            if (GameManager.m_Instance.m_JoinedPlayersInfo.Count < 4)
            {
                m_addPlayerButton.interactable = true;
            }

            if (GameManager.m_Instance.m_JoinedPlayersInfo.Count < 2)
            {
                m_startGameButton.interactable = false;
            }
            if (AuthenticationManager.m_Instance != null)
            {
                if (AuthenticationManager.m_Instance.m_User != null)
                {
                    OnAuthenticatinChangedUpdatePlayerInfo(true);
                }
                else
                {
                    OnAuthenticatinChangedUpdatePlayerInfo(false);
                }
            }

            m_backButton.onClick.AddListener(() => OnBackButtonPressed());
            m_offlineButton.onClick.AddListener(() =>
            {
                GameManager.m_Instance.m_GameInfo = new GameInfo("Offline Game", "");
                GameManager.m_Instance.m_GameType = GameType.Offline;
                ShowMenuPage(MenuPage.StartGameMenuPage);

                if (AuthenticationManager.m_Instance.m_User == null)
                {
                    OnAuthenticatinChangedUpdatePlayerInfo(false);
                }
                else
                {
                    OnAuthenticatinChangedUpdatePlayerInfo(true);
                }
            });
            m_onlineButton.onClick.AddListener(() =>
            {
                if(Application.platform != RuntimePlatform.Android)
                {
                    Toast.m_Instance.ShowMessage("Currently not implemented for " + Application.platform.ToString() + " platform");
                    return;
                }
                if(GameManager.m_Instance.IsConnectedToInternet == false)
                {
                    Toast.m_Instance.ShowMessage("You are not connected to the internet");
                }
                GameManager.m_Instance.m_GameType = GameType.Online;
                if (AuthenticationManager.m_Instance.m_User == null)
                {
                    AuthenticationManager.m_Instance.EnableAuthenticationButtons();
                    ShowMenuPage(MenuPage.SignInSignUpMenuPage);
                    OnAuthenticatinChangedUpdatePlayerInfo(false);
                }
                else
                {
                    GameManager.m_Instance.m_OwnerInfo = new PlayerInfo(AuthenticationManager.m_Instance.m_User.DisplayName, AuthenticationManager.m_Instance.m_User.UserId);
                    OnAuthenticatinChangedUpdatePlayerInfo(true);
                    ShowMenuPage(MenuPage.CreateJoinGameMenu);
                }
            });
            m_gotoSignInPageButton.onClick.AddListener(() => ShowMenuPage(MenuPage.SignInPage));
            m_gotoSignUpPageButton.onClick.AddListener(() => ShowMenuPage(MenuPage.SignUpPage));
            m_gotoSignUpGuestPageButton.onClick.AddListener(() => ShowMenuPage(MenuPage.SignUpGuestPage));
            m_gotoCreatePageButton.onClick.AddListener(() =>
            {
                ShowMenuPage(MenuPage.CreateGamePage);
            });
            m_gotoJoinGamePageButton.onClick.AddListener(() =>
            {
                m_gotoJoinGamePageButton.interactable = false;
                FirebaseRealtimeDatabase.m_Instance.GetAllCreatedGames();
            });
            m_createNewGameButton.onClick.AddListener(() =>
            {
                if (AuthenticationManager.m_Instance.m_User != null)
                {
                    if (m_gameNameTextInput.text == "")
                    {
                        Toast.m_Instance.ShowMessage("Please enter the game name", 5);
                        return;
                    }
                    m_createNewGameButton.interactable = false;
                    string creatorUID = AuthenticationManager.m_Instance.m_User.UserId;
                    FirebaseRealtimeDatabase.m_Instance.CreateNewGame(m_gameNameTextInput.text);
                }
                else
                {
                    Toast.m_Instance.ShowMessage("Please sign in to create or join the game name", 5);
                }
            });
            m_addPlayerButton.onClick.AddListener(() => OnAddPlayerButtonPressed());
            m_startGameButton.onClick.AddListener(() => {
                m_startGameButton.interactable = false;
                if (GameManager.m_Instance.m_GameType == GameType.Offline)
                {
                    SceneManager.LoadScene(1);
                }
                else
                {
                    if (GameManager.m_Instance.m_GameInfo.m_CreatorUID == AuthenticationManager.m_Instance.m_User.UserId)
                    {
                        FirebaseRealtimeDatabase.m_Instance.StartGame(GameManager.m_Instance.m_GameInfo);
                    }
                    else
                    {
                        Toast.m_Instance.ShowMessage("You can't start the game. Only creator can start the game...");
                    }
                }
            });
            m_avatarSignOutButton.onClick.AddListener(() =>
            {
                Toast.m_Instance.ShowMessage("Click on avatar");
                AuthenticationManager.m_Instance.SignOut();
            });
        }

        private void OnJoinButtonPressed(GameInfo _game)
        {
            FirebaseRealtimeDatabase.m_Instance.JoinGame(_game);
        }

        private void OnBackButtonPressed()
        {
            m_gotoJoinGamePageButton.interactable = true;
            if (GameManager.m_Instance.m_GameInfo != null && GameManager.m_Instance.m_GameType == GameType.Online)
            {
                FirebaseRealtimeDatabase.m_Instance.UnSubscribeJoinOrRemovePlayerEventHandler(GameManager.m_Instance.m_GameInfo);
                FirebaseRealtimeDatabase.m_Instance.UnSubscribeOnGameStateChangedEventHandler(GameManager.m_Instance.m_GameInfo);
            }

            if (m_stackOfGameobjectForBack.Count > 0)
            {
                m_currentlyActivePage.SetActive(false);
                m_currentlyActivePage = m_stackOfGameobjectForBack.Pop();
                m_currentlyActivePage.SetActive(true);

                if(m_stackOfGameobjectForBack.Count > 0)
                {
                    m_backButton.gameObject.SetActive(true);
                }
                else
                {
                    m_backButton.gameObject.SetActive(false);
                }
            }
        }

        private void OnRemoveButtonPressed(PlayerInfo _playerInfo)
        {
            if (GameManager.m_Instance.m_GameType == GameType.Offline)
            {
                Toast.m_Instance.ShowMessage("Player info remove : " + _playerInfo.m_PlayerName);
                RemovePlayerInfo(_playerInfo);
            }
            else if(GameManager.m_Instance.m_GameType == GameType.Online)
            {
                if (GameManager.m_Instance.m_OwnerInfo != null)
                {
                    if (GameManager.m_Instance.m_GameInfo.m_CreatorUID == GameManager.m_Instance.m_OwnerInfo.m_PlayerUID)
                    {
                        FirebaseRealtimeDatabase.m_Instance.RemoveJoinedPlayer(_playerInfo);
                    }
                    else
                    {
                        if (GameManager.m_Instance.m_OwnerInfo.m_PlayerUID == _playerInfo.m_PlayerUID)
                        {
                            FirebaseRealtimeDatabase.m_Instance.RemoveJoinedPlayer(_playerInfo);
                        }
                    }
                }
                else
                {
                    Toast.m_Instance.ShowMessage("You have to set Owner...");
                }
            }
        }

        private void OnAddPlayerButtonPressed()
        {
            if(m_playerNameTextInput.text == "")
            {
                Toast.m_Instance.ShowMessage("Please enter player name...");
                return;
            }

            foreach (var playerInfo in GameManager.m_Instance.m_JoinedPlayersInfo)
            {
                if(playerInfo.m_PlayerName == m_playerNameTextInput.text)
                {
                    Toast.m_Instance.ShowMessage("Player name is already used... " + GameManager.m_Instance.m_JoinedPlayersInfo.Count);
                    return;
                }
            }

            PlayerInfo _basicPlayerInfo = new PlayerInfo(m_playerNameTextInput.text, "");
            AddPlayerInfo(_basicPlayerInfo);
            m_playerNameTextInput.text = "";
        }

        private void AddPlayerInfo(PlayerInfo _playerInfo)
        {
            if (GameManager.m_Instance.m_GameType == GameType.Online)
            {
                foreach (var _info in GameManager.m_Instance.m_JoinedPlayersInfo)
                {
                    if (_info.m_PlayerUID == _playerInfo.m_PlayerUID)
                    {
                        return;
                    }
                }
            }

            GameManager.m_Instance.m_JoinedPlayersInfo.Add(_playerInfo);
            UpdateJoinedPlayerInfo(GameManager.m_Instance.m_JoinedPlayersInfo);
            if (GameManager.m_Instance.m_JoinedPlayersInfo.Count >= 4)
            {
                m_addPlayerButton.interactable = false;
            }
        }

        private void RemovePlayerInfo(PlayerInfo _playerInfo)
        {
            int index = -1;
            for (int i = 0; i < GameManager.m_Instance.m_JoinedPlayersInfo.Count; i++)
            {
                if (GameManager.m_Instance.m_GameType == GameType.Offline)
                {
                    if (GameManager.m_Instance.m_JoinedPlayersInfo[i].m_PlayerName == _playerInfo.m_PlayerName)
                    {
                        index = i;
                        break;
                    }
                }
                else if (GameManager.m_Instance.m_GameType == GameType.Online)
                {
                    if (GameManager.m_Instance.m_JoinedPlayersInfo[i].m_PlayerUID == _playerInfo.m_PlayerUID)
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index >= 0 && index < GameManager.m_Instance.m_JoinedPlayersInfo.Count)
            {
                //Toast.m_Instance.ShowMessage("Removed " + _playerInfo.m_PlayerUID);
                GameManager.m_Instance.m_JoinedPlayersInfo.RemoveAt(index);
            }
            else
            {
                //Toast.m_Instance.ShowMessage("Can't removed " + _playerInfo.m_PlayerUID);
                return;
            }

            UpdateJoinedPlayerInfo(GameManager.m_Instance.m_JoinedPlayersInfo);

            if (GameManager.m_Instance.m_JoinedPlayersInfo.Count < 4)
            {
                m_addPlayerButton.interactable = true;
            }
        }

        public void UpdateJoinedPlayerInfo(List<PlayerInfo> _playerInfos)
        {
            foreach (var _joinedPlayer in m_joinedPlayerInfos)
            {
                _joinedPlayer.m_removeButton.onClick.RemoveAllListeners();
                _joinedPlayer.gameObject.SetActive(false);
            }

            for (int i = 0; i < _playerInfos.Count; i++)
            {
                PlayerInfo _playerInfo = _playerInfos[i];
                m_joinedPlayerInfos[i].m_playerNameText.text = _playerInfo.m_PlayerName;
                m_joinedPlayerInfos[i].m_playerIndexText.text = "" + i;
                m_joinedPlayerInfos[i].m_removeButton.interactable = true;

                if (GameManager.m_Instance.m_GameType == GameType.Offline)
                {
                    m_joinedPlayerInfos[i].m_removeButton.onClick.AddListener(() => OnRemoveButtonPressed(_playerInfo));
                }
                else if (GameManager.m_Instance.m_GameType == GameType.Online)
                {
                    if (GameManager.m_Instance.m_OwnerInfo.m_PlayerUID == GameManager.m_Instance.m_GameInfo.m_CreatorUID)
                    {
                        m_joinedPlayerInfos[i].m_removeButton.onClick.AddListener(() => OnRemoveButtonPressed(_playerInfo));
                    }
                    else
                    {
                        if (_playerInfos[i].m_PlayerUID == GameManager.m_Instance.m_OwnerInfo.m_PlayerUID)
                        {
                            m_joinedPlayerInfos[i].m_removeButton.onClick.AddListener(() => OnRemoveButtonPressed(_playerInfo));
                        }
                        else
                        {
                            m_joinedPlayerInfos[i].m_removeButton.interactable = false;
                        }
                    }
                }
                m_joinedPlayerInfos[i].gameObject.SetActive(true);
            }

            if (GameManager.m_Instance.m_JoinedPlayersInfo.Count >= 2)
            {
                m_startGameButton.interactable = true;
            }
            else if (GameManager.m_Instance.m_JoinedPlayersInfo.Count < 2)
            {
                m_startGameButton.interactable = false;
            }
        }

        public void ShowMenuPage(MenuPage menuPage)
        {
            if (m_currentlyActivePage != null)
            {
                m_stackOfGameobjectForBack.Push(m_currentlyActivePage);
                m_currentlyActivePage.SetActive(false);
            }

            switch (menuPage)
            {
                case MenuPage.OfflineOnlineMenuPage:
                    m_offlineOnlineMenuPage.SetActive(true);
                    m_currentlyActivePage = m_offlineOnlineMenuPage;
                    m_stackOfGameobjectForBack.Clear();
                    break;
                case MenuPage.SignInSignUpMenuPage:
                    m_signInSignUpMenuPage.SetActive(true);
                    m_currentlyActivePage = m_signInSignUpMenuPage;
                    break;
                case MenuPage.SignInPage:
                    m_signInPage.SetActive(true);
                    m_currentlyActivePage = m_signInPage;
                    break;
                case MenuPage.SignUpPage:
                    m_signUpPage.SetActive(true);
                    m_currentlyActivePage = m_signUpPage;
                    break;
                case MenuPage.SignUpGuestPage:
                    m_signUpGuestPage.SetActive(true);
                    m_currentlyActivePage = m_signUpGuestPage;
                    break;
                case MenuPage.CreateJoinGameMenu:
                    m_gotoJoinGamePageButton.interactable = true;
                    if (AuthenticationManager.m_Instance.m_User == null)
                    {
                        ShowMenuPage(MenuPage.SignInSignUpMenuPage);
                    }
                    else
                    {
                        m_createJoinGameMenu.SetActive(true);
                        m_currentlyActivePage = m_createJoinGameMenu;
                        m_stackOfGameobjectForBack.Clear();
                    }
                    break;
                case MenuPage.CreateGamePage:
                    m_createNewGameButton.interactable = true;
                    m_createGamePage.SetActive(true);
                    m_currentlyActivePage = m_createGamePage;
                    break;
                case MenuPage.JoinGamePage:
                    m_joinGamePage.SetActive(true);
                    m_currentlyActivePage = m_joinGamePage;
                    break;
                case MenuPage.StartGameMenuPage:
                    m_startGameMenuPage.SetActive(true);
                    m_currentlyActivePage = m_startGameMenuPage;
                    m_gameNameText.text = GameManager.m_Instance.m_GameInfo.m_GameName;
                    GameManager.m_Instance.m_JoinedPlayersInfo.Clear();
                    if(GameManager.m_Instance.m_GameType == GameType.Offline)
                    {
                        m_addPlayerPanel.SetActive(true);
                    }
                    else
                    {
                        m_stackOfGameobjectForBack.Clear();
                        FirebaseRealtimeDatabase.m_Instance.GetAllJoinedPlayer(GameManager.m_Instance.m_GameInfo.m_CreatorUID);
                        m_addPlayerPanel.SetActive(false);
                    }
                    UpdateJoinedPlayerInfo(GameManager.m_Instance.m_JoinedPlayersInfo);
                    break;
                default:
                    if (m_stackOfGameobjectForBack.Count > 0)
                    {
                        m_currentlyActivePage = m_stackOfGameobjectForBack.Pop();
                        m_currentlyActivePage.SetActive(true);
                    }
                    break;
            }

            if(m_stackOfGameobjectForBack.Count > 0)
            {
                m_backButton.gameObject.SetActive(true);
            }
            else
            {
                m_backButton.gameObject.SetActive(false);
            }
        }
        
        // Online multiplayer
        public void OnPlayerJoined(PlayerInfo _playerInfo)
        {
            AddPlayerInfo(_playerInfo);
        }

        public void OnPlayerRemoved(PlayerInfo _playerInfo)
        {
            RemovePlayerInfo(_playerInfo);

            if (_playerInfo.m_PlayerUID == AuthenticationManager.m_Instance.m_User.UserId)
            {
                FirebaseRealtimeDatabase.m_Instance.UnSubscribeJoinOrRemovePlayerEventHandler(GameManager.m_Instance.m_GameInfo);
                GameManager.m_Instance.m_MyGameState = GameState.None;
                ShowMenuPage(MenuPage.CreateJoinGameMenu);
            }
        }

        public void UpdateAllCreatedGames(List<GameInfo> games)
        {
            foreach (var game in m_allCreatedgames)
            {
                DestroyImmediate(game.gameObject);
            }

            m_allCreatedgames.Clear();

            foreach (var game in games)
            {
                Game gameInfo = Instantiate(m_gamePrefab, m_gameContainer);
                gameInfo.m_CreatorUID = game.m_CreatorUID;
                gameInfo.m_GameNameText.text = game.m_GameName;
                gameInfo.m_JoinButton.onClick.AddListener(() => OnJoinButtonPressed(game));
                m_allCreatedgames.Add(gameInfo);
            }
        }

        public void OnAuthenticatinChangedUpdatePlayerInfo(bool _isTrue)
        {
            if (_isTrue == false)
            {
                m_avatarSignOutButton.gameObject.SetActive(false);
                m_playerNameText.gameObject.SetActive(false);
            }
            else
            {
                m_avatarSignOutButton.gameObject.SetActive(true);
                m_playerNameText.gameObject.SetActive(true);
                m_playerNameText.text = AuthenticationManager.m_Instance.m_User.DisplayName;
            }
        }
    }
}