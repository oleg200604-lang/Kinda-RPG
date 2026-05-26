using System;
using System.Collections.Generic;
using UnityEngine;

public enum pointType
{
    none,
    city,
    enemy,
    boss
}

public class PointScr : MonoBehaviour
{
    [Header("References")]
    public BattleDataCarrier dataCarrier;
    public WorldMapGeneratorScr worldMapGeneratorScr;
    public NPCZoneUIScr zoneUI;

    [Header("Type")]
    public pointType pointType;

    [Header("Zone")]
    public NPCZoneScr city;
    public EnemyZoneScr enemy;
    public EnemyZoneScr boss;

    [HideInInspector] public int id;
    [HideInInspector] public int depth;
    [HideInInspector] public List<PointScr> connections = new();

    public static Action<PointScr> OnPointClicked;

    [Header("Visual")]
    public SpriteRenderer spriteType;
    public SpriteRenderer spriteRenderer;

    public Color defaultColor = Color.white;
    public Color availableColor = Color.yellow;
    public Color currentColor = Color.green;

    public Color[] colorPoint;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void Init()
    {
        RefreshTypeVisual();
    }

    // ─────────────────────────────────────────
    // TYPE
    // ─────────────────────────────────────────

    public void SetLocationType(pointType newType)
    {
        pointType = newType;

        RefreshTypeVisual();

        if (pointType == pointType.city)
        {
            if (city != null)
            {
                city.ShopStart();
            }
        }
    }

    private void RefreshTypeVisual()
    {
        if (spriteType == null)
            return;

        // UI LINKS
        if (city != null)
        {
            city.zoneUI = zoneUI;
        }

        if (enemy != null)
        {
            enemy.zoneUI = zoneUI;
        }

        if (boss != null)
        {
            boss.zoneUI = zoneUI;
        }

        // DISABLE ALL
        if (city != null)
            city.enabled = false;

        if (enemy != null)
            enemy.enabled = false;

        if (boss != null)
            boss.enabled = false;

        // ENABLE CURRENT
        switch (pointType)
        {
            case pointType.none:

                spriteType.color = colorPoint[0];
                break;

            case pointType.enemy:

                if (enemy != null)
                    enemy.enabled = true;

                spriteType.color = colorPoint[1];
                break;

            case pointType.city:

                if (city != null)
                    city.enabled = true;

                spriteType.color = colorPoint[2];
                break;

            case pointType.boss:

                if (boss != null)
                    boss.enabled = true;

                spriteType.color = colorPoint[3];
                break;
        }
    }

    // ─────────────────────────────────────────
    // INTERACT
    // ─────────────────────────────────────────

    public void Interact()
    {
        switch (pointType)
        {
            case pointType.city:

                if (city != null)
                {
                    if (zoneUI != null)
                    {
                        bool active =
                            zoneUI.SelectNPCPanel.activeSelf;

                        zoneUI.SelectNPCPanel.SetActive(!active);

                        if (!active)
                        {
                            zoneUI.UIPanelShop.SetActive(false);
                        }
                    }
                }

                break;

            case pointType.enemy:

                if (enemy != null)
                {
                    enemy.StartBattle();

                    SetLocationType(pointType.none);
                }

                break;

            case pointType.boss:

                if (boss != null)
                {
                    boss.StartBattle();

                    SetLocationType(pointType.none);
                }

                break;
        }
    }

    // ─────────────────────────────────────────
    // SELECT
    // ─────────────────────────────────────────

    public void SelectPoint(bool isSelect)
    {
        if (city != null)
            city.isZoneNPC = false;

        if (enemy != null)
            enemy.isZoneNPC = false;

        if (boss != null)
            boss.isZoneNPC = false;

        if (!isSelect)
            return;

        switch (pointType)
        {
            case pointType.city:

                if (city != null)
                    city.isZoneNPC = true;

                break;

            case pointType.enemy:

                if (enemy != null)
                    enemy.isZoneNPC = true;

                break;

            case pointType.boss:

                if (boss != null)
                    boss.isZoneNPC = true;

                break;
        }
    }

    // ─────────────────────────────────────────
    // VISUAL STATE
    // ─────────────────────────────────────────

    public void SetDefault()
    {
        spriteRenderer.color = defaultColor;
    }

    public void SetAvailable()
    {
        spriteRenderer.color = availableColor;
    }

    public void SetCurrent()
    {
        spriteRenderer.color = currentColor;
    }

    // ─────────────────────────────────────────
    // INPUT
    // ─────────────────────────────────────────

    private void OnMouseDown()
    {
        OnPointClicked?.Invoke(this);
    }
}