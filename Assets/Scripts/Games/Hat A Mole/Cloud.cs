using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        float cloudSize = Random.Range(0.1f, 0.5f);
        Vector3 scaleChange = new Vector3(cloudSize, cloudSize, cloudSize);
        this.gameObject.transform.localScale = scaleChange;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + new Vector3(1 * speed * Time.deltaTime, 0, 0);
    }
}
