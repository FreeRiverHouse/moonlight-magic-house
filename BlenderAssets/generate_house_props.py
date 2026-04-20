"""
Moonlight Magic House — Blender 4.x house props generator
Run: blender --background --python generate_house_props.py

Generates low-poly room furniture + interactive objects
Exports to ../Assets/Models/Props/<room>/<prop>.fbx
"""

import bpy
import bmesh
import os
import math

OUTPUT_DIR = os.path.join(os.path.dirname(__file__), "..", "Assets", "Models", "Props")
os.makedirs(OUTPUT_DIR, exist_ok=True)

# Moonlight palette
PURPLE_DARK  = (0.15, 0.08, 0.30, 1.0)
PURPLE_MID   = (0.45, 0.20, 0.80, 1.0)
PINK_SOFT    = (0.90, 0.55, 0.75, 1.0)
GOLD         = (0.95, 0.80, 0.20, 1.0)
TEAL         = (0.10, 0.65, 0.60, 1.0)
WHITE_WARM   = (0.95, 0.92, 1.00, 1.0)
WOOD_WARM    = (0.55, 0.35, 0.20, 1.0)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


def mat(name, color):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nodes = m.node_tree.nodes
    nodes.clear()
    out  = nodes.new("ShaderNodeOutputMaterial")
    diff = nodes.new("ShaderNodeBsdfDiffuse")
    diff.inputs["Color"].default_value = color
    m.node_tree.links.new(diff.outputs["BSDF"], out.inputs["Surface"])
    return m


def add_box(loc, size, name="Box"):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    obj = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(obj)
    obj.location = loc
    obj.scale = size
    bpy.ops.object.select_all(action="DESELECT")
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(scale=True)
    return obj


def add_cylinder(loc, radius, depth, verts=8, name="Cyl"):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=True,
                          segments=verts, radius1=radius, radius2=radius, depth=depth)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    obj = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(obj)
    obj.location = loc
    return obj


def join_selected():
    bpy.ops.object.select_all(action="SELECT")
    bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
    bpy.ops.object.join()
    return bpy.context.active_object


def export(name, room):
    bpy.ops.object.select_all(action="SELECT")
    path = os.path.join(OUTPUT_DIR, room, f"{name}.fbx")
    os.makedirs(os.path.dirname(path), exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=path, use_selection=True,
        mesh_smooth_type="FACE", add_leaf_bones=False,
        axis_forward="-Z", axis_up="Y",
    )
    print(f"  → {path}")


# ═══════════════════════════════════════════════════════════════════════════════
# BEDROOM
# ═══════════════════════════════════════════════════════════════════════════════

def make_bed():
    clear_scene()
    frame  = add_box((0, 0, 0.2), (1.2, 2.2, 0.25))
    frame.data.materials.append(mat("Wood", WOOD_WARM))
    mattress = add_box((0, 0, 0.4), (1.1, 2.0, 0.20))
    mattress.data.materials.append(mat("Mattress", WHITE_WARM))
    pillow = add_box((0, 0.78, 0.54), (0.7, 0.45, 0.15))
    pillow.data.materials.append(mat("Pillow", PURPLE_MID))
    blanket = add_box((0, -0.2, 0.52), (1.1, 1.3, 0.12))
    blanket.data.materials.append(mat("Blanket", PINK_SOFT))
    headboard = add_box((0, 1.15, 0.7), (1.2, 0.1, 0.8))
    headboard.data.materials.append(mat("Headboard", PURPLE_DARK))
    result = join_selected()
    result.name = "Bed"
    export("Bed", "Bedroom")


def make_wardrobe():
    clear_scene()
    body = add_box((0, 0, 0.9), (1.0, 0.5, 1.8))
    body.data.materials.append(mat("WardrobeBody", WOOD_WARM))
    door_l = add_box((-0.25, -0.27, 0.9), (0.47, 0.04, 1.6))
    door_l.data.materials.append(mat("DoorL", PURPLE_MID))
    door_r = add_box(( 0.25, -0.27, 0.9), (0.47, 0.04, 1.6))
    door_r.data.materials.append(mat("DoorR", PURPLE_MID))
    knob_l = add_cylinder((-0.05, -0.31, 0.9), 0.04, 0.05)
    knob_l.data.materials.append(mat("Knob", GOLD))
    knob_r = add_cylinder(( 0.05, -0.31, 0.9), 0.04, 0.05)
    knob_r.data.materials.append(mat("Knob2", GOLD))
    result = join_selected()
    result.name = "Wardrobe"
    export("Wardrobe", "Bedroom")


# ═══════════════════════════════════════════════════════════════════════════════
# KITCHEN
# ═══════════════════════════════════════════════════════════════════════════════

