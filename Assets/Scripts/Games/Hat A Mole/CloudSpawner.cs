using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] Cloud;
    private bool canSpawnCloud;

    void Start()
    {
        canSpawnCloud = true;
    }


    void Update()
    {
        if (canSpawnCloud)
        {
            Instantiate(Cloud[Random.Range(0, Cloud.Length)], this.transform.position, Quaternion.identity);

            StartCoroutine(SpawnCloud());
        }
        
    }
    IEnumerator SpawnCloud()
    {
        canSpawnCloud = false;

        yield return new WaitForSeconds(20f);

        canSpawnCloud = true;
    }
}
