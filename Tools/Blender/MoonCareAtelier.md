# Moon Care Atelier

Deterministic Blender generator for the authored Moonlight Magic House care
station. The FBX contains a centered, ground-aligned vanity with a basin,
crescent mirror, towel tray, brush, comb, cream jar, and pastel materials.
It intentionally contains no lights, cameras, armatures, animations, or
collider-prefixed objects.

## Generate

Use Blender 4.x in headless mode:

```bash
/Applications/Blender.app/Contents/MacOS/Blender \
  --background \
  --python generate_moon_care_atelier.py \
  -- \
  --output /absolute/path/MoonCareAtelier.fbx
```

For an optional inspection source file, add:

```text
--save-blend /absolute/path/MoonCareAtelier.blend
```

The script prints `MOON_CARE_ATELIER_EXPORTED` and the exported mesh count on
success. It fails before export if a camera/light exists or geometry exceeds
the intended station bounds.

## Unity destination

Import the generated file at exactly:

```text
Assets/Resources/Models/Hero/MoonCareAtelier.fbx
```

The model is authored in meters, centered at world origin with its lowest
surface at `Z=0` in Blender. Its front faces Blender `-Y`; FBX export uses
`-Z forward` and `Y up`, matching Unity's standard Blender import convention.
