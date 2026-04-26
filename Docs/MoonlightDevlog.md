# Moonlight Magic House Devlog

## 2026-04-26 - Action Motion and Perceived Physics Pass

Reason for this pass:
- User feedback: the photoreal/procedural blend is improving, but some actions still look stiff, crooked, or physically fake.
- The main issue is perceived physics, not a true physics simulation: root movement, bone posing, camera timing, and 2D plate composition need to agree.

What changed:
- Smoothed root travel with sine easing and smaller lateral arcs so walk/run actions feel less robotic.
- Added subtle vertical travel arcs during movement so Moonlight has a little weight without popping.
- Reduced exaggerated idle bob and ribbon motion during `NAP`, where extra motion made the pose look crooked.
- Tuned walk/run cycle: smaller torso sway, softer arm swing, less mechanical double-bounce, and cleaner foot/leg offsets.
- Reworked `NAP` staging from a fake horizontal lie-down over a 2D photo plate into a lower reclined/rest pose near the bed area. This is more believable with the current 2D/3D hybrid than forcing a full bed interaction without a real 3D bed asset.
- Slowed the door approach and outdoor run slightly so transitions feel less snappy.

Verification:
- Built successfully with Unity 6000.3.2f1 in headless batchmode using the external APFS temp/cache paths.
- Ran `Moonlight Magic House.app -mmhPlaytest`.
- Player log reached `[Playtest][PASS] xp=2880 coins=16 mood=Happy wonder=100 warmth=58 rest=37 magic=100 hunger=72`.
- Inspected the action screenshots, especially `03_SleepBtn`, `04_PlayDoor`, `04_PlayBtn`, and `06_DanceBtn`.

Current honesty:
- This pass makes the action layer less stiff and removes the worst nap-floating read.
- The true next jump still needs authored animation clips/root motion and real 3D interaction props for bed/bath/snack so actions can physically contact objects instead of implying contact against a plate.

## 2026-04-26 - Photoreal Plate Integration Pass

Reason for this pass:
- The previous build was functional and more animated, but still read as a stylized character pasted into a partly procedural room.
- The biggest visual offenders were the blocky right wall, cube bed, duplicated dollhouse/window props, and low-poly outdoor meadow.

What changed:
- Removed the procedural bedroom shell as the main visual read: no more large primitive side walls, ceiling, back wallpaper, or blocky bed/dollhouse duplication in front of the photoreal room.
- Enlarged the generated photoreal bedroom plate so action cameras do not reveal empty gray background.
- Retained only lightweight gameplay props in front of the plate: snack/bath affordances, rug/action grounding, simple toy props, fairy lights, and HUD.
- Moved the `NAP` action toward the bed area in the photoreal room plate instead of the old cube bed on the right.
- Added a photoreal meadow backdrop inside the outdoor scene, so `PLAY` no longer depends only on the low-poly grass/trees.
- Kept Moonlight animated and clickable; this is a visual integration pass, not a full rig/animation replacement.

Verification:
- Built successfully with Unity 6000.3.2f1 in headless batchmode using external APFS temp/cache paths.
- Ran `Moonlight Magic House.app -mmhPlaytest`.
- Player log reached `[Playtest][PASS] xp=2760 coins=19 mood=Happy wonder=100 warmth=58 rest=37 magic=68 hunger=72`.
- Inspected `00_initial`, `03_SleepBtn`, `04_PlayDoor`, and `04_PlayBtn` screenshots in `~/Desktop/MMH-QA/playtest/`.

Current honesty:
- This is a meaningful prototype-quality upgrade: first read is warmer, more photoreal, and less obviously assembled from primitive room walls.
- It is not yet the final "top top" bar. The remaining mismatch is Moonlight/interactive props versus photoreal backgrounds. The next production pass should focus on a better child avatar/rig, authored action props, root-motion clips, and a coherent URP/HDRP-style render stack.

## 2026-04-26 - Asset Forge References + Scale/Staging Runtime Pass

Reason for this pass:
- The next-level graphics roadmap now has a controlled asset workflow in FRH-ADMIN, and Moonlight needs to stop accepting untracked pasted-looking art.
- The user called out wrong proportions and action movement: sleep should physically go to the bed, outdoor play should walk to the door, open it, and run outside.

