using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteractionController : MonoBehaviour
{
    PlayerController pc;

    private void Start()
    {
        pc = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (pc.interacting)
        {
            if (Input.GetButtonDown("Interaction"))
            {
                pc.selectedObject.Interact();
            }
        }
    }
}