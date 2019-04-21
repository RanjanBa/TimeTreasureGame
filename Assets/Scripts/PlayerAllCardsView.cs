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
    private Button m_increaseButton, m_decreaseButton, m_jumpDownButton, m_jumpUpButton, m_closeButton;
    [SerializeField]
    private TextMeshProUGUI m_fuelCardNumberText;
    [SerializeField]
    private Animation m_fuelCardPlayPanelAnimation;
    [SerializeField]
    private FrontCardView m_fuelCardOfFuelCardPlayPanel;

    private int m_fuelCardNumber;
    private bool m_isFuelCardPlayPanelClosed = false;

    private void CheckInteractable(Pawn _pawn)
    {
        if(_pawn.m_PawnInfo.m_NumberOfFuelCards <= 0)
        {
            m_decreaseButton.interactable = false;
            m_increaseButton.interactable = false;
            return;
        }

        if (m_fuelCardNumber <= 0)
        {
            m_fuelCardNumber = 0;
            m_increaseButton.interactable = true;
            m_decreaseButton.interactable = false;
        }

        if (m_fuelCardNumber >= _pawn.m_PawnInfo.m_NumberOfFuelCards)
        {
            m_increaseButton.interactable = false;
            m_decreaseButton.interactable = true;
        }
    }

    public void UpdateCards(Pawn _pawn)
    {
        if (!m_isFuelCardPlayPanelClosed)
        {
            CloseFuelCardPlayPanelAnimation();
        }

        GameplayManager.m_Instance.m_ChoosePointsView.gameObject.SetActive(false);
        m_jumpUpButton.onClick.RemoveAllListeners();
        m_jumpDownButton.onClick.RemoveAllListeners();
        m_increaseButton.onClick.RemoveAllListeners();
        m_decreaseButton.onClick.RemoveAllListeners();
        m_closeButton.onClick.RemoveAllListeners();
        m_fuelCardNumberText.text = m_fuelCardNumber + "";
        CheckInteractable(_pawn);

        m_increaseButton.onClick.AddListener(() =>
        {
            m_fuelCardNumber++;
            CheckInteractable(_pawn);
            m_fuelCardNumberText.text = m_fuelCardNumber + "";
        });

        m_decreaseButton.onClick.AddListener(() =>
        {
            m_fuelCardNumber--;
            CheckInteractable(_pawn);
            m_fuelCardNumberText.text = m_fuelCardNumber + "";
        });

        m_jumpUpButton.onClick.AddListener(() =>
        {
            if (m_fuelCardNumber > 0)
            {
                if (GameplayManager.m_Instance.IsMyTurn(_pawn.m_PawnInfo.m_PlayerInfo))
                {
                    GameplayManager.m_Instance.OnFuelCardPlayed(_pawn, CardManager.m_Instance.m_FuelCard, m_fuelCardNumber);
                }
                else
                {
                    Toast.m_Instance.ShowMessage("You can't play fuel card. This is not your turn...");
                }
            }
            else
            {
                Toast.m_Instance.ShowMessage("Please select number fuel cards...");
            }
        });

        m_jumpDownButton.onClick.AddListener(() =>
        {
            if (GameplayManager.m_Instance.IsMyTurn(_pawn.m_PawnInfo.m_PlayerInfo))
            {
                GameplayManager.m_Instance.OnFuelCardPlayed(_pawn, CardManager.m_Instance.m_FuelCard, (-1) * m_fuelCardNumber);
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
            m_powerHourCards[i].UpdateCardView(_pawn, _pawn.m_PawnInfo.m_PowerAndHourCards[i], _action: ()=> {
                if (!m_isFuelCardPlayPanelClosed)
                {
                    CloseFuelCardPlayPanelAnimation();
                }
            });
        }

        for (int i = 0; i < _pawn.m_PawnInfo.m_TrapCards.Length; i++)
        {
            m_trapCards[i].UpdateCardView(_pawn, _pawn.m_PawnInfo.m_TrapCards[i], _action: () => {
                if (!m_isFuelCardPlayPanelClosed)
                {
                    CloseFuelCardPlayPanelAnimation();
                }
            });
        }

        m_fuelCard.UpdateCardView(_pawn, CardManager.m_Instance.m_FuelCard, _action : () =>
        {
            if (m_isFuelCardPlayPanelClosed)
            {
                OpenFuelCardPlayPanelAnimation();
            }
        }, _enableButtton: true);
        m_fuelCardOfFuelCardPlayPanel.UpdateCardView(_pawn, CardManager.m_Instance.m_FuelCard, _enableButtton: false);
        m_closeButton.onClick.AddListener(() => CloseFuelCardPlayPanelAnimation());
    }

    private void OpenFuelCardPlayPanelAnimation()
    {
        m_fuelCardPlayPanelAnimation["fuelCardPlayPanelAnimation"].speed = 1f;
        m_fuelCardPlayPanelAnimation.Play();
        m_isFuelCardPlayPanelClosed = false;
    }

    private void CloseFuelCardPlayPanelAnimation()
    {
        if (m_fuelCardPlayPanelAnimation["fuelCardPlayPanelAnimation"].time <= 0.01f)
        {
            m_fuelCardPlayPanelAnimation["fuelCardPlayPanelAnimation"].time = m_fuelCardPlayPanelAnimation["fuelCardPlayPanelAnimation"].length;
        }
        m_fuelCardPlayPanelAnimation["fuelCardPlayPanelAnimation"].speed = -1f;
        m_fuelCardPlayPanelAnimation.Play();
        m_isFuelCardPlayPanelClosed = true;
    }
}
