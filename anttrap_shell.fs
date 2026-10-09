FeatureScript 3083;
import(path : "onshape/std/common.fs", version : "3083.0");

// Ants walk in through a ground-level door in the +X end, tucked into the corner so the trap can
// hug a wall, then climb a walled stairwell over a spill dam and down into the tray. Liquid in the
// tray can't reach the door until it tops the dam.
// Pet-safe enclosure for a TERRO T300 liquid ant bait station (measured 4.150 x 1.305 x 0.440 in;
// clearance default is generous because the stations are mass-molded and vary).
// Base tray (floor on z = 0) with corner stops that locate the station (floor-level slots are
// optional and off by default since they leak); a lid snaps down into the top of the walls with the same flex-finger /
// wedge-bump snap as the cyberdeck cover. Only a flat blade in the coin slot pops it open.

const LB_STATION_L = { (inch) : [0.5, 4.15, 12] } as LengthBoundSpec;
const LB_STATION_W = { (inch) : [0.5, 1.305, 12] } as LengthBoundSpec;
const LB_STATION_H = { (inch) : [0.1, 0.44, 4] } as LengthBoundSpec;
const LB_STATION_CLEAR = { (inch) : [0, 0.063, 0.25] } as LengthBoundSpec;
const LB_ALLEY = { (inch) : [0.05, 0.25, 2] } as LengthBoundSpec;
const LB_HEADROOM = { (inch) : [0, 0.245, 2] } as LengthBoundSpec;
const LB_WALL = { (inch) : [0.04, 0.1, 0.5] } as LengthBoundSpec;
const LB_FLOOR = { (inch) : [0.02, 0.08, 0.5] } as LengthBoundSpec;
const LB_CORNER_R = { (inch) : [0, 0.15, 1] } as LengthBoundSpec;
const LB_TOP_FILLET = { (inch) : [0, 0.05, 0.25] } as LengthBoundSpec;
const LB_SLOT_W = { (inch) : [0.05, 0.25, 1] } as LengthBoundSpec;
const LB_SLOT_H = { (inch) : [0.03, 0.12, 0.5] } as LengthBoundSpec;
const LB_SLOT_SILL = { (inch) : [0, 0.04, 0.5] } as LengthBoundSpec;
const LB_STOP_H = { (inch) : [0.02, 0.15, 1] } as LengthBoundSpec;
const LB_LID_PLATE = { (inch) : [0.02, 0.08, 0.5] } as LengthBoundSpec;
const LB_LID_OFFSET = { (inch) : [0, 0.5, 12] } as LengthBoundSpec;
const LB_SNAP_CLEAR = { (inch) : [0, 0.008, 0.1] } as LengthBoundSpec;
const LB_SNAP_BUMP = { (inch) : [0.005, 0.025, 0.1] } as LengthBoundSpec;
const LB_EAR_LEN = { (inch) : [0.2, 0.4, 2] } as LengthBoundSpec;
const LB_EAR_HOLE = { (inch) : [0.05, 0.17, 0.5] } as LengthBoundSpec;
const LB_ENTRY_W = { (inch) : [0.05, 0.18, 1] } as LengthBoundSpec;
const LB_ENTRY_H = { (inch) : [0.05, 0.2, 1] } as LengthBoundSpec;
const LB_ENTRY_SILL = { (inch) : [0.02, 0.25, 2] } as LengthBoundSpec;
const AB_RAMP_ANGLE = { (degree) : [15, 40, 80] } as AngleBoundSpec;

// Snap-fit lip/finger geometry, shared by the lid and the base grooves.
const SNAP_LIP_T = 0.06 * inch;
const SNAP_LIP_H = 0.25 * inch;
const SNAP_FINGER_W = 0.4 * inch;
const SNAP_RELIEF_W = 0.04 * inch;
const SNAP_BUMP_H = 0.07 * inch;

// Corner stop geometry: L-shaped posts hugging each station corner.
const STOP_ARM = 0.2 * inch;
const STOP_T = 0.06 * inch;

