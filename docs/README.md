# Maintaining the documentation site

The published site is built from this directory with Jekyll. The custom layout uses the existing Ember branding, local CSS/JavaScript, and GitHub Pages-compatible plugins. `CNAME` and `_config.yml` preserve `consoul.dev`; the build also supports a project-site `baseurl` such as `/Consoul`.

## Preview locally

Install Ruby 3.3 with Bundler (on Windows, use RubyInstaller with Devkit), then run from `docs`:

```shell
bundle install
bundle exec jekyll serve --host 127.0.0.1
```

Open `http://127.0.0.1:4000`. Restart Jekyll after changing `_config.yml`. The existing production-only analytics remain disabled during local development. This maintainer README, Bundler files, and older PDFs are excluded from the site.

## Add or edit a guide

Give every guide YAML front matter with `title`, `description`, and `searchable: true`. The default `docs` layout adds navigation and the version notice. Keep one H1 and use H2 headings for the generated “On this page” list.

Use ordinary relative Markdown links between guides, for example `[Tables](tables.md)`. `jekyll-relative-links` converts them to published HTML URLs, so links also work when reading source on GitHub. In HTML or Liquid templates, apply `relative_url` to site paths. Files outside `docs` need explicit GitHub source URLs. Update `source_branch` in `_config.yml` and external source links when moving the documentation to another branch.

Add navigation entries to `_data/navigation.yml`. Every searchable page needs one navigation entry, including archived release notes, so validation can check its active-page link. Put short, complete examples before long API lists. State the required package and imports, handle canceled prompt results, and do not imply an unpublished version is available on NuGet.

When preparing a release, update `docs_version` in `_config.yml`, current installation examples, release-note links, and the layout's version notice together. Preserve earlier release notes and their archive navigation entries.

## Output previews

From the repository root, regenerate output previews with:

```shell
dotnet run --project scripts/DocsExamples -- docs
dotnet run --project scripts/DocsExamples -- docs --check
```

The runner compiles the gallery recipes, checks that their source matches the snippets, and captures actual library output into SVGs. Keep the marked recipe bodies in `scripts/DocsExamples/Program.cs` synchronized with `examples.md`. Interactive recipes compile but are not run during generation. SVGs show plain-text capture rather than ANSI colors or animation. The Tiny Text Adventures PNG is copied unchanged from the repository's original `Consoul_1.png` and labeled as legacy.

## Validate

From `docs`:

```shell
bundle exec jekyll build --strict_front_matter --trace
python ../scripts/Verify-Site.py _site
bundle exec jekyll build --baseurl /Consoul --destination ../artifacts/site-subpath
python ../scripts/Verify-Site.py ../artifacts/site-subpath --baseurl /Consoul
```

The documentation workflow performs both builds and checks generated local links, anchors, assets, and the JSON search index. The existing .NET workflow also checks compiled gallery recipes and preview freshness. For browser review, check the home page and a long guide at desktop and mobile widths, search, code copying, keyboard navigation, and the mobile navigation disclosure.

## Hosting

This change does not deploy the site. For GitHub Pages' branch-based build, select the intended branch and `/docs` folder in repository Pages settings. Keep the custom domain `consoul.dev` configured there. The documentation workflow validates the site without changing Pages settings or publishing artifacts.
