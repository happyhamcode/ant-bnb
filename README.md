# Ant Trap: spill-proof holder for liquid ant bait

A 3D-printable enclosure for [Terro T300 liquid ant bait stations](https://www.amazon.com/TERRO-T300-2-2-Pack-Liquid-Baits/dp/B00E4GACB8).
The bait containers are easy to knock over and spill, which makes a sticky mess and defeats the point. This
box holds the container upright, lets ants in, and keeps spills inside. It is meant for floors and countertops.

## Available at these major retailers

Get the Terro T300 liquid ant baits here:

<p>
  <a href="https://www.amazon.com/TERRO-T300-2-2-Pack-Liquid-Baits/dp/B00E4GACB8"><img src="assets/logos/amazon.svg" alt="Amazon" height="40"></a>
  &nbsp;&nbsp;&nbsp;
  <a href="https://www.homedepot.com/p/TERRO-Indoor-Liquid-Ant-Killer-Baits-6-Count-T300/202532940"><img src="assets/logos/homedepot.svg" alt="The Home Depot" height="60"></a>
  &nbsp;&nbsp;&nbsp;
  <a href="https://www.lowes.com/pd/TERRO-6-Count-Ant-Bait-Station-6-Pack/5001954631"><img src="assets/logos/lowes.svg" alt="Lowe's" height="40"></a>
</p>

_Pack sizes, prices and stock vary by store. Amazon, The Home Depot and Lowe's names and logos are trademarks of their owners and are used only to link to the product._

## Features

- **Fits the Terro T300 station** with its twist top removed (4.15 in overall, about 3.65 in without the top, 1.305 in wide,
  0.44 in tall): 3.750 x 1.500 x 0.750 in interior (3.950 x 1.700 x 0.900 in outside).
- **Ants in, spills out**: a ground-level door leads to a walled ramp that climbs over a spill dam before dropping into
  the tray. Liquid has to rise 0.25 in (about 23 mL, far more than a container holds) to reach the door.
- **Snap-fit lid**: flex fingers with wedge bumps click into grooves in the walls. A coin slot lets you pry it open;
  a paw can't.
- **Command strip recess**: a pocket in the bottom takes a medium or large 3M Command strip so it can stay put.
  The recess is open at the end opposite the ramp so the strip's pull tab sticks out for easy removal.

## Print it

1. Open `anttrap_v2_A1mini.3mf`: a base and lid pair already laid out for a Bambu Lab A1 mini plate.
2. Or use `anttrap_base.3mf` and `anttrap_lid.3mf` (inches) for any other printer. The lid is flipped plate-down so
   it needs no supports.
3. PLA or PETG both work. No supports needed.

If the lid is too tight or loose, change `c` (snap clearance, default 0.008 in) or `p` (bump height, default
0.025 in) in `fusion_box_build.py` and re-export.

## Rebuild or change it

`fusion_box_build.py` builds the whole model in Fusion 360 through the Fusion API. All dimensions are
parameters at the top of the script, so changing the interior size, wall thickness or door size resizes everything.
Run it in an empty design.

The original v1 (for the full 4.15 in T300 with the top on) is an Onshape FeatureScript, `anttrap_shell.fs`. See `handover.md` for
design notes and `fusion360_conversion.md` for the build spec.

## Files

| File | What |
|---|---|
| `anttrap_v2_A1mini.3mf` | Slicer project, base + lid pair for a Bambu Lab A1 mini |
| `anttrap_base.3mf`, `anttrap_lid.3mf` | Print-ready parts (inches) |
| `assets/logos/` | Retailer logos used in this README |
| `fusion_box_build.py` | v2 parametric Fusion 360 build script |
| `anttrap_shell.fs` | v1 Onshape FeatureScript (Terro T300) |
| `handover.md`, `fusion360_conversion.md` | Design notes and build spec |

This is an independent project and isn't affiliated with Terro.
