using UnityEngine;
using UnityEngine.SceneManagement;

public class CatTemp : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