What changed:
- Added a tracked photoreal/fairytale bedroom reference through `frh-game-asset-forge`:
  - `Assets/Resources/GeneratedArt/mood-frame/fairytale-bedroom-photoreal-scale-pass/fairytale-bedroom-photoreal-scale-pass_source.png`
- Promoted the no-child generated bedroom image into `Assets/Resources/Photoreal/room-generated.png` as a temporary runtime plate behind the 3D scene.
- Runtime still keeps the authored 3D bed, dollhouse, floor, rug, lights, door, action props, and meadow in front of the plate; the image is not the old user-supplied photo and contains no child.
- Recalibrated the child avatar target height to about 1.10m and reduced the stretched/narrow read on the SD avatar.
- Enlarged the outdoor door/portal to adult-room scale instead of toy-door scale, so Moonlight no longer looks huge next to it.
- Widened and lowered the action camera a little for bath/snack/dance/door moments so movement reads less cramped.
- Increased the door walk distance and transition timing so the outdoor action reads as a real staged movement.

Verification plan:
- Build headless with Unity 6000.3.2f1.
- Run `Moonlight Magic House.app -mmhPlaytest`.
- Inspect `00_initial`, `03_SleepBtn`, `04_PlayDoor`, `04_PlayBtn`, `05_BathBtn`, and `06_DanceBtn`.

Verification result:
- Build succeeded in Unity 6000.3.2f1 batchmode/headless.
- Playtest produced all expected screenshots in `~/Desktop/MMH-QA/playtest/`.
- Player log reached `[Playtest][PASS] xp=2600 coins=23 mood=Happy wonder=80 warmth=58 rest=37 magic=34 hunger=72`.
- Inspected `00_initial`, `04_PlayDoor`, and `04_PlayBtn`; the new room plate makes the first read much richer and the larger door fixes the worst scale mismatch.
- Current honesty: foreground props are still too blocky next to the photoreal plate. Next pass should replace the bed/dollhouse/tub/rug primitives with authored assets or generated texture cards, then move toward URP.

## 2026-04-25 - SOTA Graphics Study + Bedroom Production Detail Pass

Reason for this pass:
- The user compared the current prototype against what people are shipping with Claude/AI workflows and against Pizza Gelato Rush.
- The key critique is still valid: the game only becomes premium when visuals, proportions, camera, movement, and scene construction all agree.
- I reviewed the supplied X references plus current Unity/asset/animation pipeline docs and documented the takeaways in `Docs/GraphicsSOTAStudy-2026-04-25.md`.
- I added the repeatable iteration loop in `Docs/NextLevelGraphicsLoop-2026-04-25.md` so future passes do not stop at concept work.

Reference takeaways:
- AI image/video tools are useful for cinematic previs and art direction, especially GPT Image/Seedream-style stills plus Seedance 2 animated previs, but the playable game needs a coherent runtime 3D scene.
- The strongest demos combine world data or authored scene systems with camera/motion polish.
- For Moonlight, the right target is fairytale photoreal toy room for children: warm, readable, charming, spatially coherent, and action-based.

Implementation in this pass:
- Tuned photoreal bloom for a richer but less blown-out filmic response.
- Added floorboard seams and plank color variation so the room has scale.
- Added warm window sunbeams, atmospheric glow, curtain folds, a mini shelf/books cluster, star decals, and a bedside practical lamp.
- Added a small local lamp light to improve contact warmth around the bed/action area.

Latest verification:
- Built successfully with Unity 6000.3.2f1 in headless batchmode.
- Ran `Moonlight Magic House.app -mmhPlaytest`.
- Player log reached `[Playtest][PASS] xp=2520 coins=25 mood=Happy wonder=100 warmth=58 rest=37 magic=64 hunger=72`.
- Visually inspected `00_initial`, `03_SleepBtn`, and `04_PlayBtn` after the loop documentation pass.

Current honesty:
- This is an incremental runtime-quality pass, not the final premium leap.
- The real next jump is still: URP migration, better authored room assets, a better child avatar/rig, and real action animation clips.

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

