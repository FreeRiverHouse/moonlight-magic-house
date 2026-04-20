using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoonlightMagicHouse
{
    // Full-screen overlay for when Moonlight goes to sleep.
    // Fades in a dark purple panel with star particles.
    public class MoonlightSleepVisual : MonoBehaviour
    {
        [SerializeField] CanvasGroup    overlay;
        [SerializeField] ParticleSystem dreamStars;
        [SerializeField] float          fadeTime = 1.5f;
        [SerializeField] float          sleepDuration = 3f;

        void Start()
        {
            if (overlay) overlay.alpha = 0f;
            MoonlightGameManager.Instance?.moonlight.onMoodChange.AddListener(OnMoodChange);
        }

        void OnMoodChange(MoonlightMood mood)
        {
            if (mood == MoonlightMood.Asleep) StartCoroutine(SleepSequence());
        }

        IEnumerator SleepSequence()
        {
            if (dreamStars) dreamStars.Play();
            yield return Fade(0f, 0.75f);
            yield return new WaitForSeconds(sleepDuration);
            yield return Fade(0.75f, 0f);
            if (dreamStars) dreamStars.Stop();
        }

        IEnumerator Fade(float from, float to)
        {
            float t = 0f;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                if (overlay) overlay.alpha = Mathf.Lerp(from, to, t / fadeTime);
                yield return null;
            }
            if (overlay) overlay.alpha = to;
        }
    }
}
