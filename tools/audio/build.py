"""Regenerate every game sound into Assets/Audio/.

Usage (from the repository root, needs ffmpeg on PATH):
    python tools/audio/build.py
"""

import os
import pathlib
import subprocess
import sys
import tempfile

sys.path.insert(0, str(pathlib.Path(__file__).parent))

from music import SONGS, render  # noqa: E402
from sfx import EFFECTS  # noqa: E402

OUTPUT = pathlib.Path(__file__).resolve().parents[2] / "Assets" / "Audio"


def encode(wav_path, target):
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", str(wav_path),
                    "-c:a", "libvorbis", "-q:a", "5", "-map_metadata", "-1", str(target)],
                   check=True)


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    jobs = list(EFFECTS.items())
    jobs += [(name, lambda song=song: render(song)) for name, song in SONGS.items()]
    with tempfile.TemporaryDirectory() as tmp, tempfile.TemporaryDirectory() as stage:
        staged = pathlib.Path(stage)
        # Encode everything first so a failure leaves Assets/Audio untouched.
        for name, recipe in jobs:
            wav = pathlib.Path(tmp) / (pathlib.Path(name).stem + ".wav")
            recipe().write(str(wav))
            encode(wav, staged / name)
            print("built", name)
        # Every encode succeeded: copy the results into place (keeping Unity's .meta files).
        for name, _ in jobs:
            data = (staged / name).read_bytes()
            (OUTPUT / name).write_bytes(data)


if __name__ == "__main__":
    main()
