using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Junction : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private MeshRenderer debugMeshRenderer;
    public Node Node { get; private set; }

    private void Start()
    {
        debugText.gameObject.SetActive(false);
        debugMeshRenderer.gameObject.SetActive(false);
    }

    public void JunctionInit(Node node)
    {
        Node = node;
        gameObject.name = $"Junction {Node.Index}";

        if (!Node.IsJunction)
            debugMeshRenderer.enabled = false;

        debugText.SetText($"{Node.Index}");
    }
}