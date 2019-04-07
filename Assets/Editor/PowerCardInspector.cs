using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PowerCard))]
public class PowerCardInspector : Editor
{
    private PowerCard m_powerCard;

    private void OnEnable()
    {
        m_powerCard = target as PowerCard;
    }

    public override void OnInspectorGUI()
    {
        m_powerCard.m_powerType = (PowerTypes)EditorGUILayout.EnumPopup("Power Type", m_powerCard.PowerType);

        switch (m_powerCard.PowerType)
        {
            case PowerTypes.PosX_NegX:
                EditorGUILayout.IntField("Step ",m_powerCard.m_step);
                break;
            default:
                break;
        }
        
    }
}
