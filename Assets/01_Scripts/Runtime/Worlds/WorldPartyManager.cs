using System.Collections.Generic;
using _01_Scripts.DTO;
using _01_Scripts.Interfacese;
using UnityEngine;

namespace _01_Scripts.Runtime.Worlds
{
    public class WorldPartyManager : Singleton<WorldPartyManager>
    {
        [SerializeField] private List<CharacterData> _party = new();

        public IReadOnlyList<CharacterData> Party => _party;

        public CharacterBattleData[] BuildBattleParty()
        {
            var result = new CharacterBattleData[_party.Count];
            for (int i = 0; i < _party.Count; i++)
                result[i] = new CharacterBattleData(_party[i]);
            return result;
        }

        public void AddMember(CharacterData character) => _party.Add(character);
        public void RemoveMember(CharacterData character) => _party.Remove(character);
    }
}
