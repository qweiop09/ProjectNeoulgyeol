using System.Collections.Generic;
using System.Linq;
using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Compete
{
public class CompetePhaseController : MonoBehaviour
{
    // class Variables
    [SerializeField] private BattleManager battleManager; 
    private BattlePhaseCoordinator battlePhaseCoordinator;
    
    [SerializeField] private CompeteContestController competeContestController;
    
    
    // data Variables
    private CharacterBattleData[] playerCharacterBattleDatas;
    private CharacterBattleData[] enemyCharacterTargetDatas;
    
    private CharacterBattleData[] allCharacterTargetDatas;
    
    private void Awake()
    {
        // 이벤트 구독
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        
        battlePhaseCoordinator.OnCompetePhaseStart += (data1, data2)
            => StartCompetePhaseStartProcess(
                ChangeCharacterDataToCharacterBattleData(data1),
                ChangeCharacterDataToCharacterBattleData(data2));
        
        battlePhaseCoordinator.OnCompetePhasePerform += StartCompetePhaseMiddleProcess;
        battlePhaseCoordinator.OnCompetePhaseEnd += StartCompetePhaseEndProcess;
    }
    
    private CharacterBattleData[] ChangeCharacterDataToCharacterBattleData(CharacterBattleData[] _characterStatuses)
    {
        CharacterBattleData[] _returnBattleDataArray = new CharacterBattleData[_characterStatuses.Length];
        
        for (int i = 0; i < _characterStatuses.Length; i++)
        {
            _returnBattleDataArray[i] =  _characterStatuses[i];
        }
    
        return _returnBattleDataArray;
    }
    
    // Start Phase Actions
    private void StartCompetePhaseStartProcess(CharacterBattleData[] _playerCharacters, CharacterBattleData[] _enemyCharacters)
    {
        playerCharacterBattleDatas = _playerCharacters;
        enemyCharacterTargetDatas = _enemyCharacters;
        
        List<CharacterBattleData> playerCharacters = new List<CharacterBattleData>();
        List<CharacterBattleData> enemyCharacters = new List<CharacterBattleData>();
        
        for(int i = 0; i < _playerCharacters.Length; i++)
        {
            playerCharacters.Add(_playerCharacters[i]);
        }
        for (int i = 0; i < _enemyCharacters.Length; i++)
        {
            enemyCharacters.Add(_enemyCharacters[i]);
        }
        
        SetCharactersTargetingDatas(playerCharacters, enemyCharacters);
        
        CompleteCompetePhaseStartProcess();
    }
    
    private void SetCharactersTargetingDatas(List<CharacterBattleData> _playerCharacterBattleDatas, List<CharacterBattleData> _enemyCharacterTargetDatas)
    {
        // 속도에 따른 행동 순서 결정
        // 아군, 적군 혼합
        // 속도가 정렬된 값들이 들어와야 기능함
        
        List<CharacterBattleData> _allCharacterTargetDatas = new List<CharacterBattleData>();
        
        // 속도 비교하여 행동 순서 결정
        // 속도가 더 높은 쪽이 앞쪽에 있음
        while (_playerCharacterBattleDatas.Count > 0 && _enemyCharacterTargetDatas.Count > 0)
        {
            if (_playerCharacterBattleDatas[0].GetCurrentSpeed >= _enemyCharacterTargetDatas[0].GetCurrentSpeed)
            {
                _allCharacterTargetDatas.Add(_playerCharacterBattleDatas[0]);
                _playerCharacterBattleDatas.RemoveAt(0);
            }
            else
            {
                _allCharacterTargetDatas.Add(_enemyCharacterTargetDatas[0]);
                _enemyCharacterTargetDatas.RemoveAt(0);
            }
        }
        if (_playerCharacterBattleDatas.Count == 0)
        {
            _allCharacterTargetDatas.AddRange(_enemyCharacterTargetDatas);
        }
        else 
        {
            _allCharacterTargetDatas.AddRange(_playerCharacterBattleDatas);
        }

        allCharacterTargetDatas = _allCharacterTargetDatas.ToArray();
    }    
    
    
    private void CompleteCompetePhaseStartProcess()
    {
        Debug.Log("Compete Phase Started");
        battlePhaseCoordinator.CompleteCompeteStart();
    }
    
    
    // Middle Phase Actions
    private async void StartCompetePhaseMiddleProcess()
    {
        for (int i = 0; i < allCharacterTargetDatas.Count(); i++)
        {
            CharacterBattleData currentCharacter = allCharacterTargetDatas[i];

            // Compete Cycle Phase
            await competeContestController.StartCompeteCycle(currentCharacter);

        }

    }


    private void CompleteCompetePhaseMiddleProcess()
    {
        Debug.Log("Compete Phase Performing");
        battlePhaseCoordinator.CompleteCompetePerform();
    }
    
    
    // End Phase Actions
    private void StartCompetePhaseEndProcess()
    {
        CompleteCompetePhaseEndProcess();
    }
    
    private void CompleteCompetePhaseEndProcess()
    {
        // Compete Phase Logic
        Debug.Log("Compete Phase Ended");
        
        // Compete Phase End
        battlePhaseCoordinator.CompleteCompeteEnd();
    }
}
}
