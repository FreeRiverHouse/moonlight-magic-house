using System.Collections;
using UnityEngine;

namespace MoonlightMagicHouse
{
    // Plays a particle burst + camera shake on Moonlight's stage-up event.
    public class MoonlightStageVFX : MonoBehaviour
    {
        [SerializeField] ParticleSystem evolutionBurst;
        [SerializeField] ParticleSystem glowRing;
        [SerializeField] Light          flashLight;
        [SerializeField] float          shakeDuration  = 0.5f;
        [SerializeField] float          shakeMagnitude = 0.08f;

        Camera _cam;
        Vector3 _camOrigin;

        void Start()
        {
            _cam = Camera.main;
            MoonlightGameManager.Instance?.moonlight.onStageUp.AddListener(OnStageUp);
        }

        void OnStageUp(MoonlightStage stage)
        {
            if (evolutionBurst) evolutionBurst.Play();
            if (glowRing)       glowRing.Play();
            StartCoroutine(FlashAndShake());
            AudioManager.Instance?.Play("evolution");
        }

        IEnumerator FlashAndShake()
        {
            if (_cam) _camOrigin = _cam.transform.localPosition;

            if (flashLight)
            {
                flashLight.enabled   = true;
                flashLight.intensity = 3f;
            }

            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float prog = elapsed / shakeDuration;

                if (_cam)
                {
                    float mag = shakeMagnitude * (1f - prog);
                    _cam.transform.localPosition = _camOrigin + (Vector3)Random.insideUnitCircle * mag;
                }

                if (flashLight)
                    flashLight.intensity = Mathf.Lerp(3f, 0f, prog);

                yield return null;
            }

            if (_cam)        _cam.transform.localPosition = _camOrigin;
            if (flashLight)  flashLight.enabled = false;
        }
    }
}
