# Moonlight Magic House Handover

## Snapshot

- Date: 2026-04-24
- Repo: `FreeRiverHouse/moonlight-magic-house`
- Local path: `/Volumes/SSD-FRH-1/Free-River-House/moonlight-magic-house`
- Current branch: `main`
- Current git identity: `FreeRiverHouse <freeriverhouse@gmail.com>`
- Unity: `6000.3.2f1`, Built-in RP, macOS
- Output app: `~/Desktop/MMH-Build/MoonlightMagicHouse.app`
- Player log: `~/Library/Logs/FreeRiverHouse/Moonlight Magic House/Player.log`
- QA screenshots: `~/Desktop/MMH-QA/playtest/`
- Local Unity Library symlink: `Library -> ~/MMH-Library-APFS`

The `Library` symlink is local infrastructure for the APFS/exFAT workaround and must stay out of git.

## Non-Negotiables

- Never open the Unity Editor GUI.
- Use headless/batchmode only.
- Use latest project files from git/main and local working tree.
- Use existing repo patterns and documented MOP/devlog notes.
- Build and visually verify before saying work is done.
- If a visual result is bad, iterate instead of asking the user to approve obvious polish.
- Do not commit credentials, tokens, `.app` builds, Unity `Library`, `Temp`, or generated local logs.

## Commands

Build:

```bash
/Applications/Unity/Hub/Editor/6000.3.2f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath /Volumes/SSD-FRH-1/Free-River-House/moonlight-magic-house \
  -executeMethod BuildAll.BuildMac \
  -logFile -
```

Headless playtest with screenshots:

```bash
"$HOME/Desktop/MMH-Build/MoonlightMagicHouse.app/Contents/MacOS/Moonlight Magic House" \
  -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -mmhPlaytest
```

Open playable build for the user:

```bash
open -n "$HOME/Desktop/MMH-Build/MoonlightMagicHouse.app" --args \
  -screen-fullscreen 0 -screen-width 1280 -screen-height 720
```

Check runtime log:

```bash
tail -n 200 "$HOME/Library/Logs/FreeRiverHouse/Moonlight Magic House/Player.log"
```

## Current Moonlight Direction

User wants Moonlight Magic House to become top-tier, child-targeted, photoreal, cozy, and fairy-tale magical. The generated meadow is considered emotionally correct and should be a reference for the whole game: photoreal materials with storybook warmth, soft light, pastel flowers, glowing dust, safe spaces, and readable toy-like interaction.

Gameplay actions should become tiny spatial scenes, not static button poses:

- `SNACK`: Moonlight moves to a snack spot and reacts.
- `HUG`: Moonlight moves slightly and gives a readable cuddle/hug pose.
- `NAP`: Moonlight goes to the bed and settles into a sleeping/resting pose.
- `PLAY`: Moonlight walks to a door/portal, opens it, exits, the camera/backdrop changes to the meadow, then she runs/plays outside.
- `BATH`: Moonlight returns to the room and uses the bath/care reaction.
- `DANCE`: Moonlight moves to a dance space and performs a cute dance.

The user liked the recent animation improvement but still cares a lot about proportions, cuteness, and top visual quality. Keep improving proportions without losing charm.

## Recent Implementation

Main files:

- `Docs/MoonlightDevlog.md`: direction, method, verification notes, roadmap implications.
- `Assets/Scripts/Core/MoonlightSceneDirector.cs`: action vignette director, door/portal transition, room/meadow switch, bed routine.
- `Assets/Scripts/Characters/MoonlightKidAnimator.cs`: runtime posing, idle/walk/run, action gestures, proportion pass.
- `Assets/Scripts/Core/MoonlightHouseSetup.cs`: photoreal setup, child avatar spawning, scene director wiring, larger backdrop.
- `Assets/Scripts/Core/ScreenshotCapture.cs`: `-mmhPlaytest`, staged screenshots, extra PlayDoor capture.
- `Assets/Resources/Photoreal/room-generated.png`: cozy bedroom backdrop.
- `Assets/Resources/Photoreal/meadow-generated.png`: fairy-tale meadow backdrop.
- `Assets/Resources/Mixamo/`: imported Mixamo/Sophie assets retained in repo, though current runtime uses the child-proportioned avatar path.

Latest verified result:

- Batchmode build succeeded.
- Built app ran with `-mmhPlaytest`.
- Player log reached `[Playtest][PASS]`.
- Screenshots visually checked: initial room, bed rest, door transition, meadow play, and room return.