// Wall between the stairwell and the tray, so spills can't run down the stairs to the door.
const DAM_T = 0.06 * inch;

// Coin slot in the lid edge: needs a flat blade, too narrow for a paw or muzzle.
const COIN_SLOT_W = 0.5 * inch;
const COIN_SLOT_D = 0.04 * inch;

// Axis-aligned box cut from zTop downward by depth.
function cutBox(context is Context, id is Id, body is Query, x0 is ValueWithUnits, x1 is ValueWithUnits,
    y0 is ValueWithUnits, y1 is ValueWithUnits, zTop is ValueWithUnits, depth is ValueWithUnits)
{
    const s = newSketchOnPlane(context, id + "sketch", { "sketchPlane" : plane(vector(0 * inch, 0 * inch, zTop), vector(0, 0, 1)) });
    skRectangle(s, "rect", { "firstCorner" : vector(x0, y0), "secondCorner" : vector(x1, y1) });
    skSolve(s);

    opExtrude(context, id + "extrude", {
        "entities" : qSketchRegion(id + "sketch"),
        "direction" : vector(0, 0, -1),
        "endBound" : BoundingType.BLIND,
        "endDepth" : depth
    });

    opBoolean(context, id + "cut", {
        "tools" : qCreatedBy(id + "extrude", EntityType.BODY),
        "targets" : body,
        "operationType" : BooleanOperationType.SUBTRACTION
    });
}

// Axis-aligned box grown from zBottom upward by height and unioned into body.
function addBox(context is Context, id is Id, body is Query, x0 is ValueWithUnits, x1 is ValueWithUnits,
    y0 is ValueWithUnits, y1 is ValueWithUnits, zBottom is ValueWithUnits, height is ValueWithUnits)
{
    const s = newSketchOnPlane(context, id + "sketch", { "sketchPlane" : plane(vector(0 * inch, 0 * inch, zBottom), vector(0, 0, 1)) });
    skRectangle(s, "rect", { "firstCorner" : vector(x0, y0), "secondCorner" : vector(x1, y1) });
    skSolve(s);

    opExtrude(context, id + "extrude", {
        "entities" : qSketchRegion(id + "sketch"),
        "direction" : vector(0, 0, 1),
        "endBound" : BoundingType.BLIND,
        "endDepth" : height
    });

    opBoolean(context, id + "join", {
        "tools" : qUnion([body, qCreatedBy(id + "extrude", EntityType.BODY)]),
        "operationType" : BooleanOperationType.UNION
    });
}

// Wall sides: 0 = y=0 wall, 1 = y=W wall, 2 = x=0 wall, 3 = x=L wall.
// "along" runs parallel to the wall, "nc" is the coordinate across it; sideSign points outward.
function sideSign(side is number) returns number
{
    return (side == 0 || side == 2) ? -1 : 1;
}

function sidePoint(side is number, along is ValueWithUnits, nc is ValueWithUnits, z is ValueWithUnits) returns Vector
{
    return side < 2 ? vector(along, nc, z) : vector(nc, along, z);
}

function sideAlongDir(side is number) returns Vector
{
    return side < 2 ? vector(1, 0, 0) : vector(0, 1, 0);
}

function sideNormalDir(side is number) returns Vector
{
    return side < 2 ? vector(0, sideSign(side), 0) : vector(sideSign(side), 0, 0);
}

function wallFaceNc(side is number, L is ValueWithUnits, W is ValueWithUnits, t is ValueWithUnits) returns ValueWithUnits
{
    if (side == 0 || side == 2)
        return t;
    return side == 1 ? W - t : L - t;
}

function wallOuterNc(side is number, L is ValueWithUnits, W is ValueWithUnits) returns ValueWithUnits
{
    if (side == 0 || side == 2)
        return 0 * inch;
    return side == 1 ? W : L;
}

