using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinyCS : MonoBehaviour
{
    Vector3 InitPosition;
    Quaternion InitQuaternion;
    private void Awake()
    {
        InitPosition = transform.position;
        InitQuaternion = transform.rotation;
        GameManager.OnGameReset += GameManager_OnGameReset;
    }
    void OnDestroy()
    {
        GameManager.OnGameReset -= GameManager_OnGameReset;
    }
    private void GameManager_OnGameReset()
    {
        transform.position = InitPosition;
        transform.rotation = InitQuaternion;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.instance.StopAudio(0);
            SoundManager.instance.StopAudio(1);
            GameManager.instance.gamestate = GameManager.GAMESTATE.FINISH;
            ChinemachineManager.instance.StartChinema(4);
        }
    }
}