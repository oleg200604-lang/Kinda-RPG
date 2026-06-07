using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCZoneUIScr : MonoBehaviour
{
    public static NPCZoneUIScr Instance;
    public GameObject SelectNPCPanel;
    public GameObject UIPanel;
    public GameObject UIPanelShop;
    public GameObject presF;

    private void Awake()
    {
        Instance = this;
    }
}
