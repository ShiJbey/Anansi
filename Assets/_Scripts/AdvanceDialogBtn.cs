using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvanceDialogBtn : MonoBehaviour
{
    private InkManager _inkManager;

    void Start()
    {
        _inkManager = FindObjectOfType<InkManager>();

        if (_inkManager == null)
        {
            Debug.LogError("Ink Manager was not found!");
        }
    }

    public void OnClick()
    {
        _inkManager?.AdvanceDialog();
    }
}
