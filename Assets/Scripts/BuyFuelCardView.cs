using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyFuelCardView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_playerFuelNumberText, m_countFuelText;
    [SerializeField]
    private Button m_upButton, m_downButton, m_buyButton, m_cancelButton;
    [SerializeField]
    private RotatingCircle m_rotatingCircle;
    [SerializeField]
    private FrontCardView m_fuelCardFrontView;

    private int m_usedPoint = 0;

    public void ShowBuyPanel(Pawn _pawn)
    {
        m_rotatingCircle.gameObject.SetActive(false);
        m_playerFuelNumberText.text = "" + _pawn.m_PawnInfo.m_NumberOfFuelCards;
        m_countFuelText.text = "" + m_usedPoint * GameplayManager.r_numberOfFuelCardPerPoint;
        m_upButton.onClick.RemoveAllListeners();
        m_downButton.onClick.RemoveAllListeners();
        m_buyButton.onClick.RemoveAllListeners();
        m_cancelButton.onClick.RemoveAllListeners();

        if (m_usedPoint == 0)
        {
            m_downButton.interactable = false;
            m_buyButton.interactable = false;
        }

        m_upButton.onClick.AddListener(() =>
        {
            m_usedPoint++;
            if (m_usedPoint > 0)
            {
                m_downButton.interactable = true;
            }

            m_countFuelText.text = "" + m_usedPoint * GameplayManager.r_numberOfFuelCardPerPoint;
        });

        m_downButton.onClick.AddListener(() =>
        {
            m_usedPoint--;
            if (m_usedPoint <= 0)
            {
                m_downButton.interactable = false;
            }

            if (m_usedPoint <= 0) m_buyButton.interactable = false;

            m_countFuelText.text = "" + m_usedPoint * GameplayManager.r_numberOfFuelCardPerPoint;
        });
        m_buyButton.interactable = true;
        m_buyButton.onClick.AddListener(() =>
        {
            if (GameManager.m_Instance.m_GameType == GameType.Offline)
            {
                if (GameplayManager.m_Instance.m_CurrentPawn.m_PawnInfo.m_PlayerInfo.m_PlayerName != _pawn.m_PawnInfo.m_PlayerInfo.m_PlayerName)
                {
                    Toast.m_Instance.ShowMessage("You can't by fuel. This is not your turn");
                    return;
                }
            }
            else if (GameManager.m_Instance.m_GameType == GameType.Online)
            {
                if (GameplayManager.m_Instance.m_CurrentPawn.m_PawnInfo.m_PlayerInfo.m_PlayerUID != _pawn.m_PawnInfo.m_PlayerInfo.m_PlayerUID)
                {
                    Toast.m_Instance.ShowMessage("You can't by fuel. This is not your turn");
                    return;
                }
            }

            if (m_usedPoint > _pawn.m_Point)
            {
                Toast.m_Instance.ShowMessage("You can't buy fuel. Your need " + (m_usedPoint - _pawn.m_Point) + " point...");
                return;
            }

            m_buyButton.interactable = false;

            if (GameManager.m_Instance.m_GameType == GameType.Offline)
            {
                _pawn.m_PawnInfo.m_NumberOfFuelCards += GameplayManager.r_numberOfFuelCardPerPoint * m_usedPoint;
                _pawn.m_Point -= m_usedPoint;
                GameplayCanvasManager.m_Instance.ShowGameplayCanvasMenu(GameplayCanvasMenu.GameplayBoardPanel);
            }
            else if (GameManager.m_Instance.m_GameType == GameType.Online)
            {
                m_rotatingCircle.gameObject.SetActive(true);
                FirebaseRealtimeDatabase.m_Instance.UpdateFuelCardOfPlayer(GameManager.m_Instance.m_GameInfo, _pawn.m_PawnInfo.m_PlayerInfo.m_PlayerUID, m_usedPoint);
            }
        });

        m_cancelButton.onClick.AddListener(() =>
        {
            GameplayCanvasManager.m_Instance.ShowGameplayCanvasMenu(GameplayCanvasMenu.GameplayBoardPanel);
        });
        m_fuelCardFrontView.UpdateCardView(_pawn, CardManager.m_Instance.m_FuelCard, _enableButtton: false);
    }
}
