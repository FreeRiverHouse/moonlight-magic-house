using UnityEngine;

namespace MoonlightMagicHouse
{
    // Drives the MoonlightGlowPulse shader emission intensity + color
    // based on Moonlight's current mood and stage.
    [RequireComponent(typeof(Renderer))]
    public class MoonlightGlowController : MonoBehaviour
    {
        [SerializeField] float pulseSpeed    = 1.2f;
        [SerializeField] float baseIntensity = 0.4f;
        [SerializeField] float maxIntensity  = 2.5f;

        static readonly Color[] StageColors =
        {
            new Color(0.55f, 0.30f, 0.80f),   // Moonbud    — soft purple
            new Color(0.70f, 0.55f, 1.00f),   // Starling   — lavender
            new Color(0.90f, 0.85f, 0.40f),   // Luminary   — warm gold
            new Color(0.20f, 0.70f, 0.90f),   // Sorceress  — ice teal
            new Color(1.00f, 0.95f, 0.70f),   // Moonkeeper — pure moonlight
        };

        static readonly float[] MoodIntensityMul =
        {
            0.1f,   // Asleep
            0.2f,   // Grumpy
            0.3f,   // Bored
            0.6f,   // Calm
            0.85f,  // Happy
            1.0f,   // Radiant
        };

        Renderer  _rend;
        Material  _mat;
        float     _intensityTarget = 0.6f;
        float     _intensityCurrent;

        static readonly int EmitIntensityID = Shader.PropertyToID("_EmitIntensity");
        static readonly int EmitColorID     = Shader.PropertyToID("_EmitColor");

        void Awake()
        {
            _rend = GetComponent<Renderer>();
            _mat  = _rend.material;
        }

        void Start()
        {
            var ml = MoonlightGameManager.Instance?.moonlight;
            if (ml == null) return;
            ml.onMoodChange.AddListener(OnMoodChange);
            ml.onStageUp.AddListener(OnStageUp);
            ApplyState(ml.stats.GetMood(), ml.stage);
        }

        void Update()
        {
            float pulse = baseIntensity + Mathf.Sin(Time.time * pulseSpeed) * 0.15f;
            _intensityCurrent = Mathf.Lerp(_intensityCurrent, _intensityTarget, Time.deltaTime * 2f);
            _mat.SetFloat(EmitIntensityID, _intensityCurrent + pulse);
        }

        void OnMoodChange(MoonlightMood mood)
        {
            var ml = MoonlightGameManager.Instance?.moonlight;
            ApplyState(mood, ml?.stage ?? MoonlightStage.Moonbud);
        }

        void OnStageUp(MoonlightStage stage)
        {
            var ml = MoonlightGameManager.Instance?.moonlight;
            _mat.SetColor(EmitColorID, StageColors[(int)stage]);
            ApplyState(ml?.stats.GetMood() ?? MoonlightMood.Calm, stage);
        }

        void ApplyState(MoonlightMood mood, MoonlightStage stage)
        {
            _intensityTarget = maxIntensity * MoodIntensityMul[(int)mood];
            _mat.SetColor(EmitColorID, StageColors[(int)stage]);
        }
    }
}
