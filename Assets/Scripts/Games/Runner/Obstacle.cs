using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed;

    // Start is called before the first frame update
    void Awake()
    {

    }

    // Update is called once per frame
    public void Update()
    {
        
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    
     

    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
