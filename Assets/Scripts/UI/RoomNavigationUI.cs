using UnityEngine;
using UnityEngine.UI;

namespace MoonlightMagicHouse
{
    public class RoomNavigationUI : MonoBehaviour
    {
        [System.Serializable]
        struct RoomButton
        {
            public RoomType room;
            public Button button;
        }

        [SerializeField] RoomButton[] buttons;
        [SerializeField] Color activeColor = new Color(1f, 0.85f, 1f);
        [SerializeField] Color inactiveColor = Color.white;

        RoomManager _rooms;

        void Start()
        {
            _rooms = MoonlightGameManager.Instance.rooms;
            _rooms.onRoomChanged.AddListener(OnRoomChanged);

            foreach (var rb in buttons)
            {
                var room = rb.room;
                rb.button.onClick.AddListener(() => _rooms.GoToRoom(room));
            }

            OnRoomChanged(_rooms.CurrentRoom);
        }

        void OnRoomChanged(RoomType current)
        {
            foreach (var rb in buttons)
            {
                var colors = rb.button.colors;
                colors.normalColor = rb.room == current ? activeColor : inactiveColor;
                rb.button.colors = colors;
            }
        }
    }
}
