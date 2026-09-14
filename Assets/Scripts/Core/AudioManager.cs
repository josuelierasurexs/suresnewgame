using UnityEngine;

namespace Surexs.DanceOff.Core
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource musicSource;

        public bool HasClip => musicSource != null && musicSource.clip != null;
        public bool IsPlaying => musicSource != null && musicSource.isPlaying;
        public double DurationSeconds => HasClip ? musicSource.clip.length : 0d;

        public double SongTimeSeconds
        {
            get
            {
                if (!HasClip || musicSource.clip.frequency <= 0)
                {
                    return 0d;
                }

                return (double)musicSource.timeSamples / musicSource.clip.frequency;
            }
        }

        private void Reset()
        {
            musicSource = GetComponent<AudioSource>();
        }

        public void Configure(AudioSource source)
        {
            musicSource = source;
        }

        public bool PlayFromStart()
        {
            if (musicSource == null)
            {
                musicSource = GetComponent<AudioSource>();
            }

            if (!HasClip)
            {
                Debug.LogWarning("[AudioManager] No hay un AudioClip asignado. El reloj musical permanecerá en 0.", this);
                return false;
            }

            musicSource.Stop();
            musicSource.timeSamples = 0;
            musicSource.Play();

            Debug.Log($"[AudioManager] Reproduciendo '{musicSource.clip.name}' ({musicSource.clip.length:F2} s).", this);
            return true;
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }
    }
}
