using UnityEngine;

public class testAnimation : MonoBehaviour
{
    [SerializeField]
    private Animation m_animation;
    
    public void Play()
    {
        Debug.Log("Play");
        m_animation["test"].speed = 1f;
        m_animation.Play();
    }

    public void Rewind()
    {
        if(m_animation["test"].time <= 0.01f)
        {
            m_animation["test"].time = m_animation["test"].length;
        }

        Debug.Log("Rewind");
        m_animation["test"].speed = -1f;
        m_animation.Play();
    }
}
