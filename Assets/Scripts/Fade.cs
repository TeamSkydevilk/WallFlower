using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    UnityEngine.UI.Image image;
    public bool isFadeOut;
    public bool isFadeIn;
    IEnumerator enumerator;
    public  bool isFadeoutFinish;

    private void Awake()
    {
        image = GetComponent<UnityEngine.UI.Image>();
        image.color = new Color(0, 0, 0, 0);
        gameObject.SetActive(false);
        GameManager.OnGameReset += GameManager_OnGameReset;
    }
    
    private void GameManager_OnGameReset()
    {
        StopAllCoroutines();
        image.color = new Color(0, 0, 0, 0);
        isFadeOut = false;
        isFadeIn = false;
        enumerator = null;
        isFadeoutFinish = false;
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        isFadeoutFinish = false;
        FadeOut();
    }
    private void OnDisable()
    {
        isFadeOut = false;
        isFadeIn = false;
        if (enumerator != null)
            StopCoroutine(enumerator);
        enumerator = null;
        image.color = new Color(0, 0, 0, 0);
    }
    public void FadeOut()
    {
        if (!isFadeOut)
        {
            isFadeOut = true;
            isFadeIn = false;
            if (enumerator != null)
                enumerator = null;
            enumerator = fadeout();
            StartCoroutine(enumerator);
        }
        isFadeoutFinish = true;
    }
    public void FadeIn()
    {
        if (!isFadeIn)
        {
            isFadeIn = true;
            isFadeOut = false;
            if (enumerator != null)
                enumerator = null;
            enumerator = fadein();
            StartCoroutine(enumerator);
        }
    }
    IEnumerator fadeout()
    {
        for (float i = 0; i < 1.2f; i += 0.01f)
        {
            yield return StartCoroutine(YieldTime(0.01f));// fade 속도 
            image.color = new Color(0, 0, 0, i);
        }
    }
    IEnumerator fadein()
    {
        for (float i = 1f; i > -0.2f; i -= 0.01f)
        {
            yield return StartCoroutine(YieldTime(0.01f));// fade 속도 
            image.color = new Color(0, 0, 0, i);
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