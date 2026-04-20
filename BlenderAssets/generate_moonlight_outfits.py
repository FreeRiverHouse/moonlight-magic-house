"""
Moonlight Magic House — Blender 4.x
Generates outfit accessories for Moonlight (8 outfits).
These are addon objects parented to the Moonlight rig — not full body replacements.

Exports: ../Assets/Models/Outfits/<OutfitName>.fbx
Run: blender --background --python generate_moonlight_outfits.py --
"""

import bpy, bmesh, os, math
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Models", "Outfits")
os.makedirs(OUT, exist_ok=True)

PURPLE = (0.55, 0.25, 0.80, 1.0)
GOLD   = (0.95, 0.80, 0.20, 1.0)
PINK   = (0.90, 0.55, 0.75, 1.0)
TEAL   = (0.10, 0.65, 0.60, 1.0)
WHITE  = (0.95, 0.92, 1.00, 1.0)
DARK   = (0.12, 0.05, 0.22, 1.0)
ORANGE = (0.90, 0.45, 0.10, 1.0)
ICE    = (0.70, 0.85, 1.00, 1.0)

def clear():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()

def mat(name, c, emit=0.0):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    n = m.node_tree.nodes; n.clear()
    o  = n.new("ShaderNodeOutputMaterial")
    d  = n.new("ShaderNodeBsdfDiffuse"); d.inputs["Color"].default_value = c
    e  = n.new("ShaderNodeEmission");    e.inputs["Color"].default_value = c
    e.inputs["Strength"].default_value = emit
    mx = n.new("ShaderNodeMixShader");   mx.inputs["Fac"].default_value = min(emit * 0.25, 1.0)
    m.node_tree.links.new(d.outputs["BSDF"], mx.inputs[1])
    m.node_tree.links.new(e.outputs["Emission"], mx.inputs[2])
    m.node_tree.links.new(mx.outputs["Shader"], o.inputs["Surface"])
    return m

def cyl(loc, r, d, v=8, name="Cy"):
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=True, segments=v, radius1=r, radius2=r, depth=d)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob); ob.location = loc; return ob

def sphere(loc, r, name="S"):
    bm = bmesh.new(); bmesh.ops.create_icosphere(bm, radius=r, subdivisions=1)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob); ob.location = loc; return ob

def box(loc, s, name="B"):
    bm = bmesh.new(); bmesh.ops.create_cube(bm, size=1.0)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(ob); ob.location = loc; ob.scale = s
    bpy.context.view_layer.objects.active = ob; ob.select_set(True)
    bpy.ops.object.transform_apply(scale=True); return ob

def assign(ob, m):
    if ob.data.materials: ob.data.materials[0] = m
    else: ob.data.materials.append(m)

def join_export(name):
    bpy.ops.object.select_all(action="SELECT")
    bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
    bpy.ops.object.join()
    path = os.path.join(OUT, f"{name}.fbx")
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True,
        mesh_smooth_type="FACE", add_leaf_bones=False, axis_forward="-Z", axis_up="Y")
    print(f"  → {path}")

# ── 0: Moonlit Bow (default) ──────────────────────────────────────────────────
def outfit_bow():
    clear()
    bow_l = box((-0.28, 0, 1.32), (0.14, 0.06, 0.10), "BowL")
    bow_r = box(( 0.28, 0, 1.32), (0.14, 0.06, 0.10), "BowR")
    knot  = sphere((0, 0, 1.32), 0.04, "Knot")
    m = mat("Bow", PINK)
    assign(bow_l, m); assign(bow_r, m); assign(knot, m)
    join_export("MoonlitBow")

# ── 1: Star Crown ─────────────────────────────────────────────────────────────
def outfit_star_crown():
    clear()
    base = cyl((0, 0, 1.56), 0.20, 0.08, v=8, name="CrownBase")
    assign(base, mat("CrownBase", GOLD))
    for i in range(5):
        angle = math.radians(i * 72)
        px = math.cos(angle) * 0.18; py = math.sin(angle) * 0.18
        peak = sphere((px, py, 1.67), 0.05, name=f"Pt{i}")
        assign(peak, mat(f"Pt{i}", GOLD, emit=0.8))
    join_export("StarCrown")

