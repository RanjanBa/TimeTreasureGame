using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUp : MonoBehaviour {
    [SerializeField]
    private TMP_InputField m_userName;
    [SerializeField]
    private TMP_InputField m_emailAddress;
    [SerializeField]
    private TMP_InputField m_password;
    [SerializeField]
    private Button m_signUpButton;

    private void Start()
    {
        m_signUpButton.onClick.AddListener(() => OnSignUpButtonPressed());
    }

    public void EnableSignUpButton()
    {
        Toast.m_Instance.ShowMessage("Enable button sign up1");
        m_signUpButton.interactable = true;
        Toast.m_Instance.ShowMessage("Enable button sign up2");
    }

    public void OnSignUpButtonPressed()
    {
        //if (GameManager.m_Instance.IsConnectedToInternet == false)
        //{
        //    Toast.m_Instance.ShowMessage("You are not connected to the internet");
        //    return;
        //}

        if (m_userName.text == "")
        {
            Toast.m_Instance.ShowMessage("Please enter user name");
            Debug.LogWarning("Please enter user name");
            return;
        }

        if (m_emailAddress.text == "")
        {
            Toast.m_Instance.ShowMessage("Please enter email address text");
            Debug.LogWarning("Please enter email address text");
            return;
        }

        if (m_password.text == "")
        {
            Toast.m_Instance.ShowMessage("Please enter password");
            Debug.LogWarning("Please enter password");
            return;
        }

        m_signUpButton.interactable = false;
        AuthenticationManager.m_Instance.SignUp(m_userName.text, m_emailAddress.text, m_password.text);
    }
}
