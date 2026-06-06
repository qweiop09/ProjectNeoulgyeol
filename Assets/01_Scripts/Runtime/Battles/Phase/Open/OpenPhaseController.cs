using System.Collections.Generic;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Phase.Open
{
public class OpenPhaseController : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    private CharacterHandler[] playerCharacters;
    private CharacterHandler[] enemyCharacters;
    
    [SerializeField] private CharacterHandler[] turnOrderCharacters;
    
    [SerializeField] private Transform[] playerCharacterPositions;
    [SerializeField] private Transform[] enemyCharacterPositions;
    
    private void Awake()
    {
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        battlePhaseCoordinator.OnOpenPhaseStart += StartOpenPhase;
        
        battlePhaseCoordinator.OnBattleStart += (data1, data2)
            => StartBattle(data1, data2);
    }

    private void StartBattle(CharacterHandler[] _playerCharacters, CharacterHandler[] _enemyCharacters)
    {
        Debug.Log("Starting Battle in Open Phase Controller");
        
        playerCharacters = _playerCharacters;
        enemyCharacters = _enemyCharacters;
        
        for(int i = 0; i < playerCharacters.Length; i++)
        {
            playerCharacters[i].GetCharacterBattleData().TargetingData = 
                new ActData[playerCharacters[i].characterBattleData.CharacterData.slotCount];
            
            // for( int ii = 0; ii < playerCharacters[i].characterBattleData.CharacterData.slotCount; ii++)
            // {
            //     Debug.Log(ii);
            //     
            //     playerCharacters[i].GetCharacterBattleData().TargetingData[ii] = new ActData();
            //     playerCharacters[i].GetCharacterBattleData().TargetingData[ii].CastPlayerCharacter = playerCharacters[i];
            // }
        }
        for (int i = 0; i < enemyCharacters.Length; i++)
        {
            enemyCharacters[i].GetCharacterBattleData().TargetingData = 
                new ActData[enemyCharacters[i].characterBattleData.CharacterData.slotCount];
            // for(int ii = 0; ii < enemyCharacters[i].characterBattleData.CharacterData.slotCount; ii++)
            // {
            //     playerCharacters[i].GetCharacterBattleData().TargetingData[ii] = new ActData();
            //     enemyCharacters[i].GetCharacterBattleData().TargetingData[ii].CastPlayerCharacter = enemyCharacters[i];
            // }
        }
        
        StartOpenPhase();
    }
    
    // 개막 페이즈 로직 구현
    private void StartOpenPhase()
    {
        Debug.Log("Start Open Phase");
        
        playerCharacters = SortBySpeedDescending(SetCharactersSpeed(playerCharacters));
        enemyCharacters = SortBySpeedDescending(SetCharactersSpeed(enemyCharacters));
        
        Debug.Log("속도 조정 완료 \n -------------------------------------------------------");
        
        Debug.Log(" 아군 : ");
        for (int i = 0; i < playerCharacters.Length; i++)
        {
            playerCharacters[i].GetCharacterBattleData().DebugPrintStatusData();
            
            playerCharacters[i].GetCharacterBattleData().PlacementOrder = i;
            SetCharactersPosition(playerCharacters[i], playerCharacterPositions[i]);
        } 
        
        Debug.Log("적군 : ");
        for (int i = 0; i < enemyCharacters.Length; i++)
        {
            enemyCharacters[i].GetCharacterBattleData().DebugPrintStatusData();
            
            enemyCharacters[i].GetCharacterBattleData().PlacementOrder = i;
            SetCharactersPosition(enemyCharacters[i], enemyCharacterPositions[i]);
        }
        
        turnOrderCharacters = DetermineTurnOrder
        (
            new List<CharacterHandler>(playerCharacters), 
            new List<CharacterHandler>(enemyCharacters)
        );
            
        CompleteOpenPhaseProcess();
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
                 _playerCharacterBattleDatas[0].GetCharacterBattleData().CurrentSpeed >=
                 _enemyCharacterTargetDatas[0].GetCharacterBattleData().CurrentSpeed))
            {
                _allCharacterTargetDatas.Add(_playerCharacterBattleDatas[0]);
                _playerCharacterBattleDatas.RemoveAt(0);
                
                _allCharacterTargetDatas[i].GetCharacterBattleData().TurnOrder = i;
                _allCharacterTargetDatas[i].GetCharacterBattleData().PlacementOrder = i;
            }
            else 
            {
                _allCharacterTargetDatas.Add(_enemyCharacterTargetDatas[0]);
                _enemyCharacterTargetDatas.RemoveAt(0);
                
                _allCharacterTargetDatas[i].GetCharacterBattleData().TurnOrder = i;
                _allCharacterTargetDatas[i].GetCharacterBattleData().PlacementOrder = i;
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
            _characterBattleDatas[i].GetCharacterBattleData().PlacementOrder = i;
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
            int pivot = _characterBattleDatas[(left + right) / 2].GetCharacterBattleData().CurrentSpeed;

            while (i <= j)
            {
                while (_characterBattleDatas[i].GetCharacterBattleData().CurrentSpeed > pivot) i++;
                while (_characterBattleDatas[j].GetCharacterBattleData().CurrentSpeed < pivot) j--;

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
            _characterBattleDatas[i].GetCharacterBattleData().SetRandomSpeed();
            
            _returnBattleDataArray[i] = _characterBattleDatas[i];
        }
        
        return _returnBattleDataArray;
    }
    
    private void SetCharactersPosition(CharacterHandler _characterBattleDatas, Transform _characterPositions)
    {
        // 캐릭터 위치 설정
        _characterBattleDatas.transform.position = _characterPositions.position;
        if (_characterBattleDatas.characterType == CharacterHandler.CharacterType.Friendly)
            _characterBattleDatas.transform.rotation = new Quaternion(0, 0, 0, 0);
        else
            _characterBattleDatas.transform.rotation = new Quaternion(0, 180, 0, 0);
    }
   
}
}