function cutSideBox(context is Context, id is Id, body is Query, side is number, a0 is ValueWithUnits, a1 is ValueWithUnits,
    n0 is ValueWithUnits, n1 is ValueWithUnits, zTop is ValueWithUnits, depth is ValueWithUnits)
{
    if (side < 2)
        cutBox(context, id, body, a0, a1, n0, n1, zTop, depth);
    else
        cutBox(context, id, body, n0, n1, a0, a1, zTop, depth);
}

function snapSites(L is ValueWithUnits, W is ValueWithUnits) returns array
{
    return [
        { "side" : 0, "pos" : L * 0.25 }, { "side" : 0, "pos" : L * 0.75 },
        { "side" : 1, "pos" : L * 0.25 }, { "side" : 1, "pos" : L * 0.75 },
        { "side" : 2, "pos" : W / 2 }, { "side" : 3, "pos" : W / 2 }
    ];
}

// Long-side slots sit between the snap fingers so no wall section carries both a slot and a groove.
function slotSites(L is ValueWithUnits, W is ValueWithUnits) returns array
{
    return [
        { "side" : 0, "pos" : L * 0.12 }, { "side" : 0, "pos" : L / 2 }, { "side" : 0, "pos" : L * 0.88 },
        { "side" : 1, "pos" : L * 0.12 }, { "side" : 1, "pos" : L / 2 }, { "side" : 1, "pos" : L * 0.88 },
        { "side" : 2, "pos" : W / 2 }, { "side" : 3, "pos" : W / 2 }
    ];
}

// skipCorner: 0 = (0,0), 1 = (L,0), 2 = (0,W), 3 = (L,W), -1 = fillet all four.
function filletVerticalCorners(context is Context, id is Id, body is Query, L is ValueWithUnits, W is ValueWithUnits,
    z is ValueWithUnits, r is ValueWithUnits, skipCorner is number)
{
    if (r <= 0 * inch)
        return;
    const edges = qOwnedByBody(body, EntityType.EDGE);
    const pts = [vector(0 * inch, 0 * inch, z), vector(L, 0 * inch, z), vector(0 * inch, W, z), vector(L, W, z)];
    var picked = [];
    for (var i = 0; i < 4; i += 1)
    {
        if (i != skipCorner)
            picked = append(picked, qContainsPoint(edges, pts[i]));
    }
    opFillet(context, id, { "entities" : qUnion(picked), "radius" : r });
}

function filletPerimeter(context is Context, id is Id, body is Query, L is ValueWithUnits, W is ValueWithUnits,
    z is ValueWithUnits, r is ValueWithUnits)
{
    if (r <= 0 * inch)
        return;
    const edges = qOwnedByBody(body, EntityType.EDGE);
    opFillet(context, id, {
        "entities" : qUnion([
            qContainsPoint(edges, vector(L / 2, 0 * inch, z)), qContainsPoint(edges, vector(L / 2, W, z)),
            qContainsPoint(edges, vector(0 * inch, W / 2, z)), qContainsPoint(edges, vector(L, W / 2, z))
        ]),
        "radius" : r,
        "tangentPropagation" : true
    });
}

