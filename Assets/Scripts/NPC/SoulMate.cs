using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoulMate : MonoBehaviour
{
    public Sprite[] mals;
    public UnityEngine.UI.Image tooltip;
    public Animator soulMateAnimator;
    public bool isCheck;
    public UnityEngine.UI.Image image;
    public _PlayerController pc;
    private void Awake()
    {
        soulMateAnimator = GetComponent<Animator>();
        tooltip.gameObject.SetActive(false);
        GameManager.OnGameStart += GameManager_OnGameStart;
    }
    private void OnDestroy()
    {
        GameManager.OnGameStart -= GameManager_OnGameStart;
    }
    private void GameManager_OnGameStart()
    {
        image.gameObject.SetActive(false);
        soulMateAnimator.SetInteger("Animation_int", 990);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.CompareTag("Player")&&!isCheck)
        {
            isCheck = true;
            pc = other.GetComponent<_PlayerController>();
            pc.isTrigger = true;
            pc.transform.position = pc.transform.position;
            pc.transform.rotation = pc.transform.rotation;
            pc.tooltipTransfrom.transform.LookAt(Camera.main.transform);
            tooltip.gameObject.transform.LookAt(Camera.main.transform);
            Vector3 v = pc.transform.position - transform.position;
            v.x = v.z = 0;
            transform.LookAt(pc.transform.position - v);
            v = transform.position - pc.transform.position;
            v.x = v.z = 0;
            pc.transform.LookAt(transform.position - v);
            StartCoroutine(SoulMateAnim());
        }
    }
    IEnumerator SoulMateAnim()
    {
        for (int i = 0; i < 771; ++i)
        {
            switch (i)
            {
                case 0:
                    pc.animator.SetInteger("Animation_int", 991);
                    pc.tooltipImage.gameObject.SetActive(true);
                    pc.tooltipImage.sprite = mals[1];
                    break;
                case 2:
                    pc.animator.SetInteger("Animation_int", 1000);
                    break;
                case 82:
                    pc.tooltipImage.gameObject.SetActive(false);
                    break;
                case 59:
                    soulMateAnimator.SetInteger("State", 1);
                    soulMateAnimator.SetInteger("Animation_int", 0);
                    tooltip.gameObject.SetActive(true);
                    tooltip.sprite = mals[1];
                    break;
                case 61:
                    soulMateAnimator.SetInteger("Animation_int", 1000);
                    break;
                case 158:
                    tooltip.gameObject.SetActive(false);
                    soulMateAnimator.SetInteger("State", 0);
                    break;
                case 155:
                    pc.animator.SetInteger("Animation_int", 992);
                    pc.tooltipImage.gameObject.SetActive(true);
                    pc.tooltipImage.sprite = mals[2];
                    break;
                case 157:
                    pc.animator.SetInteger("Animation_int", 1000);
                    break;
                case 247:
                    pc.tooltipImage.sprite = mals[4];
                    break;
                case 306:
                    pc.tooltipImage.gameObject.SetActive(false);
                    break;
                case 194:
                    soulMateAnimator.SetInteger("Animation_int", 991);
                    tooltip.gameObject.SetActive(true);
                    tooltip.sprite = mals[3];
                    break;
                case 196:
                    soulMateAnimator.SetInteger("Animation_int", 1000);
                    break;
                case 265:
                    tooltip.gameObject.SetActive(false);
                    break;
                case 358:
                    soulMateAnimator.SetInteger("Animation_int", 992);
                    tooltip.gameObject.SetActive(true);
                    tooltip.sprite = mals[5];
                    break;
                case 360:
                    soulMateAnimator.SetInteger("Animation_int", 1000);
                    break;
                case 409:
                    tooltip.gameObject.SetActive(false);
                    break;
                case 470:
                    pc.animator.SetInteger("Animation_int", 993);
                    pc.tooltipImage.gameObject.SetActive(true);
                    pc.tooltipImage.sprite = mals[7];
                    break;
                case 472:
                    pc.animator.SetInteger("Animation_int", 1000);
                    break;
                case 670:
                    pc.tooltipImage.sprite = mals[9];
                    break;
                case 760:
                    pc.tooltipImage.gameObject.SetActive(false);
                    break;

                case 577:
                    soulMateAnimator.SetInteger("Animation_int", 993);
                    tooltip.gameObject.SetActive(true);
                    tooltip.sprite = mals[8];
                    break;
                case 579:
                    soulMateAnimator.SetInteger("Animation_int", 1000);
                    break;
                case 643:
                    tooltip.sprite = mals[10];
                    break;
                case 770:
                    tooltip.gameObject.SetActive(false);
                    GameManager.instance.fade.SetActive(true);
                    break;
            }
            yield return new WaitForEndOfFrame();
        }
        image.color = new Color(0, 0, 0, 0);
        image.gameObject.SetActive(true);
        for (float i = 0; i < 1.2f; i += 0.01f)
        {
            yield return new WaitForSecondsRealtime(0.01f);// fade 속도 
            image.color = new Color(0, 0, 0, i);
        }
        image.gameObject.SetActive(false);
        SceneManager.LoadScene(1);
    }
}