## 2026-04-24 - Pivot: No More Photo Plane, Build a Coherent 3D Toy World

User feedback was clear: the previous composition still felt fake because it was a character pasted over a photo-like background. The reference direction from `Pizza Gelato Rush` is not "more backdrop"; it is a coherent runtime world with authored/procedural geometry, strong color, controlled lighting, bloom, camera staging, and actions that physically happen in the scene.

What changed in this pass:
- Replaced the photoreal bedroom image plane as the primary scene with a runtime 3D fairytale bedroom: walls, floor, window, string lights, dollhouse, snack table, bath corner, real bed, plush/toy props, rug, reflection probe, and stronger bloom.
- Added a runtime 3D meadow instead of relying on the generated meadow texture as the outdoor scene. `PLAY` now hides the bedroom root, enables the meadow root, and keeps Moonlight in the same gameplay space.
- Kept the child-friendly animated UnityChan avatar for this pass after testing Mixamo Sophie; Sophie had better built-in idle motion but read too adult for the target audience.
- Tuned child proportions and walk posing: less extreme chibi head, longer legs, foot swing, body tilt, and a cleaner walk/run phase.
- Fixed the director so it can find inactive scene roots such as `Moonlight3DMeadow`; this keeps the initial room clean while allowing the outdoor scene to activate at runtime.
- Extended room and meadow geometry so the camera does not reveal raw background at the edges.

How Pizza Gelato Rush informs Moonlight:
- Use a procedural/assembled 3D scene as the source of truth, not a flat plate.
- Treat visual quality as pipeline work: camera, lighting, post-process, materials, composition, props, VFX, and animation all have to agree.
- The "top top" roadmap should include a real render-pipeline pass, not only more primitives: URP/HDRP-grade post-processing, anti-aliasing, authored asset kits, better child rig, blend trees/root motion, and action-specific animation clips.

Near-term roadmap after this pass:
- Replace blocky primitive bed/furniture with higher-quality authored or marketplace/free assets.
- Add real animation clips per action: walk-to-bed, climb/rest, door open, run outside, bath, dance.
- Build a proper camera director with shot presets and less UI obstruction during vignettes.
- Consider migrating Moonlight to the same URP-style graphics stack used by Pizza Gelato Rush once the gameplay slice is stable.

Verification notes:
- Built with Unity 6000.3.2f1 batchmode/headless only.
- Ran the built app with `-mmhPlaytest`.
- Player log reached `[Playtest][PASS]`.
- Inspected `00_initial`, `03_SleepBtn`, `04_PlayDoor`, and `04_PlayBtn` screenshots. The pass is a real structural improvement, but still not the final "top top" art bar.

## 2026-04-24 - Polish Pass: Proportions, Props, UI, and QA Reality Check

Reason for this pass:
- The first 3D-room pass fixed the "character pasted on photo" problem, but the room still read too procedural/blocky.
- User reference remains `Pizza Gelato Rush`: the quality jump must come from coherent scene construction, camera, lighting, materials, movement, and polish working together.

What changed:
- Rebalanced the camera and default Moonlight placement so the room reads as a wider toy-bedroom scene rather than a cropped bed close-up.
- Made the bottom HUD less obstructive: centered action buttons, smaller candy controls, removed the large visible backing plate.
- Shrunk and softened the bed with rounded mattress/blanket/pillow forms, bed posts, warm canopy ribbon, and better nap target placement.
- Replaced some primitive-looking room clutter with local Kenney CC0 furniture assets already in the project: shelf, books, floor lamp, plant, and chair.
- Moved the snack and bath targets so Moonlight now travels toward the actual mini table/tub instead of only shifting in place.
- Adjusted room light/bloom and prop materials so the scene feels more cohesive with the PGR-style runtime world approach.

Workflow note:
- The first rebuild attempt failed because the macOS data volume had only about 101 MB free and Unity Package Manager could not open its `/tmp` socket.
- Freed temporary/cache space only (`/tmp/ondevibe-resign`, user-level regenerable caches such as `pip`, `electron`, `go-build`, `pnpm`) and reran Unity batchmode successfully.
- No Unity GUI was opened.

