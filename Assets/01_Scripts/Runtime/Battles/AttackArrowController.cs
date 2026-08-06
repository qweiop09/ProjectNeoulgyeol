using System.Collections.Generic;
using System.Linq;
using _01_Scripts.DTO.Item;
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
    [SerializeField] private float dashTiling = 3f; // 파선/점선 텍스처 반복 밀도 (길이에 비례해 자동 반복됨)

    // 캐스터별 슬롯마다 화살표를 여러 개(메인 타겟 1개 + 추가 타겟 N개) 보유
    private readonly Dictionary<CharacterHandler, List<AttackArrowView>[]> fixedArrowsByCaster
        = new Dictionary<CharacterHandler, List<AttackArrowView>[]>();

    private readonly Dictionary<ArrowLineStyle, Material> styleMaterials = new Dictionary<ArrowLineStyle, Material>();

    private AttackArrowView trackingArrow;
    private bool fixedArrowsVisible = true;

    // 랜덤 타겟 후보 미리보기(점선) — 확정 전 "이 중에서 뽑힐 수 있다"를 보여주는 용도
    private readonly List<AttackArrowView> candidatePreviewArrows = new List<AttackArrowView>();
    private HashSet<CharacterHandler> lastPreviewCandidates;

    public bool FixedArrowsVisible => fixedArrowsVisible;

    // 캐스터의 슬롯 수에 맞춰 화살표 리스트 배열 초기화
    public void InitializeCaster(CharacterHandler caster)
    {
        if (caster == null) return;
        if (fixedArrowsByCaster.ContainsKey(caster)) return;

        int slotCount = caster.GetCharacterStatus().CharacterData.slotCount;
        List<AttackArrowView>[] slots = new List<AttackArrowView>[slotCount];
        for (int i = 0; i < slotCount; i++)
            slots[i] = new List<AttackArrowView>();

        fixedArrowsByCaster[caster] = slots;
    }

    // TargetingData 기반으로 화살표 전부 지우고 다시 그림
    public void RedrawArrows(CharacterHandler[] characters)
    {
        ClearFixedArrows();

        foreach (CharacterHandler caster in characters)
        {
            ActData[] targetingData = caster.TargetingData;
            if (targetingData == null) continue;

            for (int slotIndex = 0; slotIndex < targetingData.Length; slotIndex++)
            {
                ActData actData = targetingData[slotIndex];
                if (actData == null || actData.TargetPlayerCharacter == null) continue;

                SetFixedArrow(caster, slotIndex, actData);
            }
        }
    }

    // slotIndex의 화살표를 (mainTarget=실선, additionalTargets 각각=파선 또는 점선)으로 새로 그림.
    // 캐스터 자신이 타겟(메인이든 추가든)에 포함되면 그 대상에 대한 화살표는 만들지 않는다.
    // 추가 타겟 스타일은 actData로부터 판단한다 — 랜덤으로 뽑힌 타겟은 확정 이후(실행 순간까지)도
    // 파선으로 "확정됐다"고 보여주면 안 되고 계속 점선("이 중 하나였다")으로 남아있어야 하기 때문.
    public void SetFixedArrow(CharacterHandler caster, int slotIndex, ActData actData)
    {
        if (caster == null) return;

        ClearSlotArrows(caster, slotIndex);

        List<AttackArrowView> slotArrows = GetSlotList(caster, slotIndex);
        if (slotArrows == null || actData == null) return;

        CharacterHandler mainTarget = actData.TargetPlayerCharacter;
        CharacterHandler[] additionalTargets = actData.AdditionalTargets;
        ArrowLineStyle additionalStyle = GetAdditionalTargetStyle(actData);

        if (mainTarget != null && mainTarget != caster)
            slotArrows.Add(CreateAndAimArrow(caster, mainTarget, ArrowLineStyle.Solid, slotIndex, 0));

        if (additionalTargets == null) return;

        for (int i = 0; i < additionalTargets.Length; i++)
        {
            CharacterHandler target = additionalTargets[i];
            if (target == null || target == caster) continue;

            slotArrows.Add(CreateAndAimArrow(caster, target, additionalStyle, slotIndex, i + 1));
        }
    }

    // 랜덤으로 뽑힌 추가 타겟은 점선, 그 외(일정수/모두 등 확정적으로 정해진 타겟)는 파선
    private static ArrowLineStyle GetAdditionalTargetStyle(ActData actData)
    {
        if (actData is ItemActData itemActData && itemActData.UseItem != null
            && itemActData.UseItem.targetCount == ItemTargetCount.Random)
            return ArrowLineStyle.Dotted;

        return ArrowLineStyle.Dashed;
    }

    public void HideFixedArrow(CharacterHandler caster, int slotIndex)
    {
        List<AttackArrowView> slotArrows = GetSlotList(caster, slotIndex);
        if (slotArrows == null) return;

        foreach (AttackArrowView arrow in slotArrows)
            if (arrow != null)
                arrow.gameObject.SetActive(false);
    }

    public void ShowTrackingArrow(CharacterHandler caster, Vector3 targetPosition)
    {
        if (caster == null) return;

        trackingArrow ??= CreateArrowView("TrackingAttackArrow", ArrowLineStyle.Solid);
        trackingArrow.SetTrackingTarget(caster.transform, targetPosition);
        trackingArrow.gameObject.SetActive(true);
    }

    public void HideTrackingArrow()
    {
        if (trackingArrow != null)
            trackingArrow.gameObject.SetActive(false);
    }

    // 후보 전원에게 점선 화살표를 그림 (캐스터 자신은 제외). 후보 집합이 이전과 같으면 다시 그리지 않는다
    // (매 프레임 호출돼도 실제로 후보가 바뀔 때만 재생성하도록).
    public void ShowCandidatePreview(CharacterHandler caster, IEnumerable<CharacterHandler> candidates)
    {
        if (caster == null) return;

        var candidateSet = new HashSet<CharacterHandler>(
            (candidates ?? Enumerable.Empty<CharacterHandler>()).Where(c => c != null && c != caster));

        if (lastPreviewCandidates != null && lastPreviewCandidates.SetEquals(candidateSet))
            return;

        HideCandidatePreview();
        lastPreviewCandidates = candidateSet;

        foreach (CharacterHandler candidate in candidateSet)
        {
            AttackArrowView arrow = CreateArrowView($"CandidatePreview_{candidate.name}", ArrowLineStyle.Dotted);
            arrow.SetFixedTarget(caster.transform, candidate.transform);
            arrow.gameObject.SetActive(fixedArrowsVisible);
            candidatePreviewArrows.Add(arrow);
        }
    }

    public void HideCandidatePreview()
    {
        foreach (AttackArrowView arrow in candidatePreviewArrows)
            if (arrow != null)
                Destroy(arrow.gameObject);

        candidatePreviewArrows.Clear();
        lastPreviewCandidates = null;
    }

    public void SetFixedArrowsVisible(bool visible)
    {
        fixedArrowsVisible = visible;

        foreach (List<AttackArrowView>[] slots in fixedArrowsByCaster.Values)
        {
            foreach (List<AttackArrowView> arrows in slots)
            {
                foreach (AttackArrowView arrow in arrows)
                {
                    if (arrow != null)
                        arrow.gameObject.SetActive(fixedArrowsVisible);
                }
            }
        }
    }

    public void ShowFixedArrows() => SetFixedArrowsVisible(true);
    public void HideFixedArrows() => SetFixedArrowsVisible(false);
    public void ToggleFixedArrows() => SetFixedArrowsVisible(!fixedArrowsVisible);

    public void ClearFixedArrows()
    {
        foreach (List<AttackArrowView>[] slots in fixedArrowsByCaster.Values)
        {
            foreach (List<AttackArrowView> arrows in slots)
            {
                foreach (AttackArrowView arrow in arrows)
                {
                    if (arrow != null)
                        Destroy(arrow.gameObject);
                }
            }
        }

        fixedArrowsByCaster.Clear();
    }

    public void ClearAll()
    {
        ClearFixedArrows();
        HideTrackingArrow();
        HideCandidatePreview();
    }

    // slotIndex의 기존 화살표들을 전부 파괴하고 리스트를 비운다 (새로 그리기 전 정리용)
    private void ClearSlotArrows(CharacterHandler caster, int slotIndex)
    {
        List<AttackArrowView> slotArrows = GetSlotList(caster, slotIndex);
        if (slotArrows == null) return;

        foreach (AttackArrowView arrow in slotArrows)
            if (arrow != null)
                Destroy(arrow.gameObject);

        slotArrows.Clear();
    }

    private List<AttackArrowView> GetSlotList(CharacterHandler caster, int slotIndex)
    {
        if (caster == null) return null;

        if (!fixedArrowsByCaster.ContainsKey(caster))
            InitializeCaster(caster);

        if (!fixedArrowsByCaster.TryGetValue(caster, out List<AttackArrowView>[] slots)) return null;
        if (slotIndex < 0 || slotIndex >= slots.Length) return null;

        return slots[slotIndex];
    }

    private AttackArrowView CreateAndAimArrow(CharacterHandler caster, CharacterHandler target, ArrowLineStyle style, int slotIndex, int targetIndex)
    {
        AttackArrowView arrowView = CreateArrowView($"FixedAttackArrow_{caster.name}_Slot{slotIndex}_{targetIndex}", style);
        arrowView.SetFixedTarget(caster.transform, target.transform);
        arrowView.gameObject.SetActive(fixedArrowsVisible);
        return arrowView;
    }

    private AttackArrowView CreateArrowView(string arrowName, ArrowLineStyle style)
    {
        AttackArrowView arrowView = arrowPrefab != null
            ? Instantiate(arrowPrefab, transform)
            : CreateDefaultArrowView();

        arrowView.name = arrowName;
        arrowView.Initialize(GetStyleMaterial(style), arrowColor, outlineColor, lineWidth, outlineWidth, arrowHeadSize, style);

        return arrowView;
    }

    private AttackArrowView CreateDefaultArrowView()
    {
        GameObject arrowObject = new GameObject("AttackArrow");
        arrowObject.transform.SetParent(transform, false);
        return arrowObject.AddComponent<AttackArrowView>();
    }

    // 스타일별 머티리얼을 캐시해서 같은 스타일끼리 공유한다 (실선은 기존 공유 머티리얼 그대로 재사용)
    private Material GetStyleMaterial(ArrowLineStyle style)
    {
        if (styleMaterials.TryGetValue(style, out Material cached) && cached != null)
            return cached;

        Material baseMaterial = GetArrowMaterial();
        if (baseMaterial == null) return null;

        Material styled = style == ArrowLineStyle.Solid ? baseMaterial : new Material(baseMaterial);

        if (style == ArrowLineStyle.Dashed)
        {
            styled.mainTexture = CreateStripeTexture(10, 6); // 60% 채움
            styled.mainTextureScale = new Vector2(dashTiling, 1f);
        }
        else if (style == ArrowLineStyle.Dotted)
        {
            styled.mainTexture = CreateStripeTexture(10, 3); // 30% 채움, 더 촘촘하게
            styled.mainTextureScale = new Vector2(dashTiling * 1.5f, 1f);
        }

        styleMaterials[style] = styled;
        return styled;
    }

    // size 픽셀짜리 가로 줄무늬 텍스처를 코드로 생성 (앞쪽 onLength만큼 불투명, 나머지는 투명)
    private static Texture2D CreateStripeTexture(int size, int onLength)
    {
        Texture2D texture = new Texture2D(size, 1, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Point
        };

        for (int x = 0; x < size; x++)
            texture.SetPixel(x, 0, x < onLength ? Color.white : new Color(1f, 1f, 1f, 0f));

        texture.Apply();
        return texture;
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