def make_food_bowl():
    clear_scene()
    bowl = add_cylinder((0, 0, 0.08), 0.30, 0.16, verts=10)
    bowl.data.materials.append(mat("BowlBody", TEAL))
    rim = add_cylinder((0, 0, 0.17), 0.32, 0.04, verts=10)
    rim.data.materials.append(mat("Rim", GOLD))
    result = join_selected()
    result.name = "FoodBowl"
    export("FoodBowl", "Kitchen")


def make_fridge():
    clear_scene()
    body = add_box((0, 0, 1.0), (0.8, 0.7, 2.0))
    body.data.materials.append(mat("FridgeBody", WHITE_WARM))
    door = add_box((0, -0.38, 1.0), (0.76, 0.04, 1.96))
    door.data.materials.append(mat("FridgeDoor", PURPLE_MID))
    handle = add_cylinder((0.3, -0.42, 1.1), 0.03, 0.6, verts=6)
    handle.data.materials.append(mat("Handle", GOLD))
    result = join_selected()
    result.name = "Fridge"
    export("Fridge", "Kitchen")


def make_cake():
    clear_scene()
    base   = add_cylinder((0, 0, 0.12), 0.28, 0.25, verts=10)
    base.data.materials.append(mat("Sponge", (0.95, 0.80, 0.60, 1.0)))
    icing  = add_cylinder((0, 0, 0.27), 0.29, 0.06, verts=10)
    icing.data.materials.append(mat("Icing", PINK_SOFT))
    cherry = add_cylinder((0, 0, 0.32), 0.07, 0.08, verts=6)
    cherry.data.materials.append(mat("Cherry", (0.85, 0.10, 0.20, 1.0)))
    result = join_selected()
    result.name = "Cake"
    export("Cake", "Kitchen")


# ═══════════════════════════════════════════════════════════════════════════════
# LIVING ROOM
# ═══════════════════════════════════════════════════════════════════════════════

def make_sofa():
    clear_scene()
    seat   = add_box((0, 0, 0.35), (1.6, 0.8, 0.3))
    seat.data.materials.append(mat("SeatBase", PURPLE_MID))
    back   = add_box((0, 0.42, 0.75), (1.6, 0.12, 0.8))
    back.data.materials.append(mat("SofaBack", PURPLE_DARK))
    arm_l  = add_box((-0.78, 0, 0.55), (0.12, 0.8, 0.45))
    arm_l.data.materials.append(mat("ArmL", PURPLE_DARK))
    arm_r  = add_box(( 0.78, 0, 0.55), (0.12, 0.8, 0.45))
    arm_r.data.materials.append(mat("ArmR", PURPLE_DARK))
    pillow = add_box((0, 0.1, 0.55), (0.5, 0.45, 0.25))
    pillow.data.materials.append(mat("Pillow2", PINK_SOFT))
    result = join_selected()
    result.name = "Sofa"
    export("Sofa", "LivingRoom")


def make_toy_chest():
    clear_scene()
    body = add_box((0, 0, 0.3), (0.8, 0.5, 0.6))
    body.data.materials.append(mat("ChestBody", WOOD_WARM))
    lid  = add_box((0, 0, 0.63), (0.82, 0.52, 0.08))
    lid.data.materials.append(mat("Lid", GOLD))
    latch = add_box((0, -0.28, 0.42), (0.12, 0.04, 0.08))
    latch.data.materials.append(mat("Latch", TEAL))
    star_top = add_cylinder((0, 0, 0.70), 0.07, 0.04, verts=5)
    star_top.data.materials.append(mat("Star", GOLD))
    result = join_selected()
    result.name = "ToyChest"
    export("ToyChest", "LivingRoom")


# ═══════════════════════════════════════════════════════════════════════════════
# GARDEN
# ═══════════════════════════════════════════════════════════════════════════════

def make_magic_flower():
    clear_scene()
    stem   = add_cylinder((0, 0, 0.3), 0.04, 0.6, verts=6)
    stem.data.materials.append(mat("Stem", TEAL))
    center = add_cylinder((0, 0, 0.65), 0.12, 0.10, verts=8)
    center.data.materials.append(mat("Center", GOLD))
    for i in range(6):
        angle = math.radians(i * 60)
        px = math.cos(angle) * 0.22
        py = math.sin(angle) * 0.22
        petal = add_box((px, py, 0.65), (0.12, 0.20, 0.06))
        petal.data.materials.append(mat(f"Petal{i}", PURPLE_MID))
    result = join_selected()
    result.name = "MagicFlower"
    export("MagicFlower", "Garden")


