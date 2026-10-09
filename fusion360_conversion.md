# Fusion 360 conversion spec

Source: `anttrap_shell.fs` (Onshape FeatureScript). This file gives everything needed to rebuild the
model in Fusion 360 by hand, with all derived numbers already computed for the default inputs.
Line references (`fs:NNN`) point into `anttrap_shell.fs`.

## 0. Setup (do first)

1. **Z-up**: Preferences > General > Default modeling orientation > **Z up**. The script is Z-up
   (floor on z = 0, lid above). Skipping this mirrors every Y/Z instruction below.
2. **Units**: set document units to **inch**. Fusion stores cm internally; type `in` in fields if unsure.
3. **Origin** = outer bottom corner of the base at (0, 0, 0). Base occupies **x 0..L, y 0..W, z 0..H**.
   +X end = the end with the ant door and coin slot. The door sits at the **(x = L, y = 0)** corner.
4. Build as **one component for the base** and **one for the lid**. In the FeatureScript the lid is a
   second body in the same studio, shifted up by `lidOffset` for display. Model it seated (offset 0),
   then export base and lid separately for printing. Print lid **plate-down-flat** (top face on the bed is
   fine; flip it for printing and check overhang on the lip/bumps).

## 1. User Parameters (Modify > Change Parameters > +)

Create these, in order. Expressions can reference earlier ones.

| Name | Expression | Value (in) | Meaning |
|---|---|---|---|
| `stationL` | `4.15 in` | 4.150 | TERRO T300 length (X) |
| `stationW` | `1.305 in` | 1.305 | width (Y) |
| `stationH` | `0.44 in` | 0.440 | height (Z) |
| `clr` | `0.063 in` | 0.063 | clearance per side (mass-molded, varies) |
| `alley` | `0.25 in` | 0.250 | ant gap, station to wall |
| `headroom` | `0.245 in` | 0.245 | gap above station |
| `t` | `0.1 in` | 0.100 | wall thickness |
| `f` | `0.08 in` | 0.080 | floor thickness |
| `cornerR` | `0.15 in` | 0.150 | vertical outer corner radius |
| `topFillet` | `0.05 in` | 0.050 | lid top edge fillet |
| `stopH` | `0.15 in` | 0.150 | corner stop height above floor |
| `entryW` | `0.18 in` | 0.180 | door and stairwell width |
| `entryH` | `0.2 in` | 0.200 | door height above ground |
| `entrySill` | `0.25 in` | 0.250 | stair top above inside floor (dam height) |
| `rampAng` | `40 deg` | 40 | stair angle |
| `plateT` | `0.08 in` | 0.080 | lid plate thickness |
| `snapC` | `0.008 in` | 0.008 | lip-to-wall snap clearance |
| `snapP` | `0.025 in` | 0.025 | snap bump height |
| `earLen` | `0.4 in` | 0.400 | ear length past wall |
| `earHole` | `0.17 in` | 0.170 | ear screw hole diameter |

Fixed constants (fs:39-54), also make these parameters so v2 can tweak them:

| Name | Expression | Value |
|---|---|---|
| `lipT` | `0.06 in` | lid lip wall thickness |
| `lipH` | `0.25 in` | lip drop below plate underside |
| `fingerW` | `0.4 in` | snap finger width |
| `reliefW` | `0.04 in` | relief slot beside each finger |
| `bumpH` | `0.07 in` | bump height along Z |
| `stopArm` | `0.2 in` | corner stop arm length |
| `stopT` | `0.06 in` | corner stop thickness |
| `damT` | `0.06 in` | dam wall thickness |
| `coinW` | `0.5 in` | coin slot width |
| `coinD` | `0.04 in` | coin slot depth |
| `earR` | `0.25 in` | ear radius (fs:420, hard-coded) |

Derived (user parameters too, so they update):

| Name | Expression | Default value |
|---|---|---|
| `pocketL` | `stationL + 2 * clr` | 4.276 |
| `pocketW` | `stationW + 2 * clr` | 1.431 |
| `L` | `pocketL + 2 * alley + 2 * t` | **4.976** |
| `W` | `pocketW + 2 * alley + 2 * t` | **2.131** |
| `H` | `f + stationH + headroom` | **0.765** |
| `entryTopZ` | `f + entrySill` | 0.330 (stair peak / dam top, absolute Z) |
| `peakX` | `L - entryTopZ / tan(rampAng)` | 4.583 |
| `footX` | `peakX - entrySill / tan(rampAng)` | 4.285 |

Note: `footX` uses `entrySill` (0.25), not `entryTopZ` (0.33). That is how the script is written (fs:350-351);
keep it unless you intentionally change the stair shape.

Pocket (where the station sits) spans x 0.35..4.626, y 0.35..1.781 (offset `t + alley` from each wall).

## 2. Base build order

All cuts below use "box" = sketch a rectangle on a plane at the stated Z, extrude. The script overshoots
cuts by **0.02 in** (`overcut`) so faces are not coplanar. Do the same: Fusion's boolean also fails on
coplanar faces.

