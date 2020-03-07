using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerState : MonoBehaviour
{
    public float hp;
    public float presence;
    public Image hpImage;
    public Image presenceImage;
    public Material material;               //Player_Charactor Material
    public Material[] ShadowWallMaterial;
    public GameObject OVRCamerarigObj;
    Vector3 InitCameraPosition;
    Quaternion InitCameraRotation;
    public Image ShadowImage;
    IEnumerator acidEnumerator;
    public AcidTrip.AcidTrip acidTrip;
    bool isAcid;
    bool isHpWarning;
    bool isPresenceWarning;

    EndingInterface ending;
    private void Awake()
    {
        ShadowImage.color = new Color(1, 1, 1, 0f);
        ShadowWallMaterial[0].color = new Color(1, 1, 1, 0);
        ShadowWallMaterial[1].color = new Color(1, 1, 1, 0);
        ShadowWallMaterial[2].color = new Color(1, 1, 1, 0);
        GameManager.OnGameReset += GameManager_OnGameReset;
        InitCameraPosition = OVRCamerarigObj.transform.localPosition;
        InitCameraRotation = OVRCamerarigObj.transform.localRotation;
    }

    private void GameManager_OnGameReset()
    {
        StopAllCoroutines();
        GameManager.instance.Player_Transform.gameObject.transform.localScale = new Vector3(1, 1, 1);
        acidTrip.enabled = false;
        acidTrip.DistortionStrength = 0;
        ShadowImage.color = new Color(1, 1, 1, 0f);
        ShadowWallMaterial[0].color = new Color(1, 1, 1, 0);
        ShadowWallMaterial[1].color = new Color(1, 1, 1, 0);
        ShadowWallMaterial[2].color = new Color(1, 1, 1, 0);
        material.color = new Color(1, 1, 1, 1f);
        hp = 50f;
        presence = 50f;
        isAcid = false;
        isHpWarning = false;
        isPresenceWarning = false;
        Hp_Normal();
        OVRCamerarigObj.transform.localPosition = InitCameraPosition;
        OVRCamerarigObj.transform.localRotation = InitCameraRotation;
        SoundManager.instance.StopAudio(1);
        SoundManager.instance.StopAudio(0);
        for (int i = 0; i < SoundManager.instance.BGM.Length; i++)
            SoundManager.instance.BGM[i].volume = 1;
    }

    public void Update()
    {
        if (GameManager.instance.gamestate == GameManager.GAMESTATE.INGAME)
        {
            hpImage.fillAmount = (hp / 100);
            presenceImage.fillAmount = (presence / 100);
            if (hp <= 0)
            {
                Hp_Zero();
                GameManager.instance.gamestate = GameManager.GAMESTATE.FINISH;
            }
            if (presence <= 0)
            {
                Presence_Zero();
                GameManager.instance.gamestate = GameManager.GAMESTATE.FINISH;
            }

            if (hp < 10 && !isHpWarning)
            {
                for (int i = 0; i < SoundManager.instance.BGM.Length; i++)
                    SoundManager.instance.BGM[i].volume = 0.2f;
                isHpWarning = true;
                SoundManager.instance.StartAudio(SoundManager.instance.audioClips[0], 0);
                Hp_Warning();
            }
            else if (hp >= 10 && isHpWarning)
            {
                isHpWarning = false;
                SoundManager.instance.StopAudio(0);
                Hp_Normal();
            }

            if (presence < 30f)
            {
                if (!isPresenceWarning)
                {
                    SoundManager.instance.StartAudio(SoundManager.instance.audioClips[1], 1);
                    isPresenceWarning = true;
                }
                ShadowImage.color = new Color(1, 1, 1, (1 - (presence * 0.033f)));
                material.color = new Color(1, 1, 1, presence / 30f);
                for (int i = 0; i < SoundManager.instance.BGM.Length; i++)
                    SoundManager.instance.BGM[i].volume = presence / 30;
            }
            else if (presence >= 30)
            {
                if (isPresenceWarning)
                {
                    SoundManager.instance.StopAudio(1);
                    isPresenceWarning = false;
                }
                ShadowImage.color = new Color(1, 1, 1, 0f);
                material.color = new Color(1, 1, 1, 1f);
                for (int i = 0; i < SoundManager.instance.BGM.Length; i++)
                    SoundManager.instance.BGM[i].volume = 1;
            }
        }
    }

    IEnumerator acid()
    {
        bool isLoop = true;
        acidTrip.enabled = true;
        while (isLoop)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            acidTrip.DistortionStrength += 0.05f; 
            if (acidTrip.DistortionStrength > 1f)
            {
                isLoop = false;
            }
        }
    }
    IEnumerator Deacid()
    {
        bool isLoop = true;
        acidTrip.enabled = false;
        while (isLoop)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            //     audioSource.volume = 1f;
            acidTrip.DistortionStrength -= 0.05f;
            if (acidTrip.DistortionStrength < 0f)
            {
                isLoop = false;
            }
        }
    }
    public void Hp_Normal()
    {
        if (acidEnumerator != null)
        {
            StopCoroutine(acidEnumerator);
            acidEnumerator = null;
        }
        acidEnumerator = Deacid();
        StartCoroutine(acidEnumerator);
    }
    public void Hp_Warning()
    {
        if (acidEnumerator != null)
        {
            StopCoroutine(acidEnumerator);
            acidEnumerator = null;
        }
        acidEnumerator = acid();
        StartCoroutine(acidEnumerator);
    }
    public void Hp_Zero()
    {
        StopCoroutine(acidEnumerator);
        acidTrip.enabled = false;
        ending = new MeltingEnding();
        StartCoroutine(ending.StartEnding());
    }
    public void Presence_Zero()
    {
        ShadowImage.color = new Color(1, 1, 1, 0f);
        StartCoroutine(Show_Wall());
    }
    IEnumerator Show_Wall()
    {
        SoundManager.instance.StopAudio(1);
        SoundManager.instance.StartAudio(SoundManager.instance.audioClips[2], 0);
        for (float i = 0; i <= 1.2f; i += 0.05f)
        {
            Color color = new Vector4(1, 1, 1, i);
            ShadowWallMaterial[0].color = color;
            ShadowWallMaterial[1].color = color;
            ShadowWallMaterial[2].color = color;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        ShadowImage.color = new Color(1, 1, 1, 0f);
        ChinemachineManager.instance.StartChinema(3);
    }
}