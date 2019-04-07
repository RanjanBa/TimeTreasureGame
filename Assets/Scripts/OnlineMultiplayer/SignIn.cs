using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SignIn : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField m_emailAddress;
    [SerializeField]
    private TMP_InputField m_password;
    [SerializeField]
    private Button m_signInButton;

    private void Awake()
    {
        m_signInButton.onClick.AddListener(() => OnSignInButtonPressed());
    }

    private void OnSignInButtonPressed()
    {
        //if (GameManager.m_Instance.IsConnectedToInternet == false)
        //{
        //    Toast.m_Instance.ShowMessage("You are not connected to the internet");
        //    return;
        //}
        
        if(m_emailAddress.text == "")
        {
            Toast.m_Instance.ShowMessage("Please enter email address text");
            Debug.LogWarning("Please enter email address text");
            return;
        }

        if(m_password.text == "")
        {
            Toast.m_Instance.ShowMessage("Please enter password");
            Debug.LogWarning("Please enter password");
            return;
        }

        m_signInButton.interactable = false;
        AuthenticationManager.m_Instance.SignIn(m_emailAddress.text, m_password.text);
    }

    public void EnableSignInButton()
    {
        m_signInButton.interactable = true;
    }
}
