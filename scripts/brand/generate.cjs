// Run npm install, then npm run build in this directory. Not needed for dotnet builds.
const fs = require('node:fs/promises');
const path = require('node:path');
const sharp = require('sharp');

const root = path.resolve(__dirname, '../..');
const brand = 'docs/assets/brand';
const favicons = 'docs/assets/favicons';
const navy = '#102238';

async function save(file, data) {
  const target = path.join(root, file);
  await fs.mkdir(path.dirname(target), { recursive: true });
  await fs.writeFile(target, data);
}

function svg(body, viewBox = '0 0 1280 1280') {
  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="${viewBox}">${body}</svg>\n`;
}

async function png(file, source, size) {
  await save(file, await sharp(Buffer.from(source)).resize(size, size).png().toBuffer());
}

async function main() {
  const master = await fs.readFile(path.join(root, brand, 'ember.svg'), 'utf8');
  const body = master.match(/<g[\s\S]*<\/g>/)[0];
  const outer = master.match(/id="flame"[^>]*d="([^"]+)"/)[1];
  const prompt = master.match(/id="prompt"[^>]*d="([^"]+)"/)[1];
  const monoBody = color => `<path fill="${color}" fill-rule="evenodd" transform="translate(64 64) scale(.9)" d="${outer} ${prompt}"/>`;
  const tile = svg(`<rect width="1280" height="1280" rx="256" fill="${navy}"/>${body}`);
  const maskable = svg(`<rect width="1280" height="1280" fill="${navy}"/><g transform="translate(128 128) scale(.8)">${body}</g>`);
  const touch = svg(`<rect width="1280" height="1280" fill="${navy}"/>${body}`);

  await save(`${brand}/ember-icon.svg`, tile);
  await save(`${brand}/ember-monochrome.svg`, svg(monoBody(navy)));
  await save(`${brand}/ember-monochrome-white.svg`, svg(monoBody('#FFFFFF')));
  // Safari expects a black single-layer mark with transparent prompt cutouts.
  await save(`${favicons}/safari-pinned-tab.svg`, svg(
    `<path fill="#000000" fill-rule="evenodd" transform="translate(.8 .8) scale(.01125)" d="${outer} ${prompt}"/>`, '0 0 16 16'));
  await save(`${favicons}/favicon.svg`, tile);
  for (const size of [128, 256, 512]) await png(`${brand}/ember-icon-${size}.png`, tile, size);
  await png(`${brand}/ember.png`, master, 512);
  await png(`${brand}/ember-monochrome.png`, svg(monoBody(navy)), 512);
  await png(`${brand}/ember-monochrome-white.png`, svg(monoBody('#FFFFFF')), 512);
  for (const size of [16, 32, 48]) await png(`${favicons}/favicon-${size}x${size}.png`, tile, size);
  await png(`${favicons}/apple-touch-icon.png`, touch, 180);
  for (const size of [192, 512]) await png(`${favicons}/android-chrome-${size}x${size}.png`, tile, size);
  await png(`${favicons}/maskable-icon-512x512.png`, maskable, 512);
  // Keep the established direct favicon URL working.
  await png('docs/favicon.png', tile, 32);
  // Preserve existing repository image URLs with the new shared identity.
  await png('Consoul/Consoul_Small.png', tile, 128);
  await png('Consoul/Consoul.png', tile, 512);

  // ICO directory with PNG payloads (16, 32, 48 and 256 px).
  const sizes = [16, 32, 48, 256];
  const frames = await Promise.all(sizes.map(size => sharp(Buffer.from(tile)).resize(size, size).png().toBuffer()));
  const header = Buffer.alloc(6 + frames.length * 16);
  header.writeUInt16LE(1, 2);
  header.writeUInt16LE(frames.length, 4);
  let offset = header.length;
  frames.forEach((frame, i) => {
    const pos = 6 + i * 16;
    header[pos] = header[pos + 1] = sizes[i] === 256 ? 0 : sizes[i];
    header.writeUInt16LE(1, pos + 4);
    header.writeUInt16LE(32, pos + 6);
    header.writeUInt32LE(frame.length, pos + 8);
    header.writeUInt32LE(offset, pos + 12);
    offset += frame.length;
  });
  await save(`${favicons}/favicon.ico`, Buffer.concat([header, ...frames]));

  const social = svg(`<rect width="1280" height="640" fill="${navy}"/>
    <g transform="translate(52 64) scale(.4)">${body}</g>
    <g font-family="DejaVu Sans, Arial, sans-serif" fill="#FAF7F0">
      <text x="580" y="270" font-size="88" font-weight="700">Consoul</text>
      <text x="584" y="340" font-size="28">A fiery soul for your console.</text>
      <text x="584" y="406" font-size="22" fill="#FFD45A">Console UI toolkit for .NET</text>
    </g>`, '0 0 1280 640');
  await save(`${brand}/social-preview.svg`, social);
  await save(`${brand}/social-preview.png`, await sharp(Buffer.from(social)).resize(1280, 640).png().toBuffer());
  console.log('Generated Ember brand, NuGet, favicon, touch, mask and social preview assets.');
}

main().catch(error => { console.error(error); process.exitCode = 1; });
