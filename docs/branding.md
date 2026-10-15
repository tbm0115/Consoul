---
title: Brand assets
description: Download the Consoul Ember icon, favicons, and social artwork.
searchable: true
---
# Consoul Ember

Ember is the **Prompt Face Flicker** mark: a rounded orange flame, a golden core, and a navy `>_` expression. It represents **a fiery soul for your console**. The same icon identifies Consoul, CLI, Configuration, Logging, and Text.Json.

![Consoul Ember](assets/brand/ember-icon-128.png)

## Assets

| Use | Asset | Notes |
| --- | --- | --- |
| Editable master / transparent mark | [ember.svg](assets/brand/ember.svg) | All artwork derives from these paths; no fonts, linked images, or external resources. |
| Transparent raster | [ember.png](assets/brand/ember.png) | 512 × 512, for placement on a background. |
| Scalable icon tile | [ember-icon.svg](assets/brand/ember-icon.svg) | Navy rounded square, orange flame, golden core. |
| NuGet | [128 px PNG](assets/brand/ember-icon-128.png) | Embedded as `consoul-icon.png` in all five packages. |
| GitHub README / general use | [256 px PNG](assets/brand/ember-icon-256.png), [512 px PNG](assets/brand/ember-icon-512.png) | The larger PNG also works for a GitHub avatar if wanted. |
| Dark monochrome | [SVG](assets/brand/ember-monochrome.svg), [PNG](assets/brand/ember-monochrome.png) | Navy silhouette with transparent prompt cutouts, for light backgrounds. |
| White monochrome | [SVG](assets/brand/ember-monochrome-white.svg), [PNG](assets/brand/ember-monochrome-white.png) | White silhouette with transparent prompt cutouts, for dark backgrounds. |
| Browser favicon | [SVG](assets/favicons/favicon.svg), [ICO](assets/favicons/favicon.ico) | ICO contains 16, 32, 48 and 256 px frames; standalone 16, 32 and 48 px PNGs are also provided. |
| Apple touch icon | [180 px PNG](assets/favicons/apple-touch-icon.png) | Opaque navy background; the OS applies its own corner shape. |
| Web manifest icons | [192 px PNG](assets/favicons/android-chrome-192x192.png), [512 px PNG](assets/favicons/android-chrome-512x512.png) | Referenced by the [manifest](assets/favicons/site.webmanifest). |
| Maskable icon | [512 px PNG](assets/favicons/maskable-icon-512x512.png) | Opaque, with the complete flame inside the central 80%-diameter safe circle. |
| Safari pinned tab | [SVG mask](assets/favicons/safari-pinned-tab.svg) | Single black path, 16 × 16 view box, transparent cutouts; the site supplies a darker orange tint. |
| Social preview | [1280 × 640 PNG](assets/brand/social-preview.png), [SVG source](assets/brand/social-preview.svg) | Solid background for reliable sharing previews. |

The legacy `Consoul/Consoul.png`, `Consoul/Consoul_Small.png`, and `docs/favicon.png` paths are refreshed compatibility copies. `Consoul_1.png` remains a screenshot of the application, not a logo.

## Integration

`Directory.Build.props` sets `PackageIcon`; `Directory.Build.targets` embeds the same PNG in each packable project. The package verifier compares its SHA-256 hash with the source asset, so a stale or substituted package icon fails verification. [NuGet recommends 128 × 128 PNG/JPEG artwork under 1 MB](https://learn.microsoft.com/en-us/nuget/reference/nuspec#icon).

The repository README displays Ember. GitHub's repository social preview is a separate hosted setting: after merging, upload `docs/assets/brand/social-preview.png` in **Settings → General → Social preview**. Committing the image does not change that setting. [GitHub recommends 1280 × 640 and a file under 1 MB](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/customizing-your-repositorys-social-media-preview).

Jekyll's Slate theme owns the document head and SEO tags. Its custom head include adds the favicon, touch icon, manifest, pinned-tab mask, and a small header style. The configured default social image feeds the theme's SEO tags. Existing Google analytics/advertising snippets and optional HTTPS redirect are retained. The previous duplicate head/SEO markup and references to absent CSS, JS, and favicon files were removed.

Asset links use `relative_url`, and the manifest's paths are relative to its own location. The checked-in configuration targets `https://consoul.dev` with an empty `baseurl`; for a GitHub project deployment, set `url` to the account's Pages origin and `baseurl` to `/Consoul`. The manifest uses browser display mode and does not imply offline/PWA support.

## Regeneration

Edit [the SVG master](assets/brand/ember.svg), preserving its `flame`, `core`, and `prompt` IDs and enclosing transform. Then, from the repository root:

```sh
npm --prefix scripts/brand install
npm --prefix scripts/brand run build
```

The optional tool uses Node.js and Sharp. Committed images are consumed directly by .NET and Jekyll; neither build needs Node.js or image generation. The generator also creates the social card; its text uses installed DejaVu Sans or Arial, so typography can vary between machines. Review the exported PNG before committing it.

Palette: orange `#FF8628`, gold `#FFD45A`, navy `#102238`, ivory `#FAF7F0`. Keep the prompt horizontal, preserve clear space, and use the single-color variants where color is unavailable. Do not add eyes to this mark or assign different characters to the integration packages.

## Design provenance

The concept boards and isolated Flicker reference were created with the built-in imagegen tool. The production master is a simplified vector drawing based on the selected reference; all shipped variants are rendered from that master so their geometry stays consistent. The final reference prompt is saved in [the design notes](https://github.com/tbm0115/Consoul/blob/tbm0115/v2026-overhaul/scripts/brand/design-prompt.txt).
