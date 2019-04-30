using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerGameInfo : MonoBehaviour
{
    [SerializeField]
    private Image m_image;
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
        m_image.color = _pawn.m_ColorCode;
    }
}
