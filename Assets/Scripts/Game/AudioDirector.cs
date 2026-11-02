using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>Plays the music loop and one-shot effects with a little pitch variety.</summary>
    public sealed class AudioDirector : MonoBehaviour
    {
        AudioSource music;
        AudioSource effects;

        public void Init(AudioClip musicClip)
        {
            music = gameObject.AddComponent<AudioSource>();
            music.clip = musicClip;
            music.loop = true;
            music.volume = 0.32f;
            music.playOnAwake = false;
            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            if (musicClip != null)
            {
                music.Play();
            }
        }

        public void Play(AudioClip clip, float volume = 1f, float pitchJitter = 0.06f)
        {
            if (clip == null || effects == null)
            {
                return;
            }
            effects.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            effects.PlayOneShot(clip, volume);
        }

        /// <summary>Muffles the music (pause and results screens).</summary>
        public void SetMusicDucked(bool ducked)
        {
            if (music != null)
            {
                music.volume = ducked ? 0.12f : 0.32f;
            }
        }
    }
}
