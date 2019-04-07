#pragma warning disable 649 // for disable warning in the editor

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GameplayCanvasMenu
{
    FirstGameplayMenuPanel = 0,
    GameplayBoardPanel = 1,
    AllPlayerInfoPanel = 2,
    PlayerCardsViewPanel = 3,
    BuyingFuelCardPanel = 4,
    PickCardPanel = 5,
    PlayedCardViewPanel = 6,
    WinningPanel = 7,
}

public class GameplayCanvasManager : MonoBehaviour
{
    public static GameplayCanvasManager m_Instance { get; private set; }

    [SerializeField]
    private TextMeshProUGUI m_currentTurnPlayerText, m_currentTimeAtGMT0Text, m_currentlyPlayingPlayerName, m_remainingCoinText, m_remainingTreasureText;
    [SerializeField]
    private GameObject m_firstGameplayMenuPanel, m_gameplayBoardPanel, m_allPlayerInfoPanel, m_playerCardsViewPanel, m_buyingFuelCardPanel, m_pickCardPanel, m_playedCardViewPanel, m_winningPanel;
    [SerializeField]
    private Button m_menuButton, m_allPlayerInfoButton, m_showCardsButton, m_buyFuelCardButton, m_closePanelViewButton, m_showTreasureButton, m_closeTreasureInfoButton, m_exitGameButton, m_closeMenuButton;
    [SerializeField]
    private GameObject m_distributeCardMenu;
    [SerializeField]
    private RotatingCircle m_rotatingCircularView;
    [SerializeField]
    private Button m_distributeButton;
    [SerializeField]
    private PlayerGameInfo m_playerGameInfoPrefab;
    [SerializeField]
    private Transform m_allPlayerInfoContainer;
    [SerializeField]
    private PlayerAllCardsView m_playerAllCardsView;
    [SerializeField]
    private BuyFuelCardView m_buyFuelCardView;
    [SerializeField]
    private Animation m_menuAnimation, m_playedCardAnimation, m_treasureInfoAnimation;
    [SerializeField]
    private TextMeshProUGUI m_winingPlayerNameTextPrefab;
    [SerializeField]
    private Transform m_winingPlayerContainer;
    [SerializeField]
    private GameObject m_collectedCoinTreasureInfoPanel;
    [SerializeField]
    private TextMeshProUGUI m_collectedCoinTreasureText;

    private GameObject m_closeGameObject;
    private List<PlayerGameInfo> m_playerGameInfos = new List<PlayerGameInfo>();

    private List<TextMeshProUGUI> m_winingPlayerTexts = new List<TextMeshProUGUI>();

