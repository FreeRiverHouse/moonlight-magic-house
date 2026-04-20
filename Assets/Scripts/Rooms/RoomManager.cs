using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoonlightMagicHouse
{
    public enum RoomType { Bedroom, Kitchen, LivingRoom, Garden, Library }

    public class RoomManager : MonoBehaviour
    {
        [System.Serializable]
        public class Room
        {
            public RoomType type;
            public GameObject root;
            public AudioClip ambience;
        }

        [SerializeField] List<Room> rooms;
        [SerializeField] RoomType startRoom = RoomType.LivingRoom;

        public UnityEvent<RoomType> onRoomChanged;

        Room _current;

        void Start() => GoToRoom(startRoom);

        public void GoToRoom(RoomType type)
        {
            foreach (var r in rooms)
            {
                bool active = r.type == type;
                r.root.SetActive(active);
                if (active)
                {
                    _current = r;
                    AudioManager.Instance?.PlayMusic(r.ambience);
                }
            }
            onRoomChanged?.Invoke(type);
        }

        public RoomType CurrentRoom => _current?.type ?? startRoom;
    }
}
