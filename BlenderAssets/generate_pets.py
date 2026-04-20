"""
Moonlight Magic House — Blender 4.x procedural pet generator
Run: blender --background --python generate_pets.py

Generates 8 low-poly pet species × 5 evolution stages = 40 meshes
Exports to ../Assets/Models/Pets/<species>/<stage>.fbx
"""

import bpy
import math
import os
import bmesh
from mathutils import Vector

OUTPUT_DIR = os.path.join(os.path.dirname(__file__), "..", "Assets", "Models", "Pets")
os.makedirs(OUTPUT_DIR, exist_ok=True)

# ── Palette ────────────────────────────────────────────────────────────────────
MOONLIGHT_PURPLE = (0.45, 0.20, 0.80, 1.0)
MOONLIGHT_BLUE   = (0.20, 0.35, 0.90, 1.0)
MOONLIGHT_PINK   = (0.90, 0.45, 0.70, 1.0)
MOONLIGHT_TEAL   = (0.10, 0.70, 0.65, 1.0)
MOONLIGHT_GOLD   = (0.95, 0.80, 0.20, 1.0)
MOONLIGHT_WHITE  = (0.95, 0.92, 1.00, 1.0)

SPECIES_COLORS = {
    "Dragon":  MOONLIGHT_PURPLE,
    "Cat":     MOONLIGHT_PINK,
    "Panda":   MOONLIGHT_WHITE,
    "Fox":     MOONLIGHT_GOLD,
    "Penguin": MOONLIGHT_BLUE,
    "Bunny":   MOONLIGHT_PINK,
    "Bear":    MOONLIGHT_TEAL,
    "Owl":     MOONLIGHT_PURPLE,
}

# Scale multipliers per stage: Egg=0.4, Baby=0.6, Child=0.8, Teen=1.0, Adult=1.2
STAGE_SCALES = {"Egg": 0.4, "Baby": 0.6, "Child": 0.8, "Teen": 1.0, "Adult": 1.2}
STAGES = list(STAGE_SCALES.keys())
SPECIES = list(SPECIES_COLORS.keys())


# ── Material ───────────────────────────────────────────────────────────────────
def make_toon_material(name, color):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    nodes.clear()

    output   = nodes.new("ShaderNodeOutputMaterial")
    diffuse  = nodes.new("ShaderNodeBsdfDiffuse")
    diffuse.inputs["Color"].default_value = color
    diffuse.inputs["Roughness"].default_value = 1.0
    links.new(diffuse.outputs["BSDF"], output.inputs["Surface"])
    return mat


# ── Mesh helpers ───────────────────────────────────────────────────────────────
def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()
    for mesh in bpy.data.meshes:
        bpy.data.meshes.remove(mesh)


def add_sphere(loc=(0, 0, 0), radius=1.0, subdivisions=1):
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, radius=radius, subdivisions=subdivisions)
    me = bpy.data.meshes.new("Sphere")
    bm.to_mesh(me)
    bm.free()
    obj = bpy.data.objects.new("Sphere", me)
    bpy.context.collection.objects.link(obj)
    obj.location = loc
    return obj


def add_cube(loc=(0, 0, 0), size=(1, 1, 1)):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    me = bpy.data.meshes.new("Cube")
    bm.to_mesh(me)
    bm.free()
    obj = bpy.data.objects.new("Cube", me)
    bpy.context.collection.objects.link(obj)
    obj.location = loc
    obj.scale = size
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.transform_apply(scale=True)
    return obj


def add_cone(loc=(0, 0, 0), radius=0.3, depth=0.6, verts=6):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=True,
                          segments=verts, radius1=radius, radius2=0, depth=depth)
    me = bpy.data.meshes.new("Cone")
    bm.to_mesh(me)
    bm.free()
    obj = bpy.data.objects.new("Cone", me)
    bpy.context.collection.objects.link(obj)
    obj.location = loc
    return obj


def join_all():
    bpy.ops.object.select_all(action="SELECT")
    bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
    bpy.ops.object.join()
    return bpy.context.active_object


def assign_material(obj, mat):
    if obj.data.materials:
        obj.data.materials[0] = mat
    else:
        obj.data.materials.append(mat)


# ── EGG ────────────────────────────────────────────────────────────────────────
def build_egg(color):
    body = add_sphere((0, 0, 0.1), radius=0.5, subdivisions=1)
    body.scale.z = 1.3
    bpy.context.view_layer.objects.active = body
    bpy.ops.object.transform_apply(scale=True)
    spots = []
    for angle, height in [(0, 0.3), (1.5, 0.1), (3.0, 0.2), (4.7, 0.0)]:
        s = add_sphere((math.cos(angle) * 0.25, math.sin(angle) * 0.25, height + 0.1),
                       radius=0.07)
        spots.append(s)
    mat = make_toon_material("EggMat", color)
    for o in [body] + spots:
        assign_material(o, mat)
    return join_all()


