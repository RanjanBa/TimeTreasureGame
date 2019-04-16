using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Treasure : MonoBehaviour
{
    public Vector2Int m_Position { get; private set; }
    public Chest m_Chest { get; private set; }

    private Image m_image;

    private void Start()
    {
        m_image = GetComponent<Image>();
    }

    public void Init(Vector2Int _positon, Chest _chest)
    {
        if(m_image == null)
        {
            m_image = GetComponent<Image>();
        }

        if(m_image == null)
        {
            Debug.LogWarning("Image of treasure is null");
            return;
        }
        m_image.sprite = ResourceManager.m_Instance.m_TreasureSprite;
        m_Position = _positon;
        m_Chest = _chest;
    }
}
