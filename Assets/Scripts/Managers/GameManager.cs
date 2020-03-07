using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour
{
    //private static GameManager _instance;
    public static GameManager instance;
    float minute = 3f;
    float seconds = 0f;
    public UnityEngine.UI.Text time_text;
    public enum GAMESTATE { TUTORIAL, INGAME, FINISH }
    public GAMESTATE gamestate;     //현재 게임 상태 튜토리얼, 인게임, 종료로 나눈다.
    public GameObject[] TutorialImage;  // index 0 = 버튼 index 1 = 판넬 index 2 = 튜토리얼 이미지 3 = 크레딧 오브젝트
    public Sprite[] TutorialSprites;
    public delegate void GameHandelr();
    public static event GameHandelr OnGameReset;
    public static event GameHandelr OnGameStart;
    public RectTransform CreditText;
    public Transform Player_Transform;
    Vector3 Init_Credut_Text;
    Vector3 Init_Player_Position;
    Quaternion Init_Player_Quaternion;
    public GameObject fade;
    private bool isGameEnd;
    private void Awake()
    {
        if (instance == null)
        {
            instance = gameObject.GetComponent<GameManager>();
        }
        else
        {
            Destroy(this.gameObject);
        }
        Application.targetFrameRate = 60;
        gamestate = GAMESTATE.TUTORIAL;
        Init_Credut_Text = CreditText.localPosition;
        Init_Player_Position = Player_Transform.transform.localPosition;
        Init_Player_Quaternion = Player_Transform.transform.localRotation;
        CreditText.gameObject.SetActive(false);
        OnGameReset += GameManager_OnGameReset;
    }

    private void GameManager_OnGameReset()
    {
        StopAllCoroutines();
        gamestate = GAMESTATE.TUTORIAL;
        TutorialImage[0].SetActive(true);
        TutorialImage[1].SetActive(true);
        time_text.text = "";
        minute = 3f;//분
        seconds = 0f;//초
        isGameEnd = false;
        CreditText.localPosition = Init_Credut_Text;
        Player_Transform.transform.localPosition = Init_Player_Position;
        Player_Transform.transform.localRotation = Init_Player_Quaternion;
        TutorialImage[3].SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (gamestate == GAMESTATE.INGAME)
            time_text.text = minute + ":" + string.Format("{0:N0}", seconds);
        if (gamestate == GAMESTATE.FINISH && !ChinemachineManager.instance.isPlay)
        {
            if (!isGameEnd)
            {
                isGameEnd = true;
                fade.SetActive(false);
                Credit();
            }
        }
    }

    IEnumerator Timer()
    {
        TutorialImage[0].SetActive(false);
        TutorialImage[1].SetActive(false);
        TutorialImage[2].SetActive(true);
        TutorialImage[2].GetComponent<UnityEngine.UI.Image>().sprite = TutorialSprites[0];
        bool isLoop = true;
        yield return new WaitForSecondsRealtime(5.0f);     // 튜토리얼 영상 5초 보여 준 후 게임 시작.
        TutorialImage[2].GetComponent<UnityEngine.UI.Image>().sprite = TutorialSprites[1];
        yield return new WaitForSecondsRealtime(3.0f);          // 튜토리얼 영상 5초 보여 준 후 게임 시작.
        OnGameStart();
        TutorialImage[2].SetActive(false);
        time_text.text = "6:00";
        gamestate = GAMESTATE.INGAME;
        while (isLoop)
        {
            seconds -= 0.1f;
            if (seconds < 0f)
            {
                seconds = 59f;
                minute--;
                if (minute == -1)
                {
                    gamestate = GAMESTATE.FINISH;
                    ChinemachineManager.instance.StartChinema(1);
                    time_text.text = "";
                    isLoop = false;
                }
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
    public void GameStart()                    //플레이어가 스타트 버튼을 누르면 타이머 작동
    {
        StartCoroutine(Timer());
    }

    //시네마신 시작시 gamestate는 FINISH로

    void Credit()
    {
        time_text.text = "";
        StartCoroutine(CreditCoroutine());
    }
    IEnumerator CreditCoroutine()
    {
        bool isLoop = true;
        fade.SetActive(true);
        yield return new WaitForSecondsRealtime(0.05f);
        Camera.main.cullingMask &= ~(1 << 8);
        TutorialImage[3].SetActive(true);
        float i = 1;
        while (isLoop)
        {
            yield return new WaitForSecondsRealtime(0.01f);
            CreditText.localPosition = new Vector3(CreditText.localPosition.x, CreditText.localPosition.y + 0.5f, CreditText.localPosition.z);
            if (CreditText.localPosition.y > 120)
            {
                isLoop = false;
            }
            for (int idx = 0; idx < 2; idx++)
            {
                SoundManager.instance.BGM[idx].volume = i;
            }
            i -= 0.01f;                                  //볼륨 크기 속도 조절
        }
        TutorialImage[3].SetActive(false);
        for (int idx = 0; idx < 2; idx++)
        {
            SoundManager.instance.BGM[idx].Stop();
        }
        Camera.main.cullingMask |= (1 << 8);
        OnGameReset();
    }
}