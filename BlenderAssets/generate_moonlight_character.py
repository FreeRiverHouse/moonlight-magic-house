"""
Moonlight Magic House — Blender 4.x
Generates Moonlight: a low-poly magical girl character in 5 growth stages.

Stage mesh scales and details:
  Moonbud   — tiny, round, shy (0.55)
  Starling  — small child form  (0.70)
  Luminary  — child form, glowing accessories (0.85)
  Sorceress — taller, flowing hair, staff (1.00)
  Moonkeeper— full form, wings, moon crown (1.15)

Run:  blender --background --python generate_moonlight_character.py --
Export: ../Assets/Models/Moonlight/<Stage>.fbx
"""

import bpy, bmesh, os, math
from mathutils import Vector

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Models", "Moonlight")
os.makedirs(OUT, exist_ok=True)

# ── Palette ────────────────────────────────────────────────────────────────────
SKIN      = (0.92, 0.82, 0.90, 1.0)
HAIR      = (0.55, 0.25, 0.80, 1.0)
DRESS     = (0.65, 0.30, 0.90, 1.0)
DRESS_ACC = (0.95, 0.75, 1.00, 1.0)
EYES      = (0.05, 0.02, 0.15, 1.0)
GLOW      = (0.90, 0.85, 1.00, 1.0)
GOLD      = (0.95, 0.80, 0.20, 1.0)
WING      = (0.75, 0.60, 1.00, 1.0)

STAGES = ["Moonbud", "Starling", "Luminary", "Sorceress", "Moonkeeper"]
SCALES = {"Moonbud": 0.55, "Starling": 0.70, "Luminary": 0.85, "Sorceress": 1.00, "Moonkeeper": 1.15}

# ── Helpers ────────────────────────────────────────────────────────────────────
def clear():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()

def mat(name, color, emit=0.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    nodes = m.node_tree.nodes
    nodes.clear()
    out  = nodes.new("ShaderNodeOutputMaterial")
    mix  = nodes.new("ShaderNodeMixShader")
    diff = nodes.new("ShaderNodeBsdfDiffuse")
    emit_n = nodes.new("ShaderNodeEmission")
    diff.inputs["Color"].default_value  = color
    emit_n.inputs["Color"].default_value = color
    emit_n.inputs["Strength"].default_value = emit
    mix.inputs["Fac"].default_value = emit * 0.3
    m.node_tree.links.new(diff.outputs["BSDF"],      mix.inputs[1])
    m.node_tree.links.new(emit_n.outputs["Emission"], mix.inputs[2])
    m.node_tree.links.new(mix.outputs["Shader"],     out.inputs["Surface"])
    return m

def sphere(loc, r, subs=1, name="S"):
    bm = bmesh.new()
    bmesh.ops.create_icosphere(bm, radius=r, subdivisions=subs)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob)
    ob.location = loc
    return ob

def box(loc, size, name="B"):
    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob)
    ob.location = loc
    ob.scale = size
    bpy.context.view_layer.objects.active = ob
    ob.select_set(True)
    bpy.ops.object.transform_apply(scale=True)
    return ob

def cone(loc, r, d, verts=6, name="C"):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=True,
                          segments=verts, radius1=r, radius2=0, depth=d)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob)
    ob.location = loc
    return ob

def cyl(loc, r, d, verts=8, name="Cy"):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=True,
                          segments=verts, radius1=r, radius2=r, depth=d)
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob)
    ob.location = loc
    return ob

def assign(ob, m):
    if ob.data.materials: ob.data.materials[0] = m
    else:                  ob.data.materials.append(m)

def join_all():
    bpy.ops.object.select_all(action="SELECT")
    bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
    bpy.ops.object.join()
    return bpy.context.active_object

def export_fbx(stage):
    path = os.path.join(OUT, f"{stage}.fbx")
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.fbx(
        filepath=path, use_selection=True,
        mesh_smooth_type="FACE", add_leaf_bones=False,
        axis_forward="-Z", axis_up="Y")
    print(f"  → {path}")