    private void Awake()
    {
        if (m_Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        m_Instance = this;
    }

    private void Start()
    {
        if (GameManager.m_Instance == null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            return;
        }

        if (GameManager.m_Instance.m_GameType == GameType.None)
        {
            GameManager.m_Instance.ResetGame();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
        m_menuAnimation.gameObject.SetActive(true);
        UpdateCurrentPlayingPlayerName("", false);
        if (GameManager.m_Instance.m_GameType == GameType.Offline)
        {
            UpdateCurrentPlayerName("");
        }
        else if (GameManager.m_Instance.m_GameType == GameType.Online)
        {
            UpdateCurrentPlayerName(GameManager.m_Instance.m_OwnerInfo.m_PlayerName);
        }

        ShowGameplayCanvasMenu(GameplayCanvasMenu.FirstGameplayMenuPanel);
        m_distributeCardMenu.SetActive(false);
        m_rotatingCircularView.gameObject.SetActive(false);
        m_distributeButton.gameObject.SetActive(true);

        if (m_closeGameObject == null)
        {
            m_closePanelViewButton.interactable = false;
            m_closePanelViewButton.gameObject.SetActive(false);
        }
        else
        {
            m_closePanelViewButton.interactable = true;
            m_closePanelViewButton.gameObject.SetActive(true);
        }

        m_closePanelViewButton.onClick.AddListener(() =>
        {
            if (m_closeGameObject != null)
            {
                ShowGameplayCanvasMenu(GameplayCanvasMenu.GameplayBoardPanel);
            }
        });

        if (GameManager.m_Instance == null)
        {
            return;
        }

        if (GameManager.m_Instance.m_GameType == GameType.Online)
        {
            if (GameManager.m_Instance.m_GameInfo.m_CreatorUID == AuthenticationManager.m_Instance.m_User.UserId)
            {
                m_distributeButton.interactable = true;
            }
            else
            {
                m_distributeButton.interactable = false;
            }
        }
        else
        {
            m_distributeButton.interactable = true;
        }

        EnableOrDisableAllUpperButton(false);
        m_allPlayerInfoButton.onClick.AddListener(() =>
        {
            m_closeGameObject = m_allPlayerInfoPanel;
            m_allPlayerInfoButton.interactable = false;
            UpdateAllPlayerGameInfo();
            ShowGameplayCanvasMenu(GameplayCanvasMenu.AllPlayerInfoPanel);
        });

        m_showCardsButton.onClick.AddListener(() =>
        {
            m_closeGameObject = m_playerCardsViewPanel;
            m_showCardsButton.interactable = false;
            if (GameManager.m_Instance.m_GameType == GameType.Offline)
            {
                UpdateAllCardsOfPlayer(GameplayManager.m_Instance.m_CurrentPawn);
            }
            else if (GameManager.m_Instance.m_GameType == GameType.Online)
            {
                Pawn _pawn = GameplayManager.m_Instance.m_PawnsDict[GameManager.m_Instance.m_OwnerInfo.m_PlayerUID];
                UpdateAllCardsOfPlayer(_pawn);
            }
            else
            {
                return;
            }
            ShowGameplayCanvasMenu(GameplayCanvasMenu.PlayerCardsViewPanel);
        });

        m_buyFuelCardButton.onClick.AddListener(() =>
        {
            if (GameManager.m_Instance.m_GameType == GameType.Offline)
            {
                m_buyFuelCardView.ShowBuyPanel(GameplayManager.m_Instance.m_CurrentPawn);
            }
            else if (GameManager.m_Instance.m_GameType == GameType.Online)
            {
                if (!GameplayManager.m_Instance.m_PawnsDict.ContainsKey(GameManager.m_Instance.m_OwnerInfo.m_PlayerUID))
                {
                    return;
                }
                m_buyFuelCardView.ShowBuyPanel(GameplayManager.m_Instance.m_PawnsDict[GameManager.m_Instance.m_OwnerInfo.m_PlayerUID]);
            }
            ShowGameplayCanvasMenu(GameplayCanvasMenu.BuyingFuelCardPanel);
        });
        m_menuButton.interactable = true;
        if (GameManager.m_Instance.m_GameType == GameType.Online)
        {
            if (GameManager.m_Instance.m_GameInfo.m_CreatorUID != GameManager.m_Instance.m_OwnerInfo.m_PlayerUID)
            {
                m_distributeButton.interactable = false;
            }
            else
            {
                m_distributeButton.interactable = true;
            }
        }
        else if(GameManager.m_Instance.m_GameType == GameType.Offline)
        {
            m_distributeButton.interactable = true;
        }

        m_showTreasureButton.onClick.AddListener(() =>
        {
            m_remainingCoinText.text = ResourceManager.m_Instance.RemainingCoinCount + "";
            m_remainingTreasureText.text = ResourceManager.m_Instance.RemainingTreasureCount + "";
            m_menuAnimation.clip = m_menuAnimation.GetClip("MenuCloseAnimation");
            m_menuAnimation.Play();
            m_menuButton.interactable = false;
            m_treasureInfoAnimation.gameObject.SetActive(true);
            m_treasureInfoAnimation.clip = m_treasureInfoAnimation.GetClip("TreasureInfoOpenAnimation");
            m_treasureInfoAnimation.Play();
        });

        m_closeTreasureInfoButton.onClick.AddListener(() =>
        {
            m_menuButton.interactable = true;
            m_treasureInfoAnimation.clip = m_treasureInfoAnimation.GetClip("TreasureInfoCloseAnimation");
            m_treasureInfoAnimation.Play();
        });

        m_distributeButton.onClick.AddListener(() =>
        {
            if (GameManager.m_Instance.m_GameType == GameType.Online)
            {
                if (GameManager.m_Instance.m_GameInfo.m_CreatorUID != GameManager.m_Instance.m_OwnerInfo.m_PlayerUID)
                {
                    Toast.m_Instance.ShowMessage("You can't distribute the card only creator of the game can distribute...");
                    return;
                }
            }
            m_distributeButton.interactable = false;
            m_distributeButton.gameObject.SetActive(false);
            GameplayManager.m_Instance.DistributeCards();
        });

        m_menuAnimation.gameObject.SetActive(false);
        m_treasureInfoAnimation.gameObject.SetActive(false);
        m_menuButton.onClick.AddListener(() =>
        {
            m_menuAnimation.gameObject.SetActive(true);
            if (!m_menuAnimation.isPlaying)
            {
                m_menuAnimation.clip = m_menuAnimation.GetClip("MenuOpenAnimation");
                m_menuAnimation.Play();
            }
        });

        m_exitGameButton.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            GameManager.m_Instance.ResetGame();
        });

        m_closeMenuButton.onClick.AddListener(() =>
        {
            if (!m_menuAnimation.isPlaying)
            {
                m_menuAnimation.clip = m_menuAnimation.GetClip("MenuCloseAnimation");
                m_menuAnimation.Play();
            }
        });
    }