Known visual follow-ups:

- Bed rest reads as sleeping, but should later be integrated better with bedding/depth so she does not feel pasted against a 2D photo plane.
- Door/portal communicates the scene change, but should later become a higher-quality authored prop or photoreal magical portal.
- The 3D character is still more anime/chibi than fully photoreal. User likes the animation, so improve carefully rather than replacing impulsively.
- The meadow direction is strong; propagate its fairy-tale photoreal atmosphere into bedroom props, UI, particles, and future rooms.

## Organization Context

GitHub:

- Org/account: `FreeRiverHouse`
- Working email: `freeriverhouse@gmail.com`
- Moonlight repo: `https://github.com/FreeRiverHouse/moonlight-magic-house`
- Treat any PATs or embedded remote credentials as secrets. Do not paste them into chat, docs, logs, or commits.

Linear:

- Workspace/team: `Free River House`
- Team key: `FRE`
- Admin/user visible in Linear: `Free River <freeriverhouse@gmail.com>`
- Moonlight does not currently appear as a dedicated Linear project in the queried list; track Moonlight in repo docs/devlog unless a Linear project/issue is created.

Relevant Linear projects visible on 2026-04-24:

- `VibeTalk — Mac dictation (beta 1)`: on-device macOS dictation, beta 1, WPM stats/UI polish/OAuth rename.
- `VibeTalk — PM mode (beta 2)`: voice-driven PM, LLM choices, tasks/notes/commit, PM cards, Google sync.
- `VibeTalk — Watch companion (beta 3)`: Apple Watch dictation companion.
- `VibeTalk — Parakeet engine A/B`: NVIDIA Parakeet STT alternate engine.
- `onde.surf — Storefront + Command Center`: staging/storefront/internal command center, Next.js/Cloudflare Pages.
- `onde.la — Casa Editrice`: production publishing site; staging first, never deploy direct prod.
- `Onde Flow — Creative OS`: Electron/Next/R3F creative OS with local/remote LLM agents.
- `Pizza Gelato Rush`: Unity arcade racer, WebGL+Mac.
- `FRH-ADMIN & procedure`: MOP/procedure documentation.
- `LOCAL-LLM Infra`: LM Studio, MLX/Qwen, local proxy/context work.
- `Future backlog`: parked ideas.

Recent/relevant Linear issue flavor:

- FRE-105, FRE-108, FRE-111: product UX and "resume where you left off" concepts.
- FRE-112: Grok-generated background themes.
- FRE-113: chill/vibe aesthetic pass.
- FRE-106: agent/watchdog/autonomy infrastructure.

These are not Moonlight-specific but reveal the product taste: autonomous agents, polished visual direction, strong onboarding, and shipping discipline.

## Tooling And Access

Local tools used:

- Shell/zsh in the repo.
- `git`, `rg`, `sed`, `find`, `du`, `df`, `pgrep`.
- Unity batchmode at `/Applications/Unity/Hub/Editor/6000.3.2f1/Unity.app/...`.
- Codex `apply_patch` for manual edits.
- Codex image generation for photoreal background assets.
- Codex visual inspection via screenshot/image viewing.
- Linear MCP connector for teams/projects/issues.

Optional tools the user has authorized if available in the target account/session:

- Gemini.
- Grok/xAI.
- Unity/Adobe/free asset sources or Unity Marketplace assets.

Do not assume those optional tools are authenticated in a new account. Verify access first through the relevant CLI, browser session, or connector. Never paste credentials into repo files.

## Credentials And Login Checklist

For a new account/session, verify:

- GitHub access to `FreeRiverHouse/moonlight-magic-house`; prefer credential helper, `gh auth login`, or HTTPS/SSH auth managed outside the repo.
- Git identity is set to `FreeRiverHouse <freeriverhouse@gmail.com>` if committing on behalf of FRH.
- Linear connector can see workspace `Free River House` and team key `FRE`.
- Unity Hub/license can run batchmode for Unity `6000.3.2f1`.
- OpenAI/Codex image generation is available if generating new assets.
- Gemini/Grok are available only if the session has authenticated access.

Never document raw tokens, passwords, PATs, OAuth refresh tokens, cookies, or API keys. If any token appears in shell output, redact it in any handover text.

## Handover Prompt To Paste Into A New Account

