using System.Collections.Generic;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles
{
public class AttackArrowController : MonoBehaviour
{
    [SerializeField] private AttackArrowView arrowPrefab;
    [SerializeField] private Material arrowMaterial;
    [SerializeField] private Color arrowColor = Color.red;
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] private float lineWidth = 0.06f;
    [SerializeField] private float outlineWidth = 0.12f;
    [SerializeField] private float arrowHeadSize = 0.35f;

    private readonly Dictionary<CharacterHandler, AttackArrowView> fixedArrowsByCaster = new Dictionary<CharacterHandler, AttackArrowView>();
    private AttackArrowView trackingArrow;
    private bool fixedArrowsVisible = true;

    public bool FixedArrowsVisible => fixedArrowsVisible;

    public void ShowFixedArrow(CharacterHandler caster, CharacterHandler target)
    {
        if (caster == null || target == null)
            return;

        AttackArrowView arrowView = GetOrCreateFixedArrow(caster);
        arrowView.SetFixedTarget(caster.transform, target.transform);
        arrowView.gameObject.SetActive(fixedArrowsVisible);
    }

    public void HideFixedArrow(CharacterHandler caster)
    {
        if (caster == null) return;

        if (fixedArrowsByCaster.TryGetValue(caster, out AttackArrowView arrowView) && arrowView != null)
            arrowView.gameObject.SetActive(false);
    }

    public void ShowTrackingArrow(CharacterHandler caster, Vector3 targetPosition)
    {
        if (caster == null)
            return;

        trackingArrow ??= CreateArrowView("TrackingAttackArrow");
        trackingArrow.SetTrackingTarget(caster.transform, targetPosition);
        trackingArrow.gameObject.SetActive(true);
    }

    public void HideTrackingArrow()
    {
        if (trackingArrow != null)
            trackingArrow.gameObject.SetActive(false);
    }

    public void SetFixedArrowsVisible(bool visible)
    {
        fixedArrowsVisible = visible;

        foreach (AttackArrowView arrowView in fixedArrowsByCaster.Values)
        {
            if (arrowView != null)
                arrowView.gameObject.SetActive(fixedArrowsVisible);
        }
    }

    public void ShowFixedArrows()
    {
        SetFixedArrowsVisible(true);
    }

    public void HideFixedArrows()
    {
        SetFixedArrowsVisible(false);
    }

    public void ToggleFixedArrows()
    {
        SetFixedArrowsVisible(!fixedArrowsVisible);
    }

    public void ClearFixedArrows()
    {
        foreach (AttackArrowView arrowView in fixedArrowsByCaster.Values)
        {
            if (arrowView != null)
                Destroy(arrowView.gameObject);
        }

        fixedArrowsByCaster.Clear();
    }

    public void ClearAll()
    {
        ClearFixedArrows();
        HideTrackingArrow();
    }

    private AttackArrowView GetOrCreateFixedArrow(CharacterHandler caster)
    {
        if (fixedArrowsByCaster.TryGetValue(caster, out AttackArrowView existingArrow) && existingArrow != null)
            return existingArrow;

        AttackArrowView arrowView = CreateArrowView($"FixedAttackArrow_{caster.name}");
        fixedArrowsByCaster[caster] = arrowView;

        return arrowView;
    }

    private AttackArrowView CreateArrowView(string arrowName)
    {
        AttackArrowView arrowView = arrowPrefab != null
            ? Instantiate(arrowPrefab, transform)
            : CreateDefaultArrowView();

        arrowView.name = arrowName;
        arrowView.Initialize(GetArrowMaterial(), arrowColor, outlineColor, lineWidth, outlineWidth, arrowHeadSize);

        return arrowView;
    }

    private AttackArrowView CreateDefaultArrowView()
    {
        GameObject arrowObject = new GameObject("AttackArrow");
        arrowObject.transform.SetParent(transform, false);
        return arrowObject.AddComponent<AttackArrowView>();
    }

    private Material GetArrowMaterial()
    {
        if (arrowMaterial != null)
            return arrowMaterial;

        Shader shader = Shader.Find("Sprites/Default");
        arrowMaterial = shader != null ? new Material(shader) : null;
        return arrowMaterial;
    }
}
}
