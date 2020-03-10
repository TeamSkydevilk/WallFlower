using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CatSceneController : MonoBehaviour
{
    public GameObject[] models;
    public Material[] materials;
    private void Awake()
    {
        for (int i = 0; i < models.Length; ++i)
        {
            models[i].SetActive(false);
        }
        models[souldondes.instance.soulNPCIndex].SetActive(true);
        Material[] temp = models[souldondes.instance.soulNPCIndex].GetComponent<SkinnedMeshRenderer>().materials;
        temp[0] = materials[souldondes.instance.soulNPCIndex];
        models[souldondes.instance.soulNPCIndex].GetComponent<SkinnedMeshRenderer>().materials = temp;
    }
    private void OnDisable()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
