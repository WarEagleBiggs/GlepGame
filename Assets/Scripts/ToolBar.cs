using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolBar : MonoBehaviour
{
    [Header("Toolbar Settings")]
    public int slotCount = 5;
    public int CurrSlot = 1;

    [Header("Slot Highlight Objects (size = slotCount)")]
    public GameObject[] slotHighlights;

    void Start()
    {
        CurrSlot = 1;
        UpdateVisuals();
    }

    void Update()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll > 0f)
        {
            CurrSlot--;
            if (CurrSlot < 1)
                CurrSlot = slotCount;

            UpdateVisuals();
        }
        else if (scroll < 0f)
        {
            CurrSlot++;
            if (CurrSlot > slotCount)
                CurrSlot = 1;

            UpdateVisuals();
        }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < slotHighlights.Length; i++)
        {
            slotHighlights[i].SetActive(i == CurrSlot - 1);
        }
    }
}
