using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoulMate : MonoBehaviour
{
    public Object[] RandomObj;
    private void Awake()
    {
        GameManager.OnGameStart += GameManager_OnGameStart;
        GameManager.OnGameReset += GameManager_OnGameReset;
    }

    private void GameManager_OnGameReset()
    {
        StopAllCoroutines();
    }

    private void GameManager_OnGameStart()
    {
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.CompareTag("Player"))
        {
            Debug.Log("FIND SOULMATE");
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }
}