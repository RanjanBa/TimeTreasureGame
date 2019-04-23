#pragma warning disable 649
using Firebase.Auth;
using TimeTreasure;
using UnityEngine;

public sealed class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager m_Instance
    {
        get;
        private set;
    }

    public FirebaseUser m_User
    {
        get
        {
            return m_user;
        }
    }

    private FirebaseAuth m_auth;
    private FirebaseUser m_user;

    [SerializeField]
    private SignUp m_signUp;
    [SerializeField]
    private SignIn m_signIn;
    [SerializeField]
    private GuestSignUp m_guestSignUp;

    private void Awake()
    {
        if(m_Instance != null)
        {
            DestroyImmediate(this);
            return;
        }
        m_Instance = this;
        InitializeFirebase();
        DontDestroyOnLoad(gameObject);
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (m_auth.CurrentUser != m_user)
        {
            bool signedIn = m_user != m_auth.CurrentUser && m_auth.CurrentUser != null;
            if (!signedIn && m_user != null)
            {
                Toast.m_Instance.ShowMessage("Signed out" + m_user.UserId, 10);
                MenuController.m_Instance.ShowMenuPage(MenuPage.OfflineOnlineMenuPage);
                GameManager.m_Instance.m_OwnerInfo = null;
                m_user = null;
                MenuController.m_Instance.OnAuthenticatinChangedUpdatePlayerInfo(false);
            }
            m_user = m_auth.CurrentUser;
            if (signedIn)
            {
                Toast.m_Instance.ShowMessage("Signed in " + m_user.UserId, 10);
                MenuController.m_Instance.OnAuthenticatinChangedUpdatePlayerInfo(true);
            }
        }
    }

    private void InitializeFirebase()
    {
        m_auth = FirebaseAuth.DefaultInstance;
        m_auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    public void EnableAuthenticationButtons()
    {
        if (m_signIn != null)
        {
            m_signIn.EnableSignInButton();
        }

        if (m_signUp != null)
        {
            m_signUp.EnableSignUpButton();
        }

        if (m_guestSignUp != null)
        {
            m_guestSignUp.EnableGuestSignUpButton();
        }
    }

    public void SignIn(string email, string password)
    {
        Toast.m_Instance.ShowMessageUntilinterrupt("user sign in... Is connected to internet : " + GameManager.m_Instance.IsConnectedToInternet);
        m_auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith((task) =>
        {
            if (task.IsCanceled)
            {
                Toast.m_Instance.ShowMessage("Sign in User with email and password async was canceled...", 5);
            }
            else if (task.IsFaulted)
            {
                Toast.m_Instance.ShowMessage("Sign in User with email and password async encountered an error: " + task.Exception, 5);
            }
            else
            {
                Toast.m_Instance.ShowMessage("Sign in completed " + task.Result.DisplayName);
                GameManager.m_Instance.m_OwnerInfo = new PlayerInfo(task.Result.DisplayName, task.Result.UserId);
                MenuController.m_Instance.ShowMenuPage(MenuPage.CreateJoinGameMenu);
            }
        });
    }

    public void SignUp(string userName, string email, string password)
    {
        Toast.m_Instance.ShowMessageUntilinterrupt("user sign up... Is connected to internet : " + GameManager.m_Instance.IsConnectedToInternet);
        m_auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith((task) =>
        {
            if (task.IsCanceled)
            {
                Toast.m_Instance.ShowMessage("Create User with email and password async was canceled...", 5);
            }
            else if (task.IsFaulted)
            {
                Toast.m_Instance.ShowMessage("Create User with email and password async encountered an error: " + task.Exception, 5);
            }
            else if (task.IsCompleted)
            {
                FirebaseRealtimeDatabase.m_Instance.AddNewUserToDatabase(userName, email, m_auth.CurrentUser.UserId);
                UserProfile userProfile = new UserProfile();
                userProfile.DisplayName = userName;
                m_auth.CurrentUser.UpdateUserProfileAsync(userProfile).ContinueWith(profileTask =>
                {
                    if (profileTask.IsCanceled || profileTask.IsFaulted)
                    {
                        Toast.m_Instance.ShowMessage("Profile update is canceled or faulted");
                    }
                    else if (profileTask.IsCompleted)
                    {
                        Toast.m_Instance.ShowMessage("Profile update is completed");
                        GameManager.m_Instance.m_OwnerInfo = new PlayerInfo(task.Result.DisplayName, task.Result.UserId);
                        MenuController.m_Instance.ShowMenuPage(MenuPage.CreateJoinGameMenu);
                    }
                });
            }
        });
    }

    public void GuestSignUp(string userName)
    {
        Toast.m_Instance.ShowMessageUntilinterrupt("user guest sign in... Is connected to internet : " + GameManager.m_Instance.IsConnectedToInternet);
        m_auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Toast.m_Instance.ShowMessage("SignInAnonymouslyAsync was canceled.", 5);
            }
            else if (task.IsFaulted)
            {
                Toast.m_Instance.ShowMessage("SignInAnonymouslyAsync encountered an error: " + task.Exception, 5);
            }
            else if(task.IsCompleted)
            {
                FirebaseRealtimeDatabase.m_Instance.AddNewUserToDatabase(userName, "", m_auth.CurrentUser.UserId);
                UserProfile userProfile = new UserProfile();
                userProfile.DisplayName = userName;
                m_auth.CurrentUser.UpdateUserProfileAsync(userProfile).ContinueWith(profileTask =>
                {
                    if (profileTask.IsCanceled || profileTask.IsFaulted)
                    {
                        Toast.m_Instance.ShowMessage("Profile update is canceled or faulted");
                    }
                    else if( profileTask.IsCompleted)
                    {
                        Toast.m_Instance.ShowMessage("Profile update is completed");
                        GameManager.m_Instance.m_OwnerInfo = new PlayerInfo(task.Result.DisplayName, task.Result.UserId);
                        MenuController.m_Instance.ShowMenuPage(MenuPage.CreateJoinGameMenu);
                    }
                });
            }
        });
    }

    public void SignOut()
    {
        if (m_auth != null)
        {
            m_auth.SignOut();
            MenuController.m_Instance.OnAuthenticatinChangedUpdatePlayerInfo(false);
        }
    }
}
