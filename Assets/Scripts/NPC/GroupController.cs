using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupController : MonoBehaviour, Subject
{
    List<Observer> observers;

    public void RegisterObserver(Observer o)
    {
        ((NPCController)o).isGroup = true;
        observers.Add(o);
    }

    public void RemoveObserver(Observer o)
    {
        ((NPCController)o).isGroup = false;
        observers.Remove(o);
    }

    public void NotifyObservers(bool isCheck)
    {
        for (int i = 0; i < observers.Count; ++i)
        {
            Observer observer = observers[i];
            observer.Excute(isCheck);
        }
    }
    public _PlayerController playerController;
    IEnumerator enumerator; // GroupTalk을 저장한 변수
    bool isCoolTime;                    // 플레이어와 그룹이 접촉 후 10초가 지났는가?
    private void Awake()
    {
        observers = new List<Observer>();
        for (int i = 0; i < transform.childCount; i++)
        {
            RegisterObserver(transform.GetChild(i).GetComponent<NPCController>());
            ((NPCController)observers[i]).isGroup = true;
        }
        enumerator = GroupTalk();
        GameManager.OnGameReset += GameManager_OnGameReset;
        GameManager.OnGameStart += GameManager_OnGameStart;
    }

    private void GameManager_OnGameStart()
    {
        observers.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            RegisterObserver(transform.GetChild(i).GetComponent<NPCController>());
            ((NPCController)observers[i]).isGroup = true;
        }
        isCoolTime = false;
        enumerator = null;
        enumerator = GroupTalk();
        if (playerController == null)
            playerController = GameObject.Find("Player").transform.Find("TpsCam").GetComponent<_PlayerController>();
    }

    private void GameManager_OnGameReset()
    {
        if (enumerator != null)
            StopCoroutine(enumerator);
        StopAllCoroutines();
        enumerator = null;
        enumerator = GroupTalk();
        isCoolTime = false;
        if (playerController == null)
            playerController = GameObject.Find("Player").transform.Find("TpsCam").GetComponent<_PlayerController>();
    }


    public void EnterPlayer()
    {
        NotifyObservers(true);
    }
    public void ExitPlayer()
    {
        NotifyObservers(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCoolTime && !playerController.isTrigger)
        {
            playerController.isTrigger = true;
            playerController.isGreeting = false;
            EnterPlayer();
            playerController._StartCoroutine();
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !isCoolTime && playerController.handState == _PlayerController.HandState.Shake)
        {
            playerController.handState = _PlayerController.HandState.Idle;
            if (enumerator != null)
            {
                StopCoroutine(enumerator);
                enumerator = null;
                enumerator = GroupTalk();
            }
            else
            {
                enumerator = GroupTalk();
            }
            StartCoroutine(enumerator);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isCoolTime)
        {
            if (enumerator != null)
                StopCoroutine(enumerator);
            enumerator = null;
            StartCoroutine(Timer());
        }
    }
    IEnumerator GroupTalk()
    {
        int count = Random.Range(1, 4);
        int num = 0;
        bool isLoop = true;
        while (!playerController.isGreeting)
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.25f);
        while (isLoop)
        {
            yield return null;
            int idx = Random.Range(0, observers.Count);
            Vector3 v = ((NPCController)observers[idx]).transform.position - GameManager.instance.Player_Transform.position;
            v.x = v.z = 0;
            GameManager.instance.Player_Transform.LookAt(((NPCController)observers[idx]).transform.position - v);
            playerController.isMotion = false;
            yield return ((NPCController)observers[idx]).StartCoroutine(((NPCController)observers[idx]).Talk());
            GameManager.instance.Player_Transform.localRotation = Quaternion.Euler(Vector3.zero);
            num++;
            if (num >= count)
            {
                isLoop = false;
            }
        }
        playerController.isTrigger = false;
        playerController.isMotion = false;
        isCoolTime = true;
        StartCoroutine(Timer());
        ExitPlayer();
    }
    IEnumerator Timer()
    {
        isCoolTime = true;
        yield return new WaitForSecondsRealtime(10.0f);
        isCoolTime = false;
    }
}