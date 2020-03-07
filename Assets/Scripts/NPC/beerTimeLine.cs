using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class beerTimeLine : MonoBehaviour
{
    _PlayerController pc;
    public GameObject beerBar;
    private void OnEnable()
    {
        pc = GameObject.Find("Player").transform.Find("TpsCam").GetComponent<_PlayerController>();
        pc.transform.GetChild(0).GetChild(0).Find("mainCharacter").gameObject.SetActive(false);
        beerBar.SetActive(false);
    }
    private void OnDisable()
    {
        beerBar.SetActive(true);
        if (pc != null)
        {
            pc.AlcoholStart();
            pc.transform.GetChild(0).transform.GetChild(0).transform.Find("mainCharacter").gameObject.SetActive(true);
        }
        if (ChinemachineManager.instance.CM_Two.state == PlayState.Playing)
            ChinemachineManager.instance.CM_Two.Stop();
        ChinemachineManager.instance.CM_Two.time = 0;
    }
}
