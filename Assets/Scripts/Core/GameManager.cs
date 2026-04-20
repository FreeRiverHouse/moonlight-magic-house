using UnityEngine;

namespace MoonlightMagicHouse
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        public MoonlightPet pet;
        public PetUIController ui;
        public RoomManager rooms;

        [Header("Auto-save")]
        [SerializeField] float autoSaveInterval = 120f;
        float _autoSaveTimer;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // Load or start fresh
            if (SaveManager.Instance.TryLoad(out var data))
            {
                SaveManager.Instance.ApplyToPet(data, pet);
                ui.ShowOfflineNotice();
            }

            pet.onEvolution.AddListener(OnEvolution);
            pet.onMoodChange.AddListener(ui.UpdateMoodIndicator);
            pet.onLevelUp.AddListener(ui.ShowLevelUp);
            pet.onCoinsChanged.AddListener(ui.UpdateCoins);

            ui.Refresh(pet);
        }

        void Update()
        {
            _autoSaveTimer += Time.deltaTime;
            if (_autoSaveTimer >= autoSaveInterval)
            {
                _autoSaveTimer = 0;
                SaveManager.Instance.Save(pet);
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) SaveManager.Instance.Save(pet);
        }

        void OnApplicationQuit() => SaveManager.Instance.Save(pet);

        void OnEvolution(EvolutionStage newStage)
        {
            ui.ShowEvolutionCelebration(newStage);
        }
    }
}
