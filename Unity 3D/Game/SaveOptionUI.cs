using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SaveOptionUI : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;

    public void UpdateMapName()
    {
        GameGridManager.instance.SetMapName(inputField.text);
    }
}
