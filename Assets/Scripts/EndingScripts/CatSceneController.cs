using UnityEngine;
using UnityEngine.Playables;
public class CatSceneController : MonoBehaviour
{
    public GameObject[] models;
    public Material[] materials;
    public PlayableDirector pd;
    private void Awake()
    {
        int idx = 0;
        if (null != souldondes.instance)
            idx = souldondes.instance.soulNPCIndex;
        for (int i = 0; i < models.Length; ++i)
        {
            models[i].SetActive(false);
        }
        models[idx].SetActive(true);
        Material[] temp = models[idx].GetComponent<SkinnedMeshRenderer>().materials;
        temp[0] = materials[idx];
        models[idx].GetComponent<SkinnedMeshRenderer>().materials = temp;
    }
    private void Start()
    {
        pd.Play();
    }
}
