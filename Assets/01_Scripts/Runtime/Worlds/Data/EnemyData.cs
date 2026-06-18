using _01_Scripts.DTO;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "World/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public CharacterData characterData;
    }
}
