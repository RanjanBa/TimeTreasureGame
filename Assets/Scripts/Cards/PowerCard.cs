using UnityEngine;

[CreateAssetMenu(fileName = "Power Card", menuName = "Card/Power Card")]
public class PowerCard : Card
{
    [SerializeField]
    private string m_bottomText;

    [HideInInspector]
    public PowerTypes m_powerType;
    [HideInInspector]
    public int m_step = 0;

    public override string CardTypeText
    {
        get
        {
            return "Power Card";
        }

        protected set
        {

        }
    }
    public override string CardNameText
    {
        get
        {
            return m_powerType == PowerTypes.PosX_NegX ? 
                (m_step < 0 ? "" : "+") + m_step + " hr" 
                : m_powerType.ToString() + " Card";
        }
        protected set { }
    }
    public override string CardBottomText { get { return m_bottomText; } protected set { } }

    public PowerTypes PowerType { get { return m_powerType; } }

    public PowerCard Init(PowerTypes _powerTypes)
    {
        m_powerType = _powerTypes;

        switch (m_powerType)
        {
            case PowerTypes.Master:
                m_bottomText = "Jump to any longitude.";
                break;
            case PowerTypes.GMTMaster:
                m_bottomText = "Set the time at GMT as per your strategy.";
                break;
            case PowerTypes.LongitudeMaster:
                m_bottomText = "Grab all the coins and treasures present on your longitude";
                break;
            case PowerTypes.ThreePoints:
                m_bottomText = "Take any three points from any player.";
                break;
            default:
                break;
        }

        return this;
    }

    public PowerCard Init(PowerTypes _powerTypes, int _step)
    {
        m_step = _step;
        switch (_powerTypes)
        {
            case PowerTypes.PosX_NegX:
                m_bottomText = "Jump " + _step + " hr in " + (_step < 0 ? "left" : "right") + " direction respectively";
                break;
            default:
                break;
        }

        return Init(_powerTypes);
    }

    public override bool CanPlayCard(Pawn _owner)
    {
        switch (m_powerType)
        {
            case PowerTypes.LongitudeMaster:
                bool found = false;
                for (int i = 0; i < GameplayManager.r_TotalLatitude; i++)
                {
                    Treasure treasure;
                    Coin coin;
                    Vector2Int position = new Vector2Int(i, _owner.m_PlayerPosition.y);
                    if (ResourceManager.m_Instance.CheckAnyResourceAtIndex(position, out treasure, out coin))
                    {
                        found = true;
                        break;
                    }
                }
                if (found == false)
                {
                    Debug.Log("There is no Coin or treasure at player present longitude...");
                    return false;
                }
                break;
            case PowerTypes.ThreePoints:
                break;
            case PowerTypes.PosX_NegX:
                int longitude = (24 + _owner.m_PlayerPosition.y + m_step) % 24;
                foreach (var _pawn in GameplayManager.m_Instance.m_PawnsDict.Values)
                {
                    if (_pawn.m_PlayerPosition == _owner.m_PlayerPosition)
                    {
                        continue;
                    }

                    if (new Vector2Int(_owner.m_PlayerPosition.x, longitude) == _pawn.m_PlayerPosition)
                    {
                        Debug.Log("Card is not Played...");
                        return false;
                    }
                }
                break;
            default:
                return false;
        }

        return true;
    }

    public override void OnPlayed(Pawn _owner, Card _pickedCard)
    {
        switch (m_powerType)
        {
            case PowerTypes.LongitudeMaster:
                int _numberOfTreasures = 0, _numberOfCoins = 0;
                for (int i = 0; i < GameplayManager.r_TotalLatitude; i++)
                {
                    Treasure _treasure;
                    Coin _coin;
                    Vector2Int position = new Vector2Int(i, _owner.m_PlayerPosition.y);
                    if (ResourceManager.m_Instance.CheckAnyResourceAtIndex(position, out _treasure, out _coin))
                    {
                        Debug.Log("Resource is found at position " + position);
                        if (_treasure != null)
                        {
                            Debug.Log("Treasure is found at position " + position);
                            _owner.m_PawnInfo.m_NumberOfFuelCards += _treasure.m_Chest.NumberOfFuelCard;
                            _owner.m_Point += _treasure.m_Chest.Point;
                            ResourceManager.m_Instance.RemoveTreasure(_treasure);
                            Destroy(_treasure.gameObject);
                            _numberOfTreasures++;
                        }
                        else if (_coin != null)
                        {
                            Debug.Log("Coin is found at position " + position);
                            _owner.m_Point += _coin.m_Point;
                            ResourceManager.m_Instance.RemoveCoin(_coin);
                            Destroy(_coin.gameObject);
                            _numberOfCoins++;
                        }
                    }
                }

                GameplayCanvasManager.m_Instance.UpdateCollectedCoinsOrTreasures(true, _numberOfCoins, _numberOfTreasures);
                break;
            case PowerTypes.ThreePoints:
                Debug.Log("ThreePoints Master power card is used");
                _owner.m_Point += 3;
                GameplayCanvasManager.m_Instance.UpdateCollectedCoinsOrTreasures(true);
                break;
            case PowerTypes.PosX_NegX:
                Debug.Log("PosX_NegX Master power card is used with step " + m_step);
                int longitude = (24 + _owner.m_PlayerPosition.y + m_step) % 24;
                _owner.Move(new Vector2Int(_owner.m_PlayerPosition.x, longitude));
                break;
            default:
                GameplayCanvasManager.m_Instance.UpdateCollectedCoinsOrTreasures(true);
                return;
        }

        _owner.OnPowerOrHourCardPlayed(this, _pickedCard);
    }

    public void OnPowerCardPlayed(Pawn _owner, Card _pickedCard, int _value)
    {
        if(m_powerType == PowerTypes.GMTMaster)
        {
            GameplayManager.m_Instance.m_CurrentTimeOfGMT0 = _value;
        }
        else if(m_powerType == PowerTypes.Master)
        {
            _owner.Move(new Vector2Int(_owner.m_PlayerPosition.x, _value));
        }
        GameplayCanvasManager.m_Instance.UpdateCollectedCoinsOrTreasures(true);
        _owner.OnPowerOrHourCardPlayed(this, _pickedCard);
    }
}

public enum PowerTypes
{
    Master,
    GMTMaster,
    LongitudeMaster,
    ThreePoints,
    PosX_NegX,
}
