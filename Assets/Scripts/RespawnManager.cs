using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] GameObject[] respawnPoints;
    public int currentlyActiveSpawnPoint = 0;

    public GameObject GetRespawnPoint()
    {
        return respawnPoints[currentlyActiveSpawnPoint];
    }
}
