"""Original chiptune theme for the game. Melodies are written as 'NOTE:EIGHTHS' tokens; 'R' is a rest."""

from synth import Track, note_freq, square, triangle

CHORDS = {
    "D": ("D", [0, 4, 7]), "Dm": ("D", [0, 3, 7]), "G": ("G", [0, 4, 7]),
    "A": ("A", [0, 4, 7]), "Am": ("A", [0, 3, 7]), "C": ("C", [0, 4, 7]),
    "F": ("F", [0, 4, 7]), "Bb": ("Bb", [0, 4, 7]), "Em": ("E", [0, 3, 7]),
    "Gm": ("G", [0, 3, 7]), "Bm": ("B", [0, 3, 7]),
}


def parse(melody):
    notes = []
    for token in melody.split():
        name, length = token.split(":")
        notes.append((None if name == "R" else name, int(length)))
    return notes


def kick(track, at):
    track.add_tone(at, 0.09, 150, triangle, 0.55, freq_end=45, release=0.03)


def snare(track, at, seed):
    track.add_noise(at, 0.12, 0.22, smoothing=0.25, decay_power=1.6, seed=seed)


def hat(track, at, seed):
    track.add_noise(at, 0.03, 0.06, smoothing=0.0, decay_power=2.0, seed=seed)


def render(song):
    eighth = song["eighth"]
    per_bar = song["meter"]
    bars = [bar for section in song["form"] for bar in song["sections"][section]]
    total = len(bars) * per_bar * eighth
    track = Track(total + 0.01)
    lead_wave = lambda p: square(p, 0.25)
    arp_wave = lambda p: square(p, 0.125)

    for bar_index, (melody, chord, octave_shift) in enumerate(bars):
        bar_start = bar_index * per_bar * eighth
        # Lead melody.
        cursor = 0
        for name, length in parse(melody):
            if name:
                freq = note_freq(name) * (2 ** octave_shift)
                track.add_tone(bar_start + cursor * eighth, length * eighth * 0.9, freq,
                               lead_wave, 0.22, decay=0.06, sustain=0.6, release=0.04,
                               vibrato=0.004 if length >= 3 else 0.0)
            cursor += length
        # Bass and arpeggio from the chord.
        root, intervals = CHORDS[chord]
        bass_root = note_freq(root + "2")
        fifth = bass_root * 2 ** (7 / 12)
        pattern = song["bass"]
        for step, kind in enumerate(pattern):
            if kind == ".":
                continue
            freq = bass_root if kind == "R" else fifth if kind == "F" else bass_root * 2
            track.add_tone(bar_start + step * eighth, eighth * 0.8, freq, triangle, 0.4,
                           release=0.02)
        arp_root = note_freq(root + "4")
        for step in range(per_bar):
            interval = intervals[step % len(intervals)]
            track.add_tone(bar_start + step * eighth, eighth * 0.5,
                           arp_root * 2 ** (interval / 12), arp_wave, 0.06, release=0.02)
        # Drums.
        for step, kind in enumerate(song["drums"]):
            at = bar_start + step * eighth
            if kind == "K":
                kick(track, at)
            elif kind == "S":
                snare(track, at, seed=bar_index * 31 + step)
            if kind in "KSh":
                hat(track, at, seed=bar_index * 17 + step)

    track.normalize(0.8)
    return track




SKY_A = [
    ("D5:2 F#5:1 A5:1 F#5:2 D5:2", "D", 0), ("E5:2 G5:1 B5:1 A5:4", "A", 0),
    ("B5:2 A5:1 G5:1 F#5:2 E5:2", "G", 0), ("F#5:3 E5:1 D5:4", "D", 0),
    ("D5:2 F#5:1 A5:1 D6:2 B5:2", "Bm", 0), ("A5:2 G5:1 F#5:1 G5:4", "G", 0),
    ("E5:2 F#5:1 G5:1 A5:2 C#5:2", "A", 0), ("D5:6 R:2", "D", 0),
]
SKY_B = [
    ("G5:2 G5:1 A5:1 B5:2 G5:2", "G", 0), ("F#5:2 F#5:1 G5:1 A5:2 F#5:2", "D", 0),
    ("E5:2 E5:1 F#5:1 G5:2 E5:2", "Em", 0), ("A5:3 G5:1 F#5:2 E5:2", "A", 0),
    ("G5:2 B5:1 D6:1 B5:2 G5:2", "G", 0), ("F#5:2 A5:1 D6:1 A5:2 F#5:2", "D", 0),
    ("E5:2 G5:1 B5:1 A5:2 G5:2", "A", 0), ("F#5:4 E5:2 R:2", "A", 0),
]

SONGS = {
    # Bright march that loops while flying (the last bar resolves back into the first).
    "sky_theme.ogg": {
        "eighth": 0.16, "meter": 8, "form": "AABA",
        "sections": {"A": SKY_A, "B": SKY_B},
        "bass": "R.RFR.OF", "drums": "KhShKhSh",
    },
}
