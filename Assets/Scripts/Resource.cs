using System.Collections.Generic;

public class Chest
{
    public int Point;
    public int NumberOfFuelCard;
    public List<Coin> Coins;

    public Chest(int _point, int _numberOfFuelCard, List<Coin> _coins)
    {
        Point = _point;
        NumberOfFuelCard = _numberOfFuelCard;
        Coins = _coins;
    }
}