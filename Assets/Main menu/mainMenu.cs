using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public void LoadGame()
    {
        // Load the game scene
        SceneManager.LoadScene(1);
    }
}