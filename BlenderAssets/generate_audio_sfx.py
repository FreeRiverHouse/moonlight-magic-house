"""
Moonlight Magic House — procedural SFX generator (Python + numpy + scipy)
Generates WAV files for all game sound effects without external assets.

Run: python3 generate_audio_sfx.py
Output: ../Assets/Audio/SFX/<name>.wav
"""

import numpy as np
import struct, os, math

OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Audio", "SFX")
os.makedirs(OUT, exist_ok=True)

SR = 44100   # sample rate

def write_wav(path, samples, sr=SR):
    samples = np.clip(samples, -1.0, 1.0)
    data = (samples * 32767).astype(np.int16).tobytes()
    with open(path, "wb") as f:
        # RIFF header
        f.write(b"RIFF")
        f.write(struct.pack("<I", 36 + len(data)))
        f.write(b"WAVE")
        # fmt chunk
        f.write(b"fmt ")
        f.write(struct.pack("<IHHIIHH", 16, 1, 1, sr, sr * 2, 2, 16))
        # data chunk
        f.write(b"data")
        f.write(struct.pack("<I", len(data)))
        f.write(data)

def env(t, attack=0.01, decay=0.1, sustain=0.6, release=0.2, total=None):
    n = len(t); out = np.ones(n)
    a = int(attack * SR); d = int(decay * SR)
    r = int(release * SR); s = n - a - d - r
    if s < 0: s = 0
    for i in range(min(a, n)): out[i] = i / max(a, 1)
    for i in range(min(d, n - a)): out[a + i] = 1.0 - (1.0 - sustain) * i / max(d, 1)
    for i in range(min(s, n - a - d)): out[a + d + i] = sustain
    for i in range(min(r, n - a - d - s)): out[a + d + s + i] = sustain * (1 - i / max(r, 1))
    return out

def sine(t, freq): return np.sin(2 * np.pi * freq * t)
def noise(n): return np.random.uniform(-1, 1, n)
def chirp(t, f0, f1): return np.sin(2 * np.pi * (f0 + (f1 - f0) * t / t[-1]) * t)

def make(name, samples):
    path = os.path.join(OUT, f"{name}.wav")
    write_wav(path, samples)
    print(f"  → {path}")

# ── SFX definitions ────────────────────────────────────────────────────────────

def sfx_eat():
    t = np.linspace(0, 0.35, int(SR * 0.35))
    s = chirp(t, 400, 800) * 0.5 + sine(t, 600) * 0.3
    s *= env(t, 0.005, 0.05, 0.4, 0.1)
    s += noise(len(t)) * 0.05
    make("eat", s)

def sfx_cuddle():
    t = np.linspace(0, 0.5, int(SR * 0.5))
    # Warm two-tone chime
    s = sine(t, 523) * 0.4 + sine(t, 659) * 0.3 + sine(t, 784) * 0.2
    s *= env(t, 0.01, 0.1, 0.5, 0.3)
    make("cuddle", s)

def sfx_sleep():
    t = np.linspace(0, 1.2, int(SR * 1.2))
    # Descending lullaby tone
    freqs = [523, 494, 440, 392]
    s = np.zeros(len(t))
    seg = len(t) // 4
    for i, f in enumerate(freqs):
        ts = t[i*seg:(i+1)*seg]
        seg_s = sine(ts, f) * 0.4
        seg_s *= np.linspace(1, 0, len(ts))
        s[i*seg:i*seg+len(seg_s)] += seg_s
    make("sleep", s)

def sfx_discover():
    t = np.linspace(0, 0.8, int(SR * 0.8))
    s = chirp(t, 300, 1200) * 0.5 + sine(t, 880) * 0.3
    s *= env(t, 0.01, 0.05, 0.6, 0.2)
    # Add sparkle
    spark_pos = [int(SR * x) for x in [0.1, 0.25, 0.4, 0.55]]
    for p in spark_pos:
        if p < len(s) - 500:
            s[p:p+500] += sine(np.linspace(0, 0.01, 500), 2000) * 0.3 * np.linspace(1, 0, 500)
    make("discover", s)

def sfx_stage_up():
    t = np.linspace(0, 1.5, int(SR * 1.5))
    # Ascending fanfare
    notes = [392, 523, 659, 784, 1047]
    s = np.zeros(len(t))
    seg = len(t) // len(notes)
    for i, f in enumerate(notes):
        ts = t[i*seg:(i+1)*seg]
        ns = (sine(ts, f) * 0.5 + sine(ts, f * 2) * 0.2)
        ns *= env(ts, 0.01, 0.05, 0.7, 0.2)
        s[i*seg:i*seg+len(ns)] += ns
    make("stage_up", s)

def sfx_buy():
    t = np.linspace(0, 0.3, int(SR * 0.3))
    s = chirp(t, 600, 1200) * 0.5
    s *= env(t, 0.005, 0.05, 0.5, 0.15)
    make("buy", s)

