using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class RotatingCircle : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_text;

    public void SetText(string _text)
    {
        m_text.text = _text;
    }
}
