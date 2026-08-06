using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles.Characters;
using _01_Scripts.Runtime.Battles.Phase.Open.EnemyTargeting;
using UnityEngine;
using UnityEngine.Timeline;

namespace _01_Scripts.Runtime.Battles.Phase.Open
{
public class OpenPhaseController : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    private BattlePhaseCoordinator battlePhaseCoordinator;

    [SerializeField] private EnemyActionController enemyActionController;
    [SerializeField] private AttackArrowController attackArrowController;

    [Header("전투 진입 연출")]
    [SerializeField] private ITimelineBinder entryTimelineBinder;

    private CharacterHandler[] playerCharacters;
    private CharacterHandler[] enemyCharacters;

    [SerializeField] private CharacterHandler[] turnOrderCharacters;

    [SerializeField] private Transform[] playerCharacterPositions;
    [SerializeField] private Transform[] enemyCharacterPositions;
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        battlePhaseCoordinator.OnOpenPhaseStart += StartOpenPhase;
        battlePhaseCoordinator.OnBattleStart    += OnBattleStart;
    }

    private void OnDestroy()
    {
        battlePhaseCoordinator.OnOpenPhaseStart -= StartOpenPhase;
        battlePhaseCoordinator.OnBattleStart    -= OnBattleStart;
    }

    private void OnBattleStart(CharacterHandler[] players, CharacterHandler[] enemies)
        => StartBattle(players, enemies);

    private void StartBattle(CharacterHandler[] _playerCharacters, CharacterHandler[] _enemyCharacters)
    {
        Debug.Log("Starting Battle in Open Phase Controller");
        
        playerCharacters = _playerCharacters;
        enemyCharacters = _enemyCharacters;
        
        for(int i = 0; i < playerCharacters.Length; i++)
            playerCharacters[i].TargetingData =
                new ActData[playerCharacters[i].GetCharacterStatus().CharacterData.slotCount];


        for (int i = 0; i < enemyCharacters.Length; i++)
            enemyCharacters[i].TargetingData =
                new ActData[enemyCharacters[i].GetCharacterStatus().CharacterData.slotCount];
        
        StartOpenPhase();
    }
    
    // 개막 페이즈 로직 구현
    private void StartOpenPhase(CharacterHandler[] _playerCharacters, CharacterHandler[] _enemyCharacters)
    {
        playerCharacters = _playerCharacters;
        enemyCharacters = _enemyCharacters;
        
        StartOpenPhase();
    }
    
    private async void StartOpenPhase()
    {
        Debug.Log("Start Open Phase");

        playerCharacters = SortBySpeedDescending(SetCharactersSpeed(playerCharacters));
        enemyCharacters = SortBySpeedDescending(SetCharactersSpeed(enemyCharacters));

        Debug.Log("속도 조정 완료 \n -------------------------------------------------------");

        Debug.Log(" 아군 : ");
        for (int i = 0; i < playerCharacters.Length; i++)
        {
            playerCharacters[i].DebugPrintStatusData();

            playerCharacters[i].PlacementOrder = i;
            SetCharactersPosition(playerCharacters[i], playerCharacterPositions[i]);

            CharacterAnimationMonitor.Instance.PlayAnimation(playerCharacters[i],
                playerCharacters[i].GetCharacterStatus().currentState);
        }

        Debug.Log("적군 : ");
        for (int i = 0; i < enemyCharacters.Length; i++)
        {
            enemyCharacters[i].DebugPrintStatusData();

            enemyCharacters[i].PlacementOrder = i;
            SetCharactersPosition(enemyCharacters[i], enemyCharacterPositions[i]);

            CharacterAnimationMonitor.Instance.PlayAnimation(enemyCharacters[i],
                enemyCharacters[i].GetCharacterStatus().currentState);
        }

        await PlayEntryPresentation();

        turnOrderCharacters = DetermineTurnOrder
        (
            new List<CharacterHandler>(playerCharacters),
            new List<CharacterHandler>(enemyCharacters)
        );

        enemyActionController.SetTargeting(enemyCharacters, playerCharacters);
        attackArrowController.RedrawArrows(turnOrderCharacters);

        CompleteOpenPhaseProcess();
    }

    // 캐릭터별 등장 연출을 전원 동시에 재생한다. CharacterHandler마다 자기 전용 timelineDirector가 있어서
    // 그냥 전부 PlayAsync를 호출해 Task로 모으기만 하면 실제로 병렬 재생된다.
    // 컷신(EnemyData.entryCutscene)은 아직 재생 엔진이 없어서 감지 후 로그만 남기고 기본 진입으로 대체한다.
    private async Task PlayEntryPresentation()
    {
        bool hasCutsceneTarget = enemyCharacters.Any(c => c.GetCharacterStatus().SourceEnemyData?.entryCutscene != null);
        if (hasCutsceneTarget)
            Debug.Log("[OpenPhaseController] 컷신 대상 발견(미구현) — 기본 진입 연출로 대체 재생");

        List<Task> entryTasks = new List<Task>();

        foreach (CharacterHandler character in playerCharacters.Concat(enemyCharacters))
        {
            TimelineAsset entryTimeline = character.GetCharacterStatus().CharacterData.entryTimelineAsset;
            if (entryTimeline == null) continue;

            entryTasks.Add(character.timelineDirector.PlayAsync(character, entryTimeline, entryTimelineBinder, new EntryActData(character, 0)));
        }

        if (entryTasks.Count == 0) return;

        // entryTimelineBinder가 안 붙어있으면 타임라인은 재생되지만 트랙이 하나도 안 묶여서 눈에 보이는 게 없다 —
        // 조용히 "안 되는" 상태를 피하려고 여기서 한 번 경고해준다.
        if (entryTimelineBinder == null)
            Debug.LogWarning("[OpenPhaseController] entryTimelineBinder가 비어있습니다. 진입 연출 타임라인이 바인딩 없이 재생됩니다.");

        await Task.WhenAll(entryTasks);
    }
    
    // 개막 페이즈 로직 동작 완료
    private void CompleteOpenPhaseProcess()
    {
        // Open Phase Logic
        Debug.Log("Open Phase Started");
        
        // Open Phase End
        battlePhaseCoordinator.CompleteOpenPhaseStart(turnOrderCharacters);
    }
    
    // 두개를 하나로 묶는거
    private CharacterHandler[] DetermineTurnOrder(List<CharacterHandler> _playerCharacterBattleDatas, List<CharacterHandler> _enemyCharacterTargetDatas)
    {
        // 속도에 따른 행동 순서 결정
        // 아군, 적군 혼합
        // 속도가 정렬된 값들이 들어와야 기능함
        
        List<CharacterHandler> _allCharacterTargetDatas = new List<CharacterHandler>();
        
        // 속도 비교하여 행동 순서 결정
        // 내림차순 정렬
        
        int i = 0;
        
        while (_playerCharacterBattleDatas.Count != 0 || _enemyCharacterTargetDatas.Count != 0)
        {
            // 이거 하나 다 되면 오류날 예정
            // 하고 오류나는 부분만 고치고 테스트
            
            if (_enemyCharacterTargetDatas.Count == 0 ||
                (_playerCharacterBattleDatas.Count > 0 &&
                 _playerCharacterBattleDatas[0].CurrentSpeed >=
                 _enemyCharacterTargetDatas[0].CurrentSpeed))
            {
                _allCharacterTargetDatas.Add(_playerCharacterBattleDatas[0]);
                _playerCharacterBattleDatas.RemoveAt(0);

                _allCharacterTargetDatas[i].TurnOrder = i;
                _allCharacterTargetDatas[i].PlacementOrder = i;
            }
            else
            {
                _allCharacterTargetDatas.Add(_enemyCharacterTargetDatas[0]);
                _enemyCharacterTargetDatas.RemoveAt(0);

                _allCharacterTargetDatas[i].TurnOrder = i;
                _allCharacterTargetDatas[i].PlacementOrder = i;
            }

            i++;
        }
        
        // if (_playerCharacterBattleDatas.Count == 0)
        //     _allCharacterTargetDatas.AddRange(_enemyCharacterTargetDatas);
        //
        // else 
        //     _allCharacterTargetDatas.AddRange(_playerCharacterBattleDatas);
        
        return _allCharacterTargetDatas.ToArray();
    }    

    private void SetPlacementOrder(CharacterHandler[] _characterBattleDatas)
    {
        for (int i = 0; i < _characterBattleDatas.Length; i++)
        {
            _characterBattleDatas[i].PlacementOrder = i;
        }
    }

    private CharacterHandler[] SortBySpeedDescending(CharacterHandler[] _characterBattleDatas)
    {
        if (_characterBattleDatas == null || _characterBattleDatas.Length <= 1)
            return _characterBattleDatas;

        void QuickSort(int left, int right)
        {
            int i = left;
            int j = right;
            int pivot = _characterBattleDatas[(left + right) / 2].CurrentSpeed;

            while (i <= j)
            {
                while (_characterBattleDatas[i].CurrentSpeed > pivot) i++;
                while (_characterBattleDatas[j].CurrentSpeed < pivot) j--;

                if (i <= j)
                {
                    (_characterBattleDatas[i], _characterBattleDatas[j]) 
                        = (_characterBattleDatas[j], _characterBattleDatas[i]);
                    i++;
                    j--;
                }
            }

            if (left < j) QuickSort(left, j);
            if (i < right) QuickSort(i, right);
        }

        QuickSort(0, _characterBattleDatas.Length - 1);

        return _characterBattleDatas;
    }
     
    private CharacterHandler[] SetCharactersSpeed(CharacterHandler[] _characterBattleDatas)
    {
        CharacterHandler[] _returnBattleDataArray = new CharacterHandler[_characterBattleDatas.Length];

        for (int i = 0; i < _characterBattleDatas.Length; i++)
        {
            _characterBattleDatas[i].SetRandomSpeed();

            _returnBattleDataArray[i] = _characterBattleDatas[i];
        }
        
        return _returnBattleDataArray;
    }
    
    private void SetCharactersPosition(CharacterHandler _characterBattleDatas, Transform _characterPositions)
    {
        // 회전을 먼저 정해야 아래 스폰 오프셋의 facing 기준(transform.right/up)이 정확함.
        // (0,0,0,0)은 크기가 0인 무효 쿼터니언이라 정규화 결과가 불명확해서 identity로 명시.
        if (_characterBattleDatas.characterType == CharacterHandler.CharacterType.Friendly)
            _characterBattleDatas.transform.rotation = Quaternion.identity;
        else
            _characterBattleDatas.transform.rotation = new Quaternion(0, 180, 0, 0);

        if (_characterBattleDatas.characterType == CharacterHandler.CharacterType.Enemy)
            _characterBattleDatas.GetComponent<SpriteRenderer>().flipX = true;

        // 최종 위치 + 진입 연출 스폰 오프셋(캐릭터 자신의 facing 기준)에 배치.
        // 오프셋이 (0,0)이고 entryTimelineAsset도 없으면 스폰 지점 = 최종 위치라 기존 동작과 동일.
        Vector2 spawnOffset = _characterBattleDatas.GetCharacterStatus().CharacterData.entrySpawnOffset;
        Vector3 worldOffset = _characterBattleDatas.transform.right * spawnOffset.x
                               + _characterBattleDatas.transform.up * spawnOffset.y;
        _characterBattleDatas.transform.position = _characterPositions.position + worldOffset;
    }
   
}
}
