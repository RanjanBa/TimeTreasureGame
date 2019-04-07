#pragma warning disable 649
using System;
using UnityEngine;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Collections.Generic;
using TimeTreasure;

public class FirebaseRealtimeDatabase : MonoBehaviour
{
    private static readonly string r_gameName = "gameName";
    private static readonly string r_gameState = "gameState";
    private static readonly string r_remainingPowerHourCards = "remainingPowerHourCards";
    private static readonly string r_remainingTrapCards = "remainingTrapCards";
    private static readonly string r_resources = "resources";
    private static readonly string r_players = "players";
    private static readonly string r_playedCardInfo = "playedCardInfo";
    private static readonly string r_currentGMTTimeAt0 = "currentGMTTimeAt0";
    private static readonly string r_buyFuelCard = "buyFuelCard";


    //private static readonly string r_state = "state";
    public static readonly string r_playedCard = "playedCard";
    public static readonly string r_pickedCard = "pickedCard";
    public static readonly string r_cardName = "cardName";
    public static readonly string r_cardValue = "cardValue";
    public static readonly string r_cardType = "cardType";
    public static readonly string r_playingPlayerUID = "playingPlayerUID";
    public static readonly string r_usedPointToBuyFuelCard = "usedPointToBuyFuelCard";

    public static readonly string r_gameCreatorUID = "gameCreatorUID";
    public static readonly string r_displayName = "displayName";
    public static readonly string r_initialFuelCardNumber = "initialFuelCardNumber";
    public static readonly string r_cards = "cards";
    public static readonly string r_playerInstantiationPosition = "playerInstantiationPosition";

    public static FirebaseRealtimeDatabase m_Instance
    {
        get;
        private set;
    }

    DatabaseReference m_rootDatabaseRef;

    private EventHandler<ChildChangedEventArgs> m_onPlayerJoinedEventHandler;
    private EventHandler<ChildChangedEventArgs> m_onPlayerRemovedEventHandler;
    private EventHandler<ValueChangedEventArgs> m_onGameStateValueChangedEventHandler;
    private EventHandler<ValueChangedEventArgs> m_onPlayedCardValueChangedEventHandler;
    private EventHandler<ValueChangedEventArgs> m_onFuelCardValueChangedEventHandler;

    private string GetRootGamePath()
    {
        return "/games";
    }

    private string GetGamePath(string _creatorUID)
    {
        return GetRootGamePath() + "/" + _creatorUID;
    }

    private string GetJoinedPlayerPath(string _creatorUID)
    {
        return GetGamePath(_creatorUID) + "/joinedPlayers";
    }

    private string GetGameInfoPath(string _creatorUID)
    {
        return GetGamePath(_creatorUID) + "/gameInfo";
    }

    private void Awake()
    {
        if (m_Instance != null)
        {
            DestroyImmediate(this);
            return;
        }

        m_Instance = this;
        Firebase.FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://timetreasure-3303a.firebaseio.com/");
        m_rootDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        DontDestroyOnLoad(gameObject);
        DatabaseReference connectedRef = FirebaseDatabase.DefaultInstance.GetReference(".info/connected");
        connectedRef.ValueChanged += GameManager.m_Instance.OnInternetConnectionChanged;
    }

    public void RemoveCreatedGameFromDatabase(GameInfo _gameInfo)
    {
        if (_gameInfo != null && _gameInfo.m_CreatorUID != "")
        {
            ChangeGameStateOfDatabase(GameManager.m_Instance.m_GameInfo, GameState.GameCompleted);
            FirebaseDatabase.DefaultInstance.GetReference(GetGamePath(_gameInfo.m_CreatorUID)).RemoveValueAsync();
        }
    }

