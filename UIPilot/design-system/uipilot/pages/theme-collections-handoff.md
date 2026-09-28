# Two theme collections — handoff for Claude

User direction, updated 25 September 2026. This document records design decisions and preview work, not twenty completed Unity presets.

## Current agreed direction — 25 September

After comparing the earlier nine-theme overview with the revised studies, the user approved this recommendation ("im happy with that"):

- **Use Claude's earlier menus as the foundation**, represented by `../studies/earlier-presets-reference.png` and the existing production menu design.
- Selectively bring across improved controls and theme details from the revised studies. This is a design direction, not blanket approval of every experimental model or screen.
- Make **3D loading visuals optional**. They should not determine or replace the underlying menu system.
- Keep custom loading visuals lightweight: Theme default / Custom model / None; one model or prefab slot with automatic centring/fitting; animation on/off using the theme's motion. The user rejected a larger set of lighting, positioning, material and animation editing controls as excessive for this feature.
- Buyers can supply their own scene imagery; preserve theme identity through reusable UI components.

The earlier two-collection / 20-preset target is retained as a prior target; this approval did not explicitly cancel it or approve a blending engine. Preserve both sets of studies. Implementation should follow the foundation-and-selective-refinement direction above. These remain browser design studies: Unity-ready models, production integration and runtime verification have not been completed. Further Gen X Soft Club discussion remains open.

## Previous review: refined material pass extended to the other nine

The user rejected the entire v12 3D pass ("oh theyre terrible"), then chose **refine Soft Club in 3D first**. After the smooth chrome-ribbon candidate, the user requested **the exact same pass on all the others**, with further Gen X Soft Club discussion afterwards. One material/lighting pass is now available for the other nine in `../studies/polished-loading-themes.html`. The Soft Club candidate is preserved unchanged. This authorizes the approach; visual approval of the nine new results remains pending.

The rejected v12 software-rendered visual treatment remains historical. The new pass uses the Soft Club candidate's GPU rendering, broad studio reflections, rounded edges and distinct theme materials. The previous flat designs remain the comparison baseline. The target of two collections / 20 presets and later mixing/blending is unchanged.

**Follow-up polish, 24 September:** the user requested cleaner models and themed backgrounds. Updated the same nine-theme study with size-aware bevels (small details no longer use a bevel larger than the part), more curve segments, closed arc ends, continuous Pixel frame pieces, simpler separated Fantasy settings, and an eight-sided cut-crystal JRPG model. Rebalanced studio reflections to retain material colour and define edges. Each screen now has its own abstract background: Military angular panels/registration rules; Sci-Fi orbital contours; Fantasy fine gilt seals; JRPG blue diagonal window motifs; Night glass bands; Neumorphism sculpted recesses; Ink paper layers; Pixel stepped grids; Horror worn panel seams. These are authored geometry/CSS patterns with no scene imagery. Soft Club's standalone candidate remains untouched for the next discussion.

