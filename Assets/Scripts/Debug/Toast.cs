using System.Collections;
using UnityEngine;
using TMPro;

public class Toast : MonoBehaviour {
    public static Toast m_Instance { get; private set; }

    [SerializeField]
    private GameObject m_toastPanel;
    [SerializeField]
    private TextMeshProUGUI m_toastText;
    private Coroutine m_coroutine;

    private void Awake()
    {
        if(m_Instance != null)
        {
            DestroyImmediate(this);
            return;
        }

        m_Instance = this;
    }

    public void ShowMessage(string message, float time = 3)
    {
        if(m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
        }

        m_coroutine = StartCoroutine(IEnumShowMessage(message, time));
    }

    public void ShowMessageUntilinterrupt(string message)
    {
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
        }

        m_coroutine = StartCoroutine(IEnumShowMessage(message, isInterrupt:true));
    }

    private IEnumerator IEnumShowMessage(string message, float time = 3, bool isInterrupt = false)
    {
        m_toastPanel.SetActive(true);
        m_toastText.text = message;

        if (isInterrupt)
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            yield return new WaitForSeconds(time);
            m_toastPanel.SetActive(false);
            m_toastText.text = "";
        }
    }
}