    private void UpdateAllPlayerGameInfo()
    {
        foreach (var _playerGameInfo in m_playerGameInfos)
        {
            DestroyImmediate(_playerGameInfo.gameObject);
        }

        m_playerGameInfos.Clear();
        foreach (var _pawn in GameplayManager.m_Instance.m_PawnsDict.Values)
        {
            PlayerGameInfo _playerGameInfo = Instantiate(m_playerGameInfoPrefab, m_allPlayerInfoContainer);
            _playerGameInfo.UpdatePlayerGameInfo(_pawn);
            m_playerGameInfos.Add(_playerGameInfo);
        }
    }

    private void UpdateAllCardsOfPlayer(Pawn _pawn)
    {
        if (_pawn == null)
        {
            return;
        }

        m_playerAllCardsView.UpdateCards(_pawn);
    }

    public void UpdateCurrentPlayerName(string _playerName)
    {
        m_currentTurnPlayerText.text = _playerName;
    }

    public void UpdateCurrentTimeOfGMT0(int _time)
    {
        string _timeText = "";

        if (_time == 24)
        {
            _timeText = "12 AM";
        }
        else if (_time == 12)
        {
            _timeText = "12 PM";
        }
        else if (_time > 0 && _time < 12)
        {
            _timeText = _time + " AM";
        }
        else if (_time > 12 && _time < 24)
        {
            _timeText = _time % 12 + " PM";
        }

        m_currentTimeAtGMT0Text.text = "Current Time at GMT 0  : " + _timeText;
    }

    public void UpdateCurrentPlayingPlayerName(string _playerName, bool _isVisible = true)
    {
        m_currentlyPlayingPlayerName.gameObject.SetActive(_isVisible);
        if (GameManager.m_Instance.m_GameType == GameType.Online)
        {
            if (_playerName == GameManager.m_Instance.m_OwnerInfo.m_PlayerName)
            {
                m_currentlyPlayingPlayerName.text = "It's your turn";
            }
            else
            {
                m_currentlyPlayingPlayerName.text = _playerName + " is currently playing";
            }
        }
        else if(GameManager.m_Instance.m_GameType == GameType.Offline)
        {
            m_currentlyPlayingPlayerName.text = "It's " + _playerName + " turn";
        }
    }