1. **Outer block**: rectangle (0,0)-(L,W) on XY, extrude up `H`.
2. **Outer vertical corner fillets**, radius `cornerR`, on the four vertical edges, **except the (L, 0) edge**
   (kept square so the trap sits flush against a wall). If you build the door on +Y, skip (L, W) instead.
   Do this before step 3 (inner cavity corners stay square).
3. **Cavity**: cut rectangle (t, t)-(L-t, W-t) from the top down to z = f (leaves floor thickness `f`).
4. **Corner stops**, one L per pocket corner, z from f to f + stopH. For the corner at (cx, cy) with inward
   direction (dx, dy) (signs pointing into the pocket):

   | Corner (cx, cy) | dx, dy |
   |---|---|
   | (0.35, 0.35) | +1, +1 |
   | (4.626, 0.35) | -1, +1 |
   | (0.35, 1.781) | +1, -1 |
   | (4.626, 1.781) | -1, -1 |

   - **X arm**: x from `cx - dx*stopT` to `cx + dx*stopArm`; y from `cy` to `cy - dy*stopT` (outside pocket edge).
   - **Y arm**: x from `cx` to `cx - dx*stopT`; y from `cy` to `cy + dy*stopArm`.

   Each arm is a box; they overlap at the corner and form an L hugging the station corner from outside.
5. **Ears (optional, `buildEars`)**: on the XY plane at z = 0, centered y = W/2, thickness `f + 0.04 = 0.12`.
   Profile = rectangle `2*earR` wide (0.5) plus a circle radius `earR` at the tip.
   - Ear A: rectangle from x = +0.05 (inside wall) to tip x = `-earLen`; tip center (-0.4, W/2).
   - Ear B: rectangle from x = L - 0.05 to tip x = `L + earLen`; tip center (L + 0.4, W/2).
   - Through hole dia `earHole` at each tip center.
6. **Door (`buildEntry`)**: cut box through the +X wall: x from L - t to L, y from `eY0` to `eY1`, z from 0 to `entryH`.
   - Default (-Y corner): `eY0 = t = 0.1`, `eY1 = eY0 + entryW = 0.28`.
   - `entryOnPlusY` (mirror about y = W/2): `eY0 = W - t - entryW`.
7. **Stair** (a tent-shaped ridge in the stairwell): sketch on the **XZ plane offset to y = eY0**. Closed polygon in (x, z):

   `(L, 0) -> (peakX, entryTopZ) -> (footX, f - 0.02) -> (footX, 0) -> (L, 0)`

   Default: (4.976, 0) -> (4.583, 0.330) -> (4.285, 0.060) -> (4.285, 0) -> back.
   Extrude **+Y by `entryW`** (from eY0 to eY1), join to the base. Ants walk in at the door at ground level,
   climb the up-slope to the peak, and drop onto the floor on the down-slope.
   For `entryOnPlusY`, extrude the same profile from `eY0` (= W - t - entryW) toward +Y as well (script does
   this in both cases; only `eY0` changes).
8. **Spill dam**: box x from `peakX` to `L - t + 0.02`; y from `damY0` to `damY0 + damT`;
   z from `f - 0.02` to `entryTopZ`. Default `damY0 = eY1 = 0.28` (dam y 0.28..0.34, just short of pocket at 0.35).
   For `entryOnPlusY`, `damY0 = eY0 - damT` (dam on the inner side of the stairwell).
   This wall stops liquid in the tray reaching the door until it tops `entryTopZ`.
9. **Snap grooves in base walls** (6 sites, see section 4). At each site, cut a box into the wall from its
   inner face outward by `grooveDepth = snapP - snapC + 0.01 = 0.027 in`:
   - Width along the wall: `fingerW + 0.04 = 0.44`, centered on the site.
   - Z: from `zb0 + bumpH + 0.01 - (bumpH + 0.02)` to `zb0 + bumpH + 0.01`, i.e. **z 0.545..0.635** with defaults,
     where `zb0 = H - lipH + 0.04 = 0.555`.

## 3. Lid build order

Seated position: plate underside at z = H. Build it in place, then you can leave it there or move it up for display.

1. **Plate**: rectangle (0,0)-(L,W) at z = H, extrude up `plateT` (z 0.765..0.845).
2. **Lip**: rectangle (t + snapC, t + snapC)-(L - t - snapC, W - t - snapC) = **(0.108, 0.108)-(4.868, 2.023)**,
   extruded from z = H + plateT/2 **down** to z = H - lipH (0.515). Join to the plate.
3. **Hollow the lip**: cut rectangle inset another `lipT` = **(0.168, 0.168)-(4.808, 1.963)** from z = H downward
   by `lipH + 0.02`. The plate underside stays flat at z = H. This leaves a 0.06 in thick lip.
