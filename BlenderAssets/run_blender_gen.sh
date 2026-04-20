#!/bin/bash
# Generate all Moonlight Magic House assets via Blender headless
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

echo "── Generating pet models (40 meshes) ──"
"$BLENDER" --background --python "$DIR/generate_pets.py" -- 2>&1 | grep -E "→|✅|❌|Error"

echo ""
echo "── Generating house props ──"
"$BLENDER" --background --python "$DIR/generate_house_props.py" -- 2>&1 | grep -E "→|✅|❌|Error"

echo ""
echo "✅ Done! Assets in Assets/Models/"
