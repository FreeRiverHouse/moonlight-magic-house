# Moonlight Magic House Devlog

## 2026-04-24 - Fairytale Photoreal Direction

Reference note from playtest: the generated meadow backdrop has the right emotional target for Moonlight. It feels photoreal, warm, safe, and fairytale-like without becoming noisy or dark.

Art direction to preserve:
- Photoreal materials and lighting, but with a gentle storybook atmosphere.
- Soft warm light, pastel flowers, glowing dust, cozy magic, and child-safe spaces.
- Gameplay actions should become small readable scenes, not only button-triggered poses.
- When Moonlight goes outside, she should move to the door, open it, and then walk into a beautiful meadow scene.
- Rest should take Moonlight onto the bed, settle her into a clear sleeping pose, and then return her safely to standing before the next action.

Roadmap implications:
- Accent the same fairytale photoreal mood in the bedroom, UI, particles, props, and future rooms.
- Treat each action as a tiny vignette with spatial intent: snack spot, hug pose, bed rest, outdoor run, bath corner, dance space.
- Keep the proportions cute, but less distorted than pure chibi: readable child scale, smaller head/ribbons, longer legs, softer body silhouette.

## 2026-04-24 - Implementation Notes: Photoreal Fairy-Tale Pass

What changed in this pass:
- Generated and imported a photoreal meadow backdrop to replace the supplied reference-photo dependency for the outdoor scene.
- Kept the warm bedroom direction and pushed the whole game toward a cozy fairy-tale photoreal mood instead of plain realism.
- Added a scene director that turns care buttons into small spatial vignettes: Moonlight moves inside the room, goes to the bed for rest, and uses a door/portal transition before running in the meadow.
- Improved the child avatar presentation by reducing the most chibi proportions, adding livelier idle/walk/run posing, and making gestures more expressive.
- Added extra QA screenshot timing for the door transition and outdoor play scene, so the important moments can be inspected headlessly.

How this work is being done:
- Unity Editor GUI stays closed. All Unity work is built and tested through macOS batchmode/headless commands.
- Every visual/gameplay claim should be checked with generated screenshots from the built app, not only by reading code.
- Build command remains `Unity -batchmode -nographics -quit -executeMethod BuildAll.BuildMac`.
- Playtest screenshots are written to `~/Desktop/MMH-QA/playtest/`.
- Current app build is `~/Desktop/MMH-Build/MoonlightMagicHouse.app`.
- The local `Library -> ~/MMH-Library-APFS` symlink is infrastructure only and should stay out of git.

Latest verification:
- Built successfully with Unity 6000.3.2f1 in batchmode.
- Ran the built app with `-mmhPlaytest`.
- Verified Player.log reached `[Playtest][PASS]` with no runtime exceptions.
- Visually checked the initial room, bed rest, door transition, meadow play, and room return screenshots.

Known follow-up:
- The bed-rest vignette now clearly reads as sleeping/resting, but future polish should make it feel physically integrated with bedding rather than a 3D avatar floating against a 2D photo plane.
- The door/portal transition works and communicates the scene change; later polish can replace the simple runtime geometry with a higher-quality authored prop.

## 2026-04-24 - Next Phase Pass: Bed, Portal, Proportions

Linear source of truth was created for Moonlight:
- Project: `Moonlight Magic House`
- Onboarding issue: `FRE-114`
- Active next-phase issues: `FRE-116`, `FRE-117`, `FRE-118`, `FRE-119`

Implementation direction for this pass:
- Tune Moonlight less chibi while preserving the animated/cute improvement the user liked.
- Add a runtime bedding/occlusion layer during `NAP`, so the rest pose reads more like she is tucked into the bed rather than floating over a flat photo.
- Replace the blocky feel of the PLAY door with a softer fairy portal: glow aura, meadow reveal, smaller frame, translucent panel, and pearl-like lights.
- Add subtle storybook warmth in the room through soft window/floor glow overlays, while keeping the generated photoreal bedroom as the base.

Verification gate remains unchanged:
- Build with Unity batchmode only.
- Run the built app with `-mmhPlaytest`.
- Inspect screenshots for `03_SleepBtn`, `04_PlayDoor`, `04_PlayBtn`, and the room return before calling the pass done.

## 2026-04-24 - Next Phase Result: Room-Only Treats, Bed Polish, Portal Polish

Implemented in the next-phase pass:
- Moonlight proportions were tuned less chibi: smaller head/ribbons, slimmer/taller body read, longer legs, while preserving the animated cute avatar the playtest liked.
- `NAP` now moves Moonlight to the bed, switches to a closer bed camera, lies her down, and uses a subtle soft shadow/duvet occlusion instead of the earlier obvious pink blob.
- `PLAY` now keeps the room-to-meadow transition: Moonlight walks to the fairy portal/door, opens it, then runs in the generated meadow.
- The portal was softened with a translucent panel, meadow reveal, fairy pearls, and warmer glow so it reads more fairytale and less blocky.
- Small snack macarons were enabled as room-only props. They stay in the bedroom and are hidden automatically during the outdoor meadow scene.

Latest verification:
- Built successfully in Unity 6000.3.2f1 batchmode/headless.
- Ran `Moonlight Magic House.app` with `-mmhPlaytest`.
- Player log reached `[Playtest][PASS]` with no runtime exceptions.
- Inspected `00_initial`, `03_SleepBtn`, `04_PlayDoor`, `04_PlayBtn`, and `06_DanceBtn` screenshots.

Remaining quality bar:
- The current result is improved and playable, but the avatar remains stylized against photoreal backdrops. The next big visual jump should be a better child avatar/material pass or a more authored 3D bed/action vignette layer.