4. **Snap fingers**, at each of the 6 sites (section 4), `a0 = pos - fingerW/2`, `a1 = pos + fingerW/2`:
   - **Relief slots**: two boxes through the lip thickness (across the lip) at `a0 - reliefW .. a0` and `a1 .. a1 + reliefW`,
     cut from z = H downward `lipH + 0.02`. This frees a 0.4 in wide cantilever finger hanging from the plate.
   - **Wedge bump**: triangle profile in the plane perpendicular to the wall, extruded **fingerW** along the wall from `a0`:
     - Base edge on the lip's **outer face** minus 0.01 in (slightly inside the lip so the union overlaps),
       from z = `zb0` (0.555) to `zb0 + bumpH` (0.625).
     - Apex = `snapP` (0.025) **outward of the lip outer face**, at z = `zb0 + bumpH/2` (0.590).
     - Outward = toward the nearest wall of the base (i.e., away from the box center).
     Join to the lid.
5. **Door notch in lid lip**: **only if** `entryH > H - lipH` (0.515). With defaults it is 0.2, so **skip**. If you raise the
   door above that, cut a box x `L - t - snapC - lipT .. L - t - snapC`, y `eY0..eY1`, z `H - lipH..H` from the lip.
6. **Coin slot**: box x `L - 0.12 .. L`, y `W/2 +/- coinW/2` (1.0655 +/- 0.25), z from `H` up to `H + coinD` (0.04).
   Removes plate material from the **underside** of the +X edge. A flat blade pries the lid up here; a paw can't.
   (The script's box actually starts 0.02 below H, trimming the top 0.02 of the lip; harmless, you can ignore it.)
7. **Lid fillets**: vertical corner fillet `cornerR` on the plate's four outer vertical edges (skip the same square
   corner as the base), then `topFillet` (0.05) on the top perimeter edges (tangent chain).
8. (Display only) shift the lid up `lidOffset` (0.5 in) so the interior is visible. Leave at 0 for printing.

## 4. Snap site table (default values)

Side 0 = wall at y = 0, side 1 = y = W, side 2 = x = 0, side 3 = x = L. "pos" is the coordinate along that wall.

| # | Wall | pos |
|---|---|---|
| 0 | y = 0 | x = 1.244 (`L * 0.25`) |
| 1 | y = 0 | x = 3.732 (`L * 0.75`) |
| 2 | y = W | x = 1.244 |
| 3 | y = W | x = 3.732 |
| 4 | x = 0 | y = 1.0655 (`W / 2`) |
| 5 | x = L | y = 1.0655 |

Site 5 (x = L) sits in the same wall as the door (y 0.1..0.28) and the coin slot (y 0.8..1.3). The finger
(y 0.8655..1.2655) overlaps the coin-slot y range. In the script the slot is only in the plate underside,
so both coexist. Keep that arrangement.

## 5. Optional features (off by default)

- **Floor-level entry slots** (`buildFloorSlots`): leak path, off by default, skip in the first Fusion build.
  Spec if needed (fs:154-161, 383-394): through-wall boxes `slotWidth` (0.25) wide, `slotHeight` (0.12) tall,
  bottom at `f + slotSill` (0.12). Sites: sides 0 and 1 at x = `L*0.12`, `L/2`, `L*0.88`; sides 2 and 3 at y = `W/2`.
- **Entry on +Y**: mirror the door, stairwell, dam, and square corner about y = W/2 (section 2 steps 2, 6-8).

## 6. Checks the script enforces (replace with Fusion parameter warnings or just check by eye)

| Check | Condition that **fails** (fs line) |
|---|---|
| Lid lip hits slots | slots on, lid on, and `H - lipH < slotTop + 0.05` (342) |
| Door too low for stair | `entryH < t * tan(rampAng) + 0.08` (352) |
| Stair top near rim | `entryTopZ > H - 0.05` (354) |
| Stairwell too wide | `entryW + damT > alley + clr` (356) |

Default design passes all four (0.2 >= 0.164; 0.33 <= 0.715; 0.24 <= 0.313).

## 7. Validation checklist after building

1. Base overall size = **4.976 x 2.131 x 0.765 in**; lid overall = 4.976 x 2.131 x 0.08 plate + lip below.
2. Station pocket = **4.276 x 1.431 in** clear, floor top at z = 0.08, corner stops reach z = 0.23.
3. Dam top and stair peak both at **z = 0.33**, below the rim at 0.765 and below the station top at 0.52.
4. Lid lip outer faces are **0.008 in** off the wall inner faces; bump apex is 0.017 in into the wall; groove is 0.027 in deep.
5. Lid seated: plate underside = base rim top (z = 0.765), no interference between the lip and the stair or corner stops
   (lip bottom z = 0.515 is above stop top 0.23 and dam top 0.33).

## 8. Known unverified items

- The Onshape model has not been printed or tested, per `handover.md`. Treat clearance values as starting points.
- Fusion has no direct equivalent of `regenError`; the checks in section 6 must be done manually.
- Fusion's default Y-up will silently turn Z measurements into Y measurements: do step 0.1.
