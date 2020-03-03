using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    public GameObject Player;       // Player의 위치를 받기 위함.
    public _PlayerController playerController;
    public bool isGroup;
    public bool isTrigger;          // Player와 접촉을 했는지에 대한 변수
    public Animator animator;
    public float dir1;
    public float dir2;
    IEnumerator enumerator;
    public bool isTimer;
    GroupController groupController;
    Quaternion initQuaternion;
    Vector3 initPosition;
    IEnumerator RanEnumerator;
    public enum STATE { IDLE, HI, TALK, WALK }
    public enum DIR { up,down,right,left}
    DIR dir;
    public STATE state;
    public bool isTalk;
    public enum TooltipColor { Black, Red, Blue, Yellow, Green, Orange, Claret, Purple }
    public TooltipColor tooltipColor;
    public UnityEngine.UI.Image tooltip;
    public GameObject tooltipCanvas;
    public Sprite[] tooltipSprite;
    bool isExit;
    bool isBack;
    // Use this for initialization
    private void Awake()
    {
        groupController = GetComponentInParent<GroupController>();
        animator = GetComponent<Animator>();
        initQuaternion = transform.rotation;
        initPosition = transform.position;
        state = STATE.TALK;
        animator.SetInteger("Animation_int", Random.Range(0, 9));
        animator.SetInteger("State", (int)state);
        GameManager.OnGameStart += GameManager_OnGameStart;
        GameManager.OnGameReset += GameManager_OnGameReset;
        tooltip.gameObject.SetActive(false);
        tooltip.gameObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
        tooltipCanvas = tooltip.transform.parent.gameObject;
    }

    private void GameManager_OnGameReset()
    {
        if (enumerator != null)
        {
            StopCoroutine(enumerator);
            enumerator = null;
        }
        if (RanEnumerator != null)
        {
            StopCoroutine(RanEnumerator);
            RanEnumerator = null;
        }
        StopAllCoroutines();
        isTimer = false;
        isTrigger = false;
        isBack = false;
        state = STATE.TALK;
        if (isExit)
            groupController.Join(this);
        isExit = false;
        animator.SetInteger("Animation_int", Random.Range(0, 9));
        animator.SetInteger("State", (int)state);
        tooltip.gameObject.SetActive(false);
        transform.rotation = initQuaternion;
        transform.position = initPosition;
    }

    private void GameManager_OnGameStart()
    {
        tooltip.sprite = tooltipSprite[(int)tooltipColor];
        RanEnumerator = RandomExit();
        StartCoroutine(RanEnumerator);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGroup)
        {
            if (!isBack)
            {
                if (isTrigger)
                {
                    if (enumerator != null)
                    {
                        StopCoroutine(enumerator);
                        enumerator = null;
                    }
                    if (RanEnumerator != null)
                    {
                        StopCoroutine(RanEnumerator);
                        RanEnumerator = null;
                    }
                    Vector3 v = Player.transform.position - transform.position;
                    v.x = v.z = 0;
                    transform.LookAt(Player.transform.position - v);
                    playerController._npc.tooltipColor = tooltipColor;
                    if (playerController.handState == _PlayerController.HandState.Shake)
                    {
                        isTalk = false;
                        tooltip.gameObject.SetActive(true);
                    //    tooltip.transform.LookAt(tooltip.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                        state = STATE.TALK;
                        animator.SetInteger("Animation_int", Random.Range(0, 10));
                        animator.SetInteger("State", (int)state);
                    }
                    if (isTalk)
                    {
                        tooltip.gameObject.SetActive(false);
                        playerController.isTrigger = false;
                        isTrigger = false;
                        enumerator = MoveObject();
                        StartCoroutine(enumerator);
                        if (RanEnumerator == null)
                            RanEnumerator = RandomExit();
                        StartCoroutine(RanEnumerator);
                        state = STATE.WALK;
                        animator.SetTrigger("Walk");
                        animator.SetInteger("State", (int)state);
                    }
                }
                else
                {
                    transform.Translate(Vector3.forward * Time.deltaTime);
                }
            }
            else
            {
                Vector3 v = initPosition - transform.position;
                v.x = v.z = 0;
                transform.LookAt(initPosition - v);
                transform.Translate(Vector3.forward * Time.deltaTime);
                if (Vector3.Distance(this.transform.position, initPosition) < 1f)
                {
                    this.transform.position = initPosition;
                    isBack = false;
                    isExit = false;
                    groupController.Join(this);
                    state = STATE.TALK;
                    animator.SetInteger("Animation_int", Random.Range(0, 10));
                    animator.SetInteger("State", (int)state);
                    if (RanEnumerator != null)
                    {
                        StopCoroutine(RanEnumerator);
                        RanEnumerator = null;
                    }
                    RanEnumerator = RandomExit();
                    StartCoroutine(RanEnumerator);
                }
            }
        }
        else
        {
            if (isTrigger)
            {
                Vector3 v = Player.transform.position - transform.position;
                v.x = v.z = 0;
                transform.LookAt(Player.transform.position - v);
              //  tooltip.transform.LookAt(tooltip.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                state = STATE.IDLE;
                animator.SetInteger("State", (int)state);
            }
            else
            {
                transform.rotation = initQuaternion;
                transform.position = initPosition;
                state = STATE.TALK;
                animator.SetInteger("Animation_int", Random.Range(0, 10));
                animator.SetInteger("State", (int)state);
            }
        }

        if(tooltip.gameObject.activeInHierarchy)
        {
            Vector3 v = Camera.main.transform.position - tooltipCanvas.transform.position;
            v.x = v.z = 0;
            tooltipCanvas.transform.transform.LookAt(Camera.main.transform.position - v);
            // tooltip.transform.LookAt(tooltip.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!isGroup)
            if (collision.gameObject.CompareTag("Wall"))
            {
                switch (dir)
                {
                    case DIR.down:
                        dir = DIR.up;
                        dir1 = 180;
                        break;
                    case DIR.right:
                        dir = DIR.left;
                        dir1 = 270;
                        break;
                    case DIR.left:
                        dir = DIR.down;
                        dir1 = 0;
                        break;
                    case DIR.up:
                        dir = DIR.right;
                        dir1 = 90;
                        break;
                }
                this.transform.rotation = Quaternion.Euler(0, dir1, 0);
            }
        if (!isGroup)
            if (collision.collider.CompareTag("Player"))
            {
                if (!playerController.isTrigger && !isTimer)
                {
                    isTalk = false;
                    playerController.isTrigger = true;
                    playerController._StartCoroutine();
                    isTrigger = true;
                    state = STATE.HI;
                    animator.SetInteger("Animation_int", Random.Range(0, 5));
                    animator.SetInteger("State", (int)state);
                    if (RanEnumerator != null)
                    {
                        StopCoroutine(RanEnumerator);
                        RanEnumerator = null;
                    }
                    if (enumerator != null)
                    {
                        StopCoroutine(enumerator);
                        enumerator = null;
                    }
                }
            }
        if (collision.collider.CompareTag("NPC") && !isBack)
        {
            dir = (DIR)Random.Range(0, 4);
            switch (dir)
            {
                case DIR.down:
                    dir1 = 0;
                    break;
                case DIR.right:
                    dir1 = 90;
                    break;
                case DIR.left:
                    dir1 = 180;
                    break;
                case DIR.up:
                    dir1 = 270;
                    break;
            }
            this.transform.rotation = Quaternion.Euler(0, dir1, 0);
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (!isGroup)
            if (collision.collider.CompareTag("Player") && !isTimer)
            {
                isTimer = true;
                playerController.isTrigger = false;
                playerController._StopCoroutine();
                isTrigger = false;
                state = STATE.WALK;
                animator.SetTrigger("Walk");
                animator.SetInteger("State", (int)state);
                if (enumerator != null)
                {
                    StopCoroutine(enumerator);
                    enumerator = null;
                }
                enumerator = MoveObject();
                StartCoroutine(enumerator);
                StartCoroutine(Timer());
            }
        if (!isGroup&&collision.collider.CompareTag("NPC") && !isBack)
        {
            dir = (DIR)Random.Range(0, 4);
            switch (dir)
            {
                case DIR.down:
                    dir1 = 0;
                    break;
                case DIR.right:
                    dir1 = 90;
                    break;
                case DIR.left:
                    dir1 = 180;
                    break;
                case DIR.up:
                    dir1 = 270;
                    break;
            }
            this.transform.rotation = Quaternion.Euler(0, dir1, 0);
        }

    }
    IEnumerator MoveObject()
    {
        bool isLoop = true;
        while (isLoop)
        {
            dir = (DIR)Random.Range(0, 4);
            switch (dir)
            {
                case DIR.down:
                    dir1 = 0;
                    break;
                case DIR.right:
                    dir1 = 90;
                    break;
                case DIR.left:
                    dir1 = 180;
                    break;
                case DIR.up:
                    dir1 = 270;
                    break;
            }
            this.transform.rotation = Quaternion.Euler(0, dir1, 0);
            yield return StartCoroutine(YieldTime(5.0f));
        }
    }
    IEnumerator Timer()
    {
        isTimer = true;
        yield return StartCoroutine(YieldTime(10.0f));
        isTimer = false;
    }
    public IEnumerator Talk()
    {
        isTalk = false;
     //   bool isLoop = true;
        playerController._npc = this;
        playerController._npc.tooltipColor = tooltipColor;
        yield return new WaitForSecondsRealtime(0.25f);
        //  while (isLoop)
        {
           // yield return null;
            tooltip.gameObject.SetActive(true);
          //  tooltip.transform.LookAt(tooltip.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
            state = STATE.TALK;
            animator.SetInteger("Animation_int", Random.Range(0, 10));
            animator.SetInteger("State", (int)state);
            /*
            if (isTalk&& playerController.isMotion)
            {
                tooltip.gameObject.SetActive(false);
                isLoop = false;
            }
            */
        }
        yield return new WaitForSecondsRealtime(2.0f);
        tooltip.gameObject.SetActive(false);
        isTalk = true;
          while (!playerController.isMotion)
        {
            yield return null;
        }
    }
    public void EndTalkAnimation()
    {
        isTalk = true;
    }
    public IEnumerator RandomExit()
    {
        bool isLoop = true;
        while (isLoop)
        {
            yield return null;
            if (isExit)
            {
                yield return StartCoroutine(YieldTime(10.0f));        //NPC 탈출 후 복귀 시간
                Vector3 v = initPosition - transform.position;
                v.x = v.z = 0;
                transform.LookAt(initPosition - v);
                isBack = true;
                isLoop = false;
                state = STATE.WALK;
                animator.SetTrigger("Walk");
                animator.SetInteger("State", (int)state);
                if (enumerator != null)
                {
                    StopCoroutine(enumerator);
                    enumerator = null;
                }
            }
            else
            {
                yield return StartCoroutine(YieldTime(60.0f));              // NPC 탈출 쿨타임 
                if (10 > Random.Range(0, 100) && !isTrigger)        // NPC 탈출 확률
                {
                    isExit = true;
                    groupController.Leave(this);
                    state = STATE.WALK;
                    animator.SetTrigger("Walk");
                    animator.SetInteger("State", (int)state);

                }
            }
        }
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