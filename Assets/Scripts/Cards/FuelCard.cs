using UnityEngine;

[CreateAssetMenu(fileName = "Fuel Card", menuName = "Card/Fuel Card")]
public class FuelCard : Card
{
    public override string CardTypeText
    {
        get
        {
            return "Fuel Card";
        }

        protected set
        {
            
        }
    }
    public override string CardNameText { get { return "Fuel Card"; } protected set { } }
    public override string CardBottomText { get { return "Enable to move along longitude one up or down."; } protected set { } }

    public override bool CanPlayCard(Pawn _owner)
    {
        throw new System.NotImplementedException();
    }

    public override void OnPlayed(Pawn _owner, Card _pickedCard)
    {
        throw new System.NotImplementedException();
    }

    public bool CanPlayCard(Pawn _owner, int _numberOfFuelCards)
    {
        if(_owner.m_PawnInfo.m_NumberOfFuelCards < _numberOfFuelCards)
        {
            Toast.m_Instance.ShowMessage("You can't play fuel card. You need " + (_numberOfFuelCards - _owner.m_PawnInfo.m_NumberOfFuelCards) + " more fuel card");
            return false;
        }

        int latitude = _owner.m_PlayerPosition.x + _numberOfFuelCards;
        if (latitude < 0 || latitude >= 11)
        {
            Debug.Log("You can't use " + _numberOfFuelCards + " fuel cards...");
            return false;
        }

        foreach (var _pawn in GameplayManager.m_Instance.m_PawnsDict.Values)
        {
            if (_pawn.m_PlayerPosition == new Vector2Int(latitude, _owner.m_PlayerPosition.y))
            {
                Debug.Log("Two player can't be at the same latitude...");
                return false;
            }
        }

        return true;
    }

    public void OnPlayed(Pawn _owner, int _numberOfFuelCards)
    {
        int latitude = _owner.m_PlayerPosition.x + _numberOfFuelCards;
        _owner.Move(new Vector2Int(latitude, _owner.m_PlayerPosition.y));
        _owner.m_PawnInfo.m_NumberOfFuelCards -= Mathf.Abs(_numberOfFuelCards);
        _owner.OnCardPlayed();
    }
}
