using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleActive : MonoBehaviour
{
    public void Toggle()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
}
