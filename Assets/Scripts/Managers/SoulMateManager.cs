using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulMateManager : MonoBehaviour
{
    public Transform[] positions;
    public GameObject soulMate;
    private void Awake()
    {
        GameManager.OnGameStart += GameManager_OnGameStart;
        positions = transform.GetComponentsInChildren<Transform>();
    }

    private void GameManager_OnGameStart()
    {
        int idx = Random.Range(0, positions.Length);
        soulMate.transform.position = positions[idx].position;
    }
}