using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public enum PlayedCardState
{
    None,
    PlayedCard,
    PickCard,
    Next,
    Playing,
}

// This will manage the game play 
public class GameplayManager : MonoBehaviour
{
    public static GameplayManager m_Instance { get; private set; }
    public static readonly int r_TotalLongitude = 24;
    public static readonly int r_TotalLatitude = 11;
    public static readonly int r_numberOfFuelCardPerPoint = 5;
    private static readonly int r_numberOfPowerHourCardInHand = 5;
    private static readonly int r_numberOfTrapCardInHand = 2;

    private static int m_powerCardGMTOrLongValue = 0;
    [SerializeField]
    private bool m_showTimesBelowGMT = false;
    [SerializeField]
    private int m_initialNumberOfFuelCards = 10;
    [SerializeField]
    private GameObject m_circularPointPrefab, m_gmtPointPrefab;
    [SerializeField]
    private Transform m_boardPointsContainer, m_boardGMTContainer, m_boardTimesContainer;
    [Header("References to Player : ")]
    [SerializeField]
    private Pawn m_playerPrefabs;
    [SerializeField]
    private PickCardView m_pickCardView;
    [SerializeField]
    private ChoosePointsView m_choosePointsView;
    [SerializeField]
    private FrontCardView m_playedCardFrontView;
    [SerializeField]
    private TextMeshProUGUI m_cardPlayedPlayerNameText;

    private int m_currentPlayerIndex = 0;
    private string m_currentPlayerUID = "";
    private List<PawnInfo> m_allPawnInfos = new List<PawnInfo>();
    private Dictionary<string, Pawn> m_pawns = new Dictionary<string, Pawn>();
    private Dictionary<int, int> m_currentGMTofIndices = new Dictionary<int, int>(); //GMT time - index
    private List<TextMeshProUGUI> m_timesText = new List<TextMeshProUGUI>();

    public ChoosePointsView m_ChoosePointsView { get { return m_choosePointsView; } }
    public int m_InitialNumberOfFuelCards { get { return m_initialNumberOfFuelCards; } }
    public RectTransform[,] m_BoardGMTPoints { get; private set; }
    public Dictionary<int, int> m_CurrentGMTofIndices { get { return m_currentGMTofIndices; } }
    public Dictionary<string, Pawn> m_PawnsDict { get { return m_pawns; } }
    public Pawn m_CurrentPawn {
        get
        {
            if (m_pawns.ContainsKey(m_currentPlayerUID))
                return m_pawns[m_currentPlayerUID];

            Toast.m_Instance.ShowMessage("Pawns dict contains doesn't contain currentPlayer uid");
            return null;
        }
    }
    private int m_currentTimeOfGMT0;

    public int m_CurrentTimeOfGMT0
    {
        get
        {
            return m_currentTimeOfGMT0;
        }
        set
        {
            if(value < 1 || value > 24)
            {
                return;
            }

            m_currentTimeOfGMT0 = value;
            UpdateCurrentTimeOfGMTIndices(m_currentTimeOfGMT0);
            GameplayCanvasManager.m_Instance.UpdateCurrentTimeOfGMT0(m_currentTimeOfGMT0);
        }
    }
    public Pawn m_MyPawn { get; private set; }

