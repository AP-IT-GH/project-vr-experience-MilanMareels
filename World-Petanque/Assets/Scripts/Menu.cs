using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void Startgame(int sceneNumber)
    {
           SceneManager.LoadScene(sceneNumber);
    }
}
