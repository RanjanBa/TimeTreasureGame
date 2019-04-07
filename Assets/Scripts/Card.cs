using UnityEngine;

public abstract class Card : ScriptableObject
{
    public abstract string CardTypeText { get; protected set; }

    public abstract string CardNameText { get; protected set; }

    public abstract string CardBottomText { get; protected set; }

    public abstract bool CanPlayCard(Pawn _owner);

    public abstract void OnPlayed(Pawn _owner, Card _pickedCard);
}
