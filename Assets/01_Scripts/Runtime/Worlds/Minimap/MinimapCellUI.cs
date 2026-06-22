using UnityEngine;
using UnityEngine.UI;

namespace _01_Scripts.Runtime.Worlds.Minimap
{
    public class MinimapCellUI : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private GameObject _currentIndicator; // 현재 위치 강조 표시

        private static readonly Color ColorUnvisited  = new(0.15f, 0.15f, 0.15f, 0.9f);
        private static readonly Color ColorVisited    = new(0.75f, 0.75f, 0.75f, 1f);
        private static readonly Color ColorCurrent    = new(1f,    0.88f, 0.2f,  1f);
        private static readonly Color ColorBoss       = new(0.8f,  0.2f,  0.2f,  1f);
        private static readonly Color ColorShop       = new(0.2f,  0.75f, 0.4f,  1f);
        private static readonly Color ColorRest       = new(0.25f, 0.5f,  0.9f,  1f);
        private static readonly Color ColorSafe       = new(0.3f,  0.75f, 0.75f, 1f);

        private RoomData _data;

        public void Initialize(RoomData data)
        {
            _data = data;
            if (_background != null)
                _background.color = ColorUnvisited;
            if (_currentIndicator != null)
                _currentIndicator.SetActive(false);
        }

        public void UpdateState(bool isVisited, bool isCurrent)
        {
            if (_background == null) return;

            if (isCurrent)
                _background.color = ColorCurrent;
            else if (isVisited)
                _background.color = GetVisitedColor(_data != null ? _data.roomType : RoomType.Normal);
            else
                _background.color = ColorUnvisited;

            if (_currentIndicator != null)
                _currentIndicator.SetActive(isCurrent);
        }

        private static Color GetVisitedColor(RoomType type)
        {
            return type switch
            {
                RoomType.Boss  => ColorBoss,
                RoomType.Shop  => ColorShop,
                RoomType.Rest  => ColorRest,
                RoomType.Safe  => ColorSafe,
                _              => ColorVisited
            };
        }
    }
}
