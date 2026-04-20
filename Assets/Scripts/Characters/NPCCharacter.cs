using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public enum NPCPersonality { Lunina, Stellina, Noctua }

    [System.Serializable]
    public class DialogueLine
    {
        public string text;
        public float duration = 3f;
    }

    public class NPCCharacter : MonoBehaviour
    {
        [Header("Identity")]
        public string npcName;
        public NPCPersonality personality;
        [TextArea(2, 5)] public string greeting;

        [Header("Dialogue pools")]
        [SerializeField] List<DialogueLine> idleLines;
        [SerializeField] List<DialogueLine> happyLines;
        [SerializeField] List<DialogueLine> sadLines;

        [Header("Bubble")]
        [SerializeField] NPCBubble bubble;

        [SerializeField] float idleInterval = 15f;

        Coroutine _loop;

        void OnEnable()  => _loop = StartCoroutine(IdleTalk());
        void OnDisable() { if (_loop != null) StopCoroutine(_loop); }

        IEnumerator IdleTalk()
        {
            yield return new WaitForSeconds(Random.Range(2f, idleInterval));
            while (true)
            {
                SayIdle();
                yield return new WaitForSeconds(idleInterval + Random.Range(-3f, 3f));
            }
        }

        public void SayIdle() => Say(Pick(idleLines));

        public void ReactToMood(MoonlightMood mood)
        {
            var pool = mood >= MoonlightMood.Happy ? happyLines : sadLines;
            Say(Pick(pool));
        }

        public void SayGreeting() => Say(new DialogueLine { text = greeting, duration = 4f });

        void Say(DialogueLine line)
        {
            if (line == null || bubble == null) return;
            bubble.Show(npcName, line.text, line.duration);
        }

        DialogueLine Pick(List<DialogueLine> pool)
        {
            if (pool == null || pool.Count == 0) return null;
            return pool[Random.Range(0, pool.Count)];
        }
    }
}