```text
You are inheriting work on Free River House's Unity project Moonlight Magic House.

Work in Italian with the user unless they switch language. Be proactive, visual, and concrete. The user wants a top-tier child-targeted game: cozy, photoreal, fairy-tale magical, cute, polished, and actually playable.

Project:
- Repo: https://github.com/FreeRiverHouse/moonlight-magic-house
- Local path on the original machine: /Volumes/SSD-FRH-1/Free-River-House/moonlight-magic-house
- Unity: 6000.3.2f1, Built-in RP, macOS
- Build output: ~/Desktop/MMH-Build/MoonlightMagicHouse.app
- Player log: ~/Library/Logs/FreeRiverHouse/Moonlight Magic House/Player.log
- QA screenshots: ~/Desktop/MMH-QA/playtest/
- Unity Library symlink: Library -> ~/MMH-Library-APFS, keep it local and out of git

Rules:
- Never open Unity Editor GUI.
- Use Unity batchmode/headless only.
- Build before claiming done.
- Use -mmhPlaytest and inspect screenshots/Player.log before final claims.
- Do not commit Unity Library, Temp, .app builds, logs, credentials, tokens, or local secrets.
- Do not expose raw credentials. Use configured GitHub/Linear/session auth; re-login if needed.

Important commands:
Build:
/Applications/Unity/Hub/Editor/6000.3.2f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath /Volumes/SSD-FRH-1/Free-River-House/moonlight-magic-house -executeMethod BuildAll.BuildMac -logFile -

Playtest:
"$HOME/Desktop/MMH-Build/MoonlightMagicHouse.app/Contents/MacOS/Moonlight Magic House" -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -mmhPlaytest

Open app:
open -n "$HOME/Desktop/MMH-Build/MoonlightMagicHouse.app" --args -screen-fullscreen 0 -screen-width 1280 -screen-height 720

Read first:
- README.md
- Docs/MoonlightDevlog.md
- Docs/MoonlightHandover.md
- Assets/Scripts/Core/MoonlightHouseSetup.cs
- Assets/Scripts/Core/MoonlightSceneDirector.cs
- Assets/Scripts/Characters/MoonlightKidAnimator.cs
- Assets/Scripts/Core/ScreenshotCapture.cs

Current direction:
- The meadow backdrop is the emotional target: photoreal, warm, safe, fairy-tale, glowing, child-friendly.
- Extend that feel into bedroom, UI, particles, props, and future rooms.
- Care actions must be mini-scenes with spatial intent, not static poses.
- NAP: Moonlight should go to the bed and rest.
- PLAY: Moonlight should go to the door, open it, exit, camera/backdrop changes to a cute meadow, then she runs/plays.
- Keep improving proportions and cuteness. The user likes the recent animation improvement but still wants proportions and polish at "TOP TOP TOP".

Latest implementation:
- Photoreal backdrops in Assets/Resources/Photoreal/.
- Runtime action director in MoonlightSceneDirector.
- Child avatar runtime posing/proportion pass in MoonlightKidAnimator.
- QA screenshot/playtest automation in ScreenshotCapture.
- Devlog documents fairytale photoreal direction and working method.

Organization:
- GitHub org/account: FreeRiverHouse.
- Commit identity should be FreeRiverHouse <freeriverhouse@gmail.com> if authorized.
- Linear workspace/team: Free River House, team key FRE.
- Linear admin/user visible: Free River <freeriverhouse@gmail.com>.
- No dedicated Moonlight Linear project was visible in the latest query; use repo docs/devlog unless creating a new issue/project.
- Other Linear projects include VibeTalk beta tracks, onde.surf, onde.la, Onde Flow, Pizza Gelato Rush, FRH-ADMIN, LOCAL-LLM Infra.

Credentials/tools:
- Do not ask the user to paste tokens.
- Verify GitHub with git/gh auth and redact remote tokens in any output.
- Verify Linear via connector before referencing live issues.
- Gemini/Grok are authorized by the user if available, but do not assume they are logged in.
- Image generation can be used for new photoreal assets when useful.

First action after onboarding:
1. git status and git pull/rebase from origin/main.
2. Read Docs/MoonlightDevlog.md and Docs/MoonlightHandover.md.
3. Build in batchmode.
4. Run -mmhPlaytest.
5. Inspect screenshots, especially 00_initial.png, 03_SleepBtn.png, 04_PlayDoor.png, 04_PlayBtn.png, 05_BathBtn.png.
6. Continue polishing Moonlight proportions, bed integration, door/portal quality, and fairy-tale photoreal consistency.
```
