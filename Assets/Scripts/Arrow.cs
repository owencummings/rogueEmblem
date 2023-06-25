using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody _rb;
    private float lifespan = 3.0f;
    private float timeAlive = 0.0f;

    void Awake()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
    }

    void Start()
    {
        _rb.AddForce((transform.up + transform.forward * 4) * 100f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // If it hits ground, parent it and turn off RB. same if enemy damageable, but also damage it.
    }
    
    void FixedUpdate()
    {
        timeAlive += Time.fixedDeltaTime;
        if (timeAlive > lifespan)
        {
            Destroy(this.gameObject);
        }
    }
}