// Screw ear on one short end: rectangle + round tip, with a through hole at the tip center.
function addEar(context is Context, id is Id, body is Query, xWall is ValueWithUnits, dir is number,
    yc is ValueWithUnits, earLen is ValueWithUnits, earR is ValueWithUnits, earT is ValueWithUnits, holeD is ValueWithUnits)
{
    const tipX = xWall + dir * earLen;
    const s = newSketchOnPlane(context, id + "sketch", { "sketchPlane" : plane(vector(0, 0, 0) * inch, vector(0, 0, 1)) });
    skRectangle(s, "rect", { "firstCorner" : vector(xWall - dir * 0.05 * inch, yc - earR), "secondCorner" : vector(tipX, yc + earR) });
    skCircle(s, "tip", { "center" : vector(tipX, yc), "radius" : earR });
    skSolve(s);

    opExtrude(context, id + "extrude", {
        "entities" : qSketchRegion(id + "sketch"),
        "direction" : vector(0, 0, 1),
        "endBound" : BoundingType.BLIND,
        "endDepth" : earT
    });

    opBoolean(context, id + "join", {
        "tools" : qUnion([body, qCreatedBy(id + "extrude", EntityType.BODY)]),
        "operationType" : BooleanOperationType.UNION
    });

    const h = newSketchOnPlane(context, id + "holeSketch", { "sketchPlane" : plane(vector(0 * inch, 0 * inch, earT + 0.02 * inch), vector(0, 0, 1)) });
    skCircle(h, "hole", { "center" : vector(tipX, yc), "radius" : holeD / 2 });
    skSolve(h);

    opExtrude(context, id + "holeCut", {
        "entities" : qSketchRegion(id + "holeSketch"),
        "direction" : vector(0, 0, -1),
        "endBound" : BoundingType.BLIND,
        "endDepth" : earT + 0.04 * inch
    });

    opBoolean(context, id + "hole", {
        "tools" : qCreatedBy(id + "holeCut", EntityType.BODY),
        "targets" : body,
        "operationType" : BooleanOperationType.SUBTRACTION
    });
}

