using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ChinemachineManager : MonoBehaviour
{
    public static ChinemachineManager instance;
    //시네머신 타임라인 에셋
    public PlayableDirector CM_One;
    public PlayableDirector CM_Two;
    public PlayableDirector CM_Three;
    public PlayableDirector CM_Four;
    public bool isPlay = true;
    public bool isStart = false;
    public GameObject[] gameObjects;
    private void Awake()
    {
        if (instance == null)
        {
            instance = GetComponent<ChinemachineManager>();
        }
        else
        {
            Destroy(this);
        }
        isPlay = true;
        GameManager.OnGameReset += GameManager_OnGameReset;
        gameObjects[0].SetActive(true);
        gameObjects[1].SetActive(true);
        gameObjects[2].SetActive(false);
        gameObjects[3].SetActive(true);
    }
    private void GameManager_OnGameReset()
    {
        isPlay = true;
        isStart = false;
        GameManager.instance.Player_Transform.gameObject.SetActive(true);
        if (CM_One != null)
        {
            if (CM_One.state == PlayState.Playing)
                CM_One.Stop();
            CM_One.time = 0;
        }
        if (CM_Two != null)
        {
            if (CM_Two.state == PlayState.Playing)
                CM_Two.Stop();
            CM_Two.time = 0;
        }
        if (CM_Three != null)
        {
            if (CM_Three.state == PlayState.Playing)
                CM_Three.Stop();
            CM_Three.time = 0;
        }
        if (CM_Four != null)
        {
            if (CM_Four.state == PlayState.Playing)
                CM_Four.Stop();
            CM_Four.time = 0;
        }
        gameObjects[0].SetActive(true);
        gameObjects[1].SetActive(true);
        gameObjects[2].SetActive(false);
        gameObjects[3].SetActive(true);
    }
    public void StartChinema(int index)
    {
        GameManager.instance.StopAllCoroutines();
        isStart = true;
        GameManager.instance.Player_Transform.gameObject.SetActive(false);
        gameObjects[3].SetActive(false);
        if(CM_One.state != PlayState.Playing&&  CM_Three.state != PlayState.Playing&& CM_Four.state != PlayState.Playing)
        switch (index)
        {
            case 1:
                {
                    // 6분이 지나면 실행되는 시네마 
                    if (CM_One != null && CM_One.state != PlayState.Playing)
                        CM_One.Play();
                    break;
                }
            case 2:
                {
                    // 체력이 0이되면 실행되는 시네마
                    if (CM_Two != null && CM_Two.state != PlayState.Playing)
                        CM_Two.Play();
                    break;
                }
            case 3:
                {
                    // 존재감이 0이되면 실행되는 시네마
                    if (CM_Three != null && CM_Three.state != PlayState.Playing)
                        CM_Three.Play();
                    break;
                }
            case 4:
                {
                    // 시크릿엔딩(숨겨진 캐릭터 터치시 실행되는 시네마)
                    if (CM_Four != null && CM_Four.state != PlayState.Playing)
                        CM_Four.Play();
                    break;
                }
        }
    }
}