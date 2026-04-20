// Auto-setup pattern based on PIZZA-GELATO-RUSH HC2AutoSetup.cs
// Spawns the entire scene from code — swap art assets without touching the scene.

using UnityEngine;

namespace MoonlightMagicHouse
{
    [DefaultExecutionOrder(-100)]
    public class MoonlightHouseSetup : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] GameObject petPrefab;
        [SerializeField] GameObject gameManagerPrefab;
        [SerializeField] GameObject saveManagerPrefab;
        [SerializeField] GameObject audioManagerPrefab;
        [SerializeField] GameObject uiCanvasPrefab;

        [Header("Room Prefabs")]
        [SerializeField] GameObject bedroomPrefab;
        [SerializeField] GameObject kitchenPrefab;
        [SerializeField] GameObject livingRoomPrefab;
        [SerializeField] GameObject gardenPrefab;
        [SerializeField] GameObject libraryPrefab;

        [Header("Lighting")]
        [SerializeField] Color moonlightAmbient = new Color(0.12f, 0.08f, 0.22f);
        [SerializeField] Color moonlightColor   = new Color(0.85f, 0.9f, 1f);

        void Awake()
        {
            SetupLighting();
            SpawnManagers();
            SpawnRooms();
            SpawnPet();
        }

        void SetupLighting()
        {
            RenderSettings.ambientLight = moonlightAmbient;
            var sun = new GameObject("MoonLight").AddComponent<Light>();
            sun.type      = LightType.Directional;
            sun.color     = moonlightColor;
            sun.intensity = 0.6f;
            sun.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        void SpawnManagers()
        {
            Instantiate(saveManagerPrefab);
            Instantiate(audioManagerPrefab);
        }

        void SpawnRooms()
        {
            Instantiate(bedroomPrefab);
            Instantiate(kitchenPrefab);
            Instantiate(livingRoomPrefab);
            Instantiate(gardenPrefab);
            Instantiate(libraryPrefab);
        }

        void SpawnPet()
        {
            var pet = Instantiate(petPrefab, Vector3.zero, Quaternion.identity);
            var ui  = Instantiate(uiCanvasPrefab);
            var gm  = Instantiate(gameManagerPrefab);

            var gmComp = gm.GetComponent<GameManager>();
            gmComp.pet   = pet.GetComponent<MoonlightPet>();
            gmComp.ui    = ui.GetComponent<PetUIController>();
            gmComp.rooms = FindAnyObjectByType<RoomManager>();
        }
    }
}
