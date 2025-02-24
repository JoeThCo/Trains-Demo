using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachToCar : MonoBehaviour
{
    [SerializeField] Car car;
    [SerializeField] private Transform parentTo;
    CharacterController playerController;
    public BoxCollider BoxCollider { get; private set; }

    private void Start()
    {
        BoxCollider = GetComponent<BoxCollider>();
    }

    public bool isPlayerControllerAttached()
    {
        return playerController != null;
    }

    private void OnTriggerEnter(Collider other)
    {
        CharacterController player = other.gameObject.GetComponent<CharacterController>();
        if (player != null)
        {
            playerController = player;
            player.transform.SetParent(parentTo, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CharacterController player = other.gameObject.GetComponent<CharacterController>();
        if (player != null)
        {
            player.transform.SetParent(null, true);
            player = null;
        }
    }
}