using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using _01_Scripts.DTO;
using _01_Scripts.Runtime.Battles.Compete._01_Scripts.Runtime.Battles.Compete;
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
    private CharacterHandler[] playerCharacterHandlers;
    private CharacterHandler[] enemyCharacterHandlers;
    
    private CharacterHandler[] allCharacterHandlers;
    
    private void Awake()
    {
        // 이벤트 구독
        battlePhaseCoordinator = battleManager.GetBattlePhaseCoordinator();
        
        battlePhaseCoordinator.OnCompetePhaseStart += (data1, data2)
            => StartCompetePhaseStartProcess(data1, data2);
        
        battlePhaseCoordinator.OnCompetePhasePerform += StartCompetePhaseMiddleProcess;
        battlePhaseCoordinator.OnCompetePhaseEnd += StartCompetePhaseEndProcess;
    }
    
    // Start Phase Actions
    private void StartCompetePhaseStartProcess(CharacterHandler[] _playerCharacters, CharacterHandler[] _enemyCharacters)
    {
        playerCharacterHandlers = _playerCharacters;
        enemyCharacterHandlers = _enemyCharacters;
        
        List<CharacterHandler> playerCharacters = new List<CharacterHandler>();
        List<CharacterHandler> enemyCharacters = new List<CharacterHandler>();
        
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
    
    // 두개를 하나로 묶는거
    private void SetCharactersTargetingDatas(List<CharacterHandler> _playerCharacterBattleDatas, List<CharacterHandler> _enemyCharacterTargetDatas)
    {
        // 속도에 따른 행동 순서 결정
        // 아군, 적군 혼합
        // 속도가 정렬된 값들이 들어와야 기능함
        
        List<CharacterHandler> _allCharacterTargetDatas = new List<CharacterHandler>();
        
        // 속도 비교하여 행동 순서 결정
        // 속도가 더 높은 쪽이 앞쪽에 있음
        while (_playerCharacterBattleDatas.Count > 0 && _enemyCharacterTargetDatas.Count > 0)
        {
            if (_playerCharacterBattleDatas[0].GetCharacterBattleData().CurrentSpeed >= _enemyCharacterTargetDatas[0].GetCharacterBattleData().CurrentSpeed)
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

        allCharacterHandlers = _allCharacterTargetDatas.ToArray();
    }    
    
    private void CompleteCompetePhaseStartProcess()
    {
        Debug.Log("Compete Phase Started");
        battlePhaseCoordinator.CompleteCompeteStart();
    }
    
    // Middle Phase Actions
    private async void StartCompetePhaseMiddleProcess()
    {
        // 모든 캐릭터의 행동을 실행
        for (int i = 0; i < allCharacterHandlers.Count(); i++)
        {
            CharacterHandler currentCharacter = allCharacterHandlers[i];

            // Compete Cycle Phase
            // 한 캐릭터의 모든 행동을 실행
            await competeContestController.StartCompeteCycle(currentCharacter.GetCharacterBattleData().TargetingData);
            Debug.Log("Compete Cycle Completed for Character: " + currentCharacter.name);
        }

        CompleteCompetePhaseMiddleProcess();
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
