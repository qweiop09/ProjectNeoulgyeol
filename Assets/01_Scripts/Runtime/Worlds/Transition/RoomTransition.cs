using System;
using System.Collections;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    public abstract class RoomTransition : MonoBehaviour
    {
        /// <summary>
        /// 전환 연출을 실행한다.
        /// onMidpoint: 화면이 완전히 가려졌을 때 방 교체 로직을 호출한다.
        /// </summary>
        public abstract IEnumerator Execute(Action onMidpoint);
    }
}
