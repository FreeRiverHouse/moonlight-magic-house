"""
Moonlight Magic House — procedural ambient music generator
Generates loopable room ambiences as WAV files (no external deps beyond numpy).

Run: python3 generate_ambient_music.py
Output: ../Assets/Audio/Music/<room>.wav
"""

import numpy as np, struct, os, math

SR   = 44100
OUT  = os.path.join(os.path.dirname(__file__), "..", "Assets", "Audio", "Music")
os.makedirs(OUT, exist_ok=True)

def write_wav(path, samples):
    s = np.clip(samples, -1.0, 1.0)
    d = (s * 32767).astype(np.int16).tobytes()
    with open(path, "wb") as f:
        f.write(b"RIFF"); f.write(struct.pack("<I", 36 + len(d)))
        f.write(b"WAVE"); f.write(b"fmt ")
        f.write(struct.pack("<IHHIIHH", 16, 1, 1, SR, SR*2, 2, 16))
        f.write(b"data"); f.write(struct.pack("<I", len(d))); f.write(d)
    print(f"  → {path}")

def sine(t, f, phi=0): return np.sin(2*np.pi*f*t + phi)
def noise(n, amt=1.0): return np.random.uniform(-amt, amt, n)

def smooth_noise(length, smoothing=2048):
    n = noise(length // smoothing + 2)
    idx = np.linspace(0, len(n)-1, length)
    return np.interp(idx, np.arange(len(n)), n)

def make(name, samples, fade=0.5):
    # Fade in/out for seamless loop
    f = int(fade * SR)
    if f > 0 and len(samples) > f*2:
        samples[:f]  *= np.linspace(0, 1, f)
        samples[-f:] *= np.linspace(1, 0, f)
    write_wav(os.path.join(OUT, f"{name}.wav"), samples)

DURATION = 30  # seconds per track

# ── Living Room — cosy, slow pulse, warm pad ──────────────────────────────────
def music_living_room():
    dur = DURATION
    t = np.linspace(0, dur, int(SR * dur))
    # Slow warm pad: Cmaj chord (C E G)
    pad  = sine(t, 261.6) * 0.25 + sine(t, 329.6) * 0.20 + sine(t, 392.0) * 0.18
    pad += sine(t, 130.8) * 0.15  # bass C
    # Gentle tremolo
    tremolo = 0.85 + 0.15 * sine(t, 0.3)
    pad *= tremolo
    # Soft noise layer (fireplace crackle feel)
    crack = smooth_noise(len(t), 512) * 0.04
    s = pad + crack
    make("LivingRoom", s)

# ── Kitchen — brighter, playful, G major ─────────────────────────────────────
def music_kitchen():
    dur = DURATION
    t = np.linspace(0, dur, int(SR * dur))
    pad  = sine(t, 392.0) * 0.22 + sine(t, 493.9) * 0.18 + sine(t, 587.3) * 0.16
    pad += sine(t, 196.0) * 0.14
    # Bouncy pulse every beat (120 BPM)
    beat_freq = 2.0
    pulse = 0.9 + 0.1 * np.abs(sine(t, beat_freq))
    pad *= pulse
    # Light bubbling
    bubble = smooth_noise(len(t), 256) * 0.03
    s = pad + bubble
    make("Kitchen", s)

# ── Bedroom — lullaby, slow, F major, dreamy ─────────────────────────────────
def music_bedroom():
    dur = DURATION
    t = np.linspace(0, dur, int(SR * dur))
    pad  = sine(t, 174.6) * 0.20 + sine(t, 220.0) * 0.18 + sine(t, 261.6) * 0.15
    pad += sine(t,  87.3) * 0.12  # deep bass
    # Very slow tremolo
    pad *= (0.80 + 0.20 * sine(t, 0.12))
    # Breathe filter (lowpass approximation via smooth mod)
    breath = smooth_noise(len(t), 8192) * 0.05
    s = pad + breath
    make("Bedroom", s)

# ── Garden — open, magical, A minor, nature-like ─────────────────────────────
def music_garden():
    dur = DURATION
    t = np.linspace(0, dur, int(SR * dur))
    pad  = sine(t, 220.0) * 0.22 + sine(t, 261.6) * 0.18 + sine(t, 329.6) * 0.15
    pad += sine(t, 110.0) * 0.12
    # Wind-like smooth noise
    wind = smooth_noise(len(t), 4096) * 0.08
    # Firefly sparkles: random high-frequency pings
    rng = np.random.default_rng(42)
    sparks = np.zeros(len(t))
    for _ in range(40):
        pos = rng.integers(0, len(t) - 2000)
        freq = rng.uniform(1200, 2400)
        spark_t = np.linspace(0, 0.04, 2000)
        ping = sine(spark_t, freq) * np.linspace(0.25, 0, 2000)
        sparks[pos:pos+2000] += ping
    s = pad + wind + sparks
    make("Garden", s)

# ── Library — mysterious, D minor, soft chimes ───────────────────────────────
def music_library():
    dur = DURATION
    t = np.linspace(0, dur, int(SR * dur))
    pad  = sine(t, 146.8) * 0.20 + sine(t, 174.6) * 0.16 + sine(t, 220.0) * 0.14
    pad += sine(t,  73.4) * 0.10
    # Slow mysterious pulse
    pad *= (0.75 + 0.25 * sine(t, 0.07))
    # Chime hits every ~7s
    chimes = np.zeros(len(t))
    rng = np.random.default_rng(99)
    for hit_t in [3.5, 7.0, 11.5, 16.0, 21.5, 26.5]:
        pos = int(hit_t * SR)
        if pos + 8000 < len(t):
            freq = rng.choice([523.3, 659.3, 783.9, 1046.5])
            ct = np.linspace(0, 0.18, 8000)
            chimes[pos:pos+8000] += sine(ct, freq) * np.exp(-ct * 25) * 0.35
    s = pad + chimes
    make("Library", s)

# ── Main theme (title screen) ─────────────────────────────────────────────────
def music_main_theme():
    dur = 60  # longer for title
    t = np.linspace(0, dur, int(SR * dur))
    # A major — bright and magical
    pad  = sine(t, 220.0) * 0.18 + sine(t, 277.2) * 0.16 + sine(t, 329.6) * 0.14
    pad += sine(t, 110.0) * 0.10 + sine(t, 440.0) * 0.08
    # Moon pulse: every 4 seconds
    moon_pulse = np.abs(sine(t, 0.25)) * 0.15 + 0.85
    pad *= moon_pulse
    # High shimmer layer
    shimmer = sine(t, 880) * 0.06 * (0.5 + 0.5 * sine(t, 0.7))
    shimmer += sine(t, 1174.7) * 0.04 * (0.5 + 0.5 * sine(t, 0.9))
    wind = smooth_noise(len(t), 8192) * 0.04
    s = pad + shimmer + wind
    make("MainTheme", s, fade=2.0)

if __name__ == "__main__":
    music_living_room()
    music_kitchen()
    music_bedroom()
    music_garden()
    music_library()
    music_main_theme()
    print(f"\n✅ 6 ambient music tracks generated.")
