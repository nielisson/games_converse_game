using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEverything : MonoBehaviour
{
    public GameObject treeSpawningPoint;
    public GameObject TreePrefab;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ground")
        {
            Destroy(other.gameObject);

        }

        if (other.gameObject.tag == "Trees")
        {

            Instantiate(TreePrefab, treeSpawningPoint.transform.position, Quaternion.Euler(0f, 90f, 0f));


        }
    }

    

}
