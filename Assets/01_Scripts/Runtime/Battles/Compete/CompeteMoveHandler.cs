using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CompeteMoveHandler : MonoBehaviour
{
    // 캐릭터 위치 값
    [SerializeField] private Transform[] playerCharacterPoints;
    [SerializeField] private Transform[] enemyCharacterPoints;

    public Task HandleCompeteMove( Transform _movePoint, Transform _targetPoint)
    {
        while (Vector3.Distance(_movePoint.position, _targetPoint.position) > 0.1f)
        {
            
            _movePoint.position = Vector3.Lerp(_movePoint.position, _targetPoint.position, 0.1f * Time.deltaTime);
            Task.Delay(10);

        } 
        
        return Task.CompletedTask;
    }
    
}
