using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using UnityEngine.SceneManagement;
using TimeTreasure;

public enum GameType
{
    None,
    Offline,
    Online,
}

public enum GameState
{
    None,
    Joining,
    Start,
    CardUploadingStart,
    CardUploadingCompleted,
    CardDistributingStarted,
    CardDistributingCompleted,
    UpdateShufflePowerHourCardStart,
    UpdateShufflePowerHourCardCompleted,
    UpdateShuffleTrapCardStart,
    UpdateShuffleTrapCardCompleted,
    GamePlaying,
    GameCompleted,
}

public class GameManager : MonoBehaviour
{
    public static GameManager m_Instance
    {
        get;
        private set;
    }

    public GameType m_GameType { get; set; }
    public GameInfo m_GameInfo { get; set; }
    public GameState m_MyGameState { get; set; }
    public List<PlayerInfo> m_JoinedPlayersInfo = new List<PlayerInfo>();

    public bool IsConnectedToInternet { get; private set; }

    public PlayerInfo m_OwnerInfo;

    private void Awake()
    {
        if(m_Instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }

        m_Instance = this;
        DontDestroyOnLoad(gameObject);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    public void OnInternetConnectionChanged(object sender, ValueChangedEventArgs a)
    {
        bool isConnected = (bool)a.Snapshot.Value;
        Toast.m_Instance.ShowMessage("isConnected " + isConnected, 10);
        IsConnectedToInternet = isConnected;
    }

    public void OnGameStateValueChanged(object _sender, ValueChangedEventArgs _valueChangedEventArgs)
    {
        Toast.m_Instance.ShowMessage("Game state changed");
        if (_valueChangedEventArgs.DatabaseError != null)
        {
            Toast.m_Instance.ShowMessage("On Game state value changed error : " + _valueChangedEventArgs.DatabaseError.Details);
            return;
        }

        if (_valueChangedEventArgs.Snapshot.Exists)
        {
            DataSnapshot _dataSnapshot = _valueChangedEventArgs.Snapshot;
            //Toast.m_Instance.ShowMessage("OnGameState changed _data value : " + _dataSnapshot.Value + " and GameplayManager instance is not null...");
            string _value = _dataSnapshot.Value.ToString();
            if (_value == GameState.None.ToString())
            {
                m_MyGameState = GameState.None;
            }
            else if (_value == GameState.Joining.ToString())
            {
                m_MyGameState = GameState.Joining;
            }
            if (_value == GameState.Start.ToString())
            {
                m_MyGameState = GameState.Start;
                FirebaseRealtimeDatabase.m_Instance.UnSubscribeJoinOrRemovePlayerEventHandler(m_GameInfo);
                SceneManager.LoadScene(1);
            }
            else if (_value == GameState.CardUploadingStart.ToString())
            {
                m_MyGameState = GameState.CardUploadingStart;
                if (GameplayCanvasManager.m_Instance != null)
                {
                    GameplayCanvasManager.m_Instance.ShowRotatingPanel(true);
                }
            }
            else if (_value == GameState.CardUploadingCompleted.ToString())
            {
                if (m_MyGameState != GameState.CardUploadingCompleted)
                {
                    m_MyGameState = GameState.CardUploadingCompleted;
                    if (GameplayManager.m_Instance != null)
                    {
                        GameplayCanvasManager.m_Instance.ShowRotatingPanel(false);
                        if (FirebaseRealtimeDatabase.m_Instance != null)
                        {
                            FirebaseRealtimeDatabase.m_Instance.GetGameInfoFromDatabase(m_GameInfo);
                        }
                    }
                }
            }
            else if (_value == GameState.CardDistributingStarted.ToString())
            {
                m_MyGameState = GameState.CardDistributingStarted;
            }
            else if (_value == GameState.CardDistributingCompleted.ToString())
            {
                if (m_OwnerInfo.m_PlayerUID == m_GameInfo.m_CreatorUID)
                {
                    FirebaseRealtimeDatabase.m_Instance.ChangeGameStateOfDatabase(m_GameInfo, GameState.GamePlaying);
                }
                m_MyGameState = GameState.CardDistributingCompleted;
            }
            else if (_value == GameState.GamePlaying.ToString())
            {
                m_MyGameState = GameState.GamePlaying;
            }
            else if (_value == GameState.GameCompleted.ToString())
            {
                m_MyGameState = GameState.GameCompleted;
                FirebaseRealtimeDatabase.m_Instance.UnSubscribeBuyFuelCard(m_GameInfo);
                FirebaseRealtimeDatabase.m_Instance.UnsubscribeOnPlayedCardValueChanged(m_GameInfo);
                Toast.m_Instance.ShowMessage("Loading Menu Scene...");
                SceneManager.LoadScene(0);
            }
        }
    }

    public void OnPlayerJoined(object _sender, ChildChangedEventArgs _childChangedEventArgs)
    {
        if (_childChangedEventArgs.DatabaseError != null)
        {
            Toast.m_Instance.ShowMessage("JoinedPlayer child added error : " + _childChangedEventArgs.DatabaseError.ToString());
            return;
        }

        if (MenuController.m_Instance != null)
        {
            if (m_GameInfo.m_CreatorUID == _childChangedEventArgs.Snapshot.Child(FirebaseRealtimeDatabase.r_gameCreatorUID).Value.ToString())
            {
                PlayerInfo _playerInfo = new PlayerInfo(
                _childChangedEventArgs.Snapshot.Child(FirebaseRealtimeDatabase.r_displayName).Value.ToString(),
                _childChangedEventArgs.Snapshot.Key);
                MenuController.m_Instance.OnPlayerJoined(_playerInfo);
            }
            else
            {
                Toast.m_Instance.ShowMessage("Game creator uid is not equal to " + _childChangedEventArgs.Snapshot.Child(FirebaseRealtimeDatabase.r_gameCreatorUID).Value.ToString());
            }
        }
    }

    public void OnPlayerRemoved(object _sender, ChildChangedEventArgs _childChangedEventArgs)
    {
        if (_childChangedEventArgs.DatabaseError != null)
        {
            Toast.m_Instance.ShowMessage("JoinedPlayer child removed error : " + _childChangedEventArgs.DatabaseError.ToString());
            return;
        }

        if (MenuController.m_Instance != null && m_MyGameState == GameState.Joining)
        {
            if (m_GameInfo.m_CreatorUID == _childChangedEventArgs.Snapshot.Child(FirebaseRealtimeDatabase.r_gameCreatorUID).Value.ToString())
            {
                PlayerInfo _playerInfo = new PlayerInfo(
                    _childChangedEventArgs.Snapshot.Child(FirebaseRealtimeDatabase.r_displayName).Value.ToString(),
                    _childChangedEventArgs.Snapshot.Key);
                MenuController.m_Instance.OnPlayerRemoved(_playerInfo);
            }
            else
            {
                Toast.m_Instance.ShowMessage("Game creator uid is not equal to " + _childChangedEventArgs.Snapshot.Child(FirebaseRealtimeDatabase.r_gameCreatorUID).Value.ToString());
            }
        }
    }

    public void OnCardPlayed(object _sender, ValueChangedEventArgs _valueChangedEventArgs)
    {
        if(_valueChangedEventArgs.DatabaseError != null)
        {
            Toast.m_Instance.ShowMessage("On Card played is error " + _valueChangedEventArgs.DatabaseError.Details);
            return;
        }

        if(_valueChangedEventArgs.Snapshot.Exists)
        {
            DataSnapshot _dataSnapshot = _valueChangedEventArgs.Snapshot;

            if (m_GameInfo.m_CreatorUID == _dataSnapshot.Child(FirebaseRealtimeDatabase.r_gameCreatorUID).Value.ToString())
            {
                if (GameplayManager.m_Instance != null)
                {
                    string _playerUID = _dataSnapshot.Child(FirebaseRealtimeDatabase.r_playingPlayerUID).Value.ToString();

                    if(GameplayManager.m_Instance.m_PawnsDict.ContainsKey(_playerUID))
                    {
                        Pawn _owner = GameplayManager.m_Instance.m_PawnsDict[_playerUID];

                        string _playedCardName = _dataSnapshot.Child(FirebaseRealtimeDatabase.r_playedCard).Child(FirebaseRealtimeDatabase.r_cardName).Value.ToString();
                        string _playedCardType = _dataSnapshot.Child(FirebaseRealtimeDatabase.r_playedCard).Child(FirebaseRealtimeDatabase.r_cardType).Value.ToString();

                        if(_playedCardType != typeof(FuelCard).ToString())
                        {
                            string _pickedCardName = _dataSnapshot.Child(FirebaseRealtimeDatabase.r_pickedCard).Child(FirebaseRealtimeDatabase.r_cardName).Value.ToString();
                            string _pickedCardType = _dataSnapshot.Child(FirebaseRealtimeDatabase.r_pickedCard).Child(FirebaseRealtimeDatabase.r_cardType).Value.ToString();

                            if (_playedCardType == typeof(TrapCard).ToString())
                            {
                                Card _playedCard = CardManager.m_Instance.GetTrapCardByName(_playedCardName);
                                Card _pickedCard = CardManager.m_Instance.GetTrapCardByName(_pickedCardName);

                                if(_playedCard != null && _pickedCard != null)
                                {
                                    GameplayManager.m_Instance.OnPlayedCardFromDatabse(_owner, _playedCard, _pickedCard);
                                }
                            }
                            else
                            {
                                Card _playedCard = CardManager.m_Instance.GetPowerOrHourCardByName(_playedCardName);
                                Card _pickedCard = CardManager.m_Instance.GetPowerOrHourCardByName(_pickedCardName);

                                if (_playedCard != null && _pickedCard != null)
                                {
                                    if (_playedCard.GetType() == typeof(PowerCard))
                                    {
                                        if(((PowerCard)_playedCard).m_powerType == PowerTypes.GMTMaster || ((PowerCard)_playedCard).m_powerType == PowerTypes.Master)
                                        {
                                            string _value = _dataSnapshot.Child(FirebaseRealtimeDatabase.r_playedCard).Child(FirebaseRealtimeDatabase.r_cardValue).Value.ToString();
                                            int _result = 0;
                                            if (int.TryParse(_value, out _result))
                                            {
                                                GameplayManager.m_Instance.OnPlayedCardFromDatabse(_owner, _playedCard, _pickedCard, _result);
                                            }
                                        }
                                        else
                                        {
                                            GameplayManager.m_Instance.OnPlayedCardFromDatabse(_owner, _playedCard, _pickedCard);
                                        }
                                    }
                                    else
                                    {
                                        GameplayManager.m_Instance.OnPlayedCardFromDatabse(_owner, _playedCard, _pickedCard);
                                    }
                                }
                            }
                        }
                        else
                        {
                            string _value = _dataSnapshot.Child(FirebaseRealtimeDatabase.r_playedCard).Child(FirebaseRealtimeDatabase.r_cardValue).Value.ToString();

                            int _result = 0;
                            if(int.TryParse(_value, out _result))
                            {
                                FuelCard _fuelCard = CardManager.m_Instance.m_FuelCard;
                                GameplayManager.m_Instance.OnFuelCardPlayedFromDatabase(_owner, _fuelCard, _result);
                            }
                        }
                    }

                }
            }
        }
    }

    public void OnBuyFuelCard(object _sender, ValueChangedEventArgs _valueChangedEventArgs)
    {
        if(_valueChangedEventArgs.DatabaseError != null)
        {
            Toast.m_Instance.ShowMessage("On Buy fuel card is error..." + _valueChangedEventArgs.DatabaseError.Details);
            return;
        }

        if(_valueChangedEventArgs.Snapshot.Exists)
        {
            if(_valueChangedEventArgs.Snapshot.Child(FirebaseRealtimeDatabase.r_gameCreatorUID).Value.ToString() == m_GameInfo.m_CreatorUID)
            {
                string _playerUID = _valueChangedEventArgs.Snapshot.Child(FirebaseRealtimeDatabase.r_playingPlayerUID).Value.ToString();
                string _value = _valueChangedEventArgs.Snapshot.Child(FirebaseRealtimeDatabase.r_usedPointToBuyFuelCard).Value.ToString();
                int _result = 0;
                if (int.TryParse(_value, out _result))
                {
                    if (GameplayManager.m_Instance.m_PawnsDict.ContainsKey(_playerUID))
                    {
                        Pawn _pawn = GameplayManager.m_Instance.m_PawnsDict[_playerUID];
                        GameplayManager.m_Instance.UpdateFuelCardFromDatabase(_pawn, _result);
                    }
                }
            }
        }
    }

    public void ResetGame()
    {
        if (m_GameType == GameType.Online)
        {
            if (FirebaseRealtimeDatabase.m_Instance != null && m_GameInfo != null)
            {
                FirebaseRealtimeDatabase.m_Instance.UnSubscribeJoinOrRemovePlayerEventHandler(m_GameInfo);
                FirebaseRealtimeDatabase.m_Instance.UnSubscribeOnGameStateChangedEventHandler(m_GameInfo);
                FirebaseRealtimeDatabase.m_Instance.UnsubscribeOnPlayedCardValueChanged(m_GameInfo);
            }

            if (m_OwnerInfo != null && m_GameInfo != null && FirebaseRealtimeDatabase.m_Instance != null)
            {
                if (m_OwnerInfo.m_PlayerUID == m_GameInfo.m_CreatorUID)
                {
                    FirebaseRealtimeDatabase.m_Instance.RemoveCreatedGameFromDatabase(m_GameInfo);
                }
                else
                {
                    FirebaseRealtimeDatabase.m_Instance.RemoveJoinedPlayer(m_OwnerInfo);
                }
            }
            else
            {
                if (Toast.m_Instance != null)
                {
                    Toast.m_Instance.ShowMessage("Owner info is null...");
                }
            }
        }
        else
        {
            Toast.m_Instance.ShowMessage("Game type : " + m_GameType.ToString());
        }

        m_GameType = GameType.None;
        m_MyGameState = GameState.None;
        m_GameInfo = null;
        if (m_JoinedPlayersInfo != null)
        {
            m_JoinedPlayersInfo.Clear();
        }
        else
        {
            m_JoinedPlayersInfo = new List<PlayerInfo>();
        }
    }
}
