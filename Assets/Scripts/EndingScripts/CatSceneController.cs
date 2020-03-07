using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CatSceneController : MonoBehaviour
{
    private void OnDisable()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
