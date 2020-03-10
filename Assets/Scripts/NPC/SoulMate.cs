using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoulMate : MonoBehaviour
{
    SoulMateManager soulMateManager;
    private void Awake()
    {
        soulMateManager = GameObject.Find("SoulMateManager").GetComponent<SoulMateManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.transform.CompareTag("Player"))
        {
            SoundManager.instance.StopAudio(0);
            SoundManager.instance.StopAudio(1);
            ChinemachineManager.instance.StartChinema(4);
            soulMateManager.models[souldondes.instance.soulNPCIndex].SetActive(false);
        }
    }
}