using System;
using System.Collections.Generic;
using System.Linq;
using _01_Scripts.DTO;
using _01_Scripts.DTO.Item;
using _01_Scripts.Runtime.Battles.CameraControlle;
using _01_Scripts.Runtime.Battles.Decision;
using _01_Scripts.Runtime.Battles.Phase.Decision.ActionMenu;
using _01_Scripts.Runtime.Worlds.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _01_Scripts.Runtime.Battles.Phase.Decision
{
public class ActionSelectionPhaseManager : MonoBehaviour
{
    private enum SelectionState
    {
        Stay,
        SelectingActCaster,
        SelectingAction,
        SelectingActTarget
    }

    [SerializeField] private SelectionState currentState = SelectionState.Stay;

    [SerializeField] private CharacterChoiceController characterChoiceController;
    [SerializeField] private CharacterActionUIController characterActionUIController;
    [SerializeField] private AttackArrowController attackArrowController;
    [SerializeField] private Camera trackingCamera;

    public event Action<ActData> CompleteActSelected;

    [Space(10)]
    [Header("Internal Fields")]

    [SerializeField] private CharacterHandler selectedActCaster;
    [SerializeField] private CharacterHandler selectedActTarget;

    private ActData _currentActData;

    // 이번 전투 라운드의 전체 로스터(아군+적군 병합) — 아이템 타겟 후보 조회용. StartActionSelectionPhase에서 채워지고 라운드 내내 유지됨.
    private CharacterHandler[] allBattleCharacters;

    private Action<CharacterSkill> _onActionSettingCompleted;
    private Action<Item>           _onItemActionSettingCompleted;
    private Action                 _onStayActionSettingCompleted;

    private void OnEnable()
    {
        EnsureAttackArrowController();
        EnsureTrackingCamera();

        characterChoiceController.OnCharacterSelected += HandleSelectionSignal;

        _onActionSettingCompleted = (data) =>
        {
            // 이전에 아이템을 골랐다가 스킬/공격으로 바꾸는 경우, 남아있던 예약을 먼저 풀어준다
            ReleaseIfReservedItem();

            _currentActData = new SkillActData
            {
                CastPlayerCharacter = selectedActCaster,
                UseSlot             = _currentActData?.UseSlot ?? 0,
                UseSkill            = data
            };
            ChangeSelectionState(SelectionState.SelectingActTarget);
        };

        _onItemActionSettingCompleted = (item) =>
        {
            // 같은 방식으로 이전 선택(다른 아이템 포함)의 예약부터 해제
            ReleaseIfReservedItem();

            // 장비 장착의 equippedBy와 같은 방식: 선택 시점에 바로 예약해서 같은 라운드 내 중복 사용을 막는다
            if (!InventoryManager.Instance.ReserveItem(item, 1))
            {
                Debug.LogWarning($"[ActionSelectionPhaseManager] '{item.itemName}' 예약 실패 — 이미 다른 캐릭터가 사용하기로 했습니다.");
                return;
            }

            var itemActData = new ItemActData
            {
                CastPlayerCharacter = selectedActCaster,
                UseSlot             = _currentActData?.UseSlot ?? 0,
                UseItem             = item
            };

            if (item.targetScope == ItemTargetScope.Self)
            {
                // 자신 대상 아이템은 타겟 선택 단계 없이 즉시 확정 (Stay와 같은 패턴)
                itemActData.TargetPlayerCharacter = selectedActCaster;
                itemActData.TargetSlot = 0;
                _currentActData = itemActData;

                // mainTarget==caster라 SetFixedArrow 내부에서 자동으로 화살표를 그리지 않음
                attackArrowController?.SetFixedArrow(selectedActCaster, itemActData.UseSlot, selectedActCaster, Array.Empty<CharacterHandler>());
                attackArrowController?.HideTrackingArrow();

                CompleteActSelected?.Invoke(_currentActData);

                // selectedActCaster는 여기서 null 처리하지 않는다 — ChangeSelectionState의 취소 분기가
                // (currentState가 아직 SelectingAction이라 그 경로를 타므로) selectedActCaster.name을 로그에
                // 쓴 뒤 자체적으로 null 처리한다 (Stay 확정 경로와 동일한 패턴).
                selectedActTarget = null;
                _currentActData = null;

                ChangeSelectionState(SelectionState.SelectingActCaster);
                return;
            }

            _currentActData = itemActData;
            ChangeSelectionState(SelectionState.SelectingActTarget);
        };

        _onStayActionSettingCompleted = () =>
        {
            int slot = _currentActData?.UseSlot ?? 0;
            ReleaseIfReservedItem();

            StayActData stayData = new StayActData(selectedActCaster, slot);

            attackArrowController?.HideTrackingArrow();
            CompleteActSelected?.Invoke(stayData);
            selectedActTarget = null;
            _currentActData = null;

            ChangeSelectionState(SelectionState.SelectingActCaster);
        };

        characterActionUIController.CompletedActionSetting     += _onActionSettingCompleted;
        characterActionUIController.CompletedItemActionSetting += _onItemActionSettingCompleted;
        characterActionUIController.CompletedStayActionSetting += _onStayActionSettingCompleted;
    }

    private void OnDisable()
    {
        characterChoiceController.OnCharacterSelected -= HandleSelectionSignal;

        if (characterActionUIController != null)
        {
            characterActionUIController.CompletedActionSetting     -= _onActionSettingCompleted;
            characterActionUIController.CompletedItemActionSetting -= _onItemActionSettingCompleted;
            characterActionUIController.CompletedStayActionSetting -= _onStayActionSettingCompleted;
        }

        attackArrowController?.HideTrackingArrow();
    }

    private void LateUpdate()
    {
        if (currentState != SelectionState.SelectingActTarget || selectedActCaster == null)
        {
            attackArrowController?.HideTrackingArrow();
            return;
        }

        // [변경] 트래킹 시작 시 해당 슬롯의 기존 고정 화살표 숨김
        attackArrowController?.HideFixedArrow(selectedActCaster, _currentActData?.UseSlot ?? 0);
        attackArrowController?.ShowTrackingArrow(selectedActCaster, GetPointerWorldPosition());
    }

    public void StartActionSelectionPhase(CharacterHandler[] allCharacters)
    {
        Debug.Log("Starting Action Selection Phase");
        allBattleCharacters = allCharacters;
        ClearManager();
        SelectActCaster();
    }

    public void EndActionSelectionPhase()
    {
        Debug.Log("Ending Action Selection Phase");
        attackArrowController?.ClearAll();
        ClearManager();
    }

    private void ClearManager()
    {
        DeactivateCharacterSelectionPhase();

        ReleaseIfReservedItem();

        currentState = SelectionState.Stay;
        selectedActTarget = null;
        selectedActCaster = null;
        _currentActData = null;
    }

    // 확정(CompleteActSelected 호출)되지 않은 채로 아이템 선택이 버려질 때 예약을 되돌린다
    private void ReleaseIfReservedItem()
    {
        if (_currentActData is ItemActData itemActData)
            InventoryManager.Instance.ReleaseReservation(itemActData.UseItem, 1);
    }

    // scope에 맞는 타겟 후보(생존자만) 조회. Ally scope엔 caster 자신도 포함된다.
    private IEnumerable<CharacterHandler> GetCandidates(ItemTargetScope scope, CharacterHandler caster)
    {
        if (allBattleCharacters == null) return Enumerable.Empty<CharacterHandler>();

        IEnumerable<CharacterHandler> alive = allBattleCharacters
            .Where(c => c != null && c.GetCharacterStatus().currentState != CharacterState.Dead);

        return scope switch
        {
            ItemTargetScope.Ally  => alive.Where(c => c.characterType == caster.characterType),
            ItemTargetScope.Enemy => alive.Where(c => c.characterType != caster.characterType),
            _                     => alive // Any
        };
    }

    // candidate가 이번 라운드에 이미 다른 미확정 행동의 타겟(메인 또는 추가)으로 걸려있는지
    private bool IsTargeted(CharacterHandler candidate)
    {
        if (allBattleCharacters == null) return false;

        foreach (CharacterHandler c in allBattleCharacters)
        {
            if (c == null || c.TargetingData == null) continue;

            foreach (ActData act in c.TargetingData)
            {
                if (act == null) continue;
                if (act.TargetPlayerCharacter == candidate) return true;
                if (act.AdditionalTargets != null && Array.IndexOf(act.AdditionalTargets, candidate) >= 0) return true;
            }
        }

        return false;
    }

    // 메인 타겟 제외 후보를 빈슬롯 > 진영(메인과 동일) > 속도(느린 순)로 정렬해 totalCount-1명 선택
    private CharacterHandler[] SelectPriorityTargets(CharacterHandler mainTarget, IEnumerable<CharacterHandler> candidates, int totalCount)
    {
        return candidates
            .Where(c => c != mainTarget)
            .OrderByDescending(c => !IsTargeted(c))
            .ThenByDescending(c => c.characterType == mainTarget.characterType)
            .ThenBy(c => c.CurrentSpeed)
            .Take(Mathf.Max(0, totalCount - 1))
            .ToArray();
    }

    // 메인 타겟 제외 후보에서 비복원 랜덤으로 totalCount-1명 선택
    private CharacterHandler[] SelectRandomTargets(CharacterHandler mainTarget, IEnumerable<CharacterHandler> candidates, int totalCount)
    {
        List<CharacterHandler> pool = candidates.Where(c => c != mainTarget).ToList();
        List<CharacterHandler> picked = new List<CharacterHandler>();
        int need = Mathf.Max(0, totalCount - 1);

        for (int i = 0; i < need && pool.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            picked.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return picked.ToArray();
    }

    // 아이템의 targetScope(Ally/Enemy) 제한을 클릭된 candidate가 만족하는지
    private bool IsValidTarget(Item item, CharacterHandler candidate)
    {
        if (candidate == null || selectedActCaster == null) return false;

        return item.targetScope switch
        {
            ItemTargetScope.Ally  => candidate.characterType == selectedActCaster.characterType,
            ItemTargetScope.Enemy => candidate.characterType != selectedActCaster.characterType,
            _                     => true // Any, Self(Self는 클릭 단계 자체가 없음)
        };
    }

    private void SelectActCaster()
    {
        ActivateCharacterSelectionPhase();
        ChangeSelectionState(SelectionState.SelectingActCaster);
    }

    public void ActivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Activated");
        characterChoiceController.ActivateActionSelectionPhase();
        ChangeSelectionState(SelectionState.SelectingActCaster);
    }

    public void DeactivateCharacterSelectionPhase()
    {
        Debug.Log("Action Selection Phase Deactivated");
        characterChoiceController.DeactivateActionSelectionPhase();
        ChangeSelectionState(SelectionState.Stay);
    }

    private void HandleSelectionSignal(CharacterHandler characterHandler)
    {
        // [변경] null 수신 시 return 누락 버그 수정 — 기존엔 null 체크 후에도 아래 코드가 실행됐음
        if (characterHandler == null)
        {
            Debug.Log("Null character handler received, ignoring.");
            if (currentState == SelectionState.Stay) return;
            if (currentState == SelectionState.SelectingActCaster) return;

            ChangeSelectionState(currentState - 1);
            return; // 추가
        }

        Debug.Log("Character Selected: " + characterHandler.name + " in state: " + currentState);

        if (currentState == SelectionState.SelectingActCaster)
        {
            if (characterHandler.characterType == CharacterHandler.CharacterType.Enemy) return;

            Debug.Log("Setting selected act caster: " + characterHandler.name);
            selectedActCaster = characterHandler;
            ChangeSelectionState(SelectionState.SelectingAction);
        }
        else if (currentState == SelectionState.SelectingActTarget)
        {
            if (_currentActData is ItemActData pendingItemActData && !IsValidTarget(pendingItemActData.UseItem, characterHandler))
                return; // 진영 제한 위반 클릭은 무시

            selectedActTarget = characterHandler;

            _currentActData.TargetPlayerCharacter = characterHandler;
            _currentActData.TargetSlot = 0; // TODO: 슬롯 선택 기능 추가되면 변경

            if (_currentActData is ItemActData itemActData)
            {
                Item item = itemActData.UseItem;
                IEnumerable<CharacterHandler> candidates = GetCandidates(item.targetScope, selectedActCaster);

                itemActData.AdditionalTargets = item.targetCount switch
                {
                    ItemTargetCount.Fixed  => SelectPriorityTargets(characterHandler, candidates, item.targetCountValue),
                    ItemTargetCount.All    => candidates.Where(c => c != characterHandler).ToArray(),
                    ItemTargetCount.Random => SelectRandomTargets(characterHandler, candidates, item.targetCountValue),
                    _                      => Array.Empty<CharacterHandler>() // Single
                };
            }

            // SetFixedArrow — 슬롯 인덱스 기반으로 저장, 기존과 동일
            attackArrowController?.SetFixedArrow(selectedActCaster, _currentActData.UseSlot, selectedActTarget, _currentActData.AdditionalTargets);
            attackArrowController?.HideTrackingArrow();

            CompleteActSelected?.Invoke(_currentActData);

            selectedActCaster = null;
            selectedActTarget = null;
            _currentActData = null;

            ChangeSelectionState(SelectionState.SelectingActCaster);
        }
        else
        {
            Debug.Log("Character selected in ??? state, ignoring: "
                      + characterHandler.name + "\n Current State: " + currentState);
        }
    }

    private async void ChangeSelectionState(SelectionState newState)
    {
        if (currentState == SelectionState.SelectingAction && newState != SelectionState.SelectingAction)
        {
            if (currentState == newState + 1)
            {
                Debug.Log("Clearing selected act character: " + selectedActCaster.name);
                characterActionUIController.HandleSelectionCleared();

                attackArrowController?.HideTrackingArrow();

                ReleaseIfReservedItem();
                _currentActData = null;
                selectedActCaster = null;
            }
            else
            {
                characterActionUIController.HideMenu();
            }

            // [변경] await 전에 상태 먼저 변경 → LateUpdate가 즉시 트래킹 화살표 시작
            currentState = newState;
        
            await CameraHandler.Instance.PositionResetToLerp();
            return;
        }

        if (currentState == newState) return;

        Debug.Log("Changing Selection State: " + currentState + " -> " + newState);

        currentState = newState;

        if (currentState == SelectionState.SelectingAction)
        {
            // 타겟 선택을 취소하고 돌아온 경우 포함 — 스킬/공격과 마찬가지로 아이템도 다시 골라야 하므로
            // 매달려 있던 예약이 있다면 여기서 같이 풀어준다 (안 그러면 본인이 예약한 아이템이 본인 메뉴에서 사라져 보임).
            ReleaseIfReservedItem();
            _currentActData = null;

            characterActionUIController.HandleCharacterSelected(selectedActCaster);
            await CameraHandler.Instance.MoveToLerp(
                selectedActCaster.transform.position + new Vector3(1.2f, 0.2f, -10), 1.2f);
        }
    }
    public void SetAttackArrowsVisible(bool visible) => attackArrowController?.SetFixedArrowsVisible(visible);
    public void ShowAttackArrows() => attackArrowController?.ShowFixedArrows();
    public void HideAttackArrows() => attackArrowController?.HideFixedArrows();
    public void ToggleAttackArrows() => attackArrowController?.ToggleFixedArrows();

    private void EnsureAttackArrowController()
    {
        if (attackArrowController != null) return;

        attackArrowController = FindFirstObjectByType<AttackArrowController>();

        if (attackArrowController != null) return;

        GameObject attackArrowControllerObject = new GameObject("AttackArrowController");
        attackArrowController = attackArrowControllerObject.AddComponent<AttackArrowController>();
    }

    private void EnsureTrackingCamera()
    {
        if (trackingCamera == null)
            trackingCamera = Camera.main;
    }

    private Vector3 GetPointerWorldPosition()
    {
        EnsureTrackingCamera();
    
        if (trackingCamera == null)
            return selectedActCaster.transform.position;
    
        Vector2 pointerScreenPosition = Pointer.current != null
            ? Pointer.current.position.ReadValue()
            : Vector2.zero;
    
        Vector3 screenPosition = new Vector3(
            pointerScreenPosition.x,
            pointerScreenPosition.y,
            Mathf.Abs(trackingCamera.transform.position.z - selectedActCaster.transform.position.z));
    
        return trackingCamera.ScreenToWorldPoint(screenPosition);
    }
}
}