    public void AddNewUserToDatabase(string _name, string _email, string _uid)
    {
        Dictionary<string, object> user = new Dictionary<string, object>();

        user["/users/" + _uid + "/name"] = _name;
        user["/users/" + _uid + "/email"] = _email;

        m_rootDatabaseRef.UpdateChildrenAsync(user).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Create new user task is completed...");
            }
            else if (task.IsFaulted)
            {
                Debug.Log("Create new user task is faulted...");
            }
            else if (task.IsCanceled)
            {
                Debug.Log("Create new user task is canceled...");
            }
        });
    }

    public void GetAllCreatedGames()
    {
        if (AuthenticationManager.m_Instance.m_User == null)
        {
            Toast.m_Instance.ShowMessage("You have to login or create account first...", 5);
            return;
        }

        Toast.m_Instance.ShowMessageUntilinterrupt("Getting all created Games...");
        DatabaseReference rootGameDatabseRef = FirebaseDatabase.DefaultInstance.GetReference(GetRootGamePath());

        rootGameDatabseRef.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                List<GameInfo> _gameInfos = new List<GameInfo>();
                foreach (var childSnapshot in snapshot.Children)
                {
                    if (childSnapshot.Child(r_gameState).Value.ToString() == GameState.Joining.ToString())
                    {
                        string gameName = childSnapshot.Child(r_gameName).Value.ToString();
                        string creatorUID = childSnapshot.Key;
                        GameInfo game = new GameInfo(gameName, creatorUID);
                        _gameInfos.Add(game);
                    }
                }

                MenuController.m_Instance.UpdateAllCreatedGames(_gameInfos);
                MenuController.m_Instance.ShowMenuPage(MenuPage.JoinGamePage);
                Toast.m_Instance.ShowMessage("Getting all created Games completed...", 5);
            }
            else if (task.IsCanceled)
            {
                Toast.m_Instance.ShowMessage("Get created games is Cancelled");
            }
            else if (task.IsFaulted)
            {
                Toast.m_Instance.ShowMessage("Get created games is faulted");
            }
        });
    }

    public void GetAllJoinedPlayer(string _gameCreatorUID)
    {
        if (AuthenticationManager.m_Instance.m_User == null)
        {
            Toast.m_Instance.ShowMessage("You have to login or create account first...", 5);
            return;
        }

        Toast.m_Instance.ShowMessageUntilinterrupt("Getting all joined players...");
        FirebaseDatabase.DefaultInstance.GetReference(GetJoinedPlayerPath(_gameCreatorUID)).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Toast.m_Instance.ShowMessage("Getting all joined players completed");
                DataSnapshot snapshot = task.Result;

                List<PlayerInfo> _basicPlayerInfos = new List<PlayerInfo>();
                foreach (var _child in snapshot.Children)
                {
                    _basicPlayerInfos.Add(new PlayerInfo(_child.Child(r_displayName).Value.ToString(), _child.Key));
                }
                GameManager.m_Instance.m_JoinedPlayersInfo.Clear();
                foreach (var _playerInfo in _basicPlayerInfos)
                {
                    GameManager.m_Instance.m_JoinedPlayersInfo.Add(_playerInfo);
                    MenuController.m_Instance.UpdateJoinedPlayerInfo(GameManager.m_Instance.m_JoinedPlayersInfo);
                }
            }
            else
            {
                Toast.m_Instance.ShowMessage("Getting all joined players is canceled or faulted...");
            }
        });
    }

    public void CreateNewGame(string _gameName)
    {
        if (AuthenticationManager.m_Instance.m_User == null)
        {
            Toast.m_Instance.ShowMessage("You have to login or create account first...", 5);
            return;
        }

        string _creatorUID = AuthenticationManager.m_Instance.m_User.UserId;

        if (_creatorUID == "")
        {
            Toast.m_Instance.ShowMessage("Creator UID is null...", 5);
            return;
        }

        string _displayName = AuthenticationManager.m_Instance.m_User.DisplayName;
        Toast.m_Instance.ShowMessageUntilinterrupt("First removing prvious game...");
        FirebaseDatabase.DefaultInstance.GetReference(GetGamePath(_creatorUID)).RemoveValueAsync().ContinueWith((_task) =>
        {
            if (_task.IsCompleted)
            {
                Toast.m_Instance.ShowMessageUntilinterrupt("New Game Creating...");
                Dictionary<string, object> value = new Dictionary<string, object>();
                value[GetGameInfoPath(_creatorUID)] = "None";
                value[GetGamePath(_creatorUID) + "/" + r_gameName] = _gameName;
                value[GetGamePath(_creatorUID) + "/" + r_gameState] = GameState.None.ToString();
                value[GetJoinedPlayerPath(_creatorUID) + "/" + _creatorUID + "/" + r_displayName] = _displayName;
                value[GetJoinedPlayerPath(_creatorUID) + "/" + _creatorUID + "/" + r_gameCreatorUID] = _creatorUID;


                m_rootDatabaseRef.UpdateChildrenAsync(value).ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        Toast.m_Instance.ShowMessage("Create new game task is completed...", 5);
                        GameInfo _gameInfo = new GameInfo(_gameName, _creatorUID);
                        GameManager.m_Instance.m_GameInfo = _gameInfo;

                        SubscribeOnGameStateChangedEventHandler(GameManager.m_Instance.m_GameInfo);
                        SubscribeJoinOrRemovePlayerEventHandler(GameManager.m_Instance.m_GameInfo);
                        MenuController.m_Instance.ShowMenuPage(MenuPage.StartGameMenuPage);
                        ChangeGameStateOfDatabase(_gameInfo, GameState.Joining);
                    }
                    else if (task.IsFaulted)
                    {
                        Toast.m_Instance.ShowMessage("Create new game task is faulted...");
                    }
                    else if (task.IsCanceled)
                    {
                        Toast.m_Instance.ShowMessage("Create new game task is canceled...");
                    }
                });
            }
        });
    }

    public void JoinGame(GameInfo _gameInfo)
    {
        if (AuthenticationManager.m_Instance.m_User == null)
        {
            Toast.m_Instance.ShowMessage("You have to login or create account first...", 5);
            return;
        }

        Toast.m_Instance.ShowMessageUntilinterrupt("Joining at creator uid : " + _gameInfo.m_CreatorUID);
        DatabaseReference _joinedPlayersDatabaseRef = FirebaseDatabase.DefaultInstance.GetReference(GetJoinedPlayerPath(_gameInfo.m_CreatorUID));

        _joinedPlayersDatabaseRef.RunTransaction(mutableData =>
        {
            Dictionary<string, object> _joinedPlayers = mutableData.Value as Dictionary<string, object>;
            long count = mutableData.ChildrenCount;

            string _userUID = AuthenticationManager.m_Instance.m_User.UserId;
            string _displayName = AuthenticationManager.m_Instance.m_User.DisplayName;

            bool _isPlayerAlreadyJoined = false;

            if (_joinedPlayers == null)
            {
                _joinedPlayers = new Dictionary<string, object>();
            }
            else if (_joinedPlayers.Count < 4)
            {
                foreach (var _joinedPlayer in _joinedPlayers)
                {
                    if (_joinedPlayer.Key == _userUID)
                    {
                        _isPlayerAlreadyJoined = true;
                    }
                }
            }
            else if (mutableData.ChildrenCount >= 4)
            {
                Toast.m_Instance.ShowMessage("Joined aborted at " + _gameInfo + ", child count " + count, 10);
                return TransactionResult.Abort();
            }

            if (_isPlayerAlreadyJoined == false)
            {
                Dictionary<string, object> _newJoinedPlayer = new Dictionary<string, object>();
                _newJoinedPlayer.Add(r_displayName, _displayName);
                _newJoinedPlayer.Add(r_gameCreatorUID, _gameInfo.m_CreatorUID);
                _joinedPlayers.Add(_userUID, _newJoinedPlayer);
                mutableData.Value = _joinedPlayers;
                Toast.m_Instance.ShowMessage("Joined at " + _gameInfo + ", child count " + count, 10);
            }
            else
            {
                Toast.m_Instance.ShowMessage("Joined aborted because of already joined at " + _gameInfo + ", child count " + count, 10);
            }

            GameManager.m_Instance.m_GameInfo = _gameInfo;
            GameManager.m_Instance.m_MyGameState = GameState.Joining;
            SubscribeJoinOrRemovePlayerEventHandler(GameManager.m_Instance.m_GameInfo);
            SubscribeOnGameStateChangedEventHandler(GameManager.m_Instance.m_GameInfo);
            MenuController.m_Instance.ShowMenuPage(MenuPage.StartGameMenuPage);

            return TransactionResult.Success(mutableData);
        });
    }

    public void StartGame(GameInfo _gameInfo)
    {
        ChangeGameStateOfDatabase(_gameInfo, GameState.Start);
    }

    public void RemoveJoinedPlayer(PlayerInfo _playerInfo)
    {
        if (GameManager.m_Instance.m_GameInfo == null)
        {
            Toast.m_Instance.ShowMessage("You are not joined to any game. Just Restart the game...");
        }

        if (_playerInfo.m_PlayerUID == null || _playerInfo.m_PlayerUID == "")
        {
            Toast.m_Instance.ShowMessage("Player uid is null");
        }

        if (_playerInfo.m_PlayerUID == GameManager.m_Instance.m_GameInfo.m_CreatorUID)
        {
            RemoveCreatedGameFromDatabase(GameManager.m_Instance.m_GameInfo);
        }
        else
        {
            FirebaseDatabase.DefaultInstance.GetReference(GetJoinedPlayerPath(GameManager.m_Instance.m_GameInfo.m_CreatorUID) + "/" + _playerInfo.m_PlayerUID).RemoveValueAsync().ContinueWith(_task =>
            {
                if (_task.IsCompleted)
                {
                    Toast.m_Instance.ShowMessage(_playerInfo.m_PlayerUID + " is removed from database");
                }
                else
                {
                    Toast.m_Instance.ShowMessage(_playerInfo.m_PlayerUID + " can't remove from database");
                }
            });
        }
    }

    public void ChangeGameStateOfDatabase(GameInfo _gameInfo, GameState _state)
    {
        if (_gameInfo == null)
        {
            Toast.m_Instance.ShowMessage("Game info is null for Adding game info...", 10);
        }

        DatabaseReference _gameStateDatabase = FirebaseDatabase.DefaultInstance.GetReference(GetGamePath(_gameInfo.m_CreatorUID) + "/" + r_gameState);
        _gameStateDatabase.SetValueAsync(_state.ToString()).ContinueWith(_task =>
        {
            if (!_task.IsCompleted)
            {
                Toast.m_Instance.ShowMessage("GameState update can't be updated to true true");
            }
        });
    }

    public void UnSubscribeJoinOrRemovePlayerEventHandler(GameInfo _gameInfo)
    {
        if (_gameInfo != null)
        {
            if (m_onPlayerJoinedEventHandler != null)
            {
                FirebaseDatabase.DefaultInstance.GetReference(GetJoinedPlayerPath(_gameInfo.m_CreatorUID)).ChildAdded -= m_onPlayerJoinedEventHandler;
                m_onPlayerJoinedEventHandler = null;
            }

            if (m_onPlayerRemovedEventHandler != null)
            {
                FirebaseDatabase.DefaultInstance.GetReference(GetJoinedPlayerPath(_gameInfo.m_CreatorUID)).ChildRemoved -= m_onPlayerRemovedEventHandler;
                m_onPlayerRemovedEventHandler = null;
            }
        }
    }

    public void SubscribeJoinOrRemovePlayerEventHandler(GameInfo _gameInfo)
    {
        if (_gameInfo != null)
        {
            if (_gameInfo.m_CreatorUID != "")
            {
                m_onPlayerJoinedEventHandler = GameManager.m_Instance.OnPlayerJoined;
                m_onPlayerRemovedEventHandler = GameManager.m_Instance.OnPlayerRemoved;
                FirebaseDatabase.DefaultInstance.GetReference(GetJoinedPlayerPath(_gameInfo.m_CreatorUID)).ChildAdded += m_onPlayerJoinedEventHandler;
                FirebaseDatabase.DefaultInstance.GetReference(GetJoinedPlayerPath(_gameInfo.m_CreatorUID)).ChildRemoved += m_onPlayerRemovedEventHandler;
            }
        }
    }

    public void UnSubscribeOnGameStateChangedEventHandler(GameInfo _gameInfo)
    {
        if (_gameInfo != null)
        {
            if (m_onGameStateValueChangedEventHandler != null)
            {
                FirebaseDatabase.DefaultInstance.GetReference(GetGamePath(_gameInfo.m_CreatorUID) + "/" + r_gameState).ValueChanged += m_onGameStateValueChangedEventHandler;
                m_onGameStateValueChangedEventHandler = null;
            }
        }
    }

    public void SubscribeOnGameStateChangedEventHandler(GameInfo _gameInfo)
    {
        if (_gameInfo != null)
        {
            if (_gameInfo.m_CreatorUID != "")
            {
                m_onGameStateValueChangedEventHandler = GameManager.m_Instance.OnGameStateValueChanged;
                FirebaseDatabase.DefaultInstance.GetReference(GetGamePath(_gameInfo.m_CreatorUID) + "/" + r_gameState).ValueChanged += m_onGameStateValueChangedEventHandler;
            }
        }
    }

    public void UnsubscribeOnPlayedCardValueChanged(GameInfo _gameInfo)
    {
        if (_gameInfo != null)
        {
            if (m_onPlayedCardValueChangedEventHandler != null)
            {
                m_onPlayedCardValueChangedEventHandler = GameManager.m_Instance.OnCardPlayed;
                FirebaseDatabase.DefaultInstance.GetReference(GetGameInfoPath(_gameInfo.m_CreatorUID) + "/" + r_playedCardInfo).ValueChanged -= m_onPlayedCardValueChangedEventHandler;
            }
        }
    }

    public void SubscribeOnPlayedCardValueChanged(GameInfo _gameInfo)
    {
        if (_gameInfo != null)
        {
            if (_gameInfo.m_CreatorUID != "")
            {
                m_onPlayedCardValueChangedEventHandler = GameManager.m_Instance.OnCardPlayed;
                FirebaseDatabase.DefaultInstance.GetReference(GetGameInfoPath(_gameInfo.m_CreatorUID) + "/" + r_playedCardInfo).ValueChanged += m_onPlayedCardValueChangedEventHandler;
            }
        }
    }

    public void UnSubscribeBuyFuelCard(GameInfo _gameInfo)
    {
        if (_gameInfo == null)
            return;

        if (m_onFuelCardValueChangedEventHandler != null)
        {
            m_onFuelCardValueChangedEventHandler = GameManager.m_Instance.OnBuyFuelCard;
            FirebaseDatabase.DefaultInstance.GetReference(GetGameInfoPath(_gameInfo.m_CreatorUID) + "/" + r_buyFuelCard).ValueChanged -= m_onFuelCardValueChangedEventHandler;
        }
    }

    public void SubscribeBuyFuelCard(GameInfo _gameInfo)
    {
        if (_gameInfo == null)
            return;

        if (_gameInfo.m_CreatorUID != "")
        {
            m_onFuelCardValueChangedEventHandler = GameManager.m_Instance.OnBuyFuelCard;
            FirebaseDatabase.DefaultInstance.GetReference(GetGameInfoPath(_gameInfo.m_CreatorUID) + "/" + r_buyFuelCard).ValueChanged += m_onFuelCardValueChangedEventHandler;
        }
    }

    public void AddGameInfoToDatabase(
        GameInfo _gameInfo,
        List<Card> _remainingPowerHourCards,
        List<TrapCard> _remainingTrapCards,
        Dictionary<string, List<int>> _shuffleAllCoinAndTreasurePositions,
        List<int> _playerInstantiationPositions,
        Dictionary<string, Dictionary<string, string>> _playerUIDWithCards,
        int _currentGMTTimeAt0
        )
    {
        if (_gameInfo == null)
        {
            Toast.m_Instance.ShowMessage("Game info is null for Adding game info...", 10);
        }

        ChangeGameStateOfDatabase(GameManager.m_Instance.m_GameInfo, GameState.CardUploadingStart);

        Toast.m_Instance.ShowMessageUntilinterrupt("Distributing cards...");
        DatabaseReference _gameInfoDatabaseRef = FirebaseDatabase.DefaultInstance.GetReference(GetGameInfoPath(_gameInfo.m_CreatorUID));

        Dictionary<string, object> _keyValuePairs = new Dictionary<string, object>();
        _keyValuePairs["/" + r_playedCardInfo + "/" + r_gameCreatorUID] = _gameInfo.m_CreatorUID;
        _keyValuePairs["/" + r_buyFuelCard] = "None";
        _keyValuePairs["/" + r_currentGMTTimeAt0] = _currentGMTTimeAt0;
        for (int i = 0; i < _remainingPowerHourCards.Count; i++)
        {
            _keyValuePairs["/" + r_remainingPowerHourCards + "/" + _remainingPowerHourCards[i].name] = _remainingPowerHourCards[i].GetType().ToString();
        }

        for (int i = 0; i < _remainingTrapCards.Count; i++)
        {
            _keyValuePairs["/" + r_remainingTrapCards + "/" + _remainingTrapCards[i].name] = _remainingTrapCards[i].GetType().ToString();
        }

        List<int> tempListInt = _shuffleAllCoinAndTreasurePositions[ResourceManager.r_CoinKey];
        for (int i = 0; i < tempListInt.Count; i++)
        {
            _keyValuePairs["/" + r_resources + "/" + ResourceManager.r_CoinKey + "/" + i.ToString()] = tempListInt[i];
        }

        tempListInt = _shuffleAllCoinAndTreasurePositions[ResourceManager.r_TreasureKey];

        for (int i = 0; i < tempListInt.Count; i++)
        {
            _keyValuePairs["/" + r_resources + "/" + ResourceManager.r_TreasureKey + "/" + i.ToString()] = tempListInt[i];
        }

        int n = 0;
        foreach (var _player in _playerUIDWithCards)
        {
            foreach (var _card in _player.Value)
            {
                _keyValuePairs["/" + r_players + "/" + _player.Key + "/" + r_cards + "/" + _card.Key] = _card.Value;
            }
            if (n < _playerInstantiationPositions.Count)
            {
                _keyValuePairs["/" + r_players + "/" + _player.Key + "/" + r_playerInstantiationPosition] = _playerInstantiationPositions[n];
                n++;
            }

            if (GameplayManager.m_Instance != null)
            {
                _keyValuePairs["/" + r_players + "/" + _player.Key + "/" + r_initialFuelCardNumber] = GameplayManager.m_Instance.m_InitialNumberOfFuelCards;
            }
            else
            {
                _keyValuePairs["/" + r_players + "/" + _player.Key + "/" + r_initialFuelCardNumber] = 10;
            }
        }

        _gameInfoDatabaseRef.UpdateChildrenAsync(_keyValuePairs).ContinueWith(_task =>
        {
            if (_task.IsCompleted)
            {
                Toast.m_Instance.ShowMessageUntilinterrupt("Distributing cards completed...");
                ChangeGameStateOfDatabase(_gameInfo, GameState.CardUploadingCompleted);
            }
            else
            {
                ChangeGameStateOfDatabase(_gameInfo, GameState.None);
                Toast.m_Instance.ShowMessage("Can't add remaining cards to database");
            }
        });
    }

    public void GetGameInfoFromDatabase(GameInfo _gameInfo)
    {
        if (_gameInfo == null)
        {
            Toast.m_Instance.ShowMessage("Game info is null for Adding game info...", 10);
            return;
        }

        if(_gameInfo.m_CreatorUID == "")
        {
            Toast.m_Instance.ShowMessage("Game info creator uid is null for Adding game info...", 10);
            return;
        }

        Toast.m_Instance.ShowMessageUntilinterrupt("Getting game info from database");
        UnsubscribeOnPlayedCardValueChanged(_gameInfo);
        SubscribeOnPlayedCardValueChanged(_gameInfo);
        UnSubscribeBuyFuelCard(_gameInfo);
        SubscribeBuyFuelCard(_gameInfo);
        if (GameManager.m_Instance.m_OwnerInfo.m_PlayerUID == GameManager.m_Instance.m_GameInfo.m_CreatorUID)
        {
            ChangeGameStateOfDatabase(_gameInfo, GameState.CardDistributingStarted);
        }
        DatabaseReference _gameInfoDatabaseRef = FirebaseDatabase.DefaultInstance.GetReference(GetGameInfoPath(_gameInfo.m_CreatorUID));
        _gameInfoDatabaseRef.GetValueAsync().ContinueWith(_task =>
        {
            if (_task.IsCompleted)
            {
                if (_task.Result.Exists)
                {
                    DataSnapshot _dataSnapshot = _task.Result;
                    Dictionary<string, object> _remainingPowerHourCardsDict = _dataSnapshot.Child(r_remainingPowerHourCards).Value as Dictionary<string, object>;
                    Dictionary<string, object> _remainingTrapCardsDict = _dataSnapshot.Child(r_remainingTrapCards).Value as Dictionary<string, object>;
                    Dictionary<string, object> _shuffleAllCoinAndTreasurePositionsDict = _dataSnapshot.Child(r_resources).Value as Dictionary<string, object>;
                    Dictionary<string, object> _playerUIDWithInstantiationPositionAndCardsDict = _dataSnapshot.Child(r_players).Value as Dictionary<string, object>;
                    object _currentGMTTimeAt0Object = _dataSnapshot.Child(r_currentGMTTimeAt0).Value;
                    if (GameplayManager.m_Instance != null)
                    {
                        GameplayManager.m_Instance.DecodeAllShuffleCardsAndPlayerInstantiationPositions(
                            _remainingPowerHourCardsDict,
                            _remainingTrapCardsDict,
                            _shuffleAllCoinAndTreasurePositionsDict,
                            _playerUIDWithInstantiationPositionAndCardsDict,
                            _currentGMTTimeAt0Object
                            );
                    }
                    if (GameManager.m_Instance.m_OwnerInfo.m_PlayerUID == GameManager.m_Instance.m_GameInfo.m_CreatorUID)
                    {
                        ChangeGameStateOfDatabase(_gameInfo, GameState.CardDistributingCompleted);
                    }
                }
                else
                {
                    Toast.m_Instance.ShowMessage("value doest not exist...", 5);
                }
            }
        });
    }

    public void UpdatePlayedCard(string _ownerUID, Card _playedCard, int _playedCardValue, Card _pickedCard)
    {
        if (GameManager.m_Instance.m_GameInfo == null)
        {
            Toast.m_Instance.ShowMessage("Update game info is null");
        }

        Dictionary<string, object> _values = new Dictionary<string, object>();

        //_values["/" + r_state] = "played";
        _values["/" + r_playingPlayerUID] = _ownerUID;
        _values["/" + r_playedCard + "/" + r_cardName] = _playedCard.name;
        _values["/" + r_playedCard + "/" + r_cardType] = _playedCard.GetType().ToString();
        _values["/" + r_playedCard + "/" + r_cardValue] = _playedCardValue;

        if (_playedCard.GetType() == typeof(FuelCard))
        {
            _values["/" + r_pickedCard] = "None";
        }
        else if (_pickedCard != null)
        {
            _values["/" + r_pickedCard + "/" + r_cardName] = _pickedCard.name;
            _values["/" + r_pickedCard + "/" + r_cardType] = _pickedCard.GetType().ToString();

        }
        Toast.m_Instance.ShowMessageUntilinterrupt("Updating playing card and picked card. Is connected to internet " + GameManager.m_Instance.IsConnectedToInternet);
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.GetReference(GetGameInfoPath(GameManager.m_Instance.m_GameInfo.m_CreatorUID) + "/" + r_playedCardInfo);
        _ref.UpdateChildrenAsync(_values).ContinueWith(_task =>
        {
            if (!_task.IsCompleted)
            {
                Toast.m_Instance.ShowMessage("Can't update playing card and picked card completed");
            }
            else
            {
                Toast.m_Instance.ShowMessage("Updating playing card and picked card completed");
            }
        });
    }

    public void UpdateFuelCardOfPlayer(GameInfo _gameInfo, string _playerUID, int _usedPointToBuyFuel)
    {
        if (_gameInfo == null) return;

        if (_gameInfo.m_CreatorUID == "")
            return;

        Dictionary<string, object> _values = new Dictionary<string, object>();
        _values.Add(r_gameCreatorUID, _gameInfo.m_CreatorUID);
        _values.Add(r_playingPlayerUID, _playerUID);
        _values.Add(r_usedPointToBuyFuelCard, _usedPointToBuyFuel);

        DatabaseReference _buyFuelCardDatabaseRef = FirebaseDatabase.DefaultInstance.GetReference(GetGameInfoPath(_gameInfo.m_CreatorUID) + "/" + r_buyFuelCard);

        _buyFuelCardDatabaseRef.UpdateChildrenAsync(_values);
    }
}
