using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource[] BGM;
    public AudioSource[] audioSource;
    public AudioClip[] audioClips;  // 0 = Heart Sound
    private void Awake()
    {
        if (instance == null)
        {
            instance = gameObject.GetComponent<SoundManager>();
        }
        else
        {
            Destroy(this.gameObject);
        }
        GameManager.OnGameStart += GameManager_OnGameStart;
    }

    private void GameManager_OnGameStart()
    {
        for (int i = 0; i < BGM.Length; i++)
        {
            if (BGM[i].isPlaying)
            {
                BGM[i].Stop();
            }
            BGM[i].Play();
        }
    }
    public void StartAudio(AudioClip audioClip,int index)
    {
        StopAudio(index);
        audioSource[index].clip = audioClip;
        audioSource[index].Play();
    }
    public void StopAudio(int index)
    {
        if (audioSource[index].isPlaying)
        {
            audioSource[index].Stop();
            audioSource[index].clip = null;
        }
    }
}
