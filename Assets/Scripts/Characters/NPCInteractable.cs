using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class NPCInteractable : MonoBehaviour
    {
        [SerializeField] string npcName = "Lunina";
        [SerializeField] List<string> lines;
        [SerializeField] float displayDuration = 4f;
        [SerializeField] float cooldown = 30f;

        int _index;
        float _lastTalk = -999f;

        void OnMouseDown() => TrySpeak();

        public void TrySpeak()
        {
            if (Time.time - _lastTalk < cooldown) return;
            _lastTalk = Time.time;
            if (lines.Count == 0) return;
            var bubble = FindAnyObjectByType<NPCBubble>();
            bubble?.Show(npcName, lines[_index % lines.Count], displayDuration);
            _index++;
            AudioManager.Instance?.Play("interact");
        }
    }
}
