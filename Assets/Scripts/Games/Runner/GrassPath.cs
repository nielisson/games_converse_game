using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GrassPath : MonoBehaviour
{
    public float speed;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        //Space.World
    }


    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    /*
private void OnTriggerEnter2D(Collider2D collision)
{
    Debug.Log("test");
   // Destroy(this.gameObject);
}*/
}
