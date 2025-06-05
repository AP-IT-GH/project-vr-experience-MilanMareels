using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void SceneChanger(int sceneNumber)
    {
           SceneManager.LoadScene(sceneNumber);
    }
}
