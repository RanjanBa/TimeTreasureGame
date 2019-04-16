using UnityEngine;
using UnityEngine.UI;

public enum CoinType
{
    Bronze,
    Silver,
    Gold,
}

[RequireComponent(typeof(Image))]
public class Coin : MonoBehaviour
{
    public int m_Point { get; private set; }
    public Vector2Int m_Position { get; private set; }

    private Image m_image;
    private CoinType m_coinType;

    private void Start()
    {
        m_image = GetComponent<Image>();
    }

    public void Init(Vector2Int _position, CoinType _coinType)
    {
        if (m_image == null)
        {
            m_image = GetComponent<Image>();
        }

        if (m_image == null)
        {
            Debug.LogWarning("Image of coin is null");
            return;
        }

        m_coinType = _coinType;
        m_Position = _position;
        switch (_coinType)
        {
            case CoinType.Bronze:
                m_image.sprite = ResourceManager.m_Instance.m_BronzeSprite;
                m_Point = 3;
                break;
            case CoinType.Silver:
                m_image.sprite = ResourceManager.m_Instance.m_SilverSprite;
                m_Point = 5;
                break;
            case CoinType.Gold:
                m_image.sprite = ResourceManager.m_Instance.m_GoldSprite;
                m_Point = 10;
                break;
            default:
                break;
        }
    }
}
