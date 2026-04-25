# Next-Level Graphics Loop - 2026-04-25

Purpose: keep iterating on Moonlight Magic House until the build reads as a premium, child-friendly, fairytale photoreal game rather than a prototype. This is the operational loop for every next graphics pass.

## Source Of Truth

- Linear issue: `FRE-122` - Moonlight: PGR-quality graphics and animation pipeline.
- Research doc: `Docs/GraphicsSOTAStudy-2026-04-25.md`.
- Running devlog: `Docs/MoonlightDevlog.md`.
- External project path: `/Volumes/SSD-FRH-1/Free-River-House/moonlight-magic-house`.
- Build output: `/Users/mattiapetrucciani/Desktop/MMH-Build/MoonlightMagicHouse.app`.
- QA screenshots: `/Users/mattiapetrucciani/Desktop/MMH-QA/playtest/`.

## Tool Stack

- Unity 6000.3.2f1 in batchmode/headless only. Never open the Unity GUI.
- GPT Image / Gemini / Grok for look frames, material references, and shot ideas.
- Seedance 2 for animated previs: action timing, camera language, UI mood, character appeal, and trailer-quality target clips.
- Unity runtime remains the playable source of truth. AI video is reference/previs unless explicitly used as a non-interactive trailer asset.
- External APFS Unity data lives on `/Volumes/FRH-Moonlight-APFS`.

## Loop

1. Baseline: capture the current build screenshots and note what feels fake, badly scaled, stiff, or too procedural.
2. Previs: generate or review a look frame and a short Seedance-style motion target for the specific scene/action.
3. Translate: implement the target in Unity as real runtime geometry, materials, lighting, camera, VFX, UI, and animation.
4. Build: run the batchmode build from the external SSD/APFS workflow.
5. Playtest: run the built app with `-mmhPlaytest`.
6. Inspect: open the screenshot set and check initial room, snack, nap, door/play, bath, dance, and final return.
7. Decide: accept only if the pass improves premium feel without breaking gameplay. Otherwise iterate.
8. Document: update this repo devlog and Linear with what changed, evidence, and remaining gap.
9. Commit/push: stage only relevant source/docs/assets. Do not stage Unity `Library`, caches, or accidental import churn.

## Build Gate

```bash
TMPDIR=/Volumes/FRH-Moonlight-APFS/tmp \
TEMP=/Volumes/FRH-Moonlight-APFS/tmp \
TMP=/Volumes/FRH-Moonlight-APFS/tmp \
XDG_CACHE_HOME=/Volumes/FRH-Moonlight-APFS/UnityCaches \
UPM_CACHE_PATH=/Volumes/FRH-Moonlight-APFS/UnityCaches/upm \
UPM_NPM_CACHE_PATH=/Volumes/FRH-Moonlight-APFS/UnityCaches/upm/npm \
UPM_GIT_CACHE_PATH=/Volumes/FRH-Moonlight-APFS/UnityCaches/upm/git \
/Applications/Unity/Hub/Editor/6000.3.2f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath /Volumes/SSD-FRH-1/Free-River-House/moonlight-magic-house \
  -executeMethod BuildAll.BuildMac \
  -logFile -
```

## Playtest Gate

```bash
rm -rf /Users/mattiapetrucciani/Desktop/MMH-QA/playtest
mkdir -p /Users/mattiapetrucciani/Desktop/MMH-QA/playtest
/Users/mattiapetrucciani/Desktop/MMH-Build/MoonlightMagicHouse.app/Contents/MacOS/Moonlight\ Magic\ House \
  -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -mmhPlaytest
```

Pass condition: Player.log reaches `[Playtest][PASS]` and the screenshots do not show broken proportions, obvious flat-backdrop composition, UI obstruction, missing character, or action regressions.

## Visual Quality Bar

- Moonlight reads as a child-scale character in a coherent room, not a doll pasted over a background.
- Every action moves to a believable place: bed, door, bath corner, snack/play area, dance space.
- Lighting has contact depth: floor shadows, practical warm lights, window/ambient direction, and controlled bloom.
- The room feels rich but readable for children: plush, toy, bed, door, window, rug, lamp, shelf, small magic details.
- Motion is soft and purposeful, not robotic. Seedance-style previs sets the aspiration; Unity implementation must be playable.
- UI supports the scene instead of covering it.

## Stop Criteria

Do not call a pass done unless:

- Build passes.
- Playtest passes.
- Screenshots were inspected.
- Devlog and Linear were updated.
- Git commit/push includes only intentional files.

If any visual still feels fake, badly scaled, stiff, or placeholder, continue the loop with a smaller targeted pass.
