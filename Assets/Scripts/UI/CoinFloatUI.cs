using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    // Spawns a floating "+N ⭐" label that animates toward the coin counter.
    public class CoinFloatUI : MonoBehaviour
    {
        [SerializeField] RectTransform  canvas;
        [SerializeField] RectTransform  coinTarget;   // coin counter rect
        [SerializeField] GameObject     floatPrefab;  // TMP_Text prefab
        [SerializeField] float          duration = 1.1f;

        void Start()
        {
            MoonlightGameManager.Instance?.moonlight.onCoinsChanged.AddListener(_ => { });
            // Wire via MoonlightCharacter.EarnCoins event in GameManager
        }

        public void ShowGain(int amount, Vector2 fromScreenPos)
        {
            if (floatPrefab == null || canvas == null) return;
            StartCoroutine(Animate(amount, fromScreenPos));
        }

        IEnumerator Animate(int amount, Vector2 from)
        {
            var go  = Instantiate(floatPrefab, canvas);
            var txt = go.GetComponent<TMP_Text>();
            if (txt) txt.text = $"+{amount} ⭐";

            var rect = go.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas, from, null, out var localFrom);
            rect.anchoredPosition = localFrom;

            Vector2 to = coinTarget != null ? coinTarget.anchoredPosition : Vector2.zero;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                rect.anchoredPosition = Vector2.Lerp(localFrom, to, Mathf.SmoothStep(0, 1, p));
                var c = txt.color;
                c.a = 1f - Mathf.Pow(p, 2f);
                txt.color = c;
                yield return null;
            }
            Destroy(go);
        }
    }
}
