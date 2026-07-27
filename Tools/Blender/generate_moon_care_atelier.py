#!/usr/bin/env python3
"""Generate the authored Moonlight Magic House care atelier FBX."""

import argparse
import math
import os
import sys

import bpy
from mathutils import Vector


ROOT_NAME = "MoonCareAtelier"
ROOT = None


def parse_args():
    argv = sys.argv
    argv = argv[argv.index("--") + 1 :] if "--" in argv else []
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", required=True, help="Destination .fbx path")
    parser.add_argument(
        "--save-blend",
        help="Optional diagnostic .blend path; not required by Unity",
    )
    return parser.parse_args(argv)


def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (
        bpy.data.meshes,
        bpy.data.curves,
        bpy.data.materials,
        bpy.data.cameras,
        bpy.data.lights,
    ):
        for datablock in list(datablocks):
            if datablock.users == 0:
                datablocks.remove(datablock)


def material(name, color, roughness=0.46, metallic=0.0):
    mat = bpy.data.materials.new(name=name)
    mat.diffuse_color = (*color, 1.0)
    mat.use_nodes = True
    shader = mat.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*color, 1.0)
    shader.inputs["Roughness"].default_value = roughness
    shader.inputs["Metallic"].default_value = metallic
    return mat


def assign_material(obj, mat):
    obj.data.materials.append(mat)


def parent_to_root(obj):
    if ROOT is not None:
        obj.parent = ROOT
    return obj


def finish_mesh(obj, mat, bevel=0.0, smooth=False):
    assign_material(obj, mat)
    if smooth:
        for polygon in obj.data.polygons:
            polygon.use_smooth = True
    if bevel > 0.0:
        modifier = obj.modifiers.new(name="Soft bevel", type="BEVEL")
        modifier.width = bevel
        modifier.segments = 3
        modifier.limit_method = "ANGLE"
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    return parent_to_root(obj)


def box(name, location, dimensions, mat, bevel=0.04):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish_mesh(obj, mat, bevel=bevel)


def cylinder(
    name,
    location,
    radius,
    depth,
    mat,
    rotation=(0.0, 0.0, 0.0),
    vertices=32,
    bevel=0.025,
    smooth=True,
):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices,
        radius=radius,
        depth=depth,
        location=location,
        rotation=rotation,
    )
    obj = bpy.context.object
    obj.name = name
    return finish_mesh(obj, mat, bevel=bevel, smooth=smooth)


def sphere(name, location, scale, mat):
    bpy.ops.mesh.primitive_uv_sphere_add(
        segments=40,
        ring_count=20,
        location=location,
    )
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish_mesh(obj, mat, smooth=True)


def torus(
    name,
    location,
    major_radius,
    minor_radius,
    mat,
    rotation=(0.0, 0.0, 0.0),
):
    bpy.ops.mesh.primitive_torus_add(
        major_segments=48,
        minor_segments=16,
        location=location,
        rotation=rotation,
        major_radius=major_radius,
        minor_radius=minor_radius,
    )
    obj = bpy.context.object
    obj.name = name
    return finish_mesh(obj, mat, smooth=True)


def capsule_between(name, start, end, radius, mat):
    start_v = Vector(start)
    end_v = Vector(end)
    midpoint = (start_v + end_v) * 0.5
    direction = end_v - start_v
    obj = cylinder(
        name,
        midpoint,
        radius,
        direction.length,
        mat,
        bevel=radius * 0.45,
    )
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = Vector((0.0, 0.0, 1.0)).rotation_difference(direction)
    return obj


