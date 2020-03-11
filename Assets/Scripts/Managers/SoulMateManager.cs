using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulMateManager : MonoBehaviour
{
    public Transform[] positions;
    public GameObject[] models;
    public GameObject[] models2;
    public Material[] materials;
    public GameObject soulMate;
    private void Awake()
    {
        GameManager.OnGameStart += GameManager_OnGameStart;
        positions = transform.GetComponentsInChildren<Transform>();
    }
    void OnDestroy()
    {
        GameManager.OnGameStart -= GameManager_OnGameStart;
    }
    private void GameManager_OnGameStart()
    {
        int idx = Random.Range(0, positions.Length);
        souldondes.instance.soulNPCIndex = Random.Range(0, models.Length);
        for (int i = 0; i < models.Length; ++i)
        {
            models[i].SetActive(false);
            models2[i].SetActive(false);
        }
        models[souldondes.instance.soulNPCIndex].SetActive(true);
        models2[souldondes.instance.soulNPCIndex].SetActive(true);
        Material[] temp = models[souldondes.instance.soulNPCIndex].GetComponent<SkinnedMeshRenderer>().materials;
        Material[] temp2 = models2[souldondes.instance.soulNPCIndex].GetComponent<SkinnedMeshRenderer>().materials;
        temp[0] = materials[souldondes.instance.soulNPCIndex];
        temp2[0] = materials[souldondes.instance.soulNPCIndex];
        models[souldondes.instance.soulNPCIndex].GetComponent<SkinnedMeshRenderer>().materials = temp;
        models2[souldondes.instance.soulNPCIndex].GetComponent<SkinnedMeshRenderer>().materials = temp2;
        soulMate.transform.position = positions[idx].position;
    }
}