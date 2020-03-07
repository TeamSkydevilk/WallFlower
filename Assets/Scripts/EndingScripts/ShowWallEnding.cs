using System.Collections;
using UnityEngine;

public class ShowWallEnding : MonoBehaviour, EndingInterface
{
    public Material[] ShadowWallMaterial;
    public UnityEngine.UI.Image ShadowImage;

    private void Awake()
    {
        ShadowWallMaterial[0] = Resources.Load("doorway") as Material;
        ShadowWallMaterial[1] = Resources.Load("wallshort") as Material;
        ShadowWallMaterial[2] = Resources.Load("walltall") as Material;
        ShadowImage = GameObject.Find("ShadowImage").GetComponent<UnityEngine.UI.Image>();
    }
    public IEnumerator StartEnding()
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