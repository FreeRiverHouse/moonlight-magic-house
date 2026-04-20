#!/bin/bash
# Generate all Moonlight Magic House 3D assets via Blender headless
# Usage: ./run_blender_gen.sh [path-to-blender]

BLENDER="${1:-/Applications/Blender.app/Contents/MacOS/Blender}"
DIR="$(cd "$(dirname "$0")" && pwd)"

if [ ! -f "$BLENDER" ]; then
  echo "❌ Blender not found at $BLENDER"
  echo "   Usage: $0 /path/to/Blender.app/Contents/MacOS/Blender"
  exit 1
fi

echo "🌙 Moonlight Magic House — Asset Generator"
echo "   Blender: $BLENDER"
echo ""

echo "── Moonlight character (5 stages) ──"
"$BLENDER" --background --python "$DIR/generate_moonlight_character.py" -- 2>&1 | grep -E "→|✅|❌|Error"

echo ""
echo "── Moonlight outfits (11 accessories) ──"
"$BLENDER" --background --python "$DIR/generate_moonlight_outfits.py" -- 2>&1 | grep -E "→|✅|❌|Error"

echo ""
echo "── House props (12 meshes) ──"
"$BLENDER" --background --python "$DIR/generate_house_props.py" -- 2>&1 | grep -E "→|✅|❌|Error"

echo ""
echo "── Audio SFX (numpy, no Blender needed) ──"
python3 "$DIR/generate_audio_sfx.py" 2>&1 | grep -E "→|✅|❌|Error"

echo ""
echo "── Ambient music (numpy, no Blender needed) ──"
python3 "$DIR/generate_ambient_music.py" 2>&1 | grep -E "→|✅|❌|Error"

echo ""
echo "✅ All assets generated."
echo "   Models  → Assets/Models/"
echo "   SFX     → Assets/Audio/SFX/"
echo "   Music   → Assets/Audio/Music/"
