using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RearCardView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_cardNameText;
    [SerializeField]
    private Card m_card;
    [SerializeField]
    private Card m_playedCard;
    [SerializeField]
    private Button m_button;

    private Pawn m_owner;

    public void UpdateCardView(Pawn _owner, Card _card, Card _playedCard)
    {
        m_playedCard = _playedCard;
        m_card = _card;
        m_owner = _owner;
        if (_card.GetType() == typeof(TrapCard))
        {
            m_cardNameText.text = "Trap Card";
        }
        else if (_card.GetType() == typeof(HourCard) || _card.GetType() == typeof(PowerCard))
        {
            m_cardNameText.text = "Time Treasure";
        }

        m_button.onClick.RemoveAllListeners();

        if (_card.GetType() != typeof(FuelCard))
        {
            m_button.onClick.AddListener(() => OnClickUpdatePlayerCard());
        }
    }

    private void OnClickUpdatePlayerCard()
    {
        Debug.Log("Flip card " + m_card.name + " is picked...");
        GameplayManager.m_Instance.OnPickedCard(m_owner, m_playedCard, m_card);
    }
}
