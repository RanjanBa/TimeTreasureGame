using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
// For debuging purpose
// Show Toast message like android platform
public class Toast : MonoBehaviour {
    public static Toast m_Instance { get; private set; }

    [SerializeField]
    private GameObject m_toastPanel;
    [SerializeField]
    private TextMeshProUGUI m_toastText;
    [SerializeField]
    private Button m_closeToastButton;
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

    private void Start()
    {
        if(m_coroutine == null)
        {
            m_toastPanel.SetActive(false);
        }
        m_closeToastButton.onClick.RemoveAllListeners();
        m_closeToastButton.onClick.AddListener(() => {
            if(m_coroutine != null)
            {
                StopCoroutine(m_coroutine);
                m_toastPanel.SetActive(false);
                m_toastText.text = "";
            }
        });
    }

    public void ShowMessage(string message, float time = 3)
    {
        if(m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
            m_toastPanel.SetActive(false);
            m_toastText.text = "";
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
