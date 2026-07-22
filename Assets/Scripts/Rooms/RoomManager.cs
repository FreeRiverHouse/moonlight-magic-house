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
            public Vector3 entryPosition;
            public Rect movementBounds;
        }

        public List<Room> rooms = new();
        public RoomType startRoom = RoomType.LivingRoom;

        public UnityEvent<RoomType> onRoomChanged;

        Room _current;
        MoonlightPlayerController _player;

        void Start() => GoToRoom(startRoom);

        public void AddRoom(RoomType type, GameObject root)
        {
            Vector3 center = root != null ? root.transform.position : Vector3.zero;
            rooms.Add(new Room
            {
                type = type,
                root = root,
                entryPosition = center + new Vector3(0.65f, 0f, -0.15f),
                movementBounds = new Rect(center.x - 4.25f, center.z - 3.25f, 8.5f, 6.5f)
            });
        }

        public void BindPlayer(MoonlightPlayerController player) => _player = player;

        public void GoToRoom(RoomType type)
        {
            foreach (var r in rooms)
            {
                bool active = r.type == type;
                r.root.SetActive(active);
                if (active)
                {
                    _current = r;
                    if (_player != null)
                        _player.TeleportTo(r.entryPosition, r.movementBounds);
                    if (r.ambience != null)
                        AudioManager.Instance?.PlayMusic(r.ambience);
                }
            }
            AudioManager.Instance?.Play("room-change");
            Camera.main?.GetComponent<CameraController>()?.SetRoomProfile(type, true);
            if (_current != null)
                Debug.Log($"[MoonlightNavigationQA] room-changed room={type} " +
                    $"entry={_current.entryPosition:F2} bounds={_current.movementBounds}");
            onRoomChanged?.Invoke(type);
        }

        public RoomType CurrentRoom => _current?.type ?? startRoom;
    }
}
