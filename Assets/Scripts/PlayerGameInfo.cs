using UnityEngine;
using TMPro;

public class PlayerGameInfo : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_playerNameText;
    [SerializeField]
    private TextMeshProUGUI m_playerPointText;
    [SerializeField]
    private TextMeshProUGUI m_playerFuelNumberText;

    public void UpdatePlayerGameInfo(Pawn _pawn)
    {
        Debug.Log("Player Name : " + _pawn.m_PawnInfo.m_PlayerInfo.m_PlayerName);
        m_playerNameText.text =  _pawn.m_PawnInfo.m_PlayerInfo.m_PlayerName;
        m_playerPointText.text = _pawn.m_Point + "";
        m_playerFuelNumberText.text = _pawn.m_PawnInfo.m_NumberOfFuelCards + "";
    }
}