annotation { "Feature Type Name" : "Ant Trap Shell" }
export const antTrapShell = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Bait station length (X)" }
        isLength(definition.stationLength, LB_STATION_L);

        annotation { "Name" : "Bait station width (Y)" }
        isLength(definition.stationWidth, LB_STATION_W);

        annotation { "Name" : "Bait station height (Z)" }
        isLength(definition.stationHeight, LB_STATION_H);

        annotation { "Name" : "Station clearance (per side)" }
        isLength(definition.stationClearance, LB_STATION_CLEAR);

        annotation { "Name" : "Ant alley (gap station to wall)" }
        isLength(definition.alley, LB_ALLEY);

        annotation { "Name" : "Headroom above station" }
        isLength(definition.headroom, LB_HEADROOM);

        annotation { "Name" : "Wall thickness" }
        isLength(definition.wallThickness, LB_WALL);

        annotation { "Name" : "Floor thickness" }
        isLength(definition.floorThickness, LB_FLOOR);

        annotation { "Name" : "Outer corner radius (vertical edges)" }
        isLength(definition.cornerRadius, LB_CORNER_R);

        annotation { "Name" : "Lid top edge fillet" }
        isLength(definition.topFillet, LB_TOP_FILLET);

        annotation { "Name" : "Floor-level entry slots (leak path: off keeps spills in)" }
        definition.buildFloorSlots is boolean;

        annotation { "Name" : "Entry slot width" }
        isLength(definition.slotWidth, LB_SLOT_W);

        annotation { "Name" : "Entry slot height (keep ~1/8 in: ants yes, tongues no)" }
        isLength(definition.slotHeight, LB_SLOT_H);

        annotation { "Name" : "Entry slot sill above floor (spill dam)" }
        isLength(definition.slotSill, LB_SLOT_SILL);

        annotation { "Name" : "Corner stop height" }
        isLength(definition.stopHeight, LB_STOP_H);

        annotation { "Name" : "Build corner door + stairwell (+X end)" }
        definition.buildEntry is boolean;

        annotation { "Name" : "Entry on +Y corner instead of -Y" }
        definition.entryOnPlusY is boolean;

        annotation { "Name" : "Door / stairwell width" }
        isLength(definition.entryWidth, LB_ENTRY_W);

        annotation { "Name" : "Door height above ground" }
        isLength(definition.entryHeight, LB_ENTRY_H);

        annotation { "Name" : "Stair top above inside floor (spill dam height)" }
        isLength(definition.entrySill, LB_ENTRY_SILL);

        annotation { "Name" : "Stair angle" }
        isAngle(definition.rampAngle, AB_RAMP_ANGLE);

        annotation { "Name" : "Build snap-fit lid" }
        definition.buildLid is boolean;

        annotation { "Name" : "Lid plate thickness" }
        isLength(definition.lidPlateThickness, LB_LID_PLATE);

        annotation { "Name" : "Lid display gap above base (0 = seated)" }
        isLength(definition.lidOffset, LB_LID_OFFSET);

        annotation { "Name" : "Snap clearance (lip to wall)" }
        isLength(definition.snapClearance, LB_SNAP_CLEAR);

        annotation { "Name" : "Snap bump height" }
        isLength(definition.snapBump, LB_SNAP_BUMP);

        annotation { "Name" : "Screw-down ears on short ends" }
        definition.buildEars is boolean;

        annotation { "Name" : "Ear length past wall" }
        isLength(definition.earLength, LB_EAR_LEN);

        annotation { "Name" : "Ear screw hole diameter" }
        isLength(definition.earHole, LB_EAR_HOLE);
    }
    {
        const t = definition.wallThickness;
        const f = definition.floorThickness;
        const sc = definition.stationClearance;
        const overcut = 0.02 * inch;

        // Everything derives from the measured station: pocket + ant alley + walls.
        const pocketL = definition.stationLength + 2 * sc;
        const pocketW = definition.stationWidth + 2 * sc;
        const L = pocketL + 2 * definition.alley + 2 * t;
        const W = pocketW + 2 * definition.alley + 2 * t;
        const H = f + definition.stationHeight + definition.headroom;
        const slotBottom = f + definition.slotSill;
        const slotTop = slotBottom + definition.slotHeight;

        if (definition.buildFloorSlots && definition.buildLid && H - SNAP_LIP_H < slotTop + 0.05 * inch)
            throw regenError("Lid lip would reach the entry slots: add headroom or lower the slots.");

        // Corner door at ground level in the +X wall, against the inside of the chosen side wall. The
        // stair climbs from the door's outer edge at ground level up to the dam top, then drops to the floor.
        const entrySill = f + definition.entrySill;
        const entryTop = definition.entryHeight;
        const tanA = tan(definition.rampAngle);
        const stairPeakX = L - entrySill / tanA;
        const stairFootX = stairPeakX - definition.entrySill / tanA;
        if (definition.buildEntry && entryTop < t * tanA + 0.08 * inch)
            throw regenError("Door is too low to clear the stair where it passes through the wall.");
        if (definition.buildEntry && entrySill > H - 0.05 * inch)
            throw regenError("Stair top reaches the rim: lower it or add headroom.");
        if (definition.buildEntry && definition.entryWidth + DAM_T > definition.alley + sc)
            throw regenError("Stairwell plus dam is wider than the side alley: narrow the door or widen the alley.");
        const eY0 = definition.entryOnPlusY ? W - t - definition.entryWidth : t;
        const eY1 = eY0 + definition.entryWidth;
        const damY0 = definition.entryOnPlusY ? eY0 - DAM_T : eY1;
        // The entry corner stays square so the trap sits flush against a wall.
        const squareCorner = definition.buildEntry ? (definition.entryOnPlusY ? 3 : 1) : -1;

        const outerSketch = newSketchOnPlane(context, id + "outerSketch", { "sketchPlane" : plane(vector(0, 0, 0) * meter, vector(0, 0, 1)) });
        skRectangle(outerSketch, "outerRect", { "firstCorner" : vector(0, 0) * meter, "secondCorner" : vector(L, W) });
        skSolve(outerSketch);

        opExtrude(context, id + "extrudeOuter", {
            "entities" : qSketchRegion(id + "outerSketch"),
            "direction" : vector(0, 0, 1),
            "endBound" : BoundingType.BLIND,
            "endDepth" : H
        });

        const baseBody = qCreatedBy(id + "extrudeOuter", EntityType.BODY);

        filletVerticalCorners(context, id + "baseCorners", baseBody, L, W, H / 2, definition.cornerRadius, squareCorner);

        // Open-top cavity down to the floor.
        cutBox(context, id + "cavity", baseBody, t, L - t, t, W - t, H + overcut, H - f + overcut);

        // Entry slots at floor level: ~1/8 in tall is plenty for ants, too small for a tongue or paw.
        const slots = definition.buildFloorSlots ? slotSites(L, W) : [];
        for (var i = 0; i < size(slots); i += 1)
        {
            const side = slots[i].side;
            const pos = slots[i].pos;
            const sgn = sideSign(side);
            const outer = wallOuterNc(side, L, W);
            cutSideBox(context, id + ("slot" ~ i), baseBody, side,
                pos - definition.slotWidth / 2, pos + definition.slotWidth / 2,
                outer + sgn * overcut, wallFaceNc(side, L, W, t) - sgn * overcut,
                slotTop, definition.slotHeight);
        }

        // Corner stops: an L at each station corner so it can't slide into the alley or slots.
        const px0 = L / 2 - pocketL / 2;
        const px1 = L / 2 + pocketL / 2;
        const py0 = W / 2 - pocketW / 2;
        const py1 = W / 2 + pocketW / 2;
        const corners = [[px0, py0, 1, 1], [px1, py0, -1, 1], [px0, py1, 1, -1], [px1, py1, -1, -1]];
        for (var i = 0; i < size(corners); i += 1)
        {
            const cx = corners[i][0];
            const cy = corners[i][1];
            const dx = corners[i][2];
            const dy = corners[i][3];
            // Arm along X, outside the pocket edge at y = cy.
            addBox(context, id + ("stopX" ~ i), baseBody,
                min(cx - dx * STOP_T, cx + dx * STOP_ARM), max(cx - dx * STOP_T, cx + dx * STOP_ARM),
                min(cy, cy - dy * STOP_T), max(cy, cy - dy * STOP_T), f - overcut, definition.stopHeight + overcut);
            // Arm along Y, outside the pocket edge at x = cx.
            addBox(context, id + ("stopY" ~ i), baseBody,
                min(cx, cx - dx * STOP_T), max(cx, cx - dx * STOP_T),
                min(cy, cy + dy * STOP_ARM), max(cy, cy + dy * STOP_ARM), f - overcut, definition.stopHeight + overcut);
        }

        if (definition.buildEars)
        {
            const earR = 0.25 * inch;
            addEar(context, id + "earA", baseBody, 0 * inch, -1, W / 2, definition.earLength, earR, f + 0.04 * inch, definition.earHole);
            addEar(context, id + "earB", baseBody, L, 1, W / 2, definition.earLength, earR, f + 0.04 * inch, definition.earHole);
        }

        if (definition.buildEntry)
        {
            cutBox(context, id + "entryHole", baseBody, L - t - overcut, L + overcut, eY0, eY1, entryTop, entryTop + overcut);

            // Stair: up from the door's outer edge at ground level to the dam top, then down to the floor,
            // running along the side alley beside the station so it works whichever way the station sits.
            const inPlane = plane(vector(0 * inch, eY0, 0 * inch), vector(0, -1, 0), vector(1, 0, 0));
            const ins = newSketchOnPlane(context, id + "rampInSketch", { "sketchPlane" : inPlane });
            skPolyline(ins, "stair", { "points" : [
                vector(L, 0 * inch), vector(stairPeakX, entrySill), vector(stairFootX, f - overcut),
                vector(stairFootX, 0 * inch), vector(L, 0 * inch)
            ] });
            skSolve(ins);
            opExtrude(context, id + "rampIn", {
                "entities" : qSketchRegion(id + "rampInSketch"),
                "direction" : vector(0, 1, 0),
                "endBound" : BoundingType.BLIND,
                "endDepth" : definition.entryWidth
            });
            opBoolean(context, id + "rampInJoin", { "tools" : qUnion([baseBody, qCreatedBy(id + "rampIn", EntityType.BODY)]), "operationType" : BooleanOperationType.UNION });

            // Dam along the tray side of the climb, same height as the stair top, from the peak to the end wall.
            addBox(context, id + "stairDam", baseBody, stairPeakX, L - t + overcut, damY0, damY0 + DAM_T, f - overcut, entrySill - f + overcut);
        }

        if (definition.buildLid)
        {
            const c = definition.snapClearance;
            const p = definition.snapBump;
            const plateT = definition.lidPlateThickness;
            // Bump sits near the lip tip (the lip hangs down from the lid plate at z = H).
            const zb0 = H - SNAP_LIP_H + 0.04 * inch;
            const grooveDepth = p - c + 0.01 * inch;
            const sites = snapSites(L, W);

            // Grooves in the base walls that the lid's snap bumps click into.
            for (var i = 0; i < size(sites); i += 1)
            {
                const side = sites[i].side;
                const pos = sites[i].pos;
                const sgn = sideSign(side);
                const wf = wallFaceNc(side, L, W, t);
                cutSideBox(context, id + ("groove" ~ i), baseBody, side,
                    pos - SNAP_FINGER_W / 2 - 0.02 * inch, pos + SNAP_FINGER_W / 2 + 0.02 * inch,
                    wf + sgn * grooveDepth, wf - sgn * overcut,
                    zb0 + SNAP_BUMP_H + 0.01 * inch, SNAP_BUMP_H + 0.02 * inch);
            }

            // Lid plate, flush with the base footprint.
            const ps = newSketchOnPlane(context, id + "lidPlateSketch", { "sketchPlane" : plane(vector(0 * inch, 0 * inch, H), vector(0, 0, 1)) });
            skRectangle(ps, "rect", { "firstCorner" : vector(0 * inch, 0 * inch), "secondCorner" : vector(L, W) });
            skSolve(ps);
            opExtrude(context, id + "lidPlate", { "entities" : qSketchRegion(id + "lidPlateSketch"), "direction" : vector(0, 0, 1), "endBound" : BoundingType.BLIND, "endDepth" : plateT });
            const lidBody = qCreatedBy(id + "lidPlate", EntityType.BODY);

            // Lip that drops inside the base walls; starts inside the plate so the union overlaps.
            const ls = newSketchOnPlane(context, id + "lidLipSketch", { "sketchPlane" : plane(vector(0 * inch, 0 * inch, H + plateT / 2), vector(0, 0, 1)) });
            skRectangle(ls, "rect", { "firstCorner" : vector(t + c, t + c), "secondCorner" : vector(L - t - c, W - t - c) });
            skSolve(ls);
            opExtrude(context, id + "lidLip", { "entities" : qSketchRegion(id + "lidLipSketch"), "direction" : vector(0, 0, -1), "endBound" : BoundingType.BLIND, "endDepth" : SNAP_LIP_H + plateT / 2 });
            opBoolean(context, id + "lidLipJoin", { "tools" : qUnion([lidBody, qCreatedBy(id + "lidLip", EntityType.BODY)]), "operationType" : BooleanOperationType.UNION });

            cutBox(context, id + "lidLipHollow", lidBody, t + c + SNAP_LIP_T, L - t - c - SNAP_LIP_T, t + c + SNAP_LIP_T, W - t - c - SNAP_LIP_T,
                H, SNAP_LIP_H + overcut);

            // Snap fingers: relief slots free a section of lip to flex; a symmetric wedge bump
            // clicks into the wall groove.
            for (var i = 0; i < size(sites); i += 1)
            {
                const side = sites[i].side;
                const pos = sites[i].pos;
                const sgn = sideSign(side);
                const lipOuter = wallFaceNc(side, L, W, t) - sgn * c;
                const lipInner = lipOuter - sgn * SNAP_LIP_T;
                const a0 = pos - SNAP_FINGER_W / 2;
                const a1 = pos + SNAP_FINGER_W / 2;

                cutSideBox(context, id + ("reliefA" ~ i), lidBody, side, a0 - SNAP_RELIEF_W, a0,
                    lipInner - sgn * overcut, lipOuter + sgn * overcut, H, SNAP_LIP_H + overcut);
                cutSideBox(context, id + ("reliefB" ~ i), lidBody, side, a1, a1 + SNAP_RELIEF_W,
                    lipInner - sgn * overcut, lipOuter + sgn * overcut, H, SNAP_LIP_H + overcut);

                const bumpPlane = plane(sidePoint(side, a0, lipOuter, zb0), sideAlongDir(side), sideNormalDir(side));
                const baseNc = lipOuter - sgn * 0.01 * inch;
                const b1 = worldToPlane(bumpPlane, sidePoint(side, a0, baseNc, zb0));
                const b2 = worldToPlane(bumpPlane, sidePoint(side, a0, baseNc, zb0 + SNAP_BUMP_H));
                const ap = worldToPlane(bumpPlane, sidePoint(side, a0, lipOuter + sgn * p, zb0 + SNAP_BUMP_H / 2));
                const bs = newSketchOnPlane(context, id + ("bumpSketch" ~ i), { "sketchPlane" : bumpPlane });
                skPolyline(bs, "tri", { "points" : [b1, b2, ap, b1] });
                skSolve(bs);
                opExtrude(context, id + ("bump" ~ i), { "entities" : qSketchRegion(id + ("bumpSketch" ~ i)), "direction" : sideAlongDir(side), "endBound" : BoundingType.BLIND, "endDepth" : SNAP_FINGER_W });
                opBoolean(context, id + ("bumpJoin" ~ i), { "tools" : qUnion([lidBody, qCreatedBy(id + ("bump" ~ i), EntityType.BODY)]), "operationType" : BooleanOperationType.UNION });
            }

            // Notch the lid lip only if the entry window reaches up into the lip's band.
            if (definition.buildEntry && entryTop > H - SNAP_LIP_H)
                cutBox(context, id + "lipEntryNotch", lidBody, L - t - c - SNAP_LIP_T - overcut, L - t - c + overcut, eY0, eY1,
                    H, SNAP_LIP_H + overcut);

            // Single coin slot on the +X end, cut into the lid's underside edge: a flat blade
            // pries it, a paw can't.
            cutBox(context, id + "coinSlot", lidBody, L - 0.12 * inch, L + overcut, W / 2 - COIN_SLOT_W / 2, W / 2 + COIN_SLOT_W / 2,
                H + COIN_SLOT_D, COIN_SLOT_D + overcut);

            filletVerticalCorners(context, id + "lidCorners", lidBody, L, W, H + plateT / 2 + 0.01 * inch, definition.cornerRadius, squareCorner);
            filletPerimeter(context, id + "lidTopEdge", lidBody, L, W, H + plateT, definition.topFillet);

            if (definition.lidOffset > 0 * inch)
                opTransform(context, id + "lidMove", { "bodies" : lidBody, "transform" : transform(vector(0 * inch, 0 * inch, definition.lidOffset)) });
        }
    },
    {
        "stationLength" : 4.15 * inch,
        "stationWidth" : 1.305 * inch,
        "stationHeight" : 0.44 * inch,
        "stationClearance" : 0.063 * inch,
        "alley" : 0.25 * inch,
        "headroom" : 0.245 * inch,
        "wallThickness" : 0.1 * inch,
        "floorThickness" : 0.08 * inch,
        "cornerRadius" : 0.15 * inch,
        "topFillet" : 0.05 * inch,
        "buildFloorSlots" : false,
        "slotWidth" : 0.25 * inch,
        "slotHeight" : 0.12 * inch,
        "slotSill" : 0.04 * inch,
        "stopHeight" : 0.15 * inch,
        "buildEntry" : true,
        "entryOnPlusY" : false,
        "entryWidth" : 0.18 * inch,
        "entryHeight" : 0.2 * inch,
        "entrySill" : 0.25 * inch,
        "rampAngle" : 40 * degree,
        "buildLid" : true,
        "lidPlateThickness" : 0.08 * inch,
        "lidOffset" : 0.5 * inch,
        "snapClearance" : 0.008 * inch,
        "snapBump" : 0.025 * inch,
        "buildEars" : true,
        "earLength" : 0.4 * inch,
        "earHole" : 0.17 * inch
    });
