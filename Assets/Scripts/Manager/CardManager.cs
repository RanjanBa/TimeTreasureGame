using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager m_Instance { get; private set; }

    [SerializeField]
    private Color 
        m_powerCardColor,
        m_hourCardColor,
        m_trapCardColor,
        m_fuelCardColor,
        m_hourCardMidColor,
        m_powerCardMidColor,
        m_trapCardMidColor,
        m_fuelCardMidColor;


    public FuelCard m_FuelCard { get; private set; }
    public Color m_PowerCardColor { get { return m_powerCardColor; } }
    public Color m_HourCardColor { get { return m_hourCardColor; } }
    public Color m_TrapCardColor { get { return m_trapCardColor; } }
    public Color m_FuelCardColor { get { return m_fuelCardColor; } }
    public Color m_HourCardMidColor { get { return m_hourCardMidColor; } }
    public Color m_PowerCardMidColor { get { return m_powerCardMidColor; } }
    public Color m_TrapCardMidColor { get { return m_trapCardMidColor; } }
    public Color m_FuelCardMidColor { get { return m_fuelCardMidColor; } }

    public List<Card> m_RemainingShufflePowerHourCard { get { return m_remainingShufflePowerHourCard; } }
    public List<TrapCard> m_RemainingShuffleTrapCard { get { return m_remainingShuffleTrapCard; } }

    private Dictionary<string, Card> m_powerHourCardsDict = new Dictionary<string, Card>();
    private Dictionary<string, TrapCard> m_trapCardDict = new Dictionary<string, TrapCard>();

    private List<Card> m_remainingShufflePowerHourCard = new List<Card>();
    private List<TrapCard> m_remainingShuffleTrapCard = new List<TrapCard>();

    private List<Card> m_playedPowerHourCards = new List<Card>();
    private List<TrapCard> m_playedTrapCards = new List<TrapCard>();

    private void Awake()
    {
        if (m_Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        LoadCards();
        m_Instance = this;
    }

    private void LoadCards()
    {
        List<HourCard> _hourCards = Resources.LoadAll<HourCard>("CardAssets/HourCards").ToList();
        _hourCards.ForEach(x => m_powerHourCardsDict.Add(x.name, x));
        ///Debug.Log("Hour cards : " + m_hourCards.Count);
        
        List<PowerCard> _powerCards = Resources.LoadAll<PowerCard>("CardAssets/PowerCards").ToList();
        _powerCards.ForEach(x => m_powerHourCardsDict.Add(x.name, x));
        ///Debug.Log("Power cards : " + m_powerCards.Count);

        List<TrapCard> _trapCards = Resources.LoadAll<TrapCard>("CardAssets/TrapCards").ToList();
        _trapCards.ForEach(x => m_trapCardDict.Add(x.name, x));
        ///Debug.Log("Trap cards : " + m_trapCards.Count);

        m_FuelCard = Resources.Load<FuelCard>("CardAssets/FuelCard/Fuel Card");
    }

    public Card GetPowerOrHourCardByName(string _cardName)
    {
        if (m_powerHourCardsDict.ContainsKey(_cardName))
        {
            return m_powerHourCardsDict[_cardName];
        }

        return null;
    }

    public TrapCard GetTrapCardByName(string _cardName)
    {
        if (m_trapCardDict.ContainsKey(_cardName))
        {
            return m_trapCardDict[_cardName];
        }

        return null;
    }

    public List<Card> ShufflePowerOrHourCards()
    {
        List<Card> _shufflePowerHourCards = new List<Card>();
        int _countPowerOrHourCards = m_powerHourCardsDict.Count;
        var _randGen = new System.Random();
        var _values = Enumerable.Range(0, _countPowerOrHourCards).OrderBy(x => _randGen.Next()).ToArray();

        for (int i = 0; i < _countPowerOrHourCards; i++)
        {
            _shufflePowerHourCards.Add(m_powerHourCardsDict.ElementAt(_values[i]).Value);
        }

        return _shufflePowerHourCards;
    }

    public List<TrapCard> ShuffleTrapCards()
    {
        List<TrapCard> _shuffleTrapCards = new List<TrapCard>();
        int _countTrapCards = m_trapCardDict.Count;
        var _randGen = new System.Random();
        var _values = Enumerable.Range(0, _countTrapCards).OrderBy(x => _randGen.Next()).ToArray();

        for (int i = 0; i < _countTrapCards; i++)
        {
            _shuffleTrapCards.Add(m_trapCardDict.ElementAt(_values[i]).Value);
        }

        return _shuffleTrapCards;
    }

    public void SetRemainingCards(List<Card> _remainingShufflePowerHourCard, List<TrapCard> _remainingShuffleTrapCard)
    {
        m_remainingShufflePowerHourCard = _remainingShufflePowerHourCard;
        m_remainingShuffleTrapCard = _remainingShuffleTrapCard;
    }

    public void RemovePowerOrHourCard(Card _playedCard, Card _card)
    {
        m_playedPowerHourCards.Add(_playedCard);
        m_remainingShufflePowerHourCard.Remove(_card);
    }

    public void RemoveTrapCard(TrapCard _playedCard, TrapCard _trapCard)
    {
        m_playedTrapCards.Add(_playedCard);
        m_remainingShuffleTrapCard.Remove(_trapCard);
    }
}
