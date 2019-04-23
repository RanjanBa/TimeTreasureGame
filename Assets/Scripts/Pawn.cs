using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo
{
    public Color m_PlayerColorCode { get; set; }
    public string m_PlayerName { get; private set; }
    public string m_PlayerUID { get; private set; }

    public PlayerInfo(string _playerName, string _playerUID)
    {
        m_PlayerName = _playerName;
        m_PlayerUID = _playerUID;
    }
}


[System.Serializable]
public class PawnInfo
{
    public Vector2Int m_InstantiationPosition;
    public PlayerInfo m_PlayerInfo;
    public Card[] m_PowerAndHourCards = new Card[5];
    public TrapCard[] m_TrapCards = new TrapCard[2];
    public int m_NumberOfFuelCards = 10;
}


public class Pawn : MonoBehaviour
{
    public PawnInfo m_PawnInfo { get; set; }

    public TextMeshProUGUI m_PawnNameTextPro;
    public Image m_Image;
    public Vector2Int m_PlayerPosition { get; private set; }
    public int m_Point;

    public void Move(Vector2Int _position)
    {
        m_PlayerPosition = _position;
        RectTransform _rectTransform = GameplayManager.m_Instance.m_BoardGMTPoints[_position.x, _position.y];
        transform.SetParent(_rectTransform);
        transform.localPosition = Vector3.zero;

        Treasure _treasure;
        Coin _coin;

        int _numberOfTreasures = 0, _numberOfCoins = 0;
        if (ResourceManager.m_Instance.CheckAnyResourceAtIndex(_position, out _treasure, out _coin))
        {
            Debug.Log("Resource is found at position " + _position);
            
            if (_treasure != null)
            {
                Debug.Log("Treasure is found at position " + _position);
                m_PawnInfo.m_NumberOfFuelCards += _treasure.m_Chest.NumberOfFuelCard;
                m_Point += _treasure.m_Chest.Point;
                ResourceManager.m_Instance.RemoveTreasure(_treasure);
                Destroy(_treasure.gameObject);
                _numberOfTreasures++;
            }
            else if (_coin != null)
            {
                Debug.Log("Coin is found at position " + _position);
                m_Point += _coin.m_Point;
                ResourceManager.m_Instance.RemoveCoin(_coin);
                Destroy(_coin.gameObject);
                _numberOfCoins++;
            }
        }
        GameplayCanvasManager.m_Instance.UpdateCollectedCoinsOrTreasures(true, _numberOfCoins, _numberOfTreasures);
    }

    public void OnCardPlayed()
    {
        if (GameManager.m_Instance.m_GameType == GameType.Offline)
        {
            GameplayCanvasManager.m_Instance.ShowGameplayCanvasMenu(GameplayCanvasMenu.PlayedCardViewPanel);
            GameplayManager.m_Instance.UpdateToNextPlayer();
        }
        else if (GameManager.m_Instance.m_GameType == GameType.Online)
        {
            //Toast.m_Instance.ShowMessage("Joined Count : " + GameManager.m_Instance.m_JoinedPlayersInfo.Count + ", Pawns count : " + GameplayManager.m_Instance.m_PawnsDict.Values.Count);
            GameplayManager.m_Instance.UpdateToNextPlayer();
        }
    }

    public void OnTrapCardPlayed(TrapCard _playedCard, TrapCard _pickedCard)
    {
        for (int i = 0; i < m_PawnInfo.m_TrapCards.Length; i++)
        {
            if (_playedCard == m_PawnInfo.m_TrapCards[i])
            {
                m_PawnInfo.m_TrapCards[i] = _pickedCard;
            }
        }
        CardManager.m_Instance.RemoveTrapCard(_playedCard, _pickedCard);
        OnCardPlayed();
    }

    public void OnPowerOrHourCardPlayed(Card _playedCard, Card _pickedCard)
    {
        if (_pickedCard.GetType() == typeof(TrapCard) || _pickedCard.GetType() == typeof(FuelCard))
        {
            Toast.m_Instance.ShowMessage("Trap card or fuel card can't be assign to powerOrHour card");
            return;
        }

        for (int i = 0; i < m_PawnInfo.m_PowerAndHourCards.Length; i++)
        {
            if (m_PawnInfo.m_PowerAndHourCards[i] == _playedCard)
            {
                m_PawnInfo.m_PowerAndHourCards[i] = _pickedCard;
            }
        }

        CardManager.m_Instance.RemovePowerOrHourCard(_playedCard, _pickedCard);

        OnCardPlayed();
    }
}