# ── 2: Purple Cape ────────────────────────────────────────────────────────────
def outfit_cape():
    clear()
    cape = box((0, 0.12, 0.65), (0.80, 0.04, 0.90), "Cape")
    assign(cape, mat("Cape", PURPLE))
    collar = cyl((0, 0, 1.02), 0.22, 0.06, v=10, name="Collar")
    assign(collar, mat("Collar", DARK))
    clasp  = sphere((0, -0.12, 1.01), 0.06, "Clasp")
    assign(clasp, mat("Clasp", GOLD, emit=0.5))
    join_export("PurpleCape")

# ── 3: Golden Collar ──────────────────────────────────────────────────────────
def outfit_collar():
    clear()
    collar = cyl((0, 0, 0.96), 0.24, 0.07, v=10, name="Collar")
    assign(collar, mat("Collar", GOLD))
    gem    = sphere((0, -0.23, 0.97), 0.05, "Gem")
    assign(gem, mat("Gem", (0.6, 0.3, 1.0, 1.0), emit=1.2))
    join_export("GoldenCollar")

# ── 4: Witch Hat ──────────────────────────────────────────────────────────────
def outfit_witch_hat():
    clear()
    brim = cyl((0, 0, 1.56), 0.35, 0.04, v=10, name="Brim")
    assign(brim, mat("Brim", DARK))
    cone_hat = bpy.data.meshes.new("HatCone")
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=True, segments=8, radius1=0.18, radius2=0.01, depth=0.55)
    bm.to_mesh(cone_hat); bm.free()
    hat = bpy.data.objects.new("HatCone", cone_hat)
    bpy.context.collection.objects.link(hat); hat.location = (0, 0, 1.86)
    assign(hat, mat("Hat", DARK))
    buckle = box((0, -0.17, 1.63), (0.08, 0.02, 0.06), "Buckle")
    assign(buckle, mat("Buckle", GOLD))
    star   = sphere((0.10, -0.18, 1.63), 0.03, "StarDec")
    assign(star, mat("StarDec", ORANGE, emit=0.8))
    join_export("WitchHat")

