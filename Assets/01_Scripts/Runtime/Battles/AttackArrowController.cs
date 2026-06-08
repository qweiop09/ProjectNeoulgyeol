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

    // 캐스터별 슬롯 수만큼 화살표 배열 보유
    private readonly Dictionary<CharacterHandler, AttackArrowView[]> fixedArrowsByCaster 
        = new Dictionary<CharacterHandler, AttackArrowView[]>();
    
    private AttackArrowView trackingArrow;
    private bool fixedArrowsVisible = true;

    public bool FixedArrowsVisible => fixedArrowsVisible;

    // 캐스터의 슬롯 수에 맞춰 화살표 배열 초기화
    public void InitializeCaster(CharacterHandler caster)
    {
        if (caster == null) return;
        if (fixedArrowsByCaster.ContainsKey(caster)) return;

        int slotCount = caster.characterBattleData.CharacterData.slotCount;
        fixedArrowsByCaster[caster] = new AttackArrowView[slotCount];
    }

    // TargetingData 기반으로 화살표 전부 지우고 다시 그림
    public void RedrawArrows(CharacterHandler[] characters)
    {
        ClearFixedArrows();

        foreach (CharacterHandler caster in characters)
        {
            ActData[] targetingData = caster.GetCharacterBattleData().TargetingData;
            if (targetingData == null) continue;

            for (int slotIndex = 0; slotIndex < targetingData.Length; slotIndex++)
            {
                ActData actData = targetingData[slotIndex];
                if (actData == null || actData.TargetPlayerCharacter == null) continue;

                ShowFixedArrow(caster, actData.TargetPlayerCharacter, slotIndex);
            }
        }
    }

    // slotIndex: 사용하는 슬롯 인덱스
    public void ShowFixedArrow(CharacterHandler caster, CharacterHandler target, int slotIndex)
    {
        if (caster == null || target == null) return;

        AttackArrowView arrowView = GetOrCreateFixedArrow(caster, slotIndex);
        if (arrowView == null) return;

        arrowView.SetFixedTarget(caster.transform, target.transform);
        arrowView.gameObject.SetActive(fixedArrowsVisible);
    }

    public void HideFixedArrow(CharacterHandler caster, int slotIndex)
    {
        if (caster == null) return;
        if (!fixedArrowsByCaster.TryGetValue(caster, out AttackArrowView[] arrows)) return;
        if (slotIndex < 0 || slotIndex >= arrows.Length) return;
        if (arrows[slotIndex] != null)
            arrows[slotIndex].gameObject.SetActive(false);
    }

    public void ShowTrackingArrow(CharacterHandler caster, Vector3 targetPosition)
    {
        if (caster == null) return;

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

        foreach (AttackArrowView[] arrows in fixedArrowsByCaster.Values)
        {
            foreach (AttackArrowView arrow in arrows)
            {
                if (arrow != null)
                    arrow.gameObject.SetActive(fixedArrowsVisible);
            }
        }
    }

    public void ShowFixedArrows() => SetFixedArrowsVisible(true);
    public void HideFixedArrows() => SetFixedArrowsVisible(false);
    public void ToggleFixedArrows() => SetFixedArrowsVisible(!fixedArrowsVisible);

    public void ClearFixedArrows()
    {
        foreach (AttackArrowView[] arrows in fixedArrowsByCaster.Values)
        {
            foreach (AttackArrowView arrow in arrows)
            {
                if (arrow != null)
                    Destroy(arrow.gameObject);
            }
        }

        fixedArrowsByCaster.Clear();
    }

    public void ClearAll()
    {
        ClearFixedArrows();
        HideTrackingArrow();
    }

    private AttackArrowView GetOrCreateFixedArrow(CharacterHandler caster, int slotIndex)
    {
        if (!fixedArrowsByCaster.ContainsKey(caster))
            InitializeCaster(caster);

        AttackArrowView[] arrows = fixedArrowsByCaster[caster];

        if (slotIndex < 0 || slotIndex >= arrows.Length) return null;

        if (arrows[slotIndex] == null)
            arrows[slotIndex] = CreateArrowView($"FixedAttackArrow_{caster.name}_Slot{slotIndex}");

        return arrows[slotIndex];
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