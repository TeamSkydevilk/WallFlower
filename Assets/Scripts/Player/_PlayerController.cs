using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _PlayerController : MonoBehaviour
{
    public enum HandState { Idle, Shake }
    public Transform playerTransfrom;
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private bool is_Input;
    public PlayerState playerState;
    public Vector3 m_Input;
    Vector2 m_Mouse;
    public IEnumerator enumerator;
    public IEnumerator enumerator2;
    public HandState handState;
    public bool isTrigger;
    public bool isTalk;
    public Animator animator;
    public Transform Charactor_Rotate;
    Camera viewCam;
    public bool isVR;
    public float movSpd = 1.8f; //캐릭터 속도를 위한 변수
    public float sense = 30.0f; //마우스 감도
    public Vector3 viewDir = new Vector3();
    public _LaserPointer laserPointer;
    public GameObject laserObject;
    public UnityEngine.UI.Image tooltipImage;
    public Transform tooltipTransfrom;
    public RectTransform[] tooltipArrows;
    public Sprite[] BlackTooltipSprites;
    public Sprite[] RedTooltipSprites;
    public Sprite[] BlueTooltipSprites;
    public Sprite[] YellowTooltipSprites;
    public Sprite[] GreenTooltipSprites;
    public Sprite[] OrangeTooltipSprites;
    public Sprite[] ClaretTooltipSprites;
    public Sprite[] PurpleTooltipSprites;
    public Sprite HITooltipSprite;
    public NPCController _npc;
    public bool isGreeting;
    public bool isMotion;
    public bool ismotionTimer;
    public float motionTimer;
    public float joystickTimer;
    public bool isOpenEmotion;
    bool isNTrigger;
    int motionidx;
    Rigidbody rigidbody;
    Vector3 InitPosition;
    Quaternion InitRotation;
    Quaternion InitCharactor_Rotate;
    Vector3 InitPlayerPosition;
    Quaternion InitPlayerRotation;
    bool isStart = false;

    bool isBar;
    int BarToolTipIdx;
    public Sprite[] BarTooltipSprites;
    /// <summary>
    /// 술마신 상태
    /// </summary>
    [SerializeField] private float alcoholSpeed = 2.0f;     //술 마신 다음 속도
    bool isAlcohol; // true 술취함 false 술깸
    Vector3 beforeAlcoholPosition;
    Quaternion beforeAlcoholRotation;
    public GameObject SoulMateObj;

    private void Awake()
    {
        GameManager.OnGameStart += GameManager_OnGameStart;
        GameManager.OnGameReset += GameManager_OnGameReset;

        is_Input = true;
        isGreeting = false;
        isMotion = false;
        motionidx = 0;
        handState = HandState.Idle;
        enumerator = Check_ShakeHands();
        enumerator2 = Timer();
        InitPosition = transform.position;
        InitRotation = transform.rotation;
        InitCharactor_Rotate = Charactor_Rotate.transform.localRotation;
        InitPlayerPosition = playerTransfrom.position;
        InitPlayerRotation = playerTransfrom.rotation;
        rigidbody = GetComponent<Rigidbody>();
        if (viewCam == null)
        {
            viewCam = Camera.main;
        }
#if UNITY_EDITOR
        isVR = false;
#elif UNITY_ANDROID
        isVR = true;
#else
        isVR = false;
#endif
    }
    void OnDestroy()
    {
        GameManager.OnGameReset -= GameManager_OnGameReset;
        GameManager.OnGameStart -= GameManager_OnGameStart;
    }
    private void GameManager_OnGameReset()
    {
        if (enumerator != null)
            StopCoroutine(enumerator);
        if (enumerator2 != null)
            StopCoroutine(enumerator2);
        StopAllCoroutines();
        isStart = false;
        rigidbody.isKinematic = false;
        is_Input = true;
        isTrigger = false;
        isTalk = false;
        isGreeting = false;
        isMotion = false;
        motionidx = 0;
        handState = HandState.Idle;
        enumerator = Check_ShakeHands();
        enumerator2 = Timer();
        tooltipImage.gameObject.SetActive(false);
        tooltipArrows[0].gameObject.SetActive(false);
        tooltipArrows[1].gameObject.SetActive(false);
        animator.SetInteger("State", (int)NPCController.STATE.IDLE);
        laserObject.SetActive(true);
        transform.position = InitPosition;
        transform.rotation = InitRotation;
        Charactor_Rotate.transform.localRotation = InitCharactor_Rotate;
        playerTransfrom.position = InitPlayerPosition;
        playerTransfrom.rotation = InitPlayerRotation;
        isAlcohol = false;
        isBar = false;
    }

    //게임 시작 이벤트
    private void GameManager_OnGameStart()
    {
        //SoulMateObj = GameObject.FindGameObjectWithTag("SoulMate");
        laserObject.SetActive(false);
        StartCoroutine(enumerator2);
    }
    public void EndTalkAnimation()
    {
        isOpenEmotion = false;
        ismotionTimer = false;
        motionTimer = 0;
        tooltipImage.gameObject.SetActive(false);
        isMotion = true;
        _npc = null;
    }
    private void Update()
    {
        if (tooltipImage.gameObject.activeInHierarchy)
        {
            Vector3 v = Camera.main.transform.position - tooltipTransfrom.gameObject.transform.position;
            v.x = v.z = 0;
            tooltipTransfrom.gameObject.transform.LookAt(Camera.main.transform.position - v);
        }
    }
    private void FixedUpdate()
    {
        if (GameManager.instance.gamestate == GameManager.GAMESTATE.TUTORIAL)
        {
            if (laserPointer.isButton)
            {
                if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) != 0 && !isStart)
                {
                    isStart = true;
                    GameManager.instance.GameStart();
                }
            }
        }
        if (GameManager.instance.gamestate == GameManager.GAMESTATE.INGAME)
        {
            if (isVR)
            {
                GetInput();
                playerTransfrom.Translate(m_Input * speed * Time.fixedDeltaTime, Space.Self);
            }
            else
            {
                if (!isTrigger)
                    PlayerMove();
                else
                {
                    if (isBar)
                    {
                        if (Input.GetMouseButtonDown(1))
                        {
                            tooltipTransfrom.transform.LookAt(Camera.main.transform);
                            BarToolTip();
                        }
                        animator.SetInteger("State", 0);
                        if (Input.GetKeyDown(KeyCode.P))
                        {
                            tooltipImage.gameObject.SetActive(false);
                            isBar = false;
                            isTrigger = false;
                            if (BarToolTipIdx == 0)
                            {
                                ChinemachineManager.instance.StartChinema(2);

                            }
                        }
                    }
                    else
                    {
                        if (_npc != null && _npc.isTalk)
                        {
                            if (Input.GetMouseButtonDown(1) && !isOpenEmotion)
                            {
                                isOpenEmotion = true;
                                tooltipImage.gameObject.SetActive(true);
                                //   tooltipTransfrom.transform.LookAt(tooltipTransfrom.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                                tooltipArrows[0].gameObject.SetActive(true);
                                tooltipArrows[1].gameObject.SetActive(true);
                                tooltipArrows[0].localScale = new Vector3(0.5f, 0.5f, 0.5f);
                                tooltipArrows[1].localScale = new Vector3(0.5f, 0.5f, 0.5f);
                                return;
                            }
                            //   tooltipTransfrom.transform.LookAt(tooltipTransfrom.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                            ToolTipColor(_npc.tooltipColor, motionidx);
                            if (isOpenEmotion)
                            {
                                if (ismotionTimer)
                                {
                                    motionTimer += Time.fixedDeltaTime;
                                    if (motionTimer > 2.0f)
                                        EndTalkAnimation();
                                }
                                else
                                {
                                    joystickTimer += Time.fixedDeltaTime;
                                    if (Input.GetKey(KeyCode.RightArrow))
                                    {
                                        tooltipArrows[1].localScale = new Vector3(1f, 1f, 1);
                                        if (joystickTimer > 0.15f)
                                        {
                                            joystickTimer = 0;
                                            if (motionidx == 3)
                                                motionidx = 0;
                                            else
                                                ++motionidx;
                                        }
                                    }
                                    else if (Input.GetKey(KeyCode.LeftArrow))
                                    {
                                        tooltipArrows[0].localScale = new Vector3(1f, 1f, 1);
                                        if (joystickTimer > 0.15f)
                                        {
                                            joystickTimer = 0;
                                            if (motionidx == 0)
                                                motionidx = 3;
                                            else
                                                --motionidx;
                                        }
                                    }
                                    else
                                    {
                                        tooltipArrows[0].localScale = new Vector3(0.5f, 0.5f, 0.5f);
                                        tooltipArrows[1].localScale = new Vector3(0.5f, 0.5f, 0.5f);
                                    }
                                    if (Input.GetMouseButtonDown(1))
                                    {
                                        ismotionTimer = true;
                                        tooltipArrows[0].gameObject.SetActive(false);
                                        tooltipArrows[1].gameObject.SetActive(false);
                                        animator.SetInteger("Animation_int", Random.Range(0, 3));
                                        animator.SetTrigger("Talk");
                                    }
                                }
                            }
                        }
                        animator.SetInteger("State", (int)NPCController.STATE.IDLE);
                    }
                }
                MouseMove();
            }
        }
        if (GameManager.instance.gamestate == GameManager.GAMESTATE.FINISH)
        {

        }
    }
    void GetInput()
    {
        // 이동
        if (is_Input && !isTrigger)
        {
            if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x != 0)
            {
                m_Input.x = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;
            }
            else if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x != 0)
            {
                m_Input.x = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
            }
            else
            {
                m_Input.x = 0;
            }
            if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y != 0)
            {
                m_Input.z = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
            }
            else if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y != 0)
            {
                m_Input.z = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y;
            }
            else
            {
                m_Input.z = 0;
            }
            animator.SetInteger("State", (int)NPCController.STATE.WALK);
            Vector3 lookDir = m_Input.z * Vector3.forward + m_Input.x * Vector3.right;
            if (m_Input.x != 0 || m_Input.y != 0)
                Charactor_Rotate.transform.localRotation = Quaternion.LookRotation(lookDir);
        }
        else
        {
            animator.SetInteger("State", (int)NPCController.STATE.IDLE);
            m_Input.x = 0;
            m_Input.z = 0;
        }
        if (isTrigger)
        {
            if (isBar)
            {
                if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x > 0.25f)
                {
                    tooltipTransfrom.transform.LookAt(Camera.main.transform);
                    BarToolTip();
                }
                else if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x < -0.25f)
                {
                    tooltipTransfrom.transform.LookAt(Camera.main.transform);
                    BarToolTip();
                }
                else if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x > 0.25f)
                {
                    tooltipTransfrom.transform.LookAt(Camera.main.transform);
                    BarToolTip();
                }
                else if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x < -0.25f)
                {
                    tooltipTransfrom.transform.LookAt(Camera.main.transform);
                    BarToolTip();
                }
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) != 0 || OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) != 0)
                {
                    tooltipImage.gameObject.SetActive(false);
                    isBar = false;
                    isTrigger = false;
                    if (BarToolTipIdx == 0)
                    {
                        ChinemachineManager.instance.StartChinema(2);

                    }
                }
            }
            else if (_npc != null && _npc.isTalk)
            {

                if ((OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) != 0 || OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) != 0) && !isOpenEmotion)
                {
                    isOpenEmotion = true;
                    tooltipImage.gameObject.SetActive(true);
                    tooltipArrows[0].gameObject.SetActive(true);
                    tooltipArrows[1].gameObject.SetActive(true);
                    tooltipArrows[0].localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    tooltipArrows[1].localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    isNTrigger = true;
                    return;
                }
                ToolTipColor(_npc.tooltipColor, motionidx);
                if ((OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) == 0 && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) == 0))
                {
                    isNTrigger = false;
                }
                if (isOpenEmotion && isNTrigger == false)
                {
                    if (ismotionTimer)
                    {
                        motionTimer += Time.fixedDeltaTime;
                        if (motionTimer > 2.0f)
                            EndTalkAnimation();
                    }
                    else
                    {
                        joystickTimer += Time.fixedDeltaTime;
                        if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x > 0.25f)
                        {
                            tooltipArrows[1].localScale = new Vector3(1f, 1f, 1f);
                            if (joystickTimer > 0.15f)
                            {
                                joystickTimer = 0;
                                if (motionidx == 3)
                                    motionidx = 0;
                                else
                                    ++motionidx;
                            }
                        }
                        else if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x < -0.25f)
                        {
                            tooltipArrows[0].localScale = new Vector3(1f, 1f, 1f);
                            if (joystickTimer > 0.15f)
                            {
                                joystickTimer = 0;
                                if (motionidx == 0)
                                    motionidx = 3;
                                else
                                    --motionidx;
                            }
                        }
                        else if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x > 0.25f)
                        {
                            tooltipArrows[1].localScale = new Vector3(1f, 1f, 1f);
                            if (joystickTimer > 0.15f)
                            {
                                joystickTimer = 0;
                                if (motionidx == 3)
                                    motionidx = 0;
                                else
                                    ++motionidx;
                            }
                        }
                        else if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x < -0.25f)
                        {
                            tooltipArrows[0].localScale = new Vector3(1f, 1f, 1f);
                            if (joystickTimer > 0.15f)
                            {
                                joystickTimer = 0;
                                if (motionidx == 0)
                                    motionidx = 3;
                                else
                                    --motionidx;
                            }
                        }
                        else
                        {
                            tooltipArrows[0].localScale = new Vector3(0.5f, 0.5f, 0.5f);
                            tooltipArrows[1].localScale = new Vector3(0.5f, 0.5f, 0.5f);

                        }


                        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) != 0 || OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) != 0)
                        {
                            ismotionTimer = true;
                            tooltipArrows[0].gameObject.SetActive(false);
                            tooltipArrows[1].gameObject.SetActive(false);
                            animator.SetInteger("Animation_int", Random.Range(0, 3));
                            animator.SetTrigger("Talk");
                        }
                    }
                }
            }
            animator.SetInteger("State", (int)NPCController.STATE.IDLE);
        }
    }
    void ToolTipColor(NPCController.TooltipColor npcTooltipColor, int _dir)
    {
        switch (npcTooltipColor)
        {
            case NPCController.TooltipColor.Black:
                tooltipImage.sprite = BlackTooltipSprites[_dir];
                break;
            case NPCController.TooltipColor.Blue:
                tooltipImage.sprite = BlueTooltipSprites[_dir];
                break;
            case NPCController.TooltipColor.Claret:
                tooltipImage.sprite = ClaretTooltipSprites[_dir];
                break;
            case NPCController.TooltipColor.Green:
                tooltipImage.sprite = GreenTooltipSprites[_dir];
                break;
            case NPCController.TooltipColor.Orange:
                tooltipImage.sprite = OrangeTooltipSprites[_dir];
                break;
            case NPCController.TooltipColor.Purple:
                tooltipImage.sprite = PurpleTooltipSprites[_dir];
                break;
            case NPCController.TooltipColor.Red:
                tooltipImage.sprite = RedTooltipSprites[_dir];
                break;
            case NPCController.TooltipColor.Yellow:
                tooltipImage.sprite = YellowTooltipSprites[_dir];
                break;
        }
    }
    void PlayerMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(h, 0, v);
        if (h != 0 || v != 0)
        {
            animator.SetInteger("State", (int)NPCController.STATE.WALK);
            Vector3 lookDir = v * Vector3.forward + h * Vector3.right;
            Charactor_Rotate.transform.localRotation = Quaternion.LookRotation(lookDir);
        }
        else
        {
            animator.SetInteger("State", (int)NPCController.STATE.IDLE);
        }
        transform.Translate(dir * speed * Time.deltaTime);
    }
    void MouseMove()
    {
        float mh = Input.GetAxis("Mouse X");
        float mv = Input.GetAxis("Mouse Y");

        Transform targTrf;
        targTrf = viewCam.transform;

        viewDir = targTrf.eulerAngles;

        mv += (mv * sense * Time.deltaTime);
        mv = Mathf.Clamp(viewDir.x, -90, 60);

        mh += (mh * sense * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, viewDir.y + mh, 0);
        targTrf.rotation = Quaternion.Euler(viewDir.x - mv, viewDir.y + mh, 0);
    }
    // 손 흔드는거 확인 ( Shake hand side to side )
    IEnumerator Check_ShakeHands()
    {
        bool isLoop = true;
        if (isVR)
        {
            while (isLoop)
            {
                yield return null;
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) != 0 || OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) != 0)
                {
                    animator.SetInteger("Animation_int", Random.Range(0, 2));
                    animator.SetInteger("State", (int)NPCController.STATE.HI);
                    handState = HandState.Shake;
                    tooltipImage.gameObject.SetActive(true);
                    //   tooltipTransfrom.transform.LookAt(tooltipTransfrom.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                    tooltipImage.sprite = HITooltipSprite;
                    yield return new WaitForSecondsRealtime(0.5f);
                    tooltipImage.gameObject.SetActive(false);
                    isLoop = false;
                    handState = HandState.Idle;
                    isGreeting = true;
                    motionidx = 0;
                }
            }
        }
        else
        {
            while (isLoop)
            {
                yield return null;
                if (Input.GetKeyDown(KeyCode.P))
                {
                    animator.SetInteger("Animation_int", Random.Range(0, 2));
                    animator.SetInteger("State", (int)NPCController.STATE.HI);
                    handState = HandState.Shake;
                    tooltipImage.gameObject.SetActive(true);
                    //    tooltipTransfrom.transform.LookAt(tooltipTransfrom.transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
                    tooltipImage.sprite = HITooltipSprite;
                    yield return new WaitForSecondsRealtime(0.5f);
                    tooltipImage.gameObject.SetActive(false);
                    isLoop = false;
                    handState = HandState.Idle;
                    isGreeting = true;
                    motionidx = 0;
                }
            }
        }
    }
    IEnumerator Timer()
    {
        bool isLoop = true;
        while (isLoop)
        {
            if (isTrigger && playerState.hp >= 0 && !ChinemachineManager.instance.isStart)
            {
                if (!isAlcohol)
                {
                    playerState.hp -= 1;
                }
                playerState.presence += 1;
            }
            else if (!isTrigger && playerState.presence >= 0 && !ChinemachineManager.instance.isStart)
            {
                if (!isAlcohol)
                {
                    playerState.presence -= 1;
                    playerState.hp += 1;
                }
            }
            rigidbody.isKinematic = false;
            yield return new WaitForSecondsRealtime(1.0f);
            rigidbody.isKinematic = true;
        }
    }
    public void _StartCoroutine()
    {
        if (enumerator != null)
            StopCoroutine(enumerator);
        enumerator = null;
        enumerator = Check_ShakeHands();
        StartCoroutine(enumerator);
    }
    public void _StopCoroutine()
    {
        if (enumerator != null)
            StopCoroutine(enumerator);
    }

    void BarToolTip()
    {
        if (BarToolTipIdx == 0)
        {
            tooltipImage.sprite = BarTooltipSprites[++BarToolTipIdx];
        }
        else
        {
            tooltipImage.sprite = BarTooltipSprites[--BarToolTipIdx];
        }
    }

    public void AlcoholStart()
    {
        StartCoroutine(AlcoholTime());
    }
    IEnumerator AlcoholTime()
    {
        Debug.Log("알코올 시작");
        this.transform.position = beforeAlcoholPosition;
        this.transform.rotation = beforeAlcoholRotation;
        isAlcohol = true;
        playerState.Hp_Warning();
        yield return new WaitForSecondsRealtime(30.0f);
        playerState.Hp_Normal();
        isAlcohol = false;
        playerState.hp -= 10;
        Debug.Log("알코올 끝");
    }
    public void EnterBar()
    {
        if (!isAlcohol)
        {
            tooltipTransfrom.transform.LookAt(Camera.main.transform);
            BarToolTipIdx = 0;
            BarToolTip();
            tooltipImage.gameObject.SetActive(true);
            isTrigger = true;
            isBar = true;
            beforeAlcoholPosition = this.transform.position;
            beforeAlcoholRotation = this.transform.rotation;
        }
    }
    public void ExitBar()
    {
        isBar = false;
    }
}