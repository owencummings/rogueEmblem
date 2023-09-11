using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody _rb;
    
    void Awake()
    {
        _rb = this.GetComponent<Rigidbody>();
    }

    void Start()
    {
        _rb.detectCollisions = false;
    }
}
