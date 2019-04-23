using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This will manage coins and treasures
public class ResourceManager : MonoBehaviour
{
    public const string r_CoinKey = "coins";
    public const string r_TreasureKey = "treasures";

    public static ResourceManager m_Instance { get; private set; }

    private const int r_maxCoin = 30;
    private const int r_maxTreasure = 10;

    [SerializeField, Range(1, r_maxCoin)]
    private int m_totalCoinNumber = 2;
    [SerializeField, Range(1, r_maxTreasure)]
    private int m_totalTreasureNumber = 1;
    [SerializeField]
    private Slider m_coinSlider, m_treasureSlider;
    [SerializeField]
    private TextMeshProUGUI m_coinCountText, m_treasureCountText;
    [SerializeField]
    private Treasure m_treasurePrefab;
    [SerializeField]
    private Coin m_coinPrefab;
    [SerializeField]
    private Sprite m_broneSprite, m_silverSprite, m_goldSprite, m_treasureSprite;

    private List<int> m_shuffleAllCoinPositions = new List<int>();
    private List<int> m_shuffleAllTreasurePositions = new List<int>();

    private List<Treasure> m_remainingTreasures;
    private List<Coin> m_remainingCoins;

    public Sprite m_BronzeSprite { get { return m_broneSprite; } }
    public Sprite m_SilverSprite { get { return m_silverSprite; } }
    public Sprite m_GoldSprite { get { return m_goldSprite; } }
    public Sprite m_TreasureSprite { get { return m_treasureSprite; } }

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

    private void Start()
    {
        if (GameManager.m_Instance == null)
        {
            Debug.LogWarning("GameManager instance is null");
            return;
        }

        if (GameManager.m_Instance.m_GameType == GameType.Online)
        {
            if (GameManager.m_Instance.m_GameInfo.m_CreatorUID == AuthenticationManager.m_Instance.m_User.UserId)
            {
                if (m_coinSlider != null)
                {
                    m_coinSlider.interactable = true;
                    m_coinSlider.onValueChanged.AddListener((val) =>
                    {
                        SetTotalCoinNumber(Mathf.RoundToInt(val));
                    });
                }

                if (m_treasureSlider != null)
                {
                    m_treasureSlider.interactable = true;
                    
                    m_treasureSlider.onValueChanged.AddListener((val) =>
                    {
                        SetTotalTreasureNumber(Mathf.RoundToInt(val));
                    });
                }
            }
            else
            {
                if (m_coinSlider != null)
                {
                    m_coinSlider.interactable = false;
                    m_coinSlider.value = m_totalCoinNumber;
                }

                if (m_treasureSlider != null)
                {
                    m_treasureSlider.interactable = false;
                    m_treasureSlider.value = m_totalTreasureNumber;
                }
            }
        }
        else
        {
            if (m_coinSlider != null)
            {
                m_coinSlider.interactable = true;
                m_coinSlider.value = m_totalCoinNumber;
                m_coinSlider.onValueChanged.AddListener((val) =>
                {
                    SetTotalCoinNumber(Mathf.RoundToInt(val));
                });
            }

            if (m_treasureSlider != null)
            {
                m_treasureSlider.interactable = true;
                m_treasureSlider.value = m_totalTreasureNumber;
                m_treasureSlider.onValueChanged.AddListener((val) =>
                {
                    SetTotalTreasureNumber(Mathf.RoundToInt(val));
                });
            }
        }

        if (m_coinSlider != null)
        {
            m_coinSlider.minValue = 1;
            m_coinSlider.maxValue = r_maxCoin;
            m_coinSlider.value = m_totalCoinNumber;
        }

        if (m_treasureSlider != null)
        {
            m_treasureSlider.minValue = 1;
            m_treasureSlider.maxValue = r_maxTreasure;
            m_treasureSlider.value = m_totalTreasureNumber;
        }

        SetTotalCoinNumber(m_totalCoinNumber);
        SetTotalTreasureNumber(m_totalTreasureNumber);
    }

    private void SetTotalCoinNumber(int _coinNumber)
    {
        m_totalCoinNumber = _coinNumber;
        if (m_coinCountText != null)
        {
            m_coinCountText.text = m_totalCoinNumber + "";
        }
    }

    private void SetTotalTreasureNumber(int _treasureNumber)
    {
        m_totalTreasureNumber = _treasureNumber;
        if (m_treasureCountText != null)
        {
            m_treasureCountText.text = m_totalTreasureNumber + "";
        }
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
            treasure.Init(_position, new Chest(_numberOfFuelCard: 5, _point: 15));
            m_remainingTreasures.Add(treasure);
        }

        foreach (var _value in m_shuffleAllCoinPositions)
        {
            int x = _value / GameplayManager.r_TotalLongitude;
            int y = _value % GameplayManager.r_TotalLongitude;
            Vector2Int _position = new Vector2Int(x, y);
            Debug.Log("position coin : " + _position);

            Coin coin = Instantiate(m_coinPrefab, GameplayManager.m_Instance.m_BoardGMTPoints[_position.x, _position.y]);
            int mode = _value % 3;
            if (mode == 0)
            {
                coin.Init(_position, CoinType.Bronze);
                m_remainingCoins.Add(coin);
            }
            else if (mode == 1)
            {
                coin.Init(_position, CoinType.Silver);
                m_remainingCoins.Add(coin);
            }
            else if (mode == 2)
            {
                coin.Init(_position, CoinType.Gold);
                m_remainingCoins.Add(coin);
            }
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
            if(treasure.m_Position == _position)
            {
                _treasure = treasure;
                return true;
            }
        }

        foreach (var coin in m_remainingCoins)
        {
            if(coin.m_Position == _position)
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
