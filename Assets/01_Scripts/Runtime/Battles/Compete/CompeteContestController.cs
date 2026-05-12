using System.Threading.Tasks;
using _01_Scripts.DTO;
using UnityEngine;
using UnityEngine.Serialization;

namespace _01_Scripts.Runtime.Battles.Compete
{
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
}
