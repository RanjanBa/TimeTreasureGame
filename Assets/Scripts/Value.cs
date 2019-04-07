using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Value : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_text;
    [SerializeField]
    private Button m_button;
    private int m_value;

    public void UpdateValue(Pawn _owner, PowerCard _powerCard, int _value)
    {
        m_value = _value;
        string _text = "";
        m_button.onClick.RemoveAllListeners();
        if (_powerCard.m_powerType == PowerTypes.GMTMaster)
        {
            m_value = _value;
            if (m_value == 24)
            {
                _text = "12 AM";
            }
            else if(m_value < 12)
            {
                _text = m_value + "AM";
            }
            else if(m_value == 12)
            {
                _text = "12 PM";
            }
            else if(m_value > 12)
            {
                _text = m_value % 12 + " PM";
            }
            
            m_button.onClick.AddListener(() => GMTTime(m_value, _owner, _powerCard));
        }
        else if(_powerCard.m_powerType == PowerTypes.Master)
        {
            _text = (m_value - 1 - GameplayManager.r_TotalLatitude) + "";
            m_button.onClick.AddListener(() => GoToLogitude(m_value - 1, _owner, _powerCard));
        }

        m_text.text = _text + "";
    }

    private void GMTTime(int _time, Pawn _owner, PowerCard _powerCard)
    {
        if (!GameplayManager.m_Instance.IsMyTurn(_owner.m_PawnInfo.m_PlayerInfo))
        {
            Toast.m_Instance.ShowMessage("You can't play the card. This is not your turn...");
            return;
        }

        Toast.m_Instance.ShowMessage("Set Time is : " + _time);

        GameplayManager.m_Instance.OnPowerCardPlayed(_owner, _powerCard, _time);
    }

    private void GoToLogitude(int _value, Pawn _owner, PowerCard _powerCard)
    {
        if (!GameplayManager.m_Instance.IsMyTurn(_owner.m_PawnInfo.m_PlayerInfo))
        {
            Toast.m_Instance.ShowMessage("You can't play the card. This is not your turn...");
            return;
        }

        int _longitudeIndex = _value;

        foreach (var _pawn in GameplayManager.m_Instance.m_PawnsDict.Values)
        {
            if (_pawn.m_PlayerPosition == new Vector2Int(_owner.m_PlayerPosition.x, _longitudeIndex))
            {
                Toast.m_Instance.ShowMessage("Can't go to " + _longitudeIndex + ", since other player is in that longitude " + _owner.m_PlayerPosition.y);
                return;
            }
        }

        Toast.m_Instance.ShowMessage("Pawn is moved to " + _longitudeIndex);
        GameplayManager.m_Instance.OnPowerCardPlayed(_owner, _powerCard, _longitudeIndex);
    }
}
