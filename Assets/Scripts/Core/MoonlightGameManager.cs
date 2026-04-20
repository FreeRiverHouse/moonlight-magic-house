using UnityEngine;

namespace MoonlightMagicHouse
{
    public class MoonlightGameManager : MonoBehaviour
    {
        public static MoonlightGameManager Instance { get; private set; }

        [Header("Core")]
        public MoonlightCharacter   moonlight;
        public MoonlightUI          ui;
        public RoomManager          rooms;
        public MoonlightWardrobe    wardrobe;
        public MoonlightIdleBehavior idleBehavior;

        [Header("Auto-save interval (s)")]
        [SerializeField] float autoSaveInterval = 90f;
        float _saveTimer;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            if (MoonlightSave.TryLoad(out var data))
            {
                MoonlightSave.Apply(data, moonlight);
                wardrobe?.Equip(data.currentOutfitId);
                ui?.ShowOfflineNotice();
            }

            moonlight.onStageUp.AddListener(OnStageUp);
            moonlight.onStageUp.AddListener(s => AchievementSystem.Instance?.OnStageUp(s));
            moonlight.onMoodChange.AddListener(ui.OnMoodChange);
            moonlight.onCoinsChanged.AddListener(ui.UpdateCoins);
            moonlight.onRoomUnlocked.AddListener(OnRoomUnlocked);
            moonlight.onXPGained.AddListener(ui.UpdateXP);

            ui.Refresh(moonlight);
            WebGLBridge.GameReady();
        }

        void Update()
        {
            _saveTimer += Time.deltaTime;
            if (_saveTimer >= autoSaveInterval) Save();
        }

        void OnApplicationPause(bool p) { if (p) Save(); }
        void OnApplicationQuit()         => Save();

        public void Save()
        {
            _saveTimer = 0;
            MoonlightSave.Save(moonlight, wardrobe?.CurrentId ?? 0);
            WebGLBridge.ReportProgress(moonlight.xp, moonlight.coins,
                (int)moonlight.stage, moonlight.stage.ToString());
        }

        void OnStageUp(MoonlightStage stage)
        {
            ui.ShowStageCelebration(stage);
            AchievementSystem.Instance?.Check($"stage_{stage}".ToLower());
            AnalyticsManager.TrackEvolution(stage);
        }

        void OnRoomUnlocked(int count)
        {
            ui.ShowRoomUnlocked(count);
            AudioManager.Instance?.Play("room_unlock");
        }
    }
}