def crescent_points(radius, cut_radius, cut_offset, steps=72):
    intersection_x = (
        radius * radius - cut_radius * cut_radius + cut_offset * cut_offset
    ) / (2.0 * cut_offset)
    intersection_z = math.sqrt(max(0.0, radius * radius - intersection_x**2))
    outer_angle = math.atan2(intersection_z, intersection_x)
    inner_angle = math.atan2(intersection_z, intersection_x - cut_offset)

    points = []
    outer_span = (2.0 * math.pi - outer_angle) - outer_angle
    for index in range(steps + 1):
        angle = outer_angle + outer_span * index / steps
        points.append((radius * math.cos(angle), radius * math.sin(angle)))
    inner_span = 2.0 * math.pi - 2.0 * inner_angle
    for index in range(steps // 2 + 1):
        angle = -inner_angle - inner_span * index / (steps // 2)
        points.append(
            (
                cut_offset + cut_radius * math.cos(angle),
                cut_radius * math.sin(angle),
            )
        )
    return points


def extruded_profile(name, points, location, depth, mat, scale=1.0, bevel=0.018):
    count = len(points)
    vertices = []
    for y in (-depth * 0.5, depth * 0.5):
        vertices.extend((x * scale, y, z * scale) for x, z in points)

    faces = [tuple(range(count)), tuple(reversed(range(count, count * 2)))]
    for index in range(count):
        following = (index + 1) % count
        faces.append((index, following, count + following, count + index))

    mesh = bpy.data.meshes.new(name=f"{name}Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.location = location
    obj = finish_mesh(obj, mat, bevel=bevel)
    triangulate = obj.modifiers.new(name="Unity-safe triangulation", type="TRIANGULATE")
    triangulate.keep_custom_normals = True
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=triangulate.name)
    return obj


def create_vanity(mats):
    box(
        "AtelierPlinth",
        (0.0, 0.06, 0.09),
        (3.35, 1.10, 0.18),
        mats["lilac_dark"],
        bevel=0.08,
    )
    box(
        "VanityBody",
        (0.0, 0.12, 0.69),
        (3.10, 0.94, 1.14),
        mats["cream"],
        bevel=0.10,
    )
    box(
        "VanityTop",
        (0.0, -0.02, 1.36),
        (3.42, 1.16, 0.20),
        mats["peach"],
        bevel=0.09,
    )

    for side in (-1.0, 1.0):
        box(
            f"DrawerFront_{'L' if side < 0 else 'R'}_Top",
            (side * 1.02, -0.385, 0.93),
            (0.82, 0.08, 0.38),
            mats["rose"],
            bevel=0.045,
        )
        box(
            f"DrawerFront_{'L' if side < 0 else 'R'}_Bottom",
            (side * 1.02, -0.385, 0.48),
            (0.82, 0.08, 0.38),
            mats["sage"],
            bevel=0.045,
        )
        for z in (0.48, 0.93):
            sphere(
                f"DrawerKnob_{'L' if side < 0 else 'R'}_{int(z * 100)}",
                (side * 1.02, -0.455, z),
                (0.075, 0.045, 0.075),
                mats["gold"],
            )

    box(
        "VanityCenterInset",
        (0.0, -0.395, 0.69),
        (0.78, 0.065, 0.82),
        mats["lilac"],
        bevel=0.05,
    )
    extruded_profile(
        "VanityMoonEmblem",
        crescent_points(0.24, 0.20, 0.08, steps=40),
        (0.0, -0.445, 0.69),
        0.045,
        mats["gold"],
        bevel=0.008,
    )


def create_basin_and_faucet(mats):
    sphere(
        "CareBasinBowl",
        (0.0, -0.06, 1.51),
        (0.78, 0.47, 0.19),
        mats["porcelain"],
    )
    torus(
        "CareBasinRim",
        (0.0, -0.06, 1.63),
        0.63,
        0.075,
        mats["porcelain"],
    )
    cylinder(
        "CareBasinInterior",
        (0.0, -0.06, 1.645),
        0.53,
        0.024,
        mats["water"],
        bevel=0.012,
    )
    cylinder(
        "CareBasinDrain",
        (0.0, -0.06, 1.666),
        0.085,
        0.018,
        mats["silver"],
        bevel=0.008,
    )

    cylinder(
        "FaucetBase",
        (0.0, 0.30, 1.61),
        0.14,
        0.15,
        mats["gold"],
        bevel=0.035,
    )
    capsule_between(
        "FaucetStem",
        (0.0, 0.30, 1.67),
        (0.0, 0.30, 1.98),
        0.075,
        mats["gold"],
    )
    capsule_between(
        "FaucetSpout",
        (0.0, 0.30, 1.96),
        (0.0, 0.00, 1.96),
        0.075,
        mats["gold"],
    )
    sphere(
        "FaucetSpoutCap",
        (0.0, -0.015, 1.95),
        (0.085, 0.085, 0.085),
        mats["gold"],
    )
    for side in (-1.0, 1.0):
        cylinder(
            f"FaucetHandle_{'L' if side < 0 else 'R'}",
            (side * 0.25, 0.29, 1.65),
            0.055,
            0.26,
            mats["gold"],
            rotation=(0.0, math.pi * 0.5, 0.0),
            bevel=0.02,
        )
        sphere(
            f"FaucetHandleTip_{'L' if side < 0 else 'R'}",
            (side * 0.39, 0.29, 1.65),
            (0.07, 0.07, 0.07),
            mats["rose"],
        )


def create_crescent_mirror(mats):
    cylinder(
        "MoonMirrorGlass",
        (0.0, 0.30, 2.46),
        0.64,
        0.045,
        mats["mirror"],
        rotation=(math.pi * 0.5, 0.0, 0.0),
        vertices=64,
        bevel=0.018,
    )
    torus(
        "MoonMirrorFrame",
        (0.0, 0.30, 2.46),
        0.67,
        0.065,
        mats["gold"],
        rotation=(math.pi * 0.5, 0.0, 0.0),
    )
    box(
        "MirrorSupportLeft",
        (-0.43, 0.36, 1.93),
        (0.10, 0.10, 0.52),
        mats["gold"],
        bevel=0.04,
    )
    box(
        "MirrorSupportRight",
        (0.43, 0.36, 1.93),
        (0.10, 0.10, 0.52),
        mats["gold"],
        bevel=0.04,
    )

    for index, (x, z, scale) in enumerate(
        ((-0.91, 2.66, 0.075), (0.84, 2.84, 0.065), (0.94, 2.39, 0.05))
    ):
        sphere(
            f"MirrorStar_{index + 1}",
            (x, 0.24, z),
            (scale, 0.035, scale),
            mats["gold"],
        )


def create_towel_tray(mats):
    box(
        "TowelTray",
        (1.08, -0.27, 1.58),
        (0.92, 0.50, 0.08),
        mats["gold"],
        bevel=0.035,
    )
    for index, (z, color) in enumerate(
        ((1.68, mats["sage"]), (1.79, mats["lilac"]), (1.90, mats["rose"]))
    ):
        cylinder(
            f"RolledTowel_{index + 1}",
            (1.08, -0.27, z),
            0.13,
            0.68,
            color,
            rotation=(0.0, math.pi * 0.5, 0.0),
            bevel=0.035,
        )
        torus(
            f"TowelBand_{index + 1}",
            (1.08, -0.27, z),
            0.135,
            0.018,
            mats["cream"],
            rotation=(0.0, math.pi * 0.5, 0.0),
        )


def create_brush_and_comb_area(mats):
    box(
        "CareToolsRest",
        (-1.08, -0.28, 1.57),
        (0.94, 0.48, 0.07),
        mats["sage_dark"],
        bevel=0.04,
    )

    capsule_between(
        "CareBrushHandle",
        (-1.39, -0.36, 1.65),
        (-0.94, -0.36, 1.65),
        0.055,
        mats["lilac_dark"],
    )
    sphere(
        "CareBrushHead",
        (-0.83, -0.36, 1.65),
        (0.20, 0.10, 0.13),
        mats["rose"],
    )
    for index in range(5):
        cylinder(
            f"BrushBristle_{index + 1}",
            (-0.91 + index * 0.045, -0.465, 1.65),
            0.012,
            0.07,
            mats["cream"],
            rotation=(math.pi * 0.5, 0.0, 0.0),
            vertices=12,
            bevel=0.006,
        )

    box(
        "CareCombSpine",
        (-1.10, -0.18, 1.69),
        (0.58, 0.08, 0.08),
        mats["gold"],
        bevel=0.025,
    )
    for index in range(9):
        box(
            f"CareCombTooth_{index + 1}",
            (-1.34 + index * 0.06, -0.18, 1.62),
            (0.025, 0.065, 0.16),
            mats["gold"],
            bevel=0.008,
        )

    cylinder(
        "CareCreamJar",
        (-1.45, 0.03, 1.65),
        0.14,
        0.15,
        mats["porcelain"],
        bevel=0.035,
    )
    cylinder(
        "CareCreamJarLid",
        (-1.45, 0.03, 1.75),
        0.15,
        0.05,
        mats["rose"],
        bevel=0.02,
    )


def create_side_accents(mats):
    for side, accent in ((-1.0, mats["rose"]), (1.0, mats["sage"])):
        cylinder(
            f"VanitySideFoot_{'L' if side < 0 else 'R'}",
            (side * 1.42, 0.08, 0.20),
            0.16,
            0.24,
            accent,
            bevel=0.05,
        )
        sphere(
            f"VanitySideOrb_{'L' if side < 0 else 'R'}",
            (side * 1.58, -0.30, 1.46),
            (0.12, 0.10, 0.12),
            mats["gold"],
        )


def consolidate_meshes_by_material(root):
    groups = {}
    for obj in list(root.children):
        if obj.type != "MESH":
            continue
        mat = obj.data.materials[0] if obj.data.materials else None
        key = mat.name if mat is not None else "Unassigned"
        groups.setdefault(key, []).append(obj)

    for material_name, objects in groups.items():
        bpy.ops.object.select_all(action="DESELECT")
        for obj in objects:
            obj.select_set(True)
        active = objects[0]
        bpy.context.view_layer.objects.active = active
        if len(objects) > 1:
            bpy.ops.object.join()
        active.name = f"AtelierGroup_{material_name}"


def build_scene():
    global ROOT
    reset_scene()

    ROOT = bpy.data.objects.new(ROOT_NAME, None)
    bpy.context.collection.objects.link(ROOT)
    ROOT.empty_display_type = "PLAIN_AXES"
    ROOT["authored_resource"] = "Models/Hero/MoonCareAtelier"
    ROOT["front_direction"] = "-Y"
    ROOT["ground_plane_z"] = 0.0
    ROOT["contains_lights"] = False
    ROOT["contains_cameras"] = False
    ROOT["contains_colliders"] = False

    mats = {
        "cream": material("Atelier_Cream", (0.93, 0.84, 0.72), roughness=0.58),
        "peach": material("Atelier_Peach", (0.86, 0.57, 0.54), roughness=0.52),
        "rose": material("Atelier_Rose", (0.78, 0.42, 0.55), roughness=0.52),
        "lilac": material("Atelier_Lilac", (0.60, 0.45, 0.76), roughness=0.50),
        "lilac_dark": material(
            "Atelier_LilacDark", (0.36, 0.24, 0.51), roughness=0.48
        ),
        "sage": material("Atelier_Sage", (0.47, 0.67, 0.61), roughness=0.55),
        "sage_dark": material(
            "Atelier_SageDark", (0.25, 0.47, 0.43), roughness=0.55
        ),
        "porcelain": material(
            "Atelier_Porcelain", (0.92, 0.90, 0.86), roughness=0.25
        ),
        "water": material("Atelier_Water", (0.36, 0.71, 0.76), roughness=0.18),
        "gold": material(
            "Atelier_MoonGold", (0.92, 0.66, 0.25), roughness=0.28, metallic=0.35
        ),
        "silver": material(
            "Atelier_Silver", (0.68, 0.74, 0.78), roughness=0.24, metallic=0.55
        ),
        "mirror": material(
            "Atelier_Mirror", (0.34, 0.53, 0.68), roughness=0.12, metallic=0.45
        ),
    }

    create_vanity(mats)
    create_basin_and_faucet(mats)
    create_crescent_mirror(mats)
    create_towel_tray(mats)
    create_brush_and_comb_area(mats)
    create_side_accents(mats)
    consolidate_meshes_by_material(ROOT)

    ROOT["mesh_count"] = len([obj for obj in ROOT.children if obj.type == "MESH"])
    return ROOT


def validate_scene(root):
    forbidden = [
        obj.name for obj in bpy.context.scene.objects if obj.type in {"LIGHT", "CAMERA"}
    ]
    if forbidden:
        raise RuntimeError(f"Forbidden scene objects found: {forbidden}")
    if not root.children:
        raise RuntimeError("MoonCareAtelier has no authored children")

    minimum = Vector((-1.8, -0.85, -0.01))
    maximum = Vector((1.8, 0.85, 3.5))
    for obj in root.children:
        if obj.type != "MESH":
            continue
        for corner in obj.bound_box:
            world = obj.matrix_world @ Vector(corner)
            if any(world[i] < minimum[i] or world[i] > maximum[i] for i in range(3)):
                raise RuntimeError(
                    f"{obj.name} exceeds the authored station bounds at {tuple(world)}"
                )


def export_fbx(output_path, root):
    output_path = os.path.abspath(output_path)
    if not output_path.lower().endswith(".fbx"):
        raise ValueError("--output must end in .fbx")
    os.makedirs(os.path.dirname(output_path), exist_ok=True)

    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for obj in root.children_recursive:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root

    bpy.ops.export_scene.fbx(
        filepath=output_path,
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        use_custom_props=True,
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO",
        axis_forward="-Z",
        axis_up="Y",
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        use_space_transform=True,
    )
    print(f"MOON_CARE_ATELIER_EXPORTED={output_path}")
    print(f"MOON_CARE_ATELIER_MESHES={root['mesh_count']}")


def main():
    args = parse_args()
    root = build_scene()
    validate_scene(root)
    if args.save_blend:
        blend_path = os.path.abspath(args.save_blend)
        os.makedirs(os.path.dirname(blend_path), exist_ok=True)
        bpy.ops.wm.save_as_mainfile(filepath=blend_path)
        print(f"MOON_CARE_ATELIER_BLEND={blend_path}")
    export_fbx(args.output, root)


if __name__ == "__main__":
    main()