The user also recommended Pinterest and supplied two more references: layered silver hardware with translucent cyan inserts, and precise typographic rules/patterns. Applied as original abstract panel construction and restrained hierarchy for the technical themes. No source lettering, logos, artwork, maps or invented telemetry were copied. The [UI Industrial Sci-Fi Pinterest board](https://se.pinterest.com/jlsholm/ui-industrial-sci-fi/) was inspected; a sign-in overlay limited further visual browsing. The user's supplied images remained the primary direction.

## Approved direction

Keep the earlier designs **and** the revamped designs with animated loading screens. Target **20 presets in total: two collections of ten**. The user clarified: "the earlier one and the animated one, its for claude to see too".

These are two visual collections, not a static/animated toggle counted twice. Retain the earlier artwork and styling when implementing the new collection; do not overwrite existing preset GUIDs to substitute the revamped designs. Motion can remain a separate option within a theme.

Mixing and matching or blending the collections is **future work**. Do not build a blending engine or change public theme contracts on the basis of this note alone.

## Target catalogue

Each row belongs in both the Earlier and Revamped collections. Collection names are working labels, not final product branding.

| Theme family | Earlier collection | Revamped collection |
| --- | --- | --- |
| Dark Neumorphism | Preserve/recover earlier design | Sculpted graphite, recessed controls, moving ring marker |
| Soft Club | Preserve earlier design | Silver, mineral blue, mint, reflective rocking loops |
| Soft Club Night | Preserve earlier design | Smoked blue, lilac, elliptical lens motion |
| Ink | Preserve earlier design | Ivory paper, charcoal rules, turning ink seal |
| Fantasy RPG | Preserve earlier design | Gilt frames, burgundy controls, rotating crest |
| JRPG Window | Preserve earlier design | Blue command windows, silver bevels, floating crystal |
| Pixel Retro | Preserve earlier design | Stepped frames, block cursor, discrete loading motion |
| Sci-Fi HUD | Preserve earlier design | Military/astronomy influence, orbital linework |
| Military Shooter | Preserve earlier design | Monochrome instruments, cyan/green, chevron pulses |
| Survival Horror | Preserve earlier design | Chalk, worn charcoal, faded red, slow seal movement |

Inventory caveat: the current generated-menu design document enumerates nine existing Unity presets. Dark Neumorphism exists in the ten-theme browser study. An independently preserved earlier ten-theme HTML snapshot was not found in this session. Before declaring the first collection complete, recover and identify its exact earlier revisions; do not invent a tenth production preset or claim twenty are already implemented. Existing presets and historical studies have not been replaced.

## Files Claude can review

- **Latest nine-theme material pass, awaiting review:** `../studies/polished-loading-themes.html`. Loading-only gallery of Military Shooter, Sci-Fi HUD, Fantasy RPG, JRPG Window, Soft Club Night, Dark Neumorphism, Ink, Pixel Retro and Survival Horror. Each has an authored 3D motif, themed loading panel, and its own motion. Uses one persistent Three.js renderer/environment, disposing replaced model resources on selection. Motion modes: Follow system, Play, Pause, Reduced; explicit completion/replay control. The sample progress is intentionally 38% or 100%, not a real loading measurement. No image generation, imported models or downloaded textures.
- **Soft Club refinement, preserved for the next discussion:** `../studies/soft-club-chrome-study.html`. A single smooth twisted chrome ribbon with a smaller offset mint-glass insert, pale mineral background and soft studio reflections. Uses GPU-rendered smooth geometry and a procedural lighting environment through Three.js 0.179.1 loaded from the permitted esm.sh CDN. No packages were installed into UIPilot. No image generation or downloaded texture/model is used. This candidate is unchanged by the nine-theme pass. Desktop and 320px views were inspected, with no overflow or browser errors; further refinement and inline playback remain user acceptance gates. Material implementation reference: [Three.js material documentation](https://threejs.org/docs/#api/en/materials/MeshPhysicalMaterial).
- **Rejected v12 3D study, retained for history only:** `../studies/loading-3d-theme-studies.html`. All ten revamped families were interpreted in solid geometry. The user rejected this visual result; do not promote it into production. The earlier 2D animated study remains preserved.
- `../studies/loading-3d-renderer.js`: editable renderer source embedded into the latest fragment. It projects authored 3D meshes into a 2D canvas; there is no external library, generated raster art, imported model or Unity implementation. Geometry includes toroidal rings, spheres, crystals, extruded crest/chevron profiles and voxel-like boxes. This is a lightweight browser concept renderer with flat face shading and depth sorting, not a production GPU renderer.
- `../studies/animated-theme-studies.html`: self-contained HTML fragment of the current ten revamped designs, with menus, settings, pause, loading and control states. This is a browser design study, not a Unity runtime file. It needs a fragment host/standalone wrapper to run as a full page.
- `../studies/earlier-presets-reference.png`: preserved earlier preset overview, copied unchanged from this conversation's existing reference output.
- `generated-menus.md`: existing production design and nine-preset baseline. Kept unchanged.
- Historical studies remain at `C:/Users/xyada/.codex/visualizations/2026/09/24/01a0d375-8949-7110-beac-9db6bf185e2a/uipilot-themes/`. In particular: `theme-controls-v6.md`, `theme-controls-v7.md`, `theme-identity-v8.md`, `theme-loading-v9.md`, and `theme-motion-v10.md`.
- The historical folder also contains early generated PNG concepts and rejected Soft Club experiments. **Do not treat every historical image as an approved member of the earlier collection.**

## Constraints carried forward

- Buyers add their own scene imagery. UI identity must survive swapping the scene image.
- Loading ornament can use abstract shapes, neutral patterns, symbols and materials drawn from the theme.
- No invented maps or gameplay telemetry. Military uses brackets and chevrons; Sci-Fi blends instrument styling and astronomy geometry.
- Dark Neumorphism is its own theme, not a treatment to apply to every theme.
- Main, pause, settings and loading should eventually share palette/type tokens, sliced panel assets, controls and clear idle/hover/focus/pressed/disabled states.
- Preserve reusable authored components and legible text. Avoid generated scenic art as the default theme identity.

## Animation correction and evidence

The user reported that **all themes appeared still in the inline preview**, despite running CSS animations in a separate browser test. The precise host-side suppression was not directly observable. v11 replaces CSS keyframe autoplay with direct Web Animations playback, provides an explicit Play animation override alongside Follow system/Reduced motion/Paused, shows motion status, and adds a clearly moving marker to Neumorphism.

Unchanged widget-state notifications previously caused unconditional DOM rebuilds and restarted animation poses. v11 compares restored state before rebuilding. A redundant document-hidden pause was removed; browser throttling handles background documents.

Browser verification observed transformed/opacity-changing ornaments across all ten themes, different Neumorphism ring positions across time samples, and working reduced/completion states. **The revised inline rendering still needs the user's confirmation.** Separate browser checks do not prove the chat host displays motion.

## Production boundary

### Refined GPU pass verification

Inspected all nine theme renders in the browser. All nine reported a 320px layout width and 320px scroll width; narrow Military and Horror renders were inspected visually. Observed different JRPG crystal poses over time, then a stable paused pose across subsequent screenshots. Exercised Reduced, completion (Ready / 100%) and replay, and theme switching. Browser error logs were empty for desktop and narrow wrappers. The follow-up model/background polish was rendered across all nine, with a repeated no-overflow check at 320px, a narrow JRPG visual inspection, and final Fantasy/Neumorphism lighting checks. No browser errors were observed. The preview limits drawing to approximately 30 fps and pixel density to 2; device performance has not been benchmarked. Inline playback still requires user confirmation. No production Unity changes were made or compiled.

### 3D preview verification, v12

Inspected all ten motifs in desktop browser screenshots. Corrected the crystal's top clearance and closed sphere pole triangles, then checked the corrected crystal at 320px. All ten themes fit a 320px frame with no horizontal overflow. Observed different crystal orientations across time samples and the paused pose, and checked reduced-motion, completion and restart controls. Browser error inspection was empty. Rendering in the chat host still requires user confirmation; separate browser evidence is not proof of inline playback. The preview caps its drawing cadence at 30 frames per second and pixel density at 2; target-device performance has not been benchmarked.

No Unity source, scenes, prefabs, theme assets or runtime wiring changed in this work. No Unity compilation or Game View acceptance is claimed. Follow `.cursorrules` and the design system when implementing: existing styler ownership, theme data, native UGUI/TMP, and allowed generated GameManager/shader paths. Loading implementation and any changes needed for twenty selectable presets require their own scoped implementation and Unity rendering checks.
