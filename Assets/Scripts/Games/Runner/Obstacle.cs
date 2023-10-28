using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float speed;
    public GameObject impactVFX;

    // Start is called before the first frame update
    void Start()
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Instantiate(impactVFX, this.transform.position + new Vector3(0f,3f,4f), Quaternion.identity);
            Destroy(this.gameObject);
        }
    }

  }