def make_well():
    clear_scene()
    base   = add_cylinder((0, 0, 0.2), 0.55, 0.4, verts=10)
    base.data.materials.append(mat("WellBase", (0.65, 0.55, 0.45, 1.0)))
    rim    = add_cylinder((0, 0, 0.43), 0.58, 0.08, verts=10)
    rim.data.materials.append(mat("WellRim", PURPLE_DARK))
    post_l = add_box((-0.52, 0, 0.75), (0.08, 0.08, 0.70))
    post_l.data.materials.append(mat("PostL", WOOD_WARM))
    post_r = add_box(( 0.52, 0, 0.75), (0.08, 0.08, 0.70))
    post_r.data.materials.append(mat("PostR", WOOD_WARM))
    bar    = add_cylinder((0, 0, 1.12), 0.06, 1.1, verts=6)
    bar.rotation_euler[1] = math.radians(90)
    bar.data.materials.append(mat("Bar", WOOD_WARM))
    result = join_selected()
    result.name = "MagicWell"
    export("MagicWell", "Garden")


# ═══════════════════════════════════════════════════════════════════════════════
# LIBRARY
# ═══════════════════════════════════════════════════════════════════════════════

def make_bookshelf():
    clear_scene()
    frame  = add_box((0, 0, 1.0), (1.2, 0.4, 2.0))
    frame.data.materials.append(mat("Frame", WOOD_WARM))
    shelf_colors = [PURPLE_MID, TEAL, PINK_SOFT, GOLD, PURPLE_DARK]
    for row in range(5):
        z = 0.15 + row * 0.38
        for col in range(5):
            x = -0.44 + col * 0.22
            book = add_box((x, -0.19, z), (0.16, 0.30, 0.32))
            book.data.materials.append(mat(f"Book{row}{col}", shelf_colors[col % len(shelf_colors)]))
    result = join_selected()
    result.name = "Bookshelf"
    export("Bookshelf", "Library")


def make_reading_chair():
    clear_scene()
    seat   = add_box((0, 0, 0.40), (0.8, 0.8, 0.25))
    seat.data.materials.append(mat("Seat", PURPLE_MID))
    back   = add_box((0, 0.38, 0.72), (0.8, 0.10, 0.70))
    back.data.materials.append(mat("Back", PURPLE_DARK))
    arm_l  = add_box((-0.38, 0, 0.58), (0.08, 0.8, 0.40))
    arm_l.data.materials.append(mat("ArmL2", PURPLE_DARK))
    arm_r  = add_box(( 0.38, 0, 0.58), (0.08, 0.8, 0.40))
    arm_r.data.materials.append(mat("ArmR2", PURPLE_DARK))
    for dx, dy in [(-0.3, -0.3), (0.3, -0.3), (-0.3, 0.3), (0.3, 0.3)]:
        leg = add_box((dx, dy, 0.15), (0.07, 0.07, 0.30))
        leg.data.materials.append(mat("Leg", GOLD))
    result = join_selected()
    result.name = "ReadingChair"
    export("ReadingChair", "Library")


# ═══════════════════════════════════════════════════════════════════════════════
# SHARED PROPS
# ═══════════════════════════════════════════════════════════════════════════════

def make_moon_lamp():
    """Glowing moon-shaped floor lamp — appears in multiple rooms."""
    clear_scene()
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, radius=0.30, subdivisions=2)
    me = bpy.data.meshes.new("MoonSphere")
    bm.to_mesh(me)
    bm.free()
    moon = bpy.data.objects.new("MoonSphere", me)
    bpy.context.collection.objects.link(moon)
    moon.location = (0, 0, 1.5)
    moon_mat = bpy.data.materials.new("MoonGlow")
    moon_mat.use_nodes = True
    nodes = moon_mat.node_tree.nodes
    nodes.clear()
    out    = nodes.new("ShaderNodeOutputMaterial")
    emit   = nodes.new("ShaderNodeEmission")
    emit.inputs["Color"].default_value = (0.95, 0.92, 0.75, 1.0)
    emit.inputs["Strength"].default_value = 3.0
    moon_mat.node_tree.links.new(emit.outputs["Emission"], out.inputs["Surface"])
    moon.data.materials.append(moon_mat)

    pole = add_cylinder((0, 0, 0.75), 0.03, 1.5, verts=6)
    pole.data.materials.append(mat("Pole", GOLD))
    base = add_cylinder((0, 0, 0.08), 0.20, 0.16, verts=8)
    base.data.materials.append(mat("LampBase", PURPLE_DARK))
    result = join_selected()
    result.name = "MoonLamp"
    export("MoonLamp", "Shared")


# ── Run all ────────────────────────────────────────────────────────────────────
if __name__ == "__main__":
    generators = [
        make_bed, make_wardrobe,
        make_food_bowl, make_fridge, make_cake,
        make_sofa, make_toy_chest,
        make_magic_flower, make_well,
        make_bookshelf, make_reading_chair,
        make_moon_lamp,
    ]
    for gen in generators:
        print(f"Building {gen.__name__}…")
        gen()
    print("\n✅ All house props exported.")
