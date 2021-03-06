﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinedPlayerInfo : MonoBehaviour {
    public Color m_PlayerColorCode;
    public TextMeshProUGUI m_playerNameText;
    //If you want to show player index
    //public TextMeshProUGUI m_playerIndexText;
    public Button m_removeButton;

    [SerializeField]
    private Image m_avaterHolder;

    private void Start()
    {
        m_avaterHolder.color = m_PlayerColorCode;
    }
}