    private void Awake()
    {
        if (m_Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        m_Instance = this;
        m_BoardGMTPoints = new RectTransform[r_TotalLatitude, r_TotalLongitude];
    }

    private void Start()
    {
        InstantiateBoardGMTPoints();
        InstantiateTimesOfGMT();
        InstantiateGMTTextPoints();
    }

    public bool IsMyTurn(PlayerInfo _playerInfo)
    {
        if (GameManager.m_Instance.m_GameType == GameType.Offline)
        {
            if (_playerInfo.m_PlayerName == m_CurrentPawn.m_PawnInfo.m_PlayerInfo.m_PlayerName)
            {
                return true;
            }
        }
        else if(GameManager.m_Instance.m_GameType == GameType.Online)
        {
            if (_playerInfo.m_PlayerUID == m_CurrentPawn.m_PawnInfo.m_PlayerInfo.m_PlayerUID)
            {
                return true;
            }
        }

        return false;
    }

    private void InstantiateBoardGMTPoints()
    {
        for (int i = 0; i < r_TotalLatitude; i++)
        {
            for (int j = 0; j < r_TotalLongitude; j++)
            {
                GameObject gm = Instantiate(m_circularPointPrefab, m_boardPointsContainer);
                m_BoardGMTPoints[i, j] = gm.GetComponent<RectTransform>();
            }
        }
    }

    private void InstantiateGMTTextPoints()
    {
        for (int i = 0; i < r_TotalLongitude; i++)
        {
            GameObject gm = Instantiate(m_gmtPointPrefab, m_boardGMTContainer);
            TextMeshProUGUI textPro = gm.GetComponentInChildren<TextMeshProUGUI>();
            int time = i - 11;
            textPro.text = time + "";
        }
    }

    private void InstantiateTimesOfGMT()
    {
        for (int i = 0; i < r_TotalLongitude; i++)
        {
            GameObject gm = Instantiate(m_gmtPointPrefab, m_boardTimesContainer);
            TextMeshProUGUI textPro = gm.GetComponentInChildren<TextMeshProUGUI>();
            m_timesText.Add(textPro);
        }
    }

    private void InstantiatePlayers(List<PawnInfo> _pawnInfos)
    {
        for (int i = 0; i < _pawnInfos.Count; i++)
        {
            Pawn _pawn = Instantiate(m_playerPrefabs, m_BoardGMTPoints[0, _pawnInfos[i].m_InstantiationPosition.y]) as Pawn;
            _pawn.m_PawnInfo = _pawnInfos[i];
            _pawn.Move(_pawnInfos[i].m_InstantiationPosition);
            if (_pawn.m_PawnNameTextPro == null)
                Debug.LogWarning("Player text mesh pro is null");
            else
                _pawn.m_PawnNameTextPro.text = "" + _pawnInfos[i].m_PlayerInfo.m_PlayerName[0];

            if (GameManager.m_Instance.m_GameType == GameType.Offline)
            {
                m_pawns.Add(_pawnInfos[i].m_PlayerInfo.m_PlayerName, _pawn);
            }
            else if(GameManager.m_Instance.m_GameType == GameType.Online)
            {
                if(AuthenticationManager.m_Instance.m_User.UserId == _pawnInfos[i].m_PlayerInfo.m_PlayerUID)
                {
                    m_MyPawn = _pawn;
                }
                m_pawns.Add(_pawnInfos[i].m_PlayerInfo.m_PlayerUID, _pawn);
            }
        }
    }

    private List<int> GetPlayerInstantiationPositions()
    {
        var _randGen = new System.Random();
        var _values = Enumerable.Range(0, r_TotalLongitude).OrderBy(x => _randGen.Next()).ToArray();

        List<int> _playerInstantiationPositions = new List<int>();

        for (int i = 0; i < GameManager.m_Instance.m_JoinedPlayersInfo.Count; i++)
        {
            _playerInstantiationPositions.Add(_values[i]);
        }

        return _playerInstantiationPositions;
    }

    private void UpdateCurrentTimeOfGMTIndices(int _currentTimeOfGMT0)
    {
        m_currentGMTofIndices.Clear();

        for (int i = 0; i < r_TotalLongitude; i++)
        {
            int _time = (r_TotalLongitude + _currentTimeOfGMT0 + i - r_TotalLatitude) % r_TotalLongitude;
            _time = _time == 0 ? 24 : _time;
            m_currentGMTofIndices.Add(_time, i);
        }

        foreach (var time in m_currentGMTofIndices)
        {
            m_timesText[time.Value].text = time.Key + "";
        }
    }

    public void UpdateToNextPlayer()
    {
        List<Pawn> _winingPawns = new List<Pawn>();
        if(CheckForGameCompletion(out _winingPawns))
        {
            Debug.Log("Game play win");
            GameplayCanvasManager.m_Instance.ShowWiningGamePanel(_winingPawns);
            return;
        }

        if (m_currentPlayerIndex == m_allPawnInfos.Count - 1)
        {
            m_currentTimeOfGMT0 = m_currentTimeOfGMT0 + 1;
            if (m_currentTimeOfGMT0 != 24)
            {
                m_CurrentTimeOfGMT0 = m_currentTimeOfGMT0 % 24;
            }
            else
            {
                m_CurrentTimeOfGMT0 = m_currentTimeOfGMT0;
            }
        }
        Debug.Log("current time" + m_currentTimeOfGMT0);
        m_currentPlayerIndex = (m_currentPlayerIndex + 1) % m_allPawnInfos.Count;

        if (GameManager.m_Instance.m_GameType == GameType.Offline)
        {
            m_currentPlayerUID = m_allPawnInfos[m_currentPlayerIndex].m_PlayerInfo.m_PlayerName;
            GameplayCanvasManager.m_Instance.UpdateCurrentPlayingPlayerName(m_currentPlayerUID);
            GameplayCanvasManager.m_Instance.UpdateCurrentPlayerName(m_currentPlayerUID);
        }
        else if (GameManager.m_Instance.m_GameType == GameType.Online)
        {
            m_currentPlayerUID = m_allPawnInfos[m_currentPlayerIndex].m_PlayerInfo.m_PlayerUID;
            GameplayCanvasManager.m_Instance.UpdateCurrentPlayingPlayerName(m_allPawnInfos[m_currentPlayerIndex].m_PlayerInfo.m_PlayerName);
        }
    }

    public void OnCardPlayed(Pawn _owner, Card _playedCard)
    {
        m_powerCardGMTOrLongValue = 0;
        if(_playedCard.GetType() == typeof(FuelCard))
        {
            Toast.m_Instance.ShowMessage("You can't call fuel card in this function...");
            return;
        }

        if (!IsMyTurn(_owner.m_PawnInfo.m_PlayerInfo))
        {
            Toast.m_Instance.ShowMessage("You can't play the card. This is not your turn...");
            return;
        }

        if(_playedCard.CanPlayCard(_owner))
        {
            m_pickCardView.UpdatePickCardView(_owner, _playedCard);
            GameplayCanvasManager.m_Instance.ShowGameplayCanvasMenu(GameplayCanvasMenu.PickCardPanel);
        } else
        {
            Toast.m_Instance.ShowMessage("You can't play the card : " + _playedCard.name);
        }
    }

    public void OnPowerCardPlayed(Pawn _owner, PowerCard _playedPowerCard, int _value)
    {
        m_powerCardGMTOrLongValue = _value;
        m_pickCardView.ObscurePickCardView(false);
        m_pickCardView.UpdatePickCardView(_owner, _playedPowerCard);
        GameplayCanvasManager.m_Instance.ShowGameplayCanvasMenu(GameplayCanvasMenu.PickCardPanel);
    }

    public void OnFuelCardPlayed(Pawn _owner, FuelCard _fuelCard, int _numberFuelCards)
    {
        if (!IsMyTurn(_owner.m_PawnInfo.m_PlayerInfo))
        {
            Toast.m_Instance.ShowMessage("You can't play the card. This is not your turn...");
            return;
        }

        if (_fuelCard.CanPlayCard(_owner, _numberFuelCards))
        {
            Toast.m_Instance.ShowMessage("Fuel card is played...");

            if(GameManager.m_Instance.m_GameType == GameType.Offline)
            {
                m_playedCardFrontView.UpdateCardView(_owner, _fuelCard, false);
                GameplayCanvasManager.m_Instance.ShowGameplayCanvasMenu(GameplayCanvasMenu.PlayedCardViewPanel);
                m_cardPlayedPlayerNameText.text = "Card Played Player Name : " + _owner.m_PawnInfo.m_PlayerInfo.m_PlayerName;
                _fuelCard.OnPlayed(_owner, _numberFuelCards);
            }
            else if(GameManager.m_Instance.m_GameType == GameType.Online)
            {
                if (GameManager.m_Instance.m_OwnerInfo != null)
                {
                    m_pickCardView.ObscurePickCardView(true);
                    GameplayCanvasManager.m_Instance.EnableOrDisableAllUpperButton(false);
                    FirebaseRealtimeDatabase.m_Instance.UpdatePlayedCard(GameManager.m_Instance.m_OwnerInfo.m_PlayerUID, _fuelCard, _numberFuelCards, null);
                }
                else
                {
                    Toast.m_Instance.ShowMessage("You did not set the gameManager owner");
                }
            }
        }
        else
        {
            Toast.m_Instance.ShowMessage("You can't play card : " + _fuelCard.name);
        }
    }

    public void OnPickedCard(Pawn _owner, Card _playedCard, Card _pickedCard)
    {
        if (GameManager.m_Instance.m_GameType == GameType.Offline)
        {
            if (_playedCard.GetType() == typeof(PowerCard))
            {
                if (((PowerCard)_playedCard).m_powerType == PowerTypes.GMTMaster || ((PowerCard)_playedCard).m_powerType == PowerTypes.Master)
                {
                    ((PowerCard)_playedCard).OnPowerCardPlayed(_owner, _pickedCard, m_powerCardGMTOrLongValue);
                }
                else
                {
                    _playedCard.OnPlayed(_owner, _pickedCard);
                }
            }
            else
            {
                _playedCard.OnPlayed(_owner, _pickedCard);
            }

            m_cardPlayedPlayerNameText.text = "Card Played Player Name : " + _owner.m_PawnInfo.m_PlayerInfo.m_PlayerName;
            m_playedCardFrontView.UpdateCardView(_owner, _playedCard, false);
        }
        else if (GameManager.m_Instance.m_GameType == GameType.Online)
        {
            if (GameManager.m_Instance.m_OwnerInfo != null)
            {
                m_pickCardView.ObscurePickCardView(true);
                GameplayCanvasManager.m_Instance.EnableOrDisableAllUpperButton(false);
                FirebaseRealtimeDatabase.m_Instance.UpdatePlayedCard(GameManager.m_Instance.m_OwnerInfo.m_PlayerUID, _playedCard, m_powerCardGMTOrLongValue, _pickedCard);
            }
            else
            {
                Toast.m_Instance.ShowMessage("You did not set the gameManager owner");
            }
        }
    }

    public void StartGameplay()
    {
        InstantiatePlayers(m_allPawnInfos);
        ResourceManager.m_Instance.GenerateAllCoinsAndTreasures();
        m_currentPlayerIndex = -1;
        m_CurrentTimeOfGMT0 = m_currentTimeOfGMT0;
        UpdateCurrentTimeOfGMTIndices(m_currentTimeOfGMT0);
        GameplayCanvasManager.m_Instance.EnableOrDisableAllUpperButton(true);
        GameplayCanvasManager.m_Instance.ShowGameplayCanvasMenu(GameplayCanvasMenu.GameplayBoardPanel);
        m_pickCardView.ObscurePickCardView(false);
        GameplayCanvasManager.m_Instance.EnableOrDisableAllUpperButton(true);
        UpdateToNextPlayer();

        m_boardTimesContainer.gameObject.SetActive(m_showTimesBelowGMT);
    }

    public void DistributeCards()
    {
        Dictionary<string, List<int>> _shuffleAllCoinAndTreasurePositions = ResourceManager.m_Instance.ShuffleAllCoinAndTreasurePositions();
        List<int> _playerInstantiationPositions = GetPlayerInstantiationPositions();

        List<Card> _shufflePowerHourCards = CardManager.m_Instance.ShufflePowerOrHourCards();
        List<TrapCard> _shuffleTrapCards = CardManager.m_Instance.ShuffleTrapCards();

        Queue<Card> _queueOfPowerHourCards = new Queue<Card>();
        Queue<TrapCard> _queueOfTrapCards = new Queue<TrapCard>();

        _shufflePowerHourCards.ForEach((x) => _queueOfPowerHourCards.Enqueue(x));
        _shuffleTrapCards.ForEach((x) => _queueOfTrapCards.Enqueue(x));

        if (GameManager.m_Instance.m_GameType == GameType.Offline)
        {
            for (int i = 0; i < GameManager.m_Instance.m_JoinedPlayersInfo.Count; i++)
            {
                PawnInfo _pawnInfo = new PawnInfo();
                _pawnInfo.m_InstantiationPosition = new Vector2Int(0, _playerInstantiationPositions[i]);
                _pawnInfo.m_PlayerInfo = GameManager.m_Instance.m_JoinedPlayersInfo[i];
                _pawnInfo.m_NumberOfFuelCards = m_initialNumberOfFuelCards;
                m_allPawnInfos.Add(_pawnInfo);
            }
            foreach (var _pawnInfo in m_allPawnInfos)
            {
                _pawnInfo.m_PowerAndHourCards = new Card[r_numberOfPowerHourCardInHand];
                _pawnInfo.m_TrapCards = new TrapCard[r_numberOfTrapCardInHand];
                for (int i = 0; i < r_numberOfPowerHourCardInHand; i++)
                {
                    _pawnInfo.m_PowerAndHourCards[i] = _queueOfPowerHourCards.Dequeue();
                }

                for (int i = 0; i < r_numberOfTrapCardInHand; i++)
                {
                    _pawnInfo.m_TrapCards[i] = _queueOfTrapCards.Dequeue();
                }
            }

            CardManager.m_Instance.SetRemainingCards(_queueOfPowerHourCards.ToList(), _queueOfTrapCards.ToList());
            ResourceManager.m_Instance.SetAllCoinAndTreasuresPositions(_shuffleAllCoinAndTreasurePositions[ResourceManager.r_CoinKey], _shuffleAllCoinAndTreasurePositions[ResourceManager.r_TreasureKey]);
            m_currentTimeOfGMT0 = Random.Range(1, r_TotalLongitude + 1);
            StartGameplay();
        }
        else if (GameManager.m_Instance.m_GameType == GameType.Online)
        {
            Dictionary<string, Dictionary<string, string>> _playerUIDWithCards = new Dictionary<string, Dictionary<string, string>>();
            foreach (var _joinedPlayer in GameManager.m_Instance.m_JoinedPlayersInfo)
            {
                Dictionary<string, string> _cards = new Dictionary<string, string>();
                for (int i = 0; i < r_numberOfPowerHourCardInHand; i++)
                {
                    Card _card = _queueOfPowerHourCards.Dequeue();
                    _cards.Add(_card.name, _card.GetType().ToString());
                }

                for (int i = 0; i < r_numberOfTrapCardInHand; i++)
                {
                    TrapCard _trapCard = _queueOfTrapCards.Dequeue();
                    _cards.Add(_trapCard.name, _trapCard.GetType().ToString());
                }
                _playerUIDWithCards.Add(_joinedPlayer.m_PlayerUID, _cards);
            }
            Toast.m_Instance.ShowMessage("Adding game info to database...");
            int _currentTimeOfGMT0 = Random.Range(1, r_TotalLongitude + 1);
            FirebaseRealtimeDatabase.m_Instance.AddGameInfoToDatabase(GameManager.m_Instance.m_GameInfo, _queueOfPowerHourCards.ToList(), _queueOfTrapCards.ToList(), _shuffleAllCoinAndTreasurePositions, _playerInstantiationPositions, _playerUIDWithCards, _currentTimeOfGMT0);
        }
    }

    public void UpdateFuelCardFromDatabase(Pawn _pawn, int _usedPointToBuyFuelCard)
    {
        _pawn.m_PawnInfo.m_NumberOfFuelCards += r_numberOfFuelCardPerPoint * _usedPointToBuyFuelCard;
        _pawn.m_Point -= _usedPointToBuyFuelCard;
        GameplayCanvasManager.m_Instance.ShowGameplayCanvasMenu(GameplayCanvasMenu.GameplayBoardPanel);
    }

    public void OnPlayedCardFromDatabse(Pawn _owner, Card _playedCard, Card _pickedCard, int _powerCardGMTOrLongValue = 0)
    {
        m_pickCardView.ObscurePickCardView(false);
        m_cardPlayedPlayerNameText.text = "Card Played Player Name : " + _owner.m_PawnInfo.m_PlayerInfo.m_PlayerName;
        m_playedCardFrontView.UpdateCardView(_owner, _playedCard, false);
        GameplayCanvasManager.m_Instance.EnableOrDisableAllUpperButton(true);
        GameplayCanvasManager.m_Instance.ShowGameplayCanvasMenu(GameplayCanvasMenu.PlayedCardViewPanel);
        if (_playedCard.GetType() == typeof(PowerCard))
        {
            if (((PowerCard)_playedCard).m_powerType == PowerTypes.GMTMaster || ((PowerCard)_playedCard).m_powerType == PowerTypes.Master)
            {
                ((PowerCard)_playedCard).OnPowerCardPlayed(_owner, _pickedCard, _powerCardGMTOrLongValue);
            }
            else
            {
                _playedCard.OnPlayed(_owner, _pickedCard);
            }
        }
        else
        {
            _playedCard.OnPlayed(_owner, _pickedCard);
        }
    }

    public void OnFuelCardPlayedFromDatabase(Pawn _owner, FuelCard _fuelCard, int _numberOfFuelCard)
    {
        m_playedCardFrontView.UpdateCardView(_owner, _fuelCard, false);
        m_cardPlayedPlayerNameText.text = "Card Played Player Name : " + _owner.m_PawnInfo.m_PlayerInfo.m_PlayerName;
        GameplayCanvasManager.m_Instance.EnableOrDisableAllUpperButton(true);
        GameplayCanvasManager.m_Instance.ShowGameplayCanvasMenu(GameplayCanvasMenu.PlayedCardViewPanel);
        _fuelCard.OnPlayed(_owner, _numberOfFuelCard);
    }

    public void DecodeAllShuffleCardsAndPlayerInstantiationPositions(
        Dictionary<string, object> _remainingPowerHourCardsDict,
        Dictionary<string, object> _remaininTrapCardsDict,
        Dictionary<string, object> _shuffleAllCoinAndTreasurePositionsDict,
        Dictionary<string, object> _playerUIDWithInstantiationPositionAndCardsDict,
        object _currentGMTTimeAt0Object
        )
    {
        List<Card> _remainigPowerHourCards = new List<Card>();
        foreach (var _cardName in _remainingPowerHourCardsDict)
        {
            Card _card = CardManager.m_Instance.GetPowerOrHourCardByName(_cardName.Key);
            if (_card != null)
            {
                _remainigPowerHourCards.Add(_card);
            }
        }
        List<TrapCard> _remainingTrapCards = new List<TrapCard>();
        foreach (var _cardName in _remaininTrapCardsDict)
        {
            TrapCard _trapCard = CardManager.m_Instance.GetTrapCardByName(_cardName.Key);
            if (_trapCard != null)
            {
                _remainingTrapCards.Add(_trapCard);
            }
        }

        Dictionary<string, List<int>> _shuffleAllCoinAndTreasurePositions = new Dictionary<string, List<int>>();
        foreach (var _value in _shuffleAllCoinAndTreasurePositionsDict)
        {
            List<object> _objs = _value.Value as List<object>;
            List<int> _listOfInt = new List<int>();
            foreach (var _position in _objs)
            {
                int i = 0;
                if (int.TryParse(_position.ToString(), out i))
                {
                    _listOfInt.Add(i);
                }
            }
            _shuffleAllCoinAndTreasurePositions.Add(_value.Key, _listOfInt);
        }

        CardManager.m_Instance.SetRemainingCards(_remainigPowerHourCards, _remainingTrapCards);
        ResourceManager.m_Instance.SetAllCoinAndTreasuresPositions(_shuffleAllCoinAndTreasurePositions[ResourceManager.r_CoinKey], _shuffleAllCoinAndTreasurePositions[ResourceManager.r_TreasureKey]);

        foreach (var _playerUID in _playerUIDWithInstantiationPositionAndCardsDict)
        {
            PlayerInfo _playerInfo = null;
            foreach (var _joinedPlayer in GameManager.m_Instance.m_JoinedPlayersInfo)
            {
                if (_playerUID.Key == _joinedPlayer.m_PlayerUID)
                {
                    _playerInfo = _joinedPlayer;
                }
            }
            if (_playerInfo != null)
            {
                PawnInfo _pawnInfo = new PawnInfo();
                _pawnInfo.m_PlayerInfo = _playerInfo;
                Dictionary<string, object> _dict = _playerUID.Value as Dictionary<string, object>;
                int result = 0;
                int.TryParse(_dict[FirebaseRealtimeDatabase.r_initialFuelCardNumber].ToString(), out result);
                _pawnInfo.m_NumberOfFuelCards = result;

                result = 0;
                int.TryParse(_dict[FirebaseRealtimeDatabase.r_playerInstantiationPosition].ToString(), out result);
                _pawnInfo.m_InstantiationPosition = new Vector2Int(0, result);

                _dict = _dict[FirebaseRealtimeDatabase.r_cards] as Dictionary<string, object>;
                int p = 0;
                int t = 0;
                _pawnInfo.m_PowerAndHourCards = new Card[r_numberOfPowerHourCardInHand];
                _pawnInfo.m_TrapCards = new TrapCard[r_numberOfTrapCardInHand];
                foreach (var _cardKeyValue in _dict)
                {
                    if (_cardKeyValue.Value.ToString() == typeof(PowerCard).ToString() || _cardKeyValue.Value.ToString() == typeof(HourCard).ToString())
                    {
                        Card _card = CardManager.m_Instance.GetPowerOrHourCardByName(_cardKeyValue.Key);

                        if (_card != null)
                        {
                            if (p < r_numberOfPowerHourCardInHand)
                            {
                                _pawnInfo.m_PowerAndHourCards[p] = _card;
                                p++;
                            }
                        }
                    }
                    else if (_cardKeyValue.Value.ToString() == typeof(TrapCard).ToString())
                    {
                        TrapCard _trapCard = CardManager.m_Instance.GetTrapCardByName(_cardKeyValue.Key);
                        if (_trapCard != null)
                        {
                            if (t < r_numberOfTrapCardInHand)
                            {
                                _pawnInfo.m_TrapCards[t] = _trapCard;
                                t++;
                            }
                        }
                    }
                }

                m_allPawnInfos.Add(_pawnInfo);
            }
        }
        int _result = 0;
        if(int.TryParse(_currentGMTTimeAt0Object.ToString(), out _result))
        {
            m_currentTimeOfGMT0 = _result;
        }

        StartGameplay();
    }

    private bool CheckForGameCompletion(out List<Pawn> _winingPawns)
    {
        _winingPawns = new List<Pawn>();
        if (!ResourceManager.m_Instance.IsAnyTreasureOrCoinLeft())
        {
            int _maxPoint = 0;
            foreach (var _pawn in m_PawnsDict.Values)
            {
                int _point = _pawn.m_Point + _pawn.m_PawnInfo.m_NumberOfFuelCards;
                if (_point > _maxPoint)
                {
                    _maxPoint = _point;
                    _winingPawns.Clear();
                    _winingPawns.Add(_pawn);
                }
                else if(_point == _maxPoint)
                {
                    _winingPawns.Add(_pawn);
                }
            }

            return true;
        }

        return false;
    }

/*
    private IEnumerator IEnumDistribute()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < m_players.Count; j++)
            {
                m_rearCard.UpdateCardNameText(CardManager.m_Instance.m_RemainingShufflePowerHourCard[i * m_players.Count + j + 1]);
                m_frontCard.UpdateCardNameText(CardManager.m_Instance.m_RemainingShufflePowerHourCard[i * m_players.Count + j]);
                if (j == 0)
                {
                    m_distributeAnimation.clip = m_distributeAnimation.GetClip("card_down");
                }
                else if (j == 1)
                {
                    m_distributeAnimation.clip = m_distributeAnimation.GetClip("card_right");
                }
                else if (j == 2)
                {
                    m_distributeAnimation.clip = m_distributeAnimation.GetClip("card_up");
                }
                else if (j == 3)
                {
                    m_distributeAnimation.clip = m_distributeAnimation.GetClip("card_left");
                }

                m_distributeAnimation.Play();
                yield return new WaitForSecondsRealtime(2);
                m_players[j].UpdatePowerHourCardAtIndex(CardManager.m_Instance.m_RemainingShufflePowerHourCard[i * m_players.Count + j], i);
            }
        }

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < m_players.Count; j++)
            {
                m_rearCard.UpdateCardNameText(CardManager.m_Instance.m_RemainingShuffleTrapCard[i * m_players.Count + j + 1]);
                m_frontCard.UpdateCardNameText(CardManager.m_Instance.m_RemainingShuffleTrapCard[i * m_players.Count + j]);
                if (j == 0)
                {
                    m_distributeAnimation.clip = m_distributeAnimation.GetClip("card_down");
                }
                else if (j == 1)
                {
                    m_distributeAnimation.clip = m_distributeAnimation.GetClip("card_right");
                }
                else if (j == 2)
                {
                    m_distributeAnimation.clip = m_distributeAnimation.GetClip("card_up");
                }
                else if (j == 3)
                {
                    m_distributeAnimation.clip = m_distributeAnimation.GetClip("card_left");
                }

                m_distributeAnimation.Play();
                yield return new WaitForSecondsRealtime(2);
                m_players[j].UpdateTrapCardAtIndex(CardManager.m_Instance.m_RemainingShuffleTrapCard[i * m_players.Count + j], i);
            }
        }

        yield return new WaitForEndOfFrame();
        StartGameplay();
    }
    */

/*
    private void UpdateCurrentTimeOfGMTIndices()
    {
        //GameplayCanvasManager.m_Instance.UpdateCurrentTimeTextAtGMT0(m_currentTimeOfGMT0);
        m_currentGMTofIndices.Clear();
        m_currentGMTofIndices = new Dictionary<int, int>();

        for (int i = 0; i <= r_TotalLatitude; i++)
        {
            int _time = m_currentTimeOfGMT0 - r_TotalLatitude + i;

            if(_time <= 0)
            {
                _time = 24 + _time;
            }
            m_currentGMTofIndices.Add(_time, i);
        }

        for (int i = 12; i < r_TotalLongitude; i++)
        {
            int _time = m_currentTimeOfGMT0 + i - r_TotalLatitude;

            if(_time > 24)
            {
                _time = _time % 24;
            }
            m_currentGMTofIndices.Add(_time, i);
        }

        foreach (var time in m_currentGMTofIndices)
        {
            m_timesText[time.Value].text = "" + time.Key;
        }
    }

    public void StartGame(List<PlayerInfo> _playerInfos)
    {
        m_currentTimeOfGMT0 = Random.Range(0, r_TotalLongitude);

        // Should be called in order
        InstantiateTimesOfGMT();
        InstantiateGMTTextPoints();
        InstantiateBoardGMTPoints();

        m_currentPlayerIndex = 0;
        InstantiatePlayers(_playerInfos);
        //GameplayCanvasManager.m_Instance.UpdatePlayerProfile(m_players[m_currentPlayerIndex]);

        //foreach (var _player in m_players)
        //{
        //    _player.GetSuffledCard();
        //}

        UpdateCurrentTimeOfGMTIndices();
        ResourceManager.M_Instance.GenerateAllCoins();
    }

    public Player NextPlayer()
    {
        return m_players[(m_currentPlayerIndex + 1) % m_players.Count];
    }

    public void OnCardPlayed(Card _card)
    {
        if (m_currentPlayerIndex >= m_players.Count)
        {
            Debug.Log("current player index is greater than equal to Player number...");
            return;
        }

        _card.OnPlayed(m_players[m_currentPlayerIndex]);
    }

    public void UpdateNextPlayer()
    {
        if(!ResourceManager.M_Instance.IsAnyTreasureOrCoinLeft())
        {
            Debug.Log("There is no coin and treasure left");
            Player winningPlayer = null;
            int max_val = int.MinValue;
            foreach (var player in Players)
            {
                if(player.Point > max_val)
                {
                    winningPlayer = player;
                }
            }

            //GameplayCanvasManager.m_Instance.OnWin(winningPlayer);
            return;
        }
        else
        {
            Debug.Log("There are many coin and treasure left");
        }

        if (m_currentPlayerIndex == Players.Count - 1)
        {
            GoToNextRound();
        }

        m_currentPlayerIndex = (m_currentPlayerIndex + 1) % Players.Count;
        //GameplayCanvasManager.m_Instance.UpdatePlayerProfile(Players[m_currentPlayerIndex]);
        //GameplayCanvasManager.m_Instance.DisableChoosePanel();
    }

    private void GoToNextRound()
    {
        //m_CurrentlyMoveLatitude.Clear();
        m_currentTimeOfGMT0 = (m_currentTimeOfGMT0 + 1) % r_TotalLongitude;
        UpdateCurrentTimeOfGMTIndices();

        //CardManager.M_Instance.AddCardInShuffledCards();
        //foreach (var time in m_timesWhenTreasureWillAppear)
        //{
        //    if (m_currentTimeOfGMT0 == time)
        //    {
        //        ResourceManager.M_Instance.GenerateTreasure();
        //        m_timesWhenTreasureWillAppear.Remove(time);
        //        break;
        //    }
        //}
    }

    public void SetTimeAtGMT0(int _time)
    {
        m_currentTimeOfGMT0 = _time;
        UpdateCurrentTimeOfGMTIndices();
    }

    public List<Vector2Int> PlayersCurrentPosition()
    {
        List<Vector2Int> currentPositions = new List<Vector2Int>();

        foreach (var player in Players)
        {
            currentPositions.Add(player.PlayerPosition);
        }

        return currentPositions;
    }
    */
}
