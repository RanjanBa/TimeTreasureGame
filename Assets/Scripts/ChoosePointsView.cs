using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChoosePointsView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_chooseText;
    [SerializeField]
    private Value m_choosePointPrefab;
    [SerializeField]
    private Transform m_container;

    private List<Value> m_values = new List<Value>();

    public void ShowAllChoosePoints(Pawn _owner, PowerCard _powerCard)
    {
        gameObject.SetActive(true);
        if (_powerCard.m_powerType == PowerTypes.LongitudeMaster || _powerCard.m_powerType == PowerTypes.PosX_NegX || _powerCard.m_powerType == PowerTypes.ThreePoints)
        {
            return;
        }
        m_chooseText.text = "";
        if (_powerCard.m_powerType == PowerTypes.GMTMaster)
        {
            m_chooseText.text = "Choose GMT time as strategy";
        }
        else if(_powerCard.m_powerType == PowerTypes.Master)
        {
            m_chooseText.text = "Choose GMT Point where you want to jump";
        }
        
        foreach (var _value in m_values)
        {
            DestroyImmediate(_value.gameObject);
        }

        m_values.Clear();

        for (int i = 1; i <= GameplayManager.r_TotalLongitude; i++)
        {
            Value _value = Instantiate(m_choosePointPrefab, m_container);
            _value.UpdateValue(_owner, _powerCard, i);
            m_values.Add(_value);
        }
    }
}
