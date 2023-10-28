using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public float speed;
    public GameObject PickUpVFX;
    public AudioSource audioSource;
    public AudioClip powerUpSound;
    [Range(0f,2f)]
    public float volumeSFX;

    private MeshRenderer meshR;

    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        meshR = this.GetComponent<MeshRenderer>();

        GameObject player = GameObject.Find("Player");
        playerController = player.GetComponent<PlayerController>();
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
            audioSource.PlayOneShot(powerUpSound, volumeSFX);
            Instantiate(PickUpVFX, this.gameObject.transform.position, Quaternion.identity);
            OnBecameInvisible();
            playerController.ActivateShield();
            playerController.HasShield();
        }
    }

    private void OnBecameInvisible()
    {
        meshR.enabled = false;
        Destroy(gameObject, 5f);
    }
}