def sfx_reward():
    t = np.linspace(0, 1.0, int(SR * 1.0))
    s = sine(t, 523) * 0.3 + sine(t, 659) * 0.25 + sine(t, 784) * 0.2 + sine(t, 1047) * 0.15
    s *= env(t, 0.01, 0.08, 0.5, 0.4)
    make("reward", s)

def sfx_achievement():
    t = np.linspace(0, 1.2, int(SR * 1.2))
    notes = [784, 988, 1175, 1568]
    s = np.zeros(len(t))
    for i, f in enumerate(notes):
        delay = int(i * 0.15 * SR)
        seg_len = int(0.4 * SR)
        if delay + seg_len <= len(t):
            ts = np.linspace(0, 0.4, seg_len)
            ns = (sine(ts, f) * 0.4 + sine(ts, f * 1.5) * 0.15) * env(ts, 0.005, 0.05, 0.5, 0.25)
            s[delay:delay+seg_len] += ns
    make("achievement", s)

def sfx_trick():
    t = np.linspace(0, 0.6, int(SR * 0.6))
    s = chirp(t, 400, 1600) * 0.4 + sine(t, 800) * 0.3
    s += noise(len(t)) * 0.08 * env(t, 0.01, 0.1, 0, 0)
    s *= env(t, 0.005, 0.1, 0.4, 0.25)
    make("trick", s)

def sfx_trick_learned():
    t = np.linspace(0, 0.9, int(SR * 0.9))
    s = sine(t, 523) * 0.3 + sine(t, 698) * 0.25 + sine(t, 880) * 0.2 + sine(t, 1047) * 0.15
    s *= env(t, 0.01, 0.1, 0.55, 0.3)
    make("trick_learned", s)

def sfx_interact():
    t = np.linspace(0, 0.2, int(SR * 0.2))
    s = sine(t, 700) * 0.4 + sine(t, 1050) * 0.2
    s *= env(t, 0.005, 0.03, 0.4, 0.1)
    make("interact", s)

def sfx_room_unlock():
    t = np.linspace(0, 1.8, int(SR * 1.8))
    notes = [261, 329, 392, 523, 659, 784]
    s = np.zeros(len(t))
    for i, f in enumerate(notes):
        delay = int(i * 0.18 * SR)
        seg_len = int(0.5 * SR)
        if delay + seg_len <= len(t):
            ts = np.linspace(0, 0.5, seg_len)
            ns = (sine(ts, f) * 0.4 + sine(ts, f * 2) * 0.15) * env(ts, 0.01, 0.07, 0.5, 0.3)
            s[delay:delay+seg_len] += ns
    make("room_unlock", s)

def sfx_page_turn():
    t = np.linspace(0, 0.25, int(SR * 0.25))
    s = noise(len(t)) * 0.4
    s *= env(t, 0.005, 0.02, 0.2, 0.1)
    s += sine(t, 2000) * 0.15 * env(t, 0.005, 0.02, 0, 0)
    make("page_turn", s)

def sfx_evolution():
    t = np.linspace(0, 3.0, int(SR * 3.0))
    # Rising shimmer + final chord
    shimmer = chirp(t[:int(SR*2)], 200, 2000) * 0.3
    shimmer += noise(len(shimmer)) * 0.1
    shimmer *= env(shimmer, 0.1, 0.5, 0.3, 0.5)
    chord_t = np.linspace(0, 1.0, int(SR * 1.0))
    chord = (sine(chord_t, 523) + sine(chord_t, 659) + sine(chord_t, 784) + sine(chord_t, 1047)) * 0.2
    chord *= env(chord_t, 0.02, 0.1, 0.6, 0.4)
    s = np.zeros(len(t))
    s[:len(shimmer)] += shimmer
    s[int(SR*2):int(SR*2)+len(chord)] += chord
    make("evolution", s)

def sfx_miss():
    t = np.linspace(0, 0.3, int(SR * 0.3))
    s = chirp(t, 400, 150) * 0.5
    s *= env(t, 0.005, 0.05, 0.3, 0.15)
    make("miss", s)

def sfx_error():
    t = np.linspace(0, 0.4, int(SR * 0.4))
    s = sine(t, 220) * 0.4 + sine(t, 185) * 0.3
    s *= env(t, 0.005, 0.05, 0.4, 0.2)
    make("error", s)

if __name__ == "__main__":
    sfx_eat(); sfx_cuddle(); sfx_sleep(); sfx_discover()
    sfx_stage_up(); sfx_buy(); sfx_reward(); sfx_achievement()
    sfx_trick(); sfx_trick_learned(); sfx_interact()
    sfx_room_unlock(); sfx_page_turn(); sfx_evolution()
    sfx_miss(); sfx_error()
    print(f"\n✅ {16} SFX generated in {OUT}")
