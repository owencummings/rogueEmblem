using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    Vector3 destination;
    Vector3 pivot;
    float orthoSizeDestination;
    float orthoSizeSpeed = 0.7f;
    float scrollSpeed = 1.0f;

    public float destinationAngle = 0f;

    void Start()
    {
        destination = this.transform.position;
        cam = this.GetComponent<Camera>();
        orthoSizeDestination = cam.orthographicSize;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Smooth camera to scroll destination
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, orthoSizeDestination, 0.5f);
    }

    void Update(){
        if (PauseManager.paused){ return; } 

        // Get zoom input and set zoom destination
        orthoSizeDestination += Input.mouseScrollDelta.y * orthoSizeSpeed * -1;
        orthoSizeDestination = Mathf.Clamp(orthoSizeDestination, 3f, 20f);
        scrollSpeed = 1.0f * (orthoSizeDestination/3f);
        pivot = transform.position + (transform.forward * 4 * Mathf.Sqrt(2));


        // Get cam rotate input
        if (Input.GetKey(KeyCode.Q)){
            destinationAngle += 100f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E)){
            destinationAngle -= 100f * Time.deltaTime;
        }

        // Rotate cam
        float rotateSpeed = 180f; // in degrees/second
        float degreesToRotate = 0f;
        if (destinationAngle != 0f){
            // Rotate camera angle
            degreesToRotate = Mathf.Clamp(destinationAngle, -1 * rotateSpeed * Time.deltaTime, rotateSpeed * Time.deltaTime);
            destinationAngle -= degreesToRotate;
            transform.RotateAround(pivot, Vector3.up, degreesToRotate);
        }

        // Move cam
        Vector3 moveVector = new Vector3();
        float moveSpeed = 6f;
        if (Input.GetKey(KeyCode.W)){
            moveVector += (Vector3.forward + Vector3.up);
        }
        if (Input.GetKey(KeyCode.A)){
            moveVector += Vector3.left;
        }
        if (Input.GetKey(KeyCode.S)){
            moveVector += (Vector3.back + Vector3.down);
        }
        if (Input.GetKey(KeyCode.D)){
            moveVector += Vector3.right;
        }
        transform.Translate(moveVector * Time.deltaTime *scrollSpeed * moveSpeed, Space.Self);
    }
}
