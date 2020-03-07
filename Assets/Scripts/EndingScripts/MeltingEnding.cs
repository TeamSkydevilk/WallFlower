using System.Collections;
using UnityEngine;

public class MeltingEnding : EndingInterface
{
    public IEnumerator StartEnding()
    {
        SoundManager.instance.StartAudio(SoundManager.instance.audioClips[2], 0);
        SoundManager.instance.StartAudio(SoundManager.instance.audioClips[3], 1);
        Quaternion initvec = GameManager.instance.Player_Transform.rotation;
        for (float i = 1; i >= 0f; i -= 0.05f)
        {
            GameManager.instance.Player_Transform.gameObject.transform.localScale = new Vector3(1, i, 1);
            GameManager.instance.Player_Transform.rotation = initvec;
            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        GameManager.instance.fade.SetActive(true);
        ChinemachineManager.instance.isPlay = false;
    }
}