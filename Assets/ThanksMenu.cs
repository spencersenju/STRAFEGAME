using UnityEngine;
using UnityEngine.SceneManagement;

public class ThanksMenu : MonoBehaviour
{
    public string mainMenuSceneName = "Main Menu"; 

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
