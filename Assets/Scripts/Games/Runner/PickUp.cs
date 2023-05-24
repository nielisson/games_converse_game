using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public float speed;
    void Start()
    {
        Destroy(gameObject, 4f);
    }

    public void Update()
    {

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
