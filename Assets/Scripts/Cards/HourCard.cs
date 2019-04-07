using UnityEngine;

[CreateAssetMenu(fileName = "Hour Card", menuName = "Card/Hour Card")]
public class HourCard : Card
{
    [SerializeField, Range(1, 24)]
    private int m_gmtTime;

    public override string CardTypeText
    {
        get
        {
            return "Hour Card";
        }

        protected set { }
    }

    public override string CardNameText
    {
        get
        {
            string ans = "";
            if (m_gmtTime == 24)
            {
                ans = "12 AM";
            }else if(m_gmtTime == 12)
            {
                ans = m_gmtTime + " PM";
            }
            else if( m_gmtTime < 12)
            {
                ans = m_gmtTime + " AM";
            }
            else if(m_gmtTime > 12)
            {
                ans = m_gmtTime % 12  + " PM";
            }
            return ans;
        }

        protected set { }
    }

    public override string CardBottomText
    {
        get
        {
            return "Enable to jump to related gmt time longitude";
        }

        protected set { }
    }

    public HourCard Init(int _gmt)
    {
        m_gmtTime = _gmt;
        return this;
    }

    public override bool CanPlayCard(Pawn _owner)
    {
        int longitude = GameplayManager.m_Instance.m_CurrentGMTofIndices[m_gmtTime];

        foreach (var _player in GameplayManager.m_Instance.m_PawnsDict.Values)
        {
            if (new Vector2Int(_owner.m_PlayerPosition.x, longitude) == _player.m_PlayerPosition)
            {
                Debug.Log("Currently in the same latitude and same logitude : owner " + _owner.m_PlayerPosition + ", other : " + _player.m_PlayerPosition);
                Toast.m_Instance.ShowMessage("Currently in the same latitude and same logitude : owner " + _owner.m_PlayerPosition + ", other : " + _player.m_PlayerPosition);
                return false;
            }
        }

        return true;
    }

    public override void OnPlayed(Pawn _owner, Card _pickedCard)
    {
        int longitude = GameplayManager.m_Instance.m_CurrentGMTofIndices[m_gmtTime];

        _owner.Move(new Vector2Int(_owner.m_PlayerPosition.x, longitude));
        _owner.OnPowerOrHourCardPlayed(this, _pickedCard);
        Debug.Log("Hour card is placed... gmt : " + m_gmtTime + ", index" + longitude);
    }
}
