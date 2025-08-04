using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("PlayGame button clicked");
        SceneManager.LoadScene("Level 1");
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame button clicked");
        Application.Quit();
    }
}
