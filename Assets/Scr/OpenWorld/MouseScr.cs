using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseScr : MonoBehaviour
{
    public RectTransform uiElement;
    public RectTransform uiElement1;
    public float speed = 10f;

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;

        uiElement.position = Vector2.Lerp(uiElement.position, mousePos, speed * Time.deltaTime);
        uiElement1.position = Vector2.Lerp(uiElement1.position, mousePos, speed * Time.deltaTime);
    }
}
