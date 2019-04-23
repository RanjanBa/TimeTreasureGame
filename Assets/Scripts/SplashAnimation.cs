using UnityEngine;
using TimeTreasure;

public class SplashAnimation : MonoBehaviour
{
    [SerializeField]
    private Animation m_splashAnimation;
    [SerializeField]
    private MenuController m_menuController;

    private void Start()
    {
        AnimationEvent _animationEvent = new AnimationEvent
        {
            functionName = "ShowOfflineOnlinePanel",
            time = m_splashAnimation.clip.length
        };
        m_splashAnimation.clip.AddEvent(_animationEvent);
    }

    public void ShowOfflineOnlinePanel()
    {
        m_menuController.ShowMenuPage(MenuPage.OfflineOnlineMenuPage);
    }
}
