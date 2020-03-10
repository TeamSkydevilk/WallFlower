using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class fourTimer : MonoBehaviour
{
    private void OnDisable()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
        GameManager.instance.gamestate = GameManager.GAMESTATE.FINISH;        
    }
}
