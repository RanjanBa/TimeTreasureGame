#pragma warning disable 649 // to disable warning in the editor : Field `***' is never assigned to, 
                            // and will always have its default
                            // value `null' warning
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class FrontCardView : MonoBehaviour
{
    [SerializeField]
    private Card m_card;
    [SerializeField]
    private TextMeshProUGUI m_cardTypeTextPro;
    [SerializeField]
    private TextMeshProUGUI m_cardMiddleTextPro;
    [SerializeField]
    private TextMeshProUGUI m_cardBottomTextPro;
    [SerializeField]
    private Button m_button;

    [SerializeField]
    private Image m_mainCardImage, m_upperImage, m_midImage;
    private Pawn m_owner;

    public void UpdateCardView(Pawn _owner, Card _card, bool _enableButtton = true)
    {
        if (_card == null)
            return;

        m_button.interactable = true;
        m_card = _card;
        m_owner = _owner;

        m_cardTypeTextPro.text = m_card.CardTypeText;
        m_cardMiddleTextPro.text = m_card.CardNameText;
        m_cardBottomTextPro.text = m_card.CardBottomText;

        if (m_card.GetType() == typeof(PowerCard))
        {
            m_mainCardImage.color = CardManager.m_Instance.m_PowerCardColor;
            m_upperImage.color = m_midImage.color = CardManager.m_Instance.m_PowerCardMidColor;
        }
        else if (m_card.GetType() == typeof(HourCard))
        {
            m_mainCardImage.color = CardManager.m_Instance.m_HourCardColor;
            m_upperImage.color = m_midImage.color = CardManager.m_Instance.m_HourCardMidColor;
        }
        else if (m_card.GetType() == typeof(TrapCard))
        {
            m_mainCardImage.color = CardManager.m_Instance.m_TrapCardColor;
            m_upperImage.color = m_midImage.color = CardManager.m_Instance.m_TrapCardMidColor;
        }
        else if (m_card.GetType() == typeof(FuelCard))
        {
            m_button.interactable = false;
            m_mainCardImage.color = CardManager.m_Instance.m_FuelCardColor;
            m_upperImage.color = m_midImage.color = CardManager.m_Instance.m_FuelCardMidColor;
            m_midImage.gameObject.SetActive(false);
            return;
        }

        m_midImage.gameObject.SetActive(true);
        m_button.onClick.RemoveAllListeners();
        if (_enableButtton)
        {
            m_button.onClick.AddListener(() => OnCardClick());
        }
        m_button.interactable = _enableButtton;
    }

    private void OnCardClick()
    {
        if(m_card == null)
        {
            Debug.LogWarning("Card is null... Set the reference...");
            return;
        }

        if (m_owner == null)
        {
            Debug.LogWarning("Owner is null... Set the reference...");
            return;
        }
        if(m_card.GetType() == typeof(PowerCard))
        {
            if(((PowerCard)m_card).m_powerType == PowerTypes.GMTMaster || ((PowerCard)m_card).m_powerType == PowerTypes.Master)
            {
                GameplayManager.m_Instance.m_ChoosePointsView.ShowAllChoosePoints(m_owner, (PowerCard)m_card);
                return;
            }
        }

        GameplayManager.m_Instance.OnCardPlayed(m_owner, m_card);
    }
}
