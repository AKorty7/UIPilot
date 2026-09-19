// Builds UIPilot_Documentation.html from the Markdown guide. Edge prints it to PDF.
import { readFileSync, writeFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { marked } from 'marked';

const here   = dirname(fileURLToPath(import.meta.url));
const mdPath = process.argv[2];
const shots  = process.argv[3];

const img = name =>
  'data:image/png;base64,' + readFileSync(join(shots, name)).toString('base64');

let md = readFileSync(mdPath, 'utf8');

// The cover block replaces the Markdown title and intro line about the menu path.
md = md.replace(/^# UIPilot User Guide\s*/, '');
md = md.replace(/\*\*Open the tool from the Unity menu bar: Tools > UIPilot\*\*\s*/, '');

let body = marked.parse(md, { gfm: true });

// ── Figures ────────────────────────────────────────────────────────────────
// Window images are real screenshots of the tool (Unity 6000.3.11f1, dark skin);
// menu images are renders of what it generates, over a stand-in scene.
const quickStartFigure = `
<figure class="pair">
  <div class="pair-a"><img class="shot" src="${img('Window_Build.png')}" alt="The Build section of the UIPilot window">
    <figcaption><strong>The UIPilot window.</strong> UI Health (section 6) is on top. Under Build, tick the menus, click <strong>Build UI</strong>,
      and the row beneath the buttons reports the result.</figcaption>
  </div>
  <div class="pair-b"><img src="${img('UIPilot_MainMenu_Panel.png')}" alt="Generated main menu">
    <figcaption><strong>What Build UI creates</strong>: the main menu, rendered in Unity 6000.3.11f1.
      The title and version are this project's own (its Product Name is "UIPilot"). The first
      button has focus, the second is under the mouse. The shapes behind are a stand-in scene, seen through the wash.</figcaption>
  </div>
</figure>`;

const panelsFigure = `
<figure class="duo">
  <div><img src="${img('UIPilot_PauseMenu_Panel.png')}" alt="Generated pause menu">
    <figcaption>Pause menu</figcaption></div>
  <div><img src="${img('UIPilot_Settings_Panel.png')}" alt="Generated settings menu">
    <figcaption>Settings menu. All three rows work: here the volume has been turned down to 70%.</figcaption></div>
</figure>`;

const sideShot = (file, alt, caption) => `
<figure class="side">
  <img class="shot" src="${img(file)}" alt="${alt}">
  <figcaption>${caption}</figcaption>
</figure>`;

const scanFigure = sideShot('Window_ScanRepair.png', 'Scan and Repair results',
  `<strong>Scan &amp; Repair</strong> after scanning a scene that has only a main menu and no
   GameManager yet. Each row shows a lamp and its status in words. <strong>Repair Scene</strong>
   appears because some rows are not OK.`);

const manualFigure = sideShot('Window_Manual.png', 'The Manual section',
  `<strong>Manual</strong>: the three steps Build UI performs for you, available one at a time.`);

const themesFigure = `
<figure class="trio">
  <div><img src="${img('Theme_SoftClub.png')}" alt="Soft Club theme"><figcaption><strong>Soft Club</strong> (default)</figcaption></div>
  <div><img src="${img('Theme_SoftClubNight.png')}" alt="Soft Club Night theme"><figcaption><strong>Soft Club Night</strong></figcaption></div>
  <div><img src="${img('Theme_Ink.png')}" alt="Ink theme"><figcaption><strong>Ink</strong></figcaption></div>
</figure>`;

const healthFigure = `
<figure class="health">
  <div><img class="shot" src="${img('Health_Window.png')}" alt="The UI Health section of the UIPilot window">
    <figcaption><strong>UI Health</strong> on a test scene with one problem of each kind. Click a name to
      select it; <strong>Fix</strong> is one undoable step.</figcaption>
  </div>
  <div><img class="shot" src="${img('Health_Hierarchy.png')}" alt="UI Health lamps in the Hierarchy">
    <figcaption><strong>The Hierarchy</strong>: a lamp on each object with an issue, and a small one on
      the objects above it.</figcaption>
    <img class="shot toolbar" src="${img('Health_Toolbar.png')}" alt="The UI Health lamp on Unity's main toolbar">
    <figcaption><strong>The main toolbar</strong>, once switched on: the worst lamp and the number of issues.</figcaption>
  </div>
</figure>`;

const insertAfterHeading = (html, tag, startsWith, addition) => {
  const re = new RegExp('(<' + tag + '[^>]*>' + startsWith + '[^<]*</' + tag + '>)');
  if (!re.test(html)) throw new Error('Heading not found: ' + startsWith);
  return html.replace(re, '$1' + addition);
};

const insertBeforeHeading = (html, tag, startsWith, addition) => {
  const re = new RegExp('(<' + tag + '[^>]*>' + startsWith + '[^<]*</' + tag + '>)');
  if (!re.test(html)) throw new Error('Heading not found: ' + startsWith);
  return html.replace(re, addition + '$1');
};

body = insertAfterHeading(body, 'h2', '3\\. Quick start', quickStartFigure);
body = insertAfterHeading(body, 'h2', '4\\. What UIPilot creates', panelsFigure);
body = insertAfterHeading(body, 'h3', 'Scan &amp; Repair', scanFigure);
body = insertAfterHeading(body, 'h3', 'Manual', manualFigure);
body = insertAfterHeading(body, 'h3', '5\\.4 Themes', themesFigure);
// After the table under "Where you see it", so the section starts on the page it heads.
body = insertBeforeHeading(body, 'h3', 'What it checks', healthFigure);

const css = `
@page { size: A4; margin: 17mm 19mm 18mm 19mm; }
* { box-sizing: border-box; }
:root {
  --ink: #121826; --ink-2: #222B3F; --teal: #0F8B8D; --teal-dark: #0B6E70;
  --text: #1B2230; --muted: #5B6578; --rule: #D9DEE7; --wash: #F2F5F9;
}
html { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
body { margin: 0; color: var(--text); font: 10.2pt/1.52 "Segoe UI", system-ui, sans-serif; }

/* Cover block */
.cover { background: var(--ink); color: #F3F5F8; padding: 11mm 11mm 9mm; margin-bottom: 7mm; }
.cover h1 { font: 600 30pt/1.05 Bahnschrift, "Segoe UI", sans-serif; letter-spacing: -0.4pt; margin: 0; }
.cover .rule { width: 17mm; height: 1.1mm; background: var(--teal); margin: 4mm 0 5mm; }
.cover p { margin: 0; max-width: 128mm; color: #C6CEDB; }
.cover .open { margin-top: 6mm; display: inline-block; background: var(--ink-2); padding: 2.6mm 4mm;
  font: 600 11pt Bahnschrift, "Segoe UI", sans-serif; color: #F3F5F8; }
.cover .open span { color: #7FD6D8; }

h2 { font: 600 15.5pt/1.2 Bahnschrift, "Segoe UI", sans-serif; color: var(--ink);
  margin: 8.5mm 0 2.6mm; padding-top: 3mm; border-top: 0.35mm solid var(--ink); break-after: avoid; }
h3 { font: 600 11.5pt/1.3 Bahnschrift, "Segoe UI", sans-serif; color: var(--ink);
  margin: 5.5mm 0 1.6mm; break-after: avoid; }
p { margin: 0 0 2.6mm; }
hr { display: none; }
strong { color: #0E1422; font-weight: 700; }
a { color: var(--teal-dark); }
ol, ul { margin: 0 0 3mm; padding-left: 6mm; }
li { margin-bottom: 1.5mm; }
li > p { margin-bottom: 1.2mm; }

blockquote { margin: 1.6mm 0 2.6mm; padding: 2mm 3.4mm; background: var(--wash);
  border-left: 0.9mm solid var(--teal); color: #2A3345; font-size: 9.4pt; break-inside: avoid; }
blockquote p { margin: 0; }

code { font: 9pt/1.4 "Cascadia Mono", Consolas, monospace; background: var(--wash);
  padding: 0.2mm 1mm; border-radius: 0.6mm; }
/* The font lives on <pre> itself: its line boxes take their height from the
   pre's own font, not from the <code> inside it. */
pre { background: var(--ink); color: #E6EBF2; padding: 3.2mm 4mm; margin: 1.6mm 0 3.4mm;
  font: 8.2pt/1.42 "Cascadia Mono", Consolas, monospace;
  break-inside: avoid; white-space: pre; overflow: hidden; }
pre code { background: none; padding: 0; color: inherit; font: inherit; }

/* Tables may break between rows, never inside one; the header row repeats. */
table { width: 100%; border-collapse: collapse; margin: 1.6mm 0 3.6mm; font-size: 9.5pt; }
thead { display: table-header-group; }
tr { break-inside: avoid; }
th, td { text-align: left; vertical-align: top; padding: 1.7mm 2.4mm; border-bottom: 0.2mm solid var(--rule); }
th { font: 600 9pt Bahnschrift, "Segoe UI", sans-serif; color: var(--muted); border-bottom: 0.35mm solid var(--ink); }
thead th:empty { display: none; }
td:first-child { font-weight: 600; color: #0E1422; min-width: 26mm; }
td strong, th strong { font-weight: 700; }

figure { margin: 3mm 0 5mm; break-inside: avoid; }
figcaption { font-size: 8.6pt; line-height: 1.4; color: var(--muted); margin-top: 1.8mm; }
figure img { width: 100%; display: block; border: 0.2mm solid var(--rule); }
.pair { display: grid; grid-template-columns: 58mm 1fr; gap: 6mm; align-items: start; }
.duo  { display: grid; grid-template-columns: 1fr 1fr; gap: 5mm; }
.trio { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 3mm; }

/* Screenshots of the editor window keep their own dark frame. */
figure img.shot { border: 0.25mm solid #1F1F1F; }
.side { display: grid; grid-template-columns: 62mm 1fr; gap: 6mm; align-items: start; }
.side figcaption { margin-top: 0; font-size: 9pt; }
figure img.toolbar { margin-top: 5mm; }
.health { display: grid; grid-template-columns: 1fr 1fr; gap: 6mm; align-items: start; }
`;

const html = `<!doctype html>
<html lang="en"><head><meta charset="utf-8"><title>UIPilot User Guide</title><style>${css}</style></head>
<body>
<header class="cover">
  <h1>UIPilot User Guide</h1>
  <div class="rule"></div>
  <p>Setup, the one-click workflow, everything UIPilot creates, how to make it yours, and UI Health, which checks all your UI as you work.</p>
  <div class="open">Open the tool: <span>Tools &gt; UIPilot</span></div>
</header>
${body}
</body></html>`;

writeFileSync(join(here, 'UIPilot_Documentation.html'), html);
console.log('HTML written: ' + html.length + ' bytes');
