using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsToggle : MonoBehaviour
{
    public GameObject target;

    void Update()
    {
        if (!target) return;

        target.SetActive(Input.GetKey(KeyCode.Tab));
    }
}
