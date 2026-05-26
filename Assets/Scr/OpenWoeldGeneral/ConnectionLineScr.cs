using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ConnectionLineScr : MonoBehaviour
{
    private LineRenderer lr;

    public void Init(Transform from, Transform to, Color color, float width = 0.05f)
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, from.position);
        lr.SetPosition(1, to.position);
        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.useWorldSpace = true;
    }
}
