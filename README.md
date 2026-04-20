# 🌙 Moonlight Magic House

> Unity Tamagotchi game — care for Moonlight, a magical girl who lives in an enchanted house.

**Engine:** Unity 6 (6000.0.23f1)  
**Target:** iOS / Android / WebGL  
**Audience:** Ages 4+  
**Org:** [FreeRiverHouse](https://github.com/FreeRiverHouse)

---

## Core Loop

1. **Moonlight** is the single protagonist — a magical girl who lives in the house
2. Explore 5 rooms: **Living Room · Kitchen · Bedroom · Garden · Library**
3. Feed, cuddle, rest — keep her 5 stats healthy: **wonder · warmth · rest · magic · hunger**
4. Watch her grow: **Moonbud → Starling → Luminary → Sorceress → Moonkeeper**
5. Earn coins, unlock outfits, discover 10 house secrets, read 10 library stories

## Project Structure

```
Assets/
  Scripts/
    Core/        MoonlightCharacter, MoonlightGameManager, MoonlightHouseSetup,
                 MoonlightSaveData, MoonlightTricksSystem, AchievementSystem,
                 AudioManager, LocalizationManager, SeasonalEventManager,
                 StreakTracker, NotificationManager, WebGLBridge, AnalyticsManager
    Characters/  MoonlightAnimator, MoonlightIdleBehavior, MoonlightInteractable,
                 MoonlightWardrobe, NPCCharacter, NPCInteractable
    Rooms/       RoomManager, HouseSecrets, GardenArea, LibraryRoom,
                 InteractableObject, FeedingStation, PlayArea, SleepArea
    UI/          MoonlightUI, MoonlightWardrobeUI, TricksUI, StoryPageUI,
                 AchievementToast, SecretDiscoveryToast, StreakToast, NPCBubble,
                 RoomNavigationUI, SettingsUI, PetStatsTooltip
    Data/        Items (FoodItem/ActivityItem/OutfitItem ScriptableObjects)
  Shaders/
    MoonlightToon.shader        — cel-shaded two-pass toon + outline
    StarfieldSkybox.shader      — procedural night sky with twinkling stars
    MoonlightGlowPulse.shader   — animated emission for interactables
    MoonReflection.shader       — ripple floor reflection
  Audio/
    SFX/    16 procedural WAV (numpy) — eat/cuddle/sleep/discover/stage_up/…
    Music/  6 loopable ambient tracks (numpy) — per room + main theme
  Models/
    Moonlight/  5 stage FBX (Blender headless)
    Outfits/    11 outfit accessory FBX
    Props/      12 house prop FBX
  Data/
    DefaultFoodMenu.json, DefaultAchievements.json, DefaultStoryPages.json,
    DefaultNPCDialogue.json, DefaultTricks.json, SeasonalEvents.json,
    DefaultActivities.json, DefaultOutfits.json
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
