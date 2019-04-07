using System.Collections.Generic;
using UnityEngine;

// For Showing Remaining cards on the boards
public class PickCardView : MonoBehaviour
{
    [SerializeField]
    private FrontCardView m_playedFrontCardView;
    [SerializeField]
    private RearCardView m_rearCardViewPrefab;
    [SerializeField]
    private Transform m_container;
    [SerializeField]
    private GameObject m_obscureGameobject;
    [SerializeField]
    private RotatingCircle m_rotatingCircle;

    private List<RearCardView> m_rearCardViews = new List<RearCardView>();

    public void UpdatePickCardView(Pawn _owner, Card _playedCard)
    {
        ObscurePickCardView(false);
        foreach (var _rearCardView in m_rearCardViews)
        {
            DestroyImmediate(_rearCardView.gameObject);
        }

        m_rearCardViews.Clear();

        m_playedFrontCardView.UpdateCardView(_owner, _playedCard, false);

        if (_playedCard.GetType() == typeof(PowerCard) || _playedCard.GetType() == typeof(HourCard))
        {
            foreach (var _card in CardManager.m_Instance.m_RemainingShufflePowerHourCard)
            {
                RearCardView _rearCardView = Instantiate(m_rearCardViewPrefab, m_container);
                _rearCardView.UpdateCardView(_owner, _card, _playedCard);
                m_rearCardViews.Add(_rearCardView);
            }
        }
        else if (_playedCard.GetType() == typeof(TrapCard))
        {
            foreach (var _card in CardManager.m_Instance.m_RemainingShuffleTrapCard)
            {
                RearCardView _rearCardView = Instantiate(m_rearCardViewPrefab, m_container);
                _rearCardView.UpdateCardView(_owner, _card, _playedCard);
                m_rearCardViews.Add(_rearCardView);
            }
        }
    }

    public void ObscurePickCardView(bool _isVisible)
    {
        m_rotatingCircle.SetText("Playing card");
        m_obscureGameobject.SetActive(_isVisible);
    }
}
