using System.Threading.Tasks;
using _01_Scripts.Timeline;
using UnityEngine;

namespace _01_Scripts.Runtime.Battles.Compete
{
public class CompeteMoveHandler : MonoBehaviour
{
    // 캐릭터 위치 값
    [SerializeField] private Transform[] playerCharacterPoints;
    [SerializeField] private Transform[] enemyCharacterPoints;

    public Task HandleCompeteMove( Transform _movePoint, Transform _targetPoint)
    {
        // while (Vector3.Distance(_movePoint.position, _targetPoint.position) > 0.1f)
        // {
        //     
        //     _movePoint.position = Vector3.Lerp(_movePoint.position, _targetPoint.position, 0.1f * Time.deltaTime);
        //     Task.Delay(10);
        //
        // } 
        //
        // return Task.CompletedTask;
        
        MoveToTargetTimelineDirector.Instance.PlayMoveToTargetClip(_movePoint, _targetPoint);
    }
    
}
