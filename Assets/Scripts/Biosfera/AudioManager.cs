using UnityEngine;

namespace Biosfera
{
    /// <summary>
    /// Gerenciador de áudio do jogo. Cria todos os sons por código
    /// (procedurais) e oferece métodos curtos para tocar cada efeito.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        /// <summary>Instância única (Singleton).</summary>
        public static AudioManager Instance { get; private set; }

        [Header("Volumes")]
        [Range(0f, 1f)] [Tooltip("Volume dos efeitos sonoros.")]
        public float sfxVolume = 0.7f;
        [Range(0f, 1f)] [Tooltip("Volume da trilha sonora.")]
        public float musicVolume = 0.22f;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;

        private AudioClip _collectClip;
        private AudioClip _plantClip;
        private AudioClip _neutralizeClip;
        private AudioClip _jumpClip;
        private AudioClip _hurtClip;
        private AudioClip _winClip;
        private AudioClip _loseClip;
        private AudioClip _musicClip;

        /// <summary>
        /// Cria as fontes de áudio e gera todos os clipes procedurais.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = musicVolume;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = sfxVolume;

            _collectClip = Factory.Tone("SFX_Coletar", 880f, 1400f, 0.18f, 0f, 0.55f);
            _plantClip = Factory.Tone("SFX_Plantar", 300f, 520f, 0.35f, 0.06f, 0.55f);
            _neutralizeClip = Factory.Tone("SFX_Neutralizar", 260f, 60f, 0.5f, 0.35f, 0.7f);
            _jumpClip = Factory.Tone("SFX_Pulo", 480f, 820f, 0.14f, 0f, 0.45f);
            _hurtClip = Factory.Tone("SFX_Dano", 200f, 90f, 0.28f, 0.5f, 0.7f);
            _winClip = Factory.Tone("SFX_Vitoria", 523f, 1046f, 1.0f, 0f, 0.6f);
            _loseClip = Factory.Tone("SFX_Derrota", 420f, 110f, 1.0f, 0.12f, 0.6f);
            _musicClip = Factory.Ambient("BGM_Biosfera", 130.81f, 6f, 0.5f);
        }

        /// <summary>
        /// Inicia a trilha sonora em loop.
        /// </summary>
        private void Start()
        {
            PlayMusic();
        }

        /// <summary>
        /// Toca a trilha sonora em loop.
        /// </summary>
        public void PlayMusic()
        {
            if (_musicSource == null || _musicClip == null) return;
            _musicSource.clip = _musicClip;
            if (!_musicSource.isPlaying) _musicSource.Play();
        }

        /// <summary>
        /// Toca um efeito sonoro avulso.
        /// </summary>
        /// <param name="clip">Clipe a tocar.</param>
        /// <param name="volumeScale">Multiplicador de volume (0..1).</param>
        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale) * sfxVolume);
        }

        /// <summary>Toca o som de coleta de semente.</summary>
        public void PlayCollect() { PlaySfx(_collectClip); }

        /// <summary>Toca o som de plantio de semente.</summary>
        public void PlayPlant() { PlaySfx(_plantClip); }

        /// <summary>Toca o som de neutralização de inimigo.</summary>
        public void PlayNeutralize() { PlaySfx(_neutralizeClip); }

        /// <summary>Toca o som de pulo.</summary>
        public void PlayJump() { PlaySfx(_jumpClip); }

        /// <summary>Toca o som de dano (quando um poluente atinge o jogador).</summary>
        public void PlayHurt() { PlaySfx(_hurtClip); }

        /// <summary>Toca o som de vitória.</summary>
        public void PlayWin() { PlaySfx(_winClip); }

        /// <summary>Toca o som de derrota.</summary>
        public void PlayLose() { PlaySfx(_loseClip); }
    }
}
