"""Check built Jekyll HTML, local URLs/anchors, and the search index without a server."""
import argparse
import json
from html.parser import HTMLParser
from pathlib import Path
from urllib.parse import unquote, urlsplit
import posixpath


class Page(HTMLParser):
    def __init__(self, text):
        super().__init__(convert_charrefs=True)
        self.links = []
        self.ids = set()
        self.current_links = []
        self.feed(text)

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if "id" in attrs:
            self.ids.add(attrs["id"])
        if attrs.get("aria-current") == "page":
            self.current_links.append(attrs.get("href", ""))
        for key in ("href", "src"):
            if key in attrs:
                self.links.append(attrs[key])


parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("site", type=Path)
parser.add_argument("--baseurl", default="")
args = parser.parse_args()
root = args.site.resolve()
base = args.baseurl.rstrip("/")
pages = {p: Page(p.read_text(encoding="utf-8")) for p in root.rglob("*.html")}
assert pages, f"No HTML pages in {root}"
failures = []
checked = 0


def resolve(url, source):
    parsed = urlsplit(url)
    if parsed.scheme or parsed.netloc:
        return None, None
    path = unquote(parsed.path)
    current = "/" + source.relative_to(root).as_posix()
    if path.startswith("/"):
        if base and not path.startswith(base + "/"):
            failures.append(f"{source.name}: URL escapes baseurl: {url}")
        if base and path.startswith(base + "/"):
            path = path[len(base):]
    else:
        path = posixpath.normpath(posixpath.join(posixpath.dirname(current), path)) if path else current
    target = (root / path.lstrip("/")).resolve()
    if not target.is_relative_to(root):
        failures.append(f"{source.name}: URL escapes site: {url}")
        return None, None
    if target.is_dir():
        target /= "index.html"
    return target, unquote(parsed.fragment)


for file, page in pages.items():
    for url in page.links:
        target, fragment = resolve(url, file)
        if target is None:
            continue
        checked += 1
        if not target.is_file():
            failures.append(f"{file.relative_to(root)}: missing {url}")
        elif target.suffix == ".md":
            failures.append(f"{file.relative_to(root)}: unconverted Markdown URL {url}")
        elif fragment and target in pages and fragment not in pages[target].ids:
            failures.append(f"{file.relative_to(root)}: missing anchor {url}")

search = json.loads((root / "search.json").read_text(encoding="utf-8"))
assert len(search) >= 14, "Search index is missing documentation pages"
assert len({item["url"] for item in search}) == len(search), "Duplicate search URLs"
for item in search:
    assert item["title"] and item["description"] and item["text"], "Empty search metadata"
    target, _ = resolve(item["url"], root / "index.html")
    if target not in pages:
        failures.append(f"Search result does not resolve: {item['url']}")
    else:
        current = pages[target].current_links
        if len(current) != 1 or resolve(current[0], target)[0] != target:
            failures.append(f"Missing or incorrect active navigation: {item['url']}")
    if "{%" in item["text"] or "{{" in item["text"]:
        failures.append(f"Unrendered Liquid in search: {item['url']}")

assert not failures, "\n".join(failures)
print(f"Verified {len(pages)} HTML pages, {checked} local URLs/anchors, and {len(search)} search entries (baseurl={base or '/'}).")
