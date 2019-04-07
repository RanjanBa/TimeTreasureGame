using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuestSignUp : MonoBehaviour {
    [SerializeField]
    private TMP_InputField m_userName;
    [SerializeField]
    private Button m_guestSignUpButton;

    private void Start()
    {
        m_guestSignUpButton.onClick.AddListener(() => OnGeustSignUpButtonPressed());
    }

    private void OnGeustSignUpButtonPressed()
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
        m_guestSignUpButton.interactable = false;
        AuthenticationManager.m_Instance.GuestSignUp(m_userName.text);
    }

    public void EnableGuestSignUpButton()
    {
        m_guestSignUpButton.interactable = true;
    }
}
