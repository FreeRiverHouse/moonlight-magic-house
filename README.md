# 🌙 Moonlight Magic House

> Unity Tamagotchi game — adopt a magical creature and explore a moonlit house together.

**Engine:** Unity 6 (6000.0.23f1)  
**Target:** iOS / Android / WebGL  
**Audience:** Ages 4+  
**Org:** [FreeRiverHouse](https://github.com/FreeRiverHouse)

---

## Core Loop

1. Choose your magical pet (Dragon, Cat, Panda, Fox, Penguin, Bunny, Bear, Owl)
2. Explore 5 rooms: **Bedroom · Kitchen · Living Room · Garden · Library**
3. Feed, play, clean, rest — keep your pet's 5 stats healthy
4. Watch it evolve: **Egg → Baby → Child → Teen → Adult**
5. Earn coins, unlock outfits & tricks

## Project Structure

```
Assets/
  Scripts/
    Core/        GameManager, MoonlightPet, SaveManager, AudioManager, MoonlightHouseSetup
    Characters/  PetAnimatorController
    Rooms/       RoomManager, InteractableObject, FeedingStation, PlayArea, SleepArea
    UI/          PetUIController, RoomNavigationUI
    Data/        PetData (enums + save struct), Items (ScriptableObjects)
  Shaders/
    MoonlightToon.shader       — cel-shaded toon + outline
    StarfieldSkybox.shader     — procedural night sky with twinkling stars
```

## Architecture Notes

- **MoonlightHouseSetup** auto-spawns all scene objects on Awake (HC2 pattern from Pizza Gelato Rush).
- Stats decay in real-time ticks (every 60 s). Offline time is applied on load (capped at 120 min).
- All pet data serialised via `JsonUtility` → `PlayerPrefs`.
- Room navigation via `RoomManager` — each room is a separate prefab, active/inactive swap.
- ScriptableObjects drive food/activity menus — add new items without touching code.

## Build

```bash
# Mac
/Applications/Unity/Hub/Editor/6000.0.23f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath . \
  -executeMethod BuildAll.BuildMac

# WebGL
-executeMethod BuildAll.BuildWebGL
```
