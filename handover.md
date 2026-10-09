# Ant Trap: handover

## What this is

One file: `anttrap_shell.fs`. It is an **Onshape FeatureScript** (std version 3083) that generates a
pet-safe, 3D-printable enclosure for a **TERRO T300 liquid ant bait station** (measured 4.150 x 1.305 x 0.440 in).

It is a parametric custom feature named **"Ant Trap Shell"**. Every dimension derives from the
measured station size, so changing one input resizes the whole trap.

## How it works (what you get when it regenerates)

- **Base tray**: floor on z = 0, walls, open top. Outer size is computed: station + clearance + ant alley + walls.
- **Corner stops**: four L-shaped posts that locate the station so it can't slide.
- **Corner door + stairwell** at the +X end: ants enter at ground level, climb a walled stair over a
  **spill dam**, then drop into the tray. Liquid can't reach the door until it tops the dam.
  The entry corner is left square so the trap can sit flush against a wall.
- **Snap-fit lid**: six flex fingers with wedge bumps click into grooves in the base walls
  (same design as the cyberdeck cover). A **coin slot** on the +X end lets a flat blade pry it open; a paw can't.
- **Screw-down ears** on both short ends (optional).
- **Floor-level entry slots** exist but are **off by default** because they leak.

## Code map (`anttrap_shell.fs`)

| Lines | What |
|---|---|
| 13-36 | `LB_*` / `AB_*` bound specs: min / default / max for each UI input |
| 38-54 | Fixed constants: snap lip/finger/bump sizes, corner stop size, dam thickness, coin slot |
| 56-97 | `cutBox` / `addBox`: axis-aligned box subtract / union helpers (used everywhere) |
| 99-161 | Wall-side helpers (side 0 = y=0, 1 = y=W, 2 = x=0, 3 = x=L), `snapSites`, `slotSites` |
| 163-194 | Fillet helpers (vertical corners, lid top perimeter) |
| 196-234 | `addEar` |
| 236-326 | `antTrapShell` feature UI: the parameter list |
| 328-363 | Derived dimensions + sanity checks (`regenError` messages) |
| 364-423 | Base body, cavity, slots, corner stops, ears |
| 425-448 | Door, stair, dam |
| 450-534 | Lid: grooves, plate, lip, snap fingers, coin slot, fillets, display offset |
| 536-566 | Default values (must match the `LB_*` defaults) |

## Working on another computer

There is no local build. The file only runs inside Onshape.

1. `git clone https://github.com/happyhamcode/anttrap.git` (private repo: run `gh auth login` first).
2. In Onshape: open a Part Studio's document, **+ > Feature Studio**, name it e.g. `anttrap`.
3. Paste the whole contents of `anttrap_shell.fs` over the default text. Onshape compiles on paste.
4. In a Part Studio in the same document, click the feature toolbar's **Ant Trap Shell** (it appears under custom features).
5. Edit the code in the Feature Studio, or edit locally and re-paste. Then **copy the final text back into the repo file**.
6. `git add -A && git commit -m "..." && git push`.

Onshape is the source of truth while you iterate; the repo file is only the saved copy. Always paste back before committing.

## How to adjust (v2 starting points)

- **Station is a different size**: change the defaults in the last block (lines 537-540), and the
  bound defaults at 13-16 if you want the UI slider to match.
- **Add or rename an input**: three places must agree: the `LB_*` bound (13-36), the `annotation` +
  `isLength` / boolean in the precondition (236-326), and the default map (536-566). Missing any one gives a compile error.
- **Entry door**: `entryWidth`, `entryHeight`, `entrySill` (dam height), `rampAngle`, `entryOnPlusY` (flip corner).
  The `regenError` checks at 342-357 tell you when a combination doesn't fit; read them before changing these.
- **Snap fit too tight / loose**: `snapClearance` (default 0.008 in) and `snapBump` (0.025 in). Print-tolerance dependent: expect to tune per printer.
- **Snap finger count or positions**: `snapSites` (line 144). Coin slot position: line 526.
- **Fixed geometry** (lip height, finger width, stop size, dam thickness) is a plain constant at lines 39-54, not a UI input. Promote it to an input using the three-place rule above.

## Known caveats

- **Not yet printed or tested** as far as this repo records. Treat clearance values as unverified.
  The comment at line 8 says station clearance is generous because the stations are mass-molded and vary.
- Floor slots are a leak path. Leave `buildFloorSlots` off unless you add a sill (`slotSill`) and accept the risk.
- Lid-lip vs. slot and stair-vs-rim collisions throw `regenError`; this is intended, not a bug.
- The stairwell must fit in the side alley: `entryWidth + dam (0.06 in) <= alley + clearance`.
  Widening the door means widening `alley`.
- Bump/groove geometry uses a rotated sketch plane per wall side (lines 507-516). Easiest place to introduce a sign error if you change wall sides.

## Repo

`https://github.com/happyhamcode/anttrap` (private), branch `main`.
