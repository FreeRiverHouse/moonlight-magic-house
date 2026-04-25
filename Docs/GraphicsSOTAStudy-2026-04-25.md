# Graphics SOTA Study - 2026-04-25

Scope: study current "people are shipping with Claude/AI tools" references, then translate the useful parts into a Moonlight Magic House roadmap and an immediate runtime graphics pass.

## References Reviewed

- Mark Gadala / GPT Image 2 + Seedance 2 game-trailer examples: https://x.com/markgadala/status/2047825115631518115
- Om Patel / GTA-style browser game on Google Earth cities: https://x.com/om_patel5/status/2047849798162682260
- Latte / VR flight experience with GPT Image 2 + Seedance 2: https://x.com/0xbisc/status/2048000224875143540
- INK / GPT Image 2 + Seedance 2 animated game UI reference: https://x.com/0xink_/status/2047648944004755679
- ByteDance Seedance 2 official model page: https://seed.bytedance.com/en/seedance2_0
- Unity URP docs: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/urp-introduction.html
- Unity 2026 render-pipeline strategy: https://unity.com/topics/render-pipelines-strategy-for-2026
- Unity Animation Rigging package: https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.animation.rigging.html
- Cesium Photorealistic 3D Tiles: https://cesium.com/learn/photorealistic-3d-tiles-learn/
- Poly Haven CC0 asset library: https://polyhaven.com/
- Adobe Substance 3D Assets for Unity: https://experienceleague.adobe.com/en/docs/substance-3d/ecosystem/game-engines/unity/substance-3d-assets-library-usage
- Cascadeur AI/physics-assisted animation workflow: https://cascadeur.com/

## Read Of The X References

The strongest current examples are not just "prettier pictures." They combine four things:

- Cinematic previsualization: GPT Image/Seedream-style stills plus Seedance 2 video can rapidly define mood, lighting, palette, camera language, animation intent, and the fantasy of the game.
- Runtime coherence: the playable build still needs a real 3D world. Pasting a character over a beautiful plate immediately reads fake.
- Data/world leverage: the Google Earth/Cesium example is impressive because it turns existing world data into a playable system.
- Motion quality: the VR flight reference works because camera framing, speed, cockpit scale, and atmosphere sell movement. Weak UI stability is noticeable even when the visuals are strong.

## Moonlight Direction

Moonlight should target "fairytale photoreal toy room," not raw photorealism. For a children target, the top bar is warmth, readable scale, charming motion, safe lighting, and small vignettes that physically happen in the room.

Approved direction:

- Keep runtime 3D scene as source of truth.
- Use AI-generated imagery only for concept/reference/previs unless it becomes a texture with proper 3D support.
- Use Seedance 2 as the main animated previs tool: action timing, camera motion, UI feel, character appeal, and trailer target. Do not treat Seedance output itself as gameplay unless it is a non-interactive trailer/marketing asset.
- Move toward URP because Unity's own 2026 strategy is centered on URP, while Built-in RP is entering deprecation.
- Replace procedural placeholder furniture with authored or high-quality CC0/marketplace assets.
- Use Animation Rigging, Mixamo/Cascadeur/hand-authored clips, and action-specific root motion for rest, bath, dance, cuddle, and outdoor play.
- Build camera shots per action, with UI fade and child-safe composition.

## Immediate Pass Implemented

- Tuned the photoreal bloom curve to a softer but richer filmic response.
- Added production-read room details: floorboard seams, warm plank variation, sunbeams, curtain folds, mini shelf/books, wall star decals, bedside practical lamp, and a local warm light.
- These details are deliberately small scale cues. They help Moonlight feel inside a room rather than placed in front of a decorative background.

## Next Production Steps

1. URP migration spike for Moonlight in a branch, matching the PGR graphics stack.
2. Import a coherent cozy bedroom kit: bed, rug, shelves, plushes, lamp, door, window, and bath corner.
3. Replace the current child avatar with a more suitable rigged child-safe character and consistent material pass.
4. Author action clips: walk-to-bed, climb/rest, wake, door open, run outside, bath, dance, cuddle.
5. Add Cinemachine/action-shot director and QA screenshot targets per vignette.

See also `Docs/NextLevelGraphicsLoop-2026-04-25.md` for the repeatable production loop and stop criteria.
