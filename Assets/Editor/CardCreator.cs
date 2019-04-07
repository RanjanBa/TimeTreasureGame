using System.Collections;
using UnityEditor;
using UnityEngine;

public class CardCreator : EditorWindow
{
    private static CardCreator m_window;

    [MenuItem("Call", menuItem = "Window/CardCreator")]
    private static void Menu()
    {
        m_window = GetWindow<CardCreator>();
        m_window.minSize = new Vector2(400, 200);
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 36;
        GUILayout.Label("Card Creator", style, GUILayout.Height(100));

        if (GUILayout.Button("Create TrapCards"))
        {
            Debug.Log("Creating Trap cards");
            CreateTrapCards();
        }

        if (GUILayout.Button("Create HourCards"))
        {
            Debug.Log("Creating Hour cards");
            CreateHourCards();
        }

        if (GUILayout.Button("Create PowerCards"))
        {
            Debug.Log("Creating Power cards");
            CreatePowerCards();
        }
    }

    private void CreateHourCards()
    {
        for(int i = 1; i <= 24; i++)
        {
            HourCard hrCard = CreateInstance<HourCard>().Init(i);
            AssetDatabase.CreateAsset(hrCard, "Assets/Resources/CardAssets/HourCards/HourCard" + i + ".asset");
        }
        AssetDatabase.SaveAssets();
    }

    private void CreateTrapCards()
    {
        for (int i = -11; i <= 12; i++)
        {
            TrapCard trCard = CreateInstance<TrapCard>().Init(i);
            AssetDatabase.CreateAsset(trCard, "Assets/Resources/CardAssets/TrapCards/TrapCard" + i + ".asset");
        }
        AssetDatabase.SaveAssets();
    }

    private void CreatePowerCards()
    {
        for (int i = 1; i <= 2; i++)
        {
            PowerCard prCard = CreateInstance<PowerCard>().Init(PowerTypes.Master);
            AssetDatabase.CreateAsset(prCard, "Assets/Resources/CardAssets/PowerCards/Power_Master" + i + ".asset");
        }
        AssetDatabase.SaveAssets();

        for (int i = 1; i <= 4; i++)
        {
            PowerCard prCard = CreateInstance<PowerCard>().Init(PowerTypes.GMTMaster);
            AssetDatabase.CreateAsset(prCard, "Assets/Resources/CardAssets/PowerCards/Power_GMTMaster" + i + ".asset");
        }
        AssetDatabase.SaveAssets();

        for (int i = 1; i <= 2; i++)
        {
            PowerCard prCard = CreateInstance<PowerCard>().Init(PowerTypes.LongitudeMaster);
            AssetDatabase.CreateAsset(prCard, "Assets/Resources/CardAssets/PowerCards/Power_LongitudeMaster" + i + ".asset");
        }
        AssetDatabase.SaveAssets();

        for (int i = 1; i <= 2; i++)
        {
            PowerCard prCard = CreateInstance<PowerCard>().Init(PowerTypes.ThreePoints);
            AssetDatabase.CreateAsset(prCard, "Assets/Resources/CardAssets/PowerCards/Power_ThreePoints" + i + ".asset");
        }
        AssetDatabase.SaveAssets();

        for (int i = -6; i <= 6; i++)
        {
            if (i == 0)
                continue;

            PowerCard prCard = CreateInstance<PowerCard>().Init(PowerTypes.PosX_NegX, i);
            AssetDatabase.CreateAsset(prCard, "Assets/Resources/CardAssets/PowerCards/Power_PosX_NegX" + i + ".asset");
        }
        AssetDatabase.SaveAssets();
    }
}
