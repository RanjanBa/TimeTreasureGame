using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameInfo
{
    public string m_GameName { get; private set; }
    public string m_CreatorUID { get; private set; }

    public GameInfo(string gameName, string creatorUID)
    {
        m_GameName = gameName;
        m_CreatorUID = creatorUID;
    }
}

public class Game : MonoBehaviour {
    public TextMeshProUGUI m_GameNameText;
    public Button m_JoinButton;
    [HideInInspector]
    public string m_CreatorUID;
}
