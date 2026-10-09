import adsk.core, adsk.fusion, math

IN = 2.54  # cm per inch


def run(_context: str):
    app = adsk.core.Application.get()
    design = adsk.fusion.Design.cast(app.activeProduct)
    root = design.rootComponent
    xy = root.xYConstructionPlane

    # ---- parameters (inches) ----
    intL, intW, intH = 3.75, 1.5, 0.75          # required interior
    t, f = 0.10, 0.15                            # wall, floor
    cornerR = 0.15
    stripL, stripW, stripD = 3.70, 1.00, 0.06    # command strip recess (bottom)
    entryW, entryH, sill, ang = 0.18, 0.20, 0.25, 40.0
    damT = 0.06
    plateT, lipT, lipH, c = 0.08, 0.06, 0.25, 0.008
    coinW, coinD = 0.5, 0.04
    eps = 0.02

    L = intL + 2 * t
    W = intW + 2 * t
    H = f + intH
    tanA = math.tan(math.radians(ang))
    topZ = f + sill
    peakX = L - topZ / tanA
    footX = peakX - sill / tanA
    eY0, eY1 = t, t + entryW

    def v(x):
        return adsk.core.ValueInput.createByReal(x * IN)

    def box(x0, x1, y0, y1, z0, z1, op, targets=None, name=None):
        sk = root.sketches.add(xy)
        sk.sketchCurves.sketchLines.addTwoPointRectangle(
            adsk.core.Point3D.create(x0 * IN, y0 * IN, 0), adsk.core.Point3D.create(x1 * IN, y1 * IN, 0))
        inp = root.features.extrudeFeatures.createInput(sk.profiles.item(0), op)
        inp.startExtent = adsk.fusion.OffsetStartDefinition.create(v(z0))
        inp.setOneSideExtent(adsk.fusion.DistanceExtentDefinition.create(v(z1 - z0)),
                             adsk.fusion.ExtentDirections.PositiveExtentDirection)
        if targets is not None:
            inp.participantBodies = targets
        feat = root.features.extrudeFeatures.add(inp)
        if name:
            feat.bodies.item(0).name = name
        return feat

    def fillet_corners(body, skip):
        # vertical (Z) edges at the four plan corners, except `skip` (x,y)
        corners = [(0, 0), (L, 0), (0, W), (L, W)]
        edges = adsk.core.ObjectCollection.create()
        for e in body.edges:
            a, b = e.startVertex.geometry, e.endVertex.geometry
            if abs(a.x - b.x) < 1e-6 and abs(a.y - b.y) < 1e-6 and abs(a.z - b.z) > 1e-6:
                for cx, cy in corners:
                    if (cx, cy) != skip and abs(a.x - cx * IN) < 1e-4 and abs(a.y - cy * IN) < 1e-4:
                        edges.add(e)
        fi = root.features.filletFeatures.createInput()
        fi.addConstantRadiusEdgeSet(edges, v(cornerR), True)
        root.features.filletFeatures.add(fi)

    New = adsk.fusion.FeatureOperations.NewBodyFeatureOperation
    Cut = adsk.fusion.FeatureOperations.CutFeatureOperation
    Join = adsk.fusion.FeatureOperations.JoinFeatureOperation

    # ---- base ----
    base = box(0, L, 0, W, 0, H, New, name="Base").bodies.item(0)
    fillet_corners(base, (L, 0))                        # entry corner stays square
    box(t, L - t, t, W - t, f, H + eps, Cut, [base])    # interior 3.75 x 1.5 x 0.75
    # command strip recess in the bottom face
    box(-eps, (L + stripL) / 2, (W - stripW) / 2, (W + stripW) / 2, -eps, stripD, Cut, [base])   # open at x=0 so the strip tab sticks out
    # ant door through the +X wall at ground level
    box(L - t, L + eps, eY0, eY1, -eps, entryH, Cut, [base])
    # lower the floor in the stairwell channel so the ramp is one continuous surface (no step at the wall)
    box(footX - 0.02, L - t, eY0, eY1, -eps, f + eps, Cut, [base])
    # stair ridge (profile in XZ, extruded along +Y from eY0)
    xz = root.xZConstructionPlane
    pin = root.constructionPlanes.createInput()
    pin.setByOffset(xz, v(eY0))
    off = root.constructionPlanes.add(pin)
    sk2 = root.sketches.add(off)

    def sp2(x, z):
        return sk2.modelToSketchSpace(adsk.core.Point3D.create(x * IN, eY0 * IN, z * IN))

    pts = [sp2(L, 0), sp2(peakX, topZ), sp2(footX, f), sp2(footX - 0.02, f), sp2(footX - 0.02, 0)]
    for i in range(5):
        sk2.sketchCurves.sketchLines.addByTwoPoints(pts[i], pts[(i + 1) % 5])
    si = root.features.extrudeFeatures.createInput(sk2.profiles.item(0), Join)
    si.setOneSideExtent(adsk.fusion.DistanceExtentDefinition.create(v(entryW)), adsk.fusion.ExtentDirections.PositiveExtentDirection)
    si.participantBodies = [base]
    root.features.extrudeFeatures.add(si)
    # spill dam between stairwell and tray
    box(peakX, L - t + eps, eY1, eY1 + damT, f - eps, topZ, Join, [base])

    # snap grooves in the walls
    fw, relief, bumpH, p, bumpT = 0.4, 0.04, 0.07, 0.025, 0.06
    zb0 = H - lipH + 0.04
    gd = p - c + 0.01
    sites = [(0, L * 0.25), (0, L * 0.75), (1, L * 0.25), (1, L * 0.75), (2, W / 2), (3, W / 2)]
    sgn_of = {0: -1, 1: 1, 2: -1, 3: 1}
    wallface = {0: t, 1: W - t, 2: t, 3: L - t}

    def sidebox(side, a0, a1, n0, n1, z0, z1, op, target):
        if side < 2:
            box(a0, a1, n0, n1, z0, z1, op, [target])
        else:
            box(n0, n1, a0, a1, z0, z1, op, [target])

    for side, pos in sites:
        sg = sgn_of[side]
        wf = wallface[side]
        n0, n1 = sorted((wf + sg * gd, wf - sg * eps))
        sidebox(side, pos - fw / 2 - 0.02, pos + fw / 2 + 0.02, n0, n1, zb0 - 0.01, zb0 + bumpH + 0.01, Cut, base)
    # ---- lid (seated on the base, plate underside at z = H) ----
    lid = box(0, L, 0, W, H, H + plateT, New, name="Lid").bodies.item(0)
    fillet_corners(lid, (L, 0))
    box(t + c, L - t - c, t + c, W - t - c, H - lipH, H + plateT / 2, Join, [lid])
    box(t + c + lipT, L - t - c - lipT, t + c + lipT, W - t - c - lipT, H - lipH - eps, H, Cut, [lid])
    box(L - 0.12, L + eps, W / 2 - coinW / 2, W / 2 + coinW / 2, H, H + coinD, Cut, [lid])

    # snap fingers: relief slots free a section of lip to flex, wedge bump clicks into the wall groove
    for side, pos in sites:
        sg = sgn_of[side]
        lipOuter = wallface[side] - sg * c
        lipInner = lipOuter - sg * lipT
        a0, a1 = pos - fw / 2, pos + fw / 2
        n0, n1 = sorted((lipInner - sg * eps, lipOuter + sg * eps))
        sidebox(side, a0 - relief, a0, n0, n1, H - lipH - eps, H, Cut, lid)
        sidebox(side, a1, a1 + relief, n0, n1, H - lipH - eps, H, Cut, lid)
        # wedge profile, in a plane perpendicular to the wall, centered on the finger
        base_nc = lipOuter - sg * 0.01
        apex_nc = lipOuter + sg * p
        def mp(nc, z):
            return (pos, nc, z) if side < 2 else (nc, pos, z)
        if side < 2:
            pl = root.yZConstructionPlane
            ax = 0   # offset along X
        else:
            pl = root.xZConstructionPlane
            ax = 1   # offset along Y
        def make_plane(off):
            pin = root.constructionPlanes.createInput()
            pin.setByOffset(pl, v(off))
            return root.constructionPlanes.add(pin)
        cp = make_plane(pos)
        o = cp.geometry.origin
        if abs((o.x if ax == 0 else o.y) - pos * IN) > 1e-4:
            cp.deleteMe()
            cp = make_plane(-pos)
        bs = root.sketches.add(cp)
        def to_sk(pt):
            return bs.modelToSketchSpace(adsk.core.Point3D.create(pt[0] * IN, pt[1] * IN, pt[2] * IN))
        q = [to_sk(mp(base_nc, zb0)), to_sk(mp(base_nc, zb0 + bumpH)), to_sk(mp(apex_nc, zb0 + bumpH / 2))]
        for i in range(3):
            bs.sketchCurves.sketchLines.addByTwoPoints(q[i], q[(i + 1) % 3])
        bi = root.features.extrudeFeatures.createInput(bs.profiles.item(0), Join)
        bi.setSymmetricExtent(v(fw), True)
        bi.participantBodies = [lid]
        root.features.extrudeFeatures.add(bi)
    # model is built bottom-down on the XY plane (floor at z = 0), +Z up
    for b in root.bRepBodies:
        bb = b.boundingBox
        print(b.name, "vol_in3=%.3f" % (b.volume / IN ** 3),
              "size_in=%.3f x %.3f x %.3f" % ((bb.maxPoint.x - bb.minPoint.x) / IN, (bb.maxPoint.y - bb.minPoint.y) / IN, (bb.maxPoint.z - bb.minPoint.z) / IN))