# ── Generic critter body ───────────────────────────────────────────────────────
def build_critter(species, color):
    """Low-poly body: round body + head + 4 limbs + species-specific features."""
    body = add_sphere((0, 0, 0.3), radius=0.45, subdivisions=1)
    head = add_sphere((0, 0, 0.9), radius=0.35, subdivisions=1)

    # Eyes
    eye_l = add_sphere((-0.13, -0.33, 0.93), radius=0.06)
    eye_r = add_sphere(( 0.13, -0.33, 0.93), radius=0.06)
    eye_mat = make_toon_material("EyeMat", (0.05, 0.02, 0.12, 1.0))
    assign_material(eye_l, eye_mat)
    assign_material(eye_r, eye_mat)

    # Limbs
    limb_positions = [(-0.35, 0, 0.05), (0.35, 0, 0.05),
                      (-0.25, 0, -0.2),  (0.25, 0, -0.2)]
    limbs = [add_sphere(p, radius=0.14) for p in limb_positions]

    parts = [body, head, eye_l, eye_r] + limbs

    # Species-specific features
    extra = _species_features(species, color)
    parts.extend(extra)

    mat = make_toon_material(f"{species}Mat", color)
    for p in [body, head] + limbs + extra:
        if p not in [eye_l, eye_r]:
            assign_material(p, mat)

    return join_all()


def _species_features(species, color):
    """Return extra meshes for species-distinguishing features."""
    extras = []

    if species == "Cat":
        extras.append(add_cone((-0.18, -0.05, 1.22), radius=0.10, depth=0.25, verts=4))
        extras.append(add_cone(( 0.18, -0.05, 1.22), radius=0.10, depth=0.25, verts=4))

    elif species == "Bunny":
        ear_l = add_cube((-0.15, 0, 1.3), size=(0.08, 0.05, 0.35))
        ear_r = add_cube(( 0.15, 0, 1.3), size=(0.08, 0.05, 0.35))
        extras += [ear_l, ear_r]

    elif species == "Dragon":
        extras.append(add_cone((-0.15, 0, 1.22), radius=0.09, depth=0.28, verts=4))
        extras.append(add_cone(( 0.15, 0, 1.22), radius=0.09, depth=0.28, verts=4))
        # Tail
        tail = add_sphere((0, 0.55, 0.1), radius=0.18)
        tail_tip = add_cone((0, 0.82, 0.1), radius=0.10, depth=0.30, verts=4)
        extras += [tail, tail_tip]

    elif species == "Fox":
        extras.append(add_cone((-0.17, -0.05, 1.20), radius=0.09, depth=0.28, verts=4))
        extras.append(add_cone(( 0.17, -0.05, 1.20), radius=0.09, depth=0.28, verts=4))
        # Fluffy tail
        tail = add_sphere((0, 0.55, 0.2), radius=0.25, subdivisions=1)
        extras.append(tail)

    elif species == "Owl":
        # Ear tufts
        extras.append(add_cone((-0.15, -0.05, 1.25), radius=0.07, depth=0.22, verts=4))
        extras.append(add_cone(( 0.15, -0.05, 1.25), radius=0.07, depth=0.22, verts=4))
        # Wings (flat cubes)
        wing_l = add_cube((-0.65, 0, 0.35), size=(0.30, 0.05, 0.25))
        wing_r = add_cube(( 0.65, 0, 0.35), size=(0.30, 0.05, 0.25))
        extras += [wing_l, wing_r]

    elif species == "Penguin":
        # Belly patch
        belly = add_sphere((0, -0.38, 0.3), radius=0.28)
        belly_mat = make_toon_material("BellyMat", (0.95, 0.92, 1.0, 1.0))
        assign_material(belly, belly_mat)
        extras.append(belly)

    elif species == "Panda":
        # Eye patches
        patch_l = add_sphere((-0.14, -0.31, 0.92), radius=0.10)
        patch_r = add_sphere(( 0.14, -0.31, 0.92), radius=0.10)
        patch_mat = make_toon_material("PatchMat", (0.08, 0.05, 0.12, 1.0))
        assign_material(patch_l, patch_mat)
        assign_material(patch_r, patch_mat)
        extras += [patch_l, patch_r]

    elif species == "Bear":
        # Rounded ears
        extras.append(add_sphere((-0.28, -0.05, 1.23), radius=0.12))
        extras.append(add_sphere(( 0.28, -0.05, 1.23), radius=0.12))

    return extras


# ── Main ───────────────────────────────────────────────────────────────────────
def build_pet(species, stage):
    color = SPECIES_COLORS[species]
    scale = STAGE_SCALES[stage]

    if stage == "Egg":
        obj = build_egg(color)
    else:
        obj = build_critter(species, color)

    obj.scale = (scale, scale, scale)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(scale=True)
    return obj


def export_fbx(filepath):
    os.makedirs(os.path.dirname(filepath), exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=filepath,
        use_selection=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        path_mode="COPY",
        embed_textures=False,
        axis_forward="-Z",
        axis_up="Y",
    )


if __name__ == "__main__":
    for species in SPECIES:
        for stage in STAGES:
            print(f"  Generating {species} / {stage}…")
            clear_scene()
            obj = build_pet(species, stage)
            out = os.path.join(OUTPUT_DIR, species, f"{stage}.fbx")
            export_fbx(out)
            print(f"  → {out}")

    print("\n✅ All 40 pet meshes exported.")