Verification:
- Built successfully with Unity 6000.3.2f1 in headless batchmode.
- Ran `Moonlight Magic House.app -mmhPlaytest`.
- Player log reached `[Playtest][PASS] xp=2322 coins=30 mood=Happy`.
- Visually inspected `00_initial`, `01_FeedBtn`, `03_SleepBtn`, `04_PlayDoor`, `04_PlayBtn`, and `05_BathBtn`.

Current honesty:
- This is now a coherent playable fairytale toy-room pass, not a final premium art pass.
- The next true "top top" step is a production art pipeline task: authored high-quality room kit, real root-motion action clips, better child avatar/rig, URP-style post stack, and shot-by-shot camera polish.
- Linear follow-up created: `FRE-122` Moonlight PGR-quality graphics and animation pipeline.

## 2026-04-25 - External APFS Library Migration + Action Camera Polish

Reason for this pass:
- The local macOS disk had already been pressure-cleaned, and the user asked to keep using the external SSD wherever possible.
- Moonlight was already on `/Volumes/SSD-FRH-1/Free-River-House/moonlight-magic-house`, but the Unity `Library` still needed a safer external home that avoids exFAT metadata/CoreCLR problems.
- The last playable build was improved, but actions still needed better staging, less UI obstruction, more readable proportions, and softer movement.

Storage/workflow change:
- Created an APFS sparsebundle on the external SSD: `/Volumes/SSD-FRH-1/Free-River-House/apfs/FRH-Moonlight-APFS.sparsebundle`.
- Mounted it at `/Volumes/FRH-Moonlight-APFS`.
- Migrated the Unity Library to `/Volumes/FRH-Moonlight-APFS/Moonlight/MMH-Library-APFS`.
- The project `Library` entry now points there as a symlink, and the old internal `/Users/mattiapetrucciani/MMH-Library-APFS` copy was removed after a successful build/playtest pass.
- Updated `.gitignore` so the `Library` symlink itself is ignored, not only real `Library/` directories.
- Unity build/playtest now uses APFS external temp/cache locations for `TMPDIR`, `TEMP`, `TMP`, `XDG_CACHE_HOME`, `UPM_CACHE_PATH`, `UPM_NPM_CACHE_PATH`, and `UPM_GIT_CACHE_PATH`.
- Note: sending Unity temp/package work directly to the exFAT SSD caused licensing/CoreCLR/package registration failures. The APFS sparsebundle on the SSD fixed that while still keeping the heavy Unity data off the internal disk.

Game polish in this pass:
- Added action shot presets so feed, cuddle, bath, dance, and similar vignettes use a lower/wider camera instead of cropping Moonlight awkwardly.
- Faded the HUD/action bar during action vignettes so the animation reads more like a small scene and less like a UI overlay.
- Improved walk/run motion with a reset walk phase, smoother facing yaw interpolation, a small arced path, stronger arm/leg rhythm, and reduced idle wobble while walking.
- Tuned the stylized child proportions again: smaller head/ribbons and slightly longer legs for a less distorted read.
- Improved bed/rest staging: Moonlight moves to the bed, the camera frames the rest pose more clearly, and the bed scale is less tiny.
- Added richer bedroom details: soft floor shadows/glows around key props, tiny wall frames, and a glowing moon/star mobile to push the fairytale atmosphere.

Verification:
- Built successfully with Unity 6000.3.2f1 in headless batchmode only.
- Ran the built app with `-mmhPlaytest`.
- Player log reached `[Playtest][PASS] xp=2426 coins=27 mood=Happy wonder=100 warmth=58 rest=37 magic=47 hunger=72`.
- Visually inspected `00_initial`, `03_SleepBtn`, `04_PlayBtn`, `05_BathBtn`, and `06_DanceBtn` screenshots after the pass.

Current honesty:
- This is a cleaner, more coherent fairytale toy-room/action pass and the SSD workflow is now much healthier.
- It is still not the final `Pizza Gelato Rush` art bar. The next big jump should be production-quality authored room assets, a better child rig/avatar, real action clips/root motion, and a URP-style render/post stack.
