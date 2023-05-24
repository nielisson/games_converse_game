using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public GameObject road;
    public static bool roadIsEnded;
    // Start is called before the first frame update
    void Start()
    {
        roadIsEnded = false;
    }

    void Update()
    {
        if (roadIsEnded)
        {
            roadIsEnded = false;
            SpawnNewRoad();
        }
    }

    void SpawnNewRoad()
    {
        Instantiate(road, this.transform.position, Quaternion.Euler(-90f, 0f, -90f));
    }
}
