"""Package the project plugin from a verified committed documentation snapshot."""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import zipfile
from datetime import datetime, timezone
from pathlib import Path


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source-sha", required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    sha = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=root, text=True).strip()
    if not re.fullmatch(r"[0-9a-f]{40}", args.source_sha) or args.source_sha != sha:
        raise SystemExit("The verified full source SHA must equal this checkout's HEAD.")
    dirty = subprocess.check_output(
        ["git", "status", "--porcelain", "--", "docs/project", "project-plugin", "tools/package_project_plugin.py"],
        cwd=root, text=True,
    )
    if dirty.strip():
        raise SystemExit("Commit the package's source files before packaging.")
    if not args.output.is_absolute():
        raise SystemExit("Use an absolute output path outside the plugin source directory.")
    if (root / "project-plugin") in args.output.resolve().parents:
        raise SystemExit("Keep the archive outside the plugin source directory.")

    package = "mybasiswebapi-projekt"
    skill_root = "skills/mybasiswebapi-project"
    manifest = json.loads((root / "project-plugin/plugin.json").read_text(encoding="utf-8"))
    if manifest["name"] != package or len(manifest["extensions"]["com.openai"]["interface"]["shortDescription"]) > 30:
        raise SystemExit("Invalid package identity or listing subtitle length.")
    if not re.fullmatch(r"\d+\.\d+\.\d+", manifest["version"]):
        raise SystemExit("Use a stable three-part semantic plugin version.")

    entries: dict[str, bytes] = {
        "plugin.json": (root / "project-plugin/plugin.json").read_bytes(),
        f"{skill_root}/SKILL.md": (root / "project-plugin" / skill_root / "SKILL.md").read_bytes(),
    }
    source_files = [
        *(f"docs/project/{name}.md" for name in ("BRD", "PRD", "SRS", "TDD", "DECISIONS", "BACKLOG", "BASELINE", "SOURCES")),
        "docs/project/tasks/TASK-01.md",
    ]
    hashes = []
    for source_path in source_files:
        path = root / source_path
        original = path.read_bytes()

        def pinned_link(match: re.Match[str]) -> str:
            label, target = match.group(1), match.group(2)
            if re.match(r"^[A-Za-z][A-Za-z0-9+.-]*:", target) or target.startswith("#"):
                return match.group(0)
            relative, separator, anchor = target.partition("#")
            resolved = (path.parent / relative).resolve()
            if not resolved.is_relative_to(root) or not resolved.is_file():
                raise ValueError(f"Missing or escaping document reference: {source_path}: {target}")
            repo_path = resolved.relative_to(root).as_posix()
            suffix = f"#{anchor}" if separator else ""
            return f"[{label}](https://github.com/shadpre/MyBasisWebApi/blob/{sha}/{repo_path}{suffix})"

        packed = re.sub(r"\[([^\]]+)\]\(([^)]+)\)", pinned_link, original.decode("utf-8")).encode("utf-8")
        destination = f"{skill_root}/references/{path.name}"
        entries[destination] = packed
        hashes.append({
            "source_path": source_path,
            "package_path": destination,
            "source_sha256": hashlib.sha256(original).hexdigest(),
            "package_sha256": hashlib.sha256(packed).hexdigest(),
        })

    provenance = {
        "repository": "shadpre/MyBasisWebApi",
        "documentation_source_sha": sha,
        "product_baseline_sha": "261eaf40396cf344673f66581d18e24edb71e7e5",
        "document_revision": "0.1.0.0",
        "plugin_version": manifest["version"],
        "generated_at_utc": datetime.now(timezone.utc).isoformat(),
        "snapshot_only": True,
        "live_status_url": "https://github.com/shadpre/MyBasisWebApi/blob/docs/api-template-foundation/docs/project/STATUS.md",
        "link_transformation": "Relative Markdown file links resolve to the exact committed GitHub source; file bodies otherwise retain their source text.",
        "files": hashes,
    }
    entries[f"{skill_root}/references/SOURCE.json"] = (json.dumps(provenance, indent=2) + "\n").encode()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(args.output, "w", zipfile.ZIP_DEFLATED) as archive:
        for path, content in sorted(entries.items()):
            archive.writestr(f"{package}/{path}", content)
    with zipfile.ZipFile(args.output) as archive:
        if archive.testzip() is not None or len(archive.namelist()) != len(entries):
            raise SystemExit("Archive verification failed.")
    print(json.dumps({"archive": str(args.output), "files": len(entries), "version": manifest["version"], "source_sha": sha, "archive_sha256": hashlib.sha256(args.output.read_bytes()).hexdigest()}))


if __name__ == "__main__":
    main()
