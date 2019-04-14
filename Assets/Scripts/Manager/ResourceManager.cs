using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// This will manage coins and treasures
public class ResourceManager : MonoBehaviour
{
    public static readonly string r_CoinKey = "coins";
    public static readonly string r_TreasureKey = "treasures";

    public static ResourceManager m_Instance { get; private set; }

    [SerializeField, Range(1, 30)]
    private int m_totalCoinNumber = 2;
    [SerializeField, Range(1, 10)]
    private int m_totalTreasureNumber = 1;
    [SerializeField]
    private Treasure m_treasurePrefab;
    [SerializeField]
    private Coin m_coinPrefab;

    private List<int> m_shuffleAllCoinPositions = new List<int>();
    private List<int> m_shuffleAllTreasurePositions = new List<int>();

    private List<Treasure> m_remainingTreasures;
    private List<Coin> m_remainingCoins;

    public int RemainingTreasureCount
    {
        get
        {
            return m_remainingTreasures.Count;
        }
    }

    public int RemainingCoinCount
    {
        get
        {
            return m_remainingCoins.Count;
        }
    }

    private void Awake()
    {
        if (m_Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        m_Instance = this;
        m_remainingTreasures = new List<Treasure>();
        m_remainingCoins = new List<Coin>();
    }

    public void GenerateAllCoinsAndTreasures()
    {
        foreach (var _value in m_shuffleAllTreasurePositions)
        {
            int x = _value / GameplayManager.r_TotalLongitude;
            int y = _value % GameplayManager.r_TotalLongitude;
            Vector2Int _position = new Vector2Int(x, y);

            Debug.Log("position t : " + _position);

            Treasure treasure = Instantiate(m_treasurePrefab, GameplayManager.m_Instance.m_BoardGMTPoints[_position.x, _position.y]);
            treasure.m_Chest = new Chest(_coins: null, _numberOfFuelCard: 5, _point: 10);
            treasure.Position = _position;
            m_remainingTreasures.Add(treasure);
        }

        foreach (var _value in m_shuffleAllCoinPositions)
        {
            int x = _value / GameplayManager.r_TotalLongitude;
            int y = _value % GameplayManager.r_TotalLongitude;
            Vector2Int _position = new Vector2Int(x, y);
            Debug.Log("position coin : " + _position);

            Coin coin = Instantiate(m_coinPrefab, GameplayManager.m_Instance.m_BoardGMTPoints[_position.x, _position.y]);
            coin.Point = 5;
            coin.Position = _position;
            m_remainingCoins.Add(coin);
        }
    }

    public Dictionary<string, List<int>> ShuffleAllCoinAndTreasurePositions()
    {
        List<int> _shuffleAllCoinPositions = new List<int>();
        List<int> _shuffleAllTreasurePositions = new List<int>();

        var _randGen = new System.Random();
        var _values = Enumerable.Range(24, 24 * 10).OrderBy(x => _randGen.Next()).ToArray();
        for (int i = 0; i < m_totalCoinNumber; i++)
        {
            _shuffleAllCoinPositions.Add(_values[i]);
        }

        for (int i = 0; i < m_totalTreasureNumber; i++)
        {
            _shuffleAllTreasurePositions.Add(_values[m_totalCoinNumber + i]);
        }
        Dictionary<string, List<int>> _shuffleAllCoinAndTreasurePositions = new Dictionary<string, List<int>>();
        _shuffleAllCoinAndTreasurePositions.Add(r_CoinKey, _shuffleAllCoinPositions);
        _shuffleAllCoinAndTreasurePositions.Add(r_TreasureKey, _shuffleAllTreasurePositions);
        return _shuffleAllCoinAndTreasurePositions;
    }

    public void SetAllCoinAndTreasuresPositions(List<int> _shuffleAllCoinPositions, List<int> _shuffleAllTreasurePositions)
    {
        m_shuffleAllCoinPositions = _shuffleAllCoinPositions;
        m_shuffleAllTreasurePositions = _shuffleAllTreasurePositions;
    }

    public bool CheckAnyResourceAtIndex(Vector2Int _position, out Treasure _treasure, out Coin _coin)
    {
        _treasure = null;
        _coin = null;
        foreach (var treasure in m_remainingTreasures)
        {
            if(treasure.Position == _position)
            {
                _treasure = treasure;
                return true;
            }
        }

        foreach (var coin in m_remainingCoins)
        {
            if(coin.Position == _position)
            {
                _coin = coin;
                return true;
            }
        }

        return false;
    }

    public void RemoveTreasure(Treasure _treasure)
    {
        if(m_remainingTreasures.Contains(_treasure))
        {
            m_remainingTreasures.Remove(_treasure);
        }
    }

    public void RemoveCoin(Coin _coin)
    {
        if(m_remainingCoins.Contains(_coin))
        {
            m_remainingCoins.Remove(_coin);
        }
    }

    public bool IsAnyTreasureOrCoinLeft()
    {
        if(m_remainingCoins.Count <= 0 && m_remainingTreasures.Count <= 0)
        {
            return false;
        }

        return true;
    }
}
