using System.Collections;
using UnityEngine;
using TMPro;

namespace MoonlightMagicHouse
{
    public class NPCBubble : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text nameLabel;
        [SerializeField] TMP_Text bodyLabel;
        [SerializeField] float typeSpeed = 0.04f;

        Coroutine _co;

        public void Show(string npcName, string text, float duration)
        {
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(Display(npcName, text, duration));
        }

        IEnumerator Display(string npcName, string text, float duration)
        {
            nameLabel.text = npcName;
            bodyLabel.text = "";
            root.SetActive(true);

            foreach (char c in text)
            {
                bodyLabel.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }

            yield return new WaitForSeconds(duration);
            root.SetActive(false);
        }

        public void Hide() => root.SetActive(false);
    }
}
