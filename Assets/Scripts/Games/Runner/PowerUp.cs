using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public float speed;
    public GameObject PickUpVFX;

    // Start is called before the first frame update
    void Awake()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.Translate(Vector3.forward * speed * Time.deltaTime);



    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("collision with player");
            Instantiate(PickUpVFX, this.gameObject.transform.position, Quaternion.identity);
            OnBecameInvisible();
        }
    }

            private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