    public void ShowGameplayCanvasMenu(GameplayCanvasMenu _gameplayCanvasMenu)
    {
        m_firstGameplayMenuPanel.SetActive(false);
        m_gameplayBoardPanel.SetActive(false);
        m_allPlayerInfoPanel.SetActive(false);
        m_playerCardsViewPanel.SetActive(false);
        m_buyingFuelCardPanel.SetActive(false);
        m_pickCardPanel.SetActive(false);
        m_playedCardViewPanel.SetActive(false);
        m_winningPanel.SetActive(false);

        switch (_gameplayCanvasMenu)
        {
            case GameplayCanvasMenu.FirstGameplayMenuPanel:
                m_firstGameplayMenuPanel.SetActive(true);
                break;
            case GameplayCanvasMenu.GameplayBoardPanel:
                m_gameplayBoardPanel.SetActive(true);
                m_closeGameObject = null;
                break;
            case GameplayCanvasMenu.AllPlayerInfoPanel:
                m_closeGameObject = m_allPlayerInfoPanel;
                m_gameplayBoardPanel.SetActive(true);
                m_allPlayerInfoPanel.SetActive(true);
                break;
            case GameplayCanvasMenu.PlayerCardsViewPanel:
                m_gameplayBoardPanel.SetActive(true);
                m_playerCardsViewPanel.SetActive(true);
                m_closeGameObject = m_playerCardsViewPanel;
                break;
            case GameplayCanvasMenu.BuyingFuelCardPanel:
                m_gameplayBoardPanel.SetActive(true);
                m_buyingFuelCardPanel.SetActive(true);
                m_closeGameObject = m_buyingFuelCardPanel;
                break;
            case GameplayCanvasMenu.PickCardPanel:
                m_gameplayBoardPanel.SetActive(true);
                m_pickCardPanel.SetActive(true);
                m_closeGameObject = null;
                break;
            case GameplayCanvasMenu.PlayedCardViewPanel:
                m_gameplayBoardPanel.SetActive(true);
                m_playedCardViewPanel.SetActive(true);
                m_playedCardAnimation.Play();
                m_closeGameObject = m_playedCardViewPanel;
                break;
            case GameplayCanvasMenu.WinningPanel:
                m_gameplayBoardPanel.SetActive(false);
                m_winningPanel.SetActive(true);
                m_closeGameObject = null;
                break;
            default:
                m_firstGameplayMenuPanel.SetActive(true);
                break;
        }

        if (m_closeGameObject != null)
        {
            m_closePanelViewButton.interactable = true;
            m_closePanelViewButton.gameObject.SetActive(true);
            EnableOrDisableAllUpperButton(false);
        }
        else
        {
            m_closePanelViewButton.interactable = false;
            m_closePanelViewButton.gameObject.SetActive(false);
            EnableOrDisableAllUpperButton(true);
        }
    }

    public void ShowRotatingPanel(bool _isTrue)
    {
        m_rotatingCircularView.gameObject.SetActive(_isTrue);
        m_distributeCardMenu.SetActive(!_isTrue);
        m_rotatingCircularView.SetText("Shuffling...");
    }

    public void EnableOrDisableAllUpperButton(bool _enable)
    {
        m_allPlayerInfoButton.interactable = _enable;
        m_showCardsButton.interactable = _enable;
        m_buyFuelCardButton.interactable = _enable;
    }

    public void ShowWiningGamePanel(List<Pawn> _winingPawns)
    {
        foreach (var _winingPlayerText in m_winingPlayerTexts)
        {
            DestroyImmediate(_winingPlayerText.gameObject);
        }

        m_winingPlayerTexts.Clear();
        if (_winingPawns.Count <= 0)
        {
            TextMeshProUGUI _winingText = Instantiate(m_winingPlayerNameTextPrefab, m_winingPlayerContainer);
            _winingText.text = "No player won";
            m_winingPlayerTexts.Add(_winingText);
        }
        else
        {
            foreach (var _winingPawn in _winingPawns)
            {
                TextMeshProUGUI _winingText = Instantiate(m_winingPlayerNameTextPrefab, m_winingPlayerContainer);
                _winingText.text = _winingPawn.m_PawnInfo.m_PlayerInfo.m_PlayerName + " won";
                m_winingPlayerTexts.Add(_winingText);
            }
        }
        ShowGameplayCanvasMenu(GameplayCanvasMenu.WinningPanel);
        EnableOrDisableAllUpperButton(false);
    }

    public void UpdateCollectedCoinsOrTreasures(bool _enableOrDiable, int _numberOfCoins = 0, int _numberOfTreasures = 0)
    {
        m_collectedCoinTreasureInfoPanel.SetActive(_enableOrDiable);
        m_collectedCoinTreasureText.text = "Total " + _numberOfCoins + " coin" + (_numberOfCoins > 1 ? "s" : "") + " and " + _numberOfTreasures + " treasure" + (_numberOfTreasures > 1 ? "s" : "") + " are collected";
    }
}