using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MVPInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameNameText;
    [SerializeField] private TextMeshProUGUI companyNameText;
    [SerializeField] private TextMeshProUGUI versionText;

    void Start()
    {
        gameNameText.SetText(Application.productName);
        companyNameText.SetText(Application.companyName);
        versionText.SetText(Application.version);
    }
}
