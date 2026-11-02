"""Sound effect recipes. Each function returns a rendered Track."""

from synth import Track, note_freq, square, triangle, saw


def flap():
    """A soft wing beat: a dark noise puff with a low upward swoosh."""
    t = Track(0.25)
    t.add_noise(0, 0.14, 0.45, smoothing=0.82, decay_power=1.6, seed=1)
    t.add_tone(0, 0.09, 180, triangle, 0.35, freq_end=320, decay=0.05, sustain=0.4,
               release=0.05)
    return t


def point():
    """Passing a gate: the classic two-note ding."""
    t = Track(0.4)
    t.add_tone(0, 0.07, note_freq("E6"), triangle, 0.4, release=0.02)
    t.add_tone(0.07, 0.2, note_freq("A6"), triangle, 0.4, decay=0.12, sustain=0.35,
               release=0.1)
    return t


def coin():
    t = Track(0.35)
    t.add_tone(0, 0.05, note_freq("B5"), lambda p: square(p, 0.5), 0.25, release=0.02)
    t.add_tone(0.05, 0.18, note_freq("E6"), lambda p: square(p, 0.5), 0.25,
               decay=0.12, sustain=0.4, release=0.08)
    return t


def hit():
    """Crashing into rock."""
    t = Track(0.45)
    t.add_noise(0, 0.25, 0.8, smoothing=0.7, decay_power=1.8, seed=3)
    t.add_tone(0, 0.18, 160, triangle, 0.8, freq_end=45, release=0.08)
    return t


def fall():
    """The dragon tumbling down after a crash."""
    t = Track(0.9)
    t.add_tone(0.05, 0.7, 900, lambda p: square(p, 0.25), 0.18, freq_end=180,
               release=0.1, vibrato=0.01)
    return t


def fireball():
    t = Track(0.6)
    t.add_noise(0, 0.5, 0.55, smoothing=0.55, decay_power=1.3, seed=5)
    t.add_tone(0, 0.3, 520, saw, 0.16, freq_end=140, release=0.12)
    return t


def burst():
    """A bat hit by a fireball."""
    t = Track(0.7)
    t.add_noise(0, 0.6, 0.7, smoothing=0.85, decay_power=2.2, seed=6)
    t.add_tone(0, 0.25, 120, triangle, 0.5, freq_end=40, release=0.1)
    t.add_tone(0.02, 0.1, note_freq("A5"), lambda p: square(p, 0.25), 0.12,
               freq_end=note_freq("A4"), release=0.05)
    return t


def medal():
    """Short fanfare for a medal or a new best score."""
    t = Track(1.4)
    lead = lambda p: square(p, 0.25)
    for name, at, length in [("C5", 0.0, 0.1), ("E5", 0.11, 0.1), ("G5", 0.22, 0.1),
                             ("C6", 0.33, 0.6)]:
        t.add_tone(at, length, note_freq(name), lead, 0.26, decay=0.06, sustain=0.65,
                   release=0.1, vibrato=0.006 if length > 0.5 else 0.0)
    for name in ["E5", "G5"]:
        t.add_tone(0.33, 0.6, note_freq(name), lambda p: square(p, 0.125), 0.08, release=0.3)
    t.add_tone(0.33, 0.6, note_freq("C3"), triangle, 0.45, release=0.2)
    return t


def click():
    t = Track(0.08)
    t.add_tone(0, 0.025, 1300, lambda p: square(p, 0.5), 0.2, freq_end=900, release=0.02)
    return t


EFFECTS = {
    "flap.ogg": flap,
    "point.ogg": point,
    "coin.ogg": coin,
    "hit.ogg": hit,
    "fall.ogg": fall,
    "fireball.ogg": fireball,
    "burst.ogg": burst,
    "medal.ogg": medal,
    "click.ogg": click,
}