# ── 5: Night Sky Cloak ────────────────────────────────────────────────────────
def outfit_night_cloak():
    clear()
    cloak = box((0, 0.15, 0.60), (0.90, 0.04, 1.10), "Cloak")
    assign(cloak, mat("Cloak", DARK))
    # Star speckles on cloak
    for i in range(8):
        px = (i % 3 - 1) * 0.22; pz = 0.30 + (i // 3) * 0.28
        sp = sphere((px, 0.13, pz), 0.025, f"CloakStar{i}")
        assign(sp, mat(f"CStar{i}", WHITE, emit=1.5))
    moon_patch = sphere((0.28, 0.13, 0.88), 0.06, "MoonPatch")
    assign(moon_patch, mat("MoonPatch", GOLD, emit=1.0))
    join_export("NightSkyCloak")

# ── 6: Fairy Wings ────────────────────────────────────────────────────────────
def outfit_fairy_wings():
    clear()
    for dx, angle in [(-1, 30), (1, -30)]:
        wing = box((dx * 0.55, 0.05, 0.78), (0.45, 0.03, 0.50), "Wing")
        wing.rotation_euler[2] = math.radians(angle)
        assign(wing, mat("Wing", (0.80, 0.70, 1.00, 1.0), emit=0.5))
        # Wing veins
        for j in range(3):
            vein = box((dx * (0.42 + j * 0.05), 0.04, 0.65 + j * 0.12), (0.02, 0.02, 0.28), f"Vein{dx}{j}")
            assign(vein, mat(f"Vein{dx}{j}", WHITE, emit=0.3))
    join_export("FairyWings")

# ── 7: Lunar Armor ────────────────────────────────────────────────────────────
def outfit_lunar_armor():
    clear()
    chest  = box((0, -0.12, 0.65), (0.46, 0.06, 0.42), "Chest")
    assign(chest, mat("Chest", TEAL))
    pauldron_l = box((-0.28, 0, 0.82), (0.16, 0.14, 0.18), "PaulL")
    pauldron_r = box(( 0.28, 0, 0.82), (0.16, 0.14, 0.18), "PaulR")
    pm = mat("Pauldron", TEAL)
    assign(pauldron_l, pm); assign(pauldron_r, pm)
    moon_sigil = sphere((0, -0.16, 0.72), 0.07, "Sigil")
    assign(moon_sigil, mat("Sigil", GOLD, emit=1.5))
    crown  = cyl((0, 0, 1.56), 0.20, 0.09, v=8, name="ArmorCrown")
    assign(crown, mat("ArmorCrown", TEAL))
    for i in range(4):
        ang = math.radians(i * 90 + 45)
        spike = sphere((math.cos(ang) * 0.17, math.sin(ang) * 0.17, 1.67), 0.04, f"Spike{i}")
        assign(spike, mat(f"Spike{i}", GOLD, emit=0.6))
    join_export("LunarArmor")

# ── Seasonal exclusive outfits ────────────────────────────────────────────────
def outfit_witch_hat_seasonal():   outfit_witch_hat()   # reuse
def outfit_snowflake_crown():
    clear()
    base = cyl((0, 0, 1.56), 0.18, 0.07, v=6, name="SnowBase")
    assign(base, mat("SnowBase", ICE))
    for i in range(6):
        ang = math.radians(i * 60)
        flake = box((math.cos(ang)*0.22, math.sin(ang)*0.22, 1.62), (0.18, 0.02, 0.04), f"Flake{i}")
        flake.rotation_euler[2] = ang
        assign(flake, mat(f"Flake{i}", ICE, emit=0.6))
    join_export("SnowflakeCrown")

def outfit_flower_wreath():
    clear()
    ring = cyl((0, 0, 1.56), 0.22, 0.04, v=12, name="Ring")
    assign(ring, mat("Ring", (0.30, 0.55, 0.20, 1.0)))
    colors = [PINK, (0.95, 0.80, 0.20, 1.0), WHITE, (0.90, 0.40, 0.60, 1.0)]
    for i in range(8):
        ang = math.radians(i * 45)
        fl  = sphere((math.cos(ang)*0.22, math.sin(ang)*0.22, 1.60), 0.05, f"Fl{i}")
        assign(fl, mat(f"Fl{i}", colors[i % 4], emit=0.2))
    join_export("FlowerWreath")

def outfit_starfire_cape():
    clear()
    cape = box((0, 0.14, 0.62), (0.88, 0.04, 1.05), "FireCape")
    assign(cape, mat("FireCape", (0.80, 0.20, 0.10, 1.0)))
    for i in range(6):
        st = sphere(((i % 3 - 1) * 0.25, 0.13, 0.40 + (i // 3) * 0.35), 0.04, f"FireStar{i}")
        assign(st, mat(f"FStar{i}", ORANGE, emit=2.0))
    join_export("StarfireCape")

if __name__ == "__main__":
    jobs = [
        ("MoonlitBow",       outfit_bow),
        ("StarCrown",        outfit_star_crown),
        ("PurpleCape",       outfit_cape),
        ("GoldenCollar",     outfit_collar),
        ("WitchHat",         outfit_witch_hat),
        ("NightSkyCloak",    outfit_night_cloak),
        ("FairyWings",       outfit_fairy_wings),
        ("LunarArmor",       outfit_lunar_armor),
        ("SnowflakeCrown",   outfit_snowflake_crown),
        ("FlowerWreath",     outfit_flower_wreath),
        ("StarfireCape",     outfit_starfire_cape),
    ]
    for name, fn in jobs:
        print(f"  Building {name}…")
        fn()
    print("\n✅ All 11 outfit accessories exported.")
