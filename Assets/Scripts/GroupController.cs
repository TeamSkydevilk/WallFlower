using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupController : MonoBehaviour
{
    public _PlayerController playerController;
    IEnumerator enumerator; // GroupTalk을 저장한 변수
    List<NPCController> groupNPCList;   // 각 그룹별 NPC 리스트
    bool isCoolTime;                    // 플레이어와 그룹이 접촉 후 10초가 지났는가?
    private void Awake()
    {
        groupNPCList = new List<NPCController>();
        for (int i = 0; i < transform.childCount; i++)
        {
            groupNPCList.Add(transform.GetChild(i).GetComponent<NPCController>());
            groupNPCList[i].isGroup = true;
        }
        enumerator = GroupTalk();
        GameManager.OnGameReset += GameManager_OnGameReset;
    }

    private void GameManager_OnGameReset()
    {
        if (enumerator != null)
            StopCoroutine(enumerator);
        StopAllCoroutines();
        enumerator = null;
        enumerator = GroupTalk();
        isCoolTime = false;
    }

    /*
     * NPC가 그룹에 들어왔을 때의 함수
     * 매개변수로 NPCController 받아서 리스트에 추가
     */
    public void Join(NPCController _npc)
    {
        _npc.isGroup = true;
        groupNPCList.Add(_npc);
    }

    /*
     * NPC가 그룹에 나갈 때의 함수
     * 매개변수로 NPCController 받아서 리스트에서 제거
     */
    public void Leave(NPCController _npc)
    {
        for (int i = 0; i < groupNPCList.Count; i++)
        {
            if (groupNPCList[i] == _npc)
            {
                _npc.isGroup = false;
                groupNPCList.RemoveAt(i);
                return;
            }
        }
    }

    //Player와 그룹이 충돌했을 때
    public void EnterPlayer()
    {
        for (int i = 0; i < groupNPCList.Count; i++)
        {
            groupNPCList[i].isTrigger = true;
            if (Random.Range(1, 101) < 50)
            {
                groupNPCList[i].animator.SetInteger("Animation_int", 0);
                groupNPCList[i].animator.SetInteger("State", (int)NPCController.STATE.HI);
            }
        }
        StartCoroutine(HIEmotion());
    }
    IEnumerator HIEmotion()
    {
        for (int i = 0; i < groupNPCList.Count; i++)
        {
            groupNPCList[i].tooltip.gameObject.SetActive(true);
          //  groupNPCList[i].tooltip.transform.LookAt(groupNPCList[i].tooltip.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
            groupNPCList[i].tooltip.sprite = groupNPCList[i].tooltipSprite[8];
        }
        yield return new WaitForSecondsRealtime(1.0f);
        for (int i = 0; i < groupNPCList.Count; i++)
        {
            groupNPCList[i].tooltip.gameObject.SetActive(false);

            groupNPCList[i].tooltip.sprite = groupNPCList[i].tooltipSprite[(int)groupNPCList[i].tooltipColor];
        }
    }
    //Player가 그룹에서 나갔을 때
    public void ExitPlayer()
    {
        for (int i = 0; i < groupNPCList.Count; i++)
        {
            groupNPCList[i].isTrigger = false;
        }
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
            int idx = Random.Range(0, groupNPCList.Count);
            Vector3 v = groupNPCList[idx].transform.position- GameManager.instance.Player_Transform.position;
            v.x = v.z = 0;
            GameManager.instance.Player_Transform.LookAt(groupNPCList[idx].transform.position - v);
            playerController.isMotion = false;
            yield return groupNPCList[idx].StartCoroutine(groupNPCList[idx].Talk());
    
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
        yield return StartCoroutine(YieldTime(10.0f));
        isCoolTime = false;
    }
    IEnumerator YieldTime(float _Time)
    {
        float _time = 0f;
        bool isLoop = true;
        while (isLoop)
        {
            yield return new WaitForFixedUpdate();
            _time += Time.fixedDeltaTime;
            if (_time > _Time)
            {
                isLoop = false;
            }
        }
    }
}