# ── Build Moonlight ────────────────────────────────────────────────────────────
def build_moonlight(stage):
    parts = []

    # ── Body ──────────────────────────────────────────────────────────────
    torso = box((0, 0, 0.55), (0.38, 0.22, 0.50), "Torso")
    assign(torso, mat("Dress", DRESS))
    parts.append(torso)

    # Dress skirt (wider cone below torso)
    skirt = cone((0, 0, 0.18), 0.32, 0.32, verts=10, name="Skirt")
    assign(skirt, mat("Skirt", DRESS))
    parts.append(skirt)

    # Dress ruffle accent
    ruffle = cyl((0, 0, 0.20), 0.34, 0.06, verts=10, name="Ruffle")
    assign(ruffle, mat("Ruffle", DRESS_ACC))
    parts.append(ruffle)

    # ── Head ──────────────────────────────────────────────────────────────
    head = sphere((0, 0, 1.10), 0.28, subs=1, name="Head")
    assign(head, mat("Skin", SKIN))
    parts.append(head)

    # Eyes
    eye_l = sphere((-0.10, -0.27, 1.14), 0.055, name="EyeL")
    eye_r = sphere(( 0.10, -0.27, 1.14), 0.055, name="EyeR")
    em = mat("Eyes", EYES)
    assign(eye_l, em); assign(eye_r, em)
    parts += [eye_l, eye_r]

    # Eye sparkle
    sp_l = sphere((-0.08, -0.29, 1.17), 0.018, name="SpL")
    sp_r = sphere(( 0.12, -0.29, 1.17), 0.018, name="SpR")
    gm = mat("Sparkle", GLOW, emit=1.5)
    assign(sp_l, gm); assign(sp_r, gm)
    parts += [sp_l, sp_r]

    # Rosy cheeks
    ch_l = sphere((-0.18, -0.24, 1.07), 0.04, name="ChL")
    ch_r = sphere(( 0.18, -0.24, 1.07), 0.04, name="ChR")
    cm = mat("Cheeks", (0.95, 0.60, 0.65, 1.0))
    assign(ch_l, cm); assign(ch_r, cm)
    parts += [ch_l, ch_r]

    # ── Hair ──────────────────────────────────────────────────────────────
    hair_top = sphere((0, 0, 1.34), 0.30, subs=1, name="HairTop")
    assign(hair_top, mat("Hair", HAIR))
    parts.append(hair_top)

    # Side pigtails
    ptail_l = cyl((-0.28, 0, 1.05), 0.09, 0.40, verts=6, name="PTL")
    ptail_r = cyl(( 0.28, 0, 1.05), 0.09, 0.40, verts=6, name="PTR")
    assign(ptail_l, mat("PigtailL", HAIR))
    assign(ptail_r, mat("PigtailR", HAIR))
    ptail_l.rotation_euler[2] = math.radians(-10)
    ptail_r.rotation_euler[2] = math.radians(10)
    parts += [ptail_l, ptail_r]

    # Hair ribbons
    rib_l = box((-0.28, 0, 1.28), (0.12, 0.06, 0.10), "RibL")
    rib_r = box(( 0.28, 0, 1.28), (0.12, 0.06, 0.10), "RibR")
    rm_ = mat("Ribbon", DRESS_ACC)
    assign(rib_l, rm_); assign(rib_r, rm_)
    parts += [rib_l, rib_r]

    # ── Arms ──────────────────────────────────────────────────────────────
    arm_l = cyl((-0.28, 0, 0.68), 0.07, 0.38, verts=6, name="ArmL")
    arm_r = cyl(( 0.28, 0, 0.68), 0.07, 0.38, verts=6, name="ArmR")
    arm_l.rotation_euler[2] = math.radians(20)
    arm_r.rotation_euler[2] = math.radians(-20)
    sm = mat("SkinArm", SKIN)
    assign(arm_l, sm); assign(arm_r, sm)
    parts += [arm_l, arm_r]

    # Hands
    hand_l = sphere((-0.38, 0, 0.50), 0.07, name="HandL")
    hand_r = sphere(( 0.38, 0, 0.50), 0.07, name="HandR")
    assign(hand_l, sm); assign(hand_r, sm)
    parts += [hand_l, hand_r]

    # ── Legs ──────────────────────────────────────────────────────────────
    leg_l = cyl((-0.10, 0, -0.08), 0.07, 0.38, verts=6, name="LegL")
    leg_r = cyl(( 0.10, 0, -0.08), 0.07, 0.38, verts=6, name="LegR")
    lm = mat("SkinLeg", SKIN)
    assign(leg_l, lm); assign(leg_r, lm)
    parts += [leg_l, leg_r]

    # Moon boots
    boot_l = box((-0.10, 0, -0.32), (0.14, 0.18, 0.16), "BootL")
    boot_r = box(( 0.10, 0, -0.32), (0.14, 0.18, 0.16), "BootR")
    bm_ = mat("Boots", HAIR)
    assign(boot_l, bm_); assign(boot_r, bm_)
    parts += [boot_l, boot_r]

    # ── Stage-specific extras ──────────────────────────────────────────────
    if stage == "Moonbud":
        # Tiny crescent moon on head
        crescent = cyl((0, 0, 1.52), 0.08, 0.04, verts=10, name="Crescent")
        assign(crescent, mat("CrescentMoon", GOLD, emit=0.8))
        parts.append(crescent)

    elif stage == "Starling":
        # Small star accessories on pigtails
        for dx in [-0.28, 0.28]:
            st = sphere((dx, 0, 1.32), 0.06, name="StarAcc")
            assign(st, mat("StarAcc", GOLD, emit=0.6))
            parts.append(st)

    elif stage == "Luminary":
        # Moon crown
        crown = cyl((0, 0, 1.56), 0.18, 0.08, verts=8, name="Crown")
        assign(crown, mat("Crown", GOLD, emit=0.5))
        crescent2 = cone((0, -0.18, 1.64), 0.07, 0.20, verts=8, name="CrownPeak")
        assign(crescent2, mat("CrownPeak", GOLD, emit=1.0))
        parts += [crown, crescent2]

    elif stage == "Sorceress":
        # Full moon crown + magic staff
        crown = cyl((0, 0, 1.56), 0.20, 0.10, verts=8, name="Crown")
        assign(crown, mat("SorcCrown", GOLD, emit=0.6))
        parts.append(crown)
        # Staff
        staff_pole = cyl((0.55, 0, 0.55), 0.03, 1.0, verts=6, name="StaffPole")
        assign(staff_pole, mat("StaffPole", (0.65, 0.50, 0.25, 1.0)))
        staff_orb = sphere((0.55, 0, 1.10), 0.10, name="StaffOrb")
        assign(staff_orb, mat("StaffOrb", GLOW, emit=2.0))
        parts += [staff_pole, staff_orb]

    elif stage == "Moonkeeper":
        # Full crown + wings + permanent glow halo
        crown = cyl((0, 0, 1.58), 0.22, 0.12, verts=8, name="Crown")
        assign(crown, mat("KeeperCrown", GOLD, emit=0.8))
        parts.append(crown)
        # Wings (flat curved boxes)
        for dx, angle in [(-1, -1), (1, 1)]:
            wing = box((dx * 0.55, 0, 0.75), (0.50, 0.04, 0.60), "Wing")
            wing.rotation_euler[2] = math.radians(angle * 20)
            assign(wing, mat("Wings", WING, emit=0.4))
            parts.append(wing)
        # Halo
        halo = cyl((0, 0, 1.72), 0.24, 0.025, verts=12, name="Halo")
        assign(halo, mat("Halo", GLOW, emit=3.0))
        parts.append(halo)
        # Staff
        staff_pole = cyl((0.55, 0, 0.55), 0.03, 1.0, verts=6, name="StaffPole")
        assign(staff_pole, mat("KeeperPole", (0.70, 0.55, 0.25, 1.0)))
        staff_orb = sphere((0.55, 0, 1.10), 0.13, name="StaffOrb")
        assign(staff_orb, mat("KeeperOrb", GLOW, emit=3.5))
        moon_gem = sphere((0.55, 0, 1.10), 0.08, name="MoonGem")
        assign(moon_gem, mat("MoonGem", (0.80, 0.70, 1.00, 1.0), emit=1.5))
        parts += [staff_pole, staff_orb, moon_gem]

    return join_all()

# ── Run ────────────────────────────────────────────────────────────────────────
if __name__ == "__main__":
    for stage in STAGES:
        print(f"  Building Moonlight / {stage}…")
        clear()
        ob = build_moonlight(stage)
        sc = SCALES[stage]
        ob.scale = (sc, sc, sc)
        bpy.context.view_layer.objects.active = ob
        ob.select_set(True)
        bpy.ops.object.transform_apply(scale=True)
        export_fbx(stage)

    print("\n✅ Moonlight — all 5 stages exported.")
