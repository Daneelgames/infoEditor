using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool interacting = false;
    public float sensitivity = 1;
    public float speed = 1;
    public InteractiveObjectController selectedObject;

    float moveFB;
    float moveLR;
    float rotX;
    float rotY;
    CharacterController characterController;
    PlayerInteractionController pic;
    GameObject cam;
    RaycastHit hit;
    GameManager gm;
    Quaternion camInitialRotation;
    float stopInteractionDelay = 1;
    bool canStopInteraction = false;

    public LayerMask layerMask;

    private void Start()
    {
        gm = GameManager.instance;
        pic = GetComponent<PlayerInteractionController>();
        characterController = GetComponent<CharacterController>();
        cam = Camera.main.gameObject;
        ToggleCursor(false);
    }

    void ToggleCursor(bool active)
    {
        //Cursor.visible = active;


        if (active == true)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Update()
    {
        if (!interacting)
        {
            Move();
            Interact();
        }
        else
        {
            if (selectedObject != null && canStopInteraction)
            {
                if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
                {
                    if (stopInteractionDelay > 0)
                        stopInteractionDelay -= Time.deltaTime;
                    else
                    {
                        StopInteraction();
                    }
                }
                else if (stopInteractionDelay < 1)
                {
                    stopInteractionDelay = 1;
                }
            }
        }
    }

    void Move()
    {
        moveFB = Input.GetAxisRaw("Vertical");
        moveLR = Input.GetAxisRaw("Horizontal");

        rotX = Input.GetAxis("Mouse X") * sensitivity;
        rotY += Input.GetAxis("Mouse Y") * sensitivity;

        if (rotY < -50) rotY = -50;
        else if (rotY > 50) rotY = 50;

        var movement = new Vector3(moveLR, 0, moveFB);
        transform.Rotate(0, rotX, 0);

        movement = transform.rotation * movement;
        characterController.Move(movement.normalized * speed * Time.deltaTime);

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    private void LateUpdate()
    {
        if (!interacting)
            cam.transform.localRotation = Quaternion.Euler(-rotY, 0f, 0f);
    }

    void Interact()
    {
        // find object
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 3, layerMask))
        {
            foreach (InteractiveObjectController interactiveObject in gm.interactiveObjects)
            {
                if (interactiveObject.gameObject == hit.collider.gameObject)
                {
                    if (selectedObject == null || interactiveObject.gameObject != selectedObject.gameObject)
                    {
                        if (selectedObject != null)
                            selectedObject.ToggleOutline(false);
                        selectedObject = interactiveObject;
                        selectedObject.ToggleOutline(true);
                    }
                }
            }
        }
        else if (selectedObject != null)
        {
            selectedObject.ToggleOutline(false);
            selectedObject = null;
        }

        if (Input.GetButtonDown("Interaction") && selectedObject != null)
        {
            ToggleCursor(true);
            selectedObject.interacting = true;
            StartCoroutine(MoveCamera(selectedObject.camHolder.position, selectedObject.camHolder.rotation, true));
        }
    }

    void StopInteraction()
    {
        ToggleCursor(false);
        selectedObject.interacting = false;
        StartCoroutine(MoveCamera(transform.position + Vector3.up * 2, camInitialRotation, false));
    }

    IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot, bool active)
    {
        if (active)
        {
            stopInteractionDelay = 1;
            canStopInteraction = false;
            camInitialRotation = cam.transform.rotation;
            interacting = true;
        }

        Vector3 startPosition = cam.transform.position;
        Quaternion startRotation = cam.transform.rotation;
        float t = 0;
        while (t <= 1)
        {
            cam.transform.position = Vector3.Lerp(startPosition, targetPos, t);
            cam.transform.rotation = Quaternion.Lerp(startRotation, targetRot, t);
            t += Time.deltaTime;
            yield return null;
        }

        if (!active)
        {
            interacting = false;
            cam.transform.localPosition = Vector3.up * 2;
        }
        else
            canStopInteraction = true;
    }
}