// HC2 auto-spawn pattern — entire scene built from code on Awake.
// Assign prefabs in the Inspector on this one Bootstrap object.

using UnityEngine;

namespace MoonlightMagicHouse
{
    [DefaultExecutionOrder(-100)]
    public class MoonlightHouseSetup : MonoBehaviour
    {
        [Header("Moonlight")]
        [SerializeField] GameObject moonlightPrefab;

        [Header("Managers")]
        [SerializeField] GameObject gameManagerPrefab;
        [SerializeField] GameObject audioManagerPrefab;
        [SerializeField] GameObject localizationPrefab;
        [SerializeField] GameObject notificationPrefab;
        [SerializeField] GameObject achievementPrefab;
        [SerializeField] GameObject seasonalEventPrefab;
        [SerializeField] GameObject tricksPrefab;
        [SerializeField] GameObject streakPrefab;

        [Header("UI")]
        [SerializeField] GameObject uiCanvasPrefab;

        [Header("Rooms (all start inactive except Living Room)")]
        [SerializeField] GameObject livingRoomPrefab;
        [SerializeField] GameObject kitchenPrefab;
        [SerializeField] GameObject bedroomPrefab;
        [SerializeField] GameObject gardenPrefab;
        [SerializeField] GameObject libraryPrefab;

        [Header("Moonlight atmosphere")]
        [SerializeField] Color ambientColor = new Color(0.10f, 0.06f, 0.20f);
        [SerializeField] Color moonColor    = new Color(0.85f, 0.90f, 1.00f);

        void Awake()
        {
            SetupAtmosphere();

            // Managers (DontDestroyOnLoad inside each)
            Spawn(audioManagerPrefab);
            Spawn(localizationPrefab);
            Spawn(notificationPrefab);
            Spawn(achievementPrefab);
            if (seasonalEventPrefab) Spawn(seasonalEventPrefab);
            if (tricksPrefab)        Spawn(tricksPrefab);
            if (streakPrefab)        Spawn(streakPrefab);

            // Rooms
            var living  = Spawn(livingRoomPrefab);
            var kitchen = Spawn(kitchenPrefab);   kitchen.SetActive(false);
            var bedroom = Spawn(bedroomPrefab);   bedroom.SetActive(false);
            var garden  = Spawn(gardenPrefab);    garden.SetActive(false);
            var library = Spawn(libraryPrefab);   library.SetActive(false);

            // RoomManager wiring
            var roomMgr = FindAnyObjectByType<RoomManager>();

            // Moonlight character
            var mlGO = Spawn(moonlightPrefab);
            var ml   = mlGO.GetComponent<MoonlightCharacter>();

            // UI
            var uiGO = Spawn(uiCanvasPrefab);
            var ui   = uiGO.GetComponent<MoonlightUI>();

            // Game manager
            var gmGO = Spawn(gameManagerPrefab);
            var gm   = gmGO.GetComponent<MoonlightGameManager>();
            gm.moonlight    = ml;
            gm.ui           = ui;
            gm.rooms        = roomMgr;
            gm.wardrobe     = mlGO.GetComponent<MoonlightWardrobe>();
            gm.idleBehavior = mlGO.GetComponent<MoonlightIdleBehavior>();
        }

        void SetupAtmosphere()
        {
            RenderSettings.ambientLight = ambientColor;
            var sun = new GameObject("Moon").AddComponent<Light>();
            sun.type      = LightType.Directional;
            sun.color     = moonColor;
            sun.intensity = 0.55f;
            sun.transform.rotation = Quaternion.Euler(40f, -20f, 0f);
        }

        static GameObject Spawn(GameObject prefab)
        {
            if (prefab == null) return new GameObject("Missing");
            return Instantiate(prefab);
        }
    }
}
