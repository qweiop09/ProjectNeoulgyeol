using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

public class CompeteContestController : MonoBehaviour
{
    [SerializeField] private CompeteJudgmentHandler competeJudgmentHandler;
    [FormerlySerializedAs("competeINCharacterAnimationHandler")] [SerializeField] private CompeteCharacterAnimationHandler competeCharacterAnimationHandler;
    
    
    // 경합
    public Task StartCompeteCycle(CharacterBattleData currentCompeteCharacter)
    {
        // 경합 사이클 로직
        Debug.Log("Compete Cycle Started");
        
        return Task.CompletedTask;
    }
    
}
