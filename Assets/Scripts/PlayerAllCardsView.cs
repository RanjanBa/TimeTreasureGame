using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerAllCardsView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_playerNameText;
    [SerializeField]
    private TextMeshProUGUI m_fuelCountText;
    [SerializeField]
    private FrontCardView[] m_powerHourCards = new FrontCardView[5];
    [SerializeField]
    private FrontCardView[] m_trapCards = new FrontCardView[2];
    [SerializeField]
    private FrontCardView m_fuelCard;
    [SerializeField]
    private Button m_upButton, m_downButton, m_useButton;
    [SerializeField]
    private TextMeshProUGUI m_fuelCardNumberText;

    private int m_fuelCardNumber;

    private void CheckInteractable(Pawn _pawn)
    {
        
        if (m_fuelCardNumber + _pawn.m_PlayerPosition.x >= GameplayManager.r_TotalLatitude - 1)
        {
            m_upButton.interactable = false;
            m_downButton.interactable = true;
        }
        else if (m_fuelCardNumber + _pawn.m_PlayerPosition.x <= 0)
        {
            m_upButton.interactable = true;
            m_downButton.interactable = false;
        }
        else
        {
            m_upButton.interactable = true;
            m_downButton.interactable = true;
        }
    }

    public void UpdateCards(Pawn _pawn)
    {
        GameplayManager.m_Instance.m_ChoosePointsView.gameObject.SetActive(false);
        m_useButton.onClick.RemoveAllListeners();
        m_upButton.onClick.RemoveAllListeners();
        m_downButton.onClick.RemoveAllListeners();
        m_fuelCardNumberText.text = m_fuelCardNumber + "";
        CheckInteractable(_pawn);

        m_upButton.onClick.AddListener(() =>
        {
            m_fuelCardNumber++;
            CheckInteractable(_pawn);
            m_fuelCardNumberText.text = m_fuelCardNumber + "";
        });

        m_downButton.onClick.AddListener(() =>
        {
            m_fuelCardNumber--;
            CheckInteractable(_pawn);
            m_fuelCardNumberText.text = m_fuelCardNumber + "";
        });


        m_useButton.onClick.AddListener(() =>
        {
            if (GameplayManager.m_Instance.IsMyTurn(_pawn.m_PawnInfo.m_PlayerInfo))
            {
                GameplayManager.m_Instance.OnFuelCardPlayed(_pawn, CardManager.m_Instance.m_FuelCard, m_fuelCardNumber);
            }
            else
            {
                Toast.m_Instance.ShowMessage("You can't play fuel card. This is not your turn...");
            }
        });

        m_playerNameText.text = _pawn.m_PawnInfo.m_PlayerInfo.m_PlayerName;
        m_fuelCountText.text = _pawn.m_PawnInfo.m_NumberOfFuelCards + "";
        for (int i = 0; i < _pawn.m_PawnInfo.m_PowerAndHourCards.Length; i++)
        {
            m_powerHourCards[i].UpdateCardView(_pawn, _pawn.m_PawnInfo.m_PowerAndHourCards[i]);
        }

        for (int i = 0; i < _pawn.m_PawnInfo.m_TrapCards.Length; i++)
        {
            m_trapCards[i].UpdateCardView(_pawn, _pawn.m_PawnInfo.m_TrapCards[i]);
        }

        m_fuelCard.UpdateCardView(_pawn, CardManager.m_Instance.m_FuelCard);
    }
}
