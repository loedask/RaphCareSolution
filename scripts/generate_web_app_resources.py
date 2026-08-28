#!/usr/bin/env python3
"""Generate RaphCare.Web AppResources.resx (+ fr/ln/sw).

Preferred workflow:
1. Edit scripts/web_app_resources_en.txt (KEY|English)
2. Run: python scripts/generate_web_app_resources.py en
3. Run language targets (fr / sw / ln) with deep-translator installed,
   or keep existing satellite files and only append new English keys.

If an embedded-catalog generator overwrote satellites, re-run language targets
or patch missing keys into the satellite .resx files.
"""

from __future__ import annotations

import html
import re
import sys
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "RaphCare.Web" / "Resources" / "Strings"
CATALOG_PATH = Path(__file__).with_name("web_app_resources_en.txt")

RESX_HEADER = """<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
"""

PLACEHOLDER_RE = re.compile(r"\{\d+\}")
PROTECTED = [
    "RaphCare",
    "Agora:AppId",
    "Agora:AppCertificate",
    "Agora",
    "SpO₂",
    "SpO2",
    "Y6",
    "4G",
    "SOS",
    "2FA",
    "UTC",
    "HR",
    "bpm",
]


def log(msg: str) -> None:
    print(msg, flush=True)


def load_catalog() -> list[tuple[str, str]]:
    items: list[tuple[str, str]] = []
    seen: set[str] = set()
    for line in CATALOG_PATH.read_text(encoding="utf-8-sig").splitlines():
        line = line.strip().lstrip("\ufeff")
        if not line or line.startswith("#") or "|" not in line:
            continue
        key, value = line.split("|", 1)
        key = key.lstrip("\ufeff")
        if key in seen:
            continue
        seen.add(key)
        items.append((key, value.replace("—", "-").replace("–", "-")))
    return items


def protect(text: str) -> tuple[str, dict[str, str]]:
    tokens: dict[str, str] = {}
    out = text
    i = 0
    for m in list(PLACEHOLDER_RE.finditer(text)):
        token = f"XPH{i}X"
        tokens[token] = m.group(0)
        out = out.replace(m.group(0), token, 1)
        i += 1
    for name in PROTECTED:
        if name in out:
            token = f"XPR{i}X"
            tokens[token] = name
            out = out.replace(name, token)
            i += 1
    return out, tokens


def unprotect(text: str, tokens: dict[str, str]) -> str:
    out = text
    for token, original in tokens.items():
        out = out.replace(token, original)
    out = re.sub(r"XPH(\d+)X", r"{\1}", out)
    out = re.sub(r"XP(\d+)X", r"{\1}", out)
    return out


def should_skip(value: str) -> bool:
    if value in {"RaphCare", "-", "…", "UTC", "HR", "SpO2", "SpO₂", "bpm"}:
        return True
    if "@" in value and " " not in value.strip():
        return True
    if re.fullmatch(r"[\d\s\W]+", value or ""):
        return True
    return False


def translate_one(target: str, value: str) -> tuple[str, str]:
    from deep_translator import GoogleTranslator

    translator = GoogleTranslator(source="en", target=target)
    protected, meta = protect(value)
    for attempt in range(4):
        try:
            translated = translator.translate(protected)
            return value, unprotect(translated or value, meta)
        except Exception:
            time.sleep(0.4 * (attempt + 1))
    return value, value


def translate_all(values: list[str], target: str, workers: int = 8) -> list[str]:
    cache: dict[str, str] = {}
    unique = [v for v in dict.fromkeys(values) if not should_skip(v)]
    log(f"  {target}: {len(unique)} unique strings, {workers} workers")

    done = 0
    with ThreadPoolExecutor(max_workers=workers) as pool:
        futures = [pool.submit(translate_one, target, v) for v in unique]
        for fut in as_completed(futures):
            src, dst = fut.result()
            cache[src] = dst
            done += 1
            if done % 50 == 0 or done == len(unique):
                log(f"  {target}: {done}/{len(unique)}")

    return [value if should_skip(value) else cache.get(value, value) for value in values]


def write_resx(path: Path, entries: list[tuple[str, str]]) -> None:
    parts = [RESX_HEADER]
    for key, value in entries:
        parts.append(f'  <data name="{html.escape(key, quote=True)}" xml:space="preserve">\n')
        parts.append(f"    <value>{html.escape(value)}</value>\n")
        parts.append("  </data>\n")
    parts.append("</root>\n")
    path.write_text("".join(parts), encoding="utf-8")
    log(f"Wrote {path.name} ({len(entries)} keys)")


def main() -> None:
    only = sys.argv[1] if len(sys.argv) > 1 else "en"
    catalog = load_catalog()
    if not catalog:
        raise SystemExit(f"No catalog entries in {CATALOG_PATH}")

    OUT.mkdir(parents=True, exist_ok=True)
    keys = [k for k, _ in catalog]
    english = [v for _, v in catalog]

    if only in {"all", "en"}:
        write_resx(OUT / "AppResources.resx", list(zip(keys, english)))

    targets: list[tuple[str, str]] = []
    if only == "all":
        targets = [("fr", "fr"), ("sw", "sw"), ("ln", "ln")]
    elif only in {"fr", "sw", "ln"}:
        targets = [(only, only)]

    for code, target in targets:
        log(f"Translating {code}...")
        translated = translate_all(english, target)
        assert len(translated) == len(keys)
        write_resx(OUT / f"AppResources.{code}.resx", list(zip(keys, translated)))

    log(f"Done. Catalog size: {len(keys)}")


if __name__ == "__main__":
    main()
