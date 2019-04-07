using UnityEngine;

[CreateAssetMenu(fileName = "Trap Card", menuName = "Card/Trap Card")]
public class TrapCard : Card
{
    [SerializeField, Range(-11, 12)]
    private int m_gmt;

    public int m_GMT
    {
        get
        {
            return m_gmt;
        }
    }

    public override string CardTypeText
    {
        get
        {
            return "Trap Card";
        }

        protected set { }
    }

    public override string CardNameText
    {
        get
        {
            return "GMT " + m_gmt;
        }

        protected set { }
    }

    public override string CardBottomText
    {
        get
        {
            return "Collect Text\n10pm-6am = 2 fuel\n7am-2pm = 2 fuel\n3pm-9pm = 4 fuel";
        }

        protected set { }
    }

    public TrapCard Init(int _gmt)
    {
        m_gmt = _gmt;
        return this;
    }

    private void ConditionToPoint(Pawn _owner, Pawn _otherPlayer, int _time)
    {
        if(_time >= 22|| _time <= 6)
        {
            Debug.Log("2 Fuel cards");
            _owner.m_PawnInfo.m_NumberOfFuelCards += 2;
            _otherPlayer.m_PawnInfo.m_NumberOfFuelCards -= 2;
        }
        else if(_time >= 7 && _time <= 14)
        {
            Debug.Log("3 Fuel cards");
            _owner.m_PawnInfo.m_NumberOfFuelCards += 3;
            _otherPlayer.m_PawnInfo.m_NumberOfFuelCards -= 3;
        }
        else if(_time >= 15 && _time <= 21)
        {
            Debug.Log("4 Fuel cards");
            _owner.m_PawnInfo.m_NumberOfFuelCards += 4;
            _owner.m_PawnInfo.m_NumberOfFuelCards -= 4;
        }
    }

    public override bool CanPlayCard(Pawn _owner)
    {
        bool _canPlay = false;
        int time = GameplayManager.m_Instance.m_CurrentTimeOfGMT0 + m_gmt;

        if (time <= 0)
        {
            time = 24 + time;
        }
        else if (time > 24)
        {
            time = time % 24;
        }

        int index = GameplayManager.m_Instance.m_CurrentGMTofIndices[time];

        foreach (var _keyValuePair in GameplayManager.m_Instance.m_PawnsDict)
        {
            var _player = _keyValuePair.Value;
            if (_player.m_PlayerPosition == _owner.m_PlayerPosition)
            {
                continue;
            }

            if (_player.m_PlayerPosition.y == index)
            {
                
                _canPlay = true;
            }
        }

        if (_canPlay)
        {
            Debug.Log("GMT : " + m_gmt + ", time : " + time + ", Other Player present is at index " + index);
        }
        else
        {
            Debug.Log("GMT : " + m_gmt + ", time : " + time + ", there is no player at index " + index);
        }

        return _canPlay;
    }

    public override void OnPlayed(Pawn _owner, Card _pickedCard)
    {
        int time = GameplayManager.m_Instance.m_CurrentTimeOfGMT0 + m_gmt;

        if (time <= 0)
        {
            time = 24 + time;
        }
        else if (time > 24)
        {
            time = time % 24;
        }

        int index = GameplayManager.m_Instance.m_CurrentGMTofIndices[time];

        foreach (var _keyValuePair in GameplayManager.m_Instance.m_PawnsDict)
        {
            var _player = _keyValuePair.Value;
            if (_player.m_PlayerPosition == _owner.m_PlayerPosition)
            {
                continue;
            }

            if(_player.m_PlayerPosition.y == index)
            {
                Debug.Log("GMT : " + m_gmt + ", time : " + time + "," + _player.m_PawnInfo.m_PlayerInfo.m_PlayerName + " is at index " + index);
                ConditionToPoint(_owner, _player, time);
            }
        }

        Debug.Log("GMT : " + m_gmt + ", time : " + time + ", there is no player at index " + index);
        _owner.OnTrapCardPlayed(this, (TrapCard)_pickedCard);
        GameplayCanvasManager.m_Instance.UpdateCollectedCoinsOrTreasures(false);
    }
}
