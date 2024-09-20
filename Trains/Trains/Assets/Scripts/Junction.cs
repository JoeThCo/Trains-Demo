using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Junction : MonoBehaviour
{
    [SerializeField] private MeshRenderer debugMeshRenderer;
    public Node Node { get; private set; }

    public void JunctionInit(Node node)
    {
        Node = node;
        gameObject.name = $"Junction {Node.Index}";

        if (!Node.IsJunction)
            debugMeshRenderer.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Engine engine = other.gameObject.GetComponentInParent<Engine>();
        if (engine)
        {
            Debug.Log($"Collision with Junction {Node.Index}");
        }
    }
}