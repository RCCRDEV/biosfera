using System;
using UnityEngine;

namespace Biosfera
{
    /// <summary>
    /// Gerencia o estado da fase: contadores de sementes/inimigos, tempo,
    /// pausa, vitória e derrota. Implementado como Singleton simples para
    /// que HUD e demais scripts acessem facilmente.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>Instância única do gerenciador (Singleton).</summary>
        public static GameManager Instance { get; private set; }

        [Header("Configuração da Fase")]
        [Tooltip("Tempo total da fase, em segundos.")]
        public float levelTimeLimit = 240f;

        [Tooltip("Quantidade de sementes que devem ser plantadas para vencer.")]
        public int seedsToWin = 5;

        [Tooltip("Vidas do robô. Cada golpe de poluente tira 1 vida; chegando a 0, é DERROTA.")]
        public int maxLives = 3;
        [Tooltip("Tempo mínimo entre dois golpes recebidos (evita perder várias vidas de uma vez).")]
        public float hitCooldown = 1f;

        [Header("Estado (somente leitura em runtime)")]
        [Tooltip("Sementes já coletadas pelo jogador no total.")]
        public int seedsCollected;
        [Tooltip("Sementes já plantadas na zona de restauração.")]
        public int seedsPlanted;
        [Tooltip("Inimigos neutralizados (pisados).")]
        public int enemiesNeutralized;
        [Tooltip("Quantas vezes o jogador foi atingido por um poluente.")]
        public int playerHits;
        [Tooltip("Vidas restantes do robô (começa em maxLives).")]
        public int lives;
        [Tooltip("Motivo da derrota, exibido no título do painel final.")]
        public string loseReason = "TEMPO ESGOTADO";
        [Tooltip("Tempo restante da fase.")]
        public float timeLeft;
        [Tooltip("Se a fase está pausada.")]
        public bool isPaused;
        [Tooltip("Se a fase terminou (vitória ou derrota).")]
        public bool levelFinished;

        /// <summary>Disparado sempre que algum contador muda (atualiza o HUD).</summary>
        public event Action OnStatsChanged;
        /// <summary>Disparado a cada frame com o tempo restante.</summary>
        public event Action<float> OnTimeChanged;
        /// <summary>Disparado quando a fase termina. true = vitória.</summary>
        public event Action<bool> OnLevelEnded;

        /// <summary>Instante (Time.time) a partir do qual o jogador pode ser atingido de novo.</summary>
        private float _nextHitAllowed;

        /// <summary>
        /// Inicializa o Singleton e zera o cronômetro da fase.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            timeLeft = levelTimeLimit;
            lives = maxLives;
            loseReason = "TEMPO ESGOTADO";
        }

        /// <summary>
        /// Conta o tempo para trás enquanto a fase estiver ativa.
        /// </summary>
        private void Update()
        {
            if (levelFinished || isPaused) return;

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                loseReason = "TEMPO ESGOTADO";
                EndLevel(false);
            }
            if (OnTimeChanged != null) OnTimeChanged(timeLeft);
        }

        /// <summary>
        /// Registra a coleta de uma semente e toca o som correspondente.
        /// </summary>
        public void RegisterSeedCollected()
        {
            seedsCollected++;
            if (OnStatsChanged != null) OnStatsChanged();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayCollect();
        }

        /// <summary>
        /// Registra o plantio de uma semente; se atingir a meta, vence a fase.
        /// </summary>
        public void RegisterSeedPlanted()
        {
            seedsPlanted++;
            if (OnStatsChanged != null) OnStatsChanged();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayPlant();

            if (seedsPlanted >= seedsToWin) EndLevel(true);
        }

        /// <summary>
        /// Registra a neutralização de um inimigo.
        /// </summary>
        public void RegisterEnemyNeutralized()
        {
            enemiesNeutralized++;
            if (OnStatsChanged != null) OnStatsChanged();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayNeutralize();
        }

        /// <summary>
        /// Registra que o jogador foi atingido por um poluente: perde 1 VIDA,
        /// desconta tempo do cronômetro (penalidade) e atualiza o HUD. Se as
        /// vidas acabarem, a fase termina em DERROTA ("S.E.E.D. DESATIVADO").
        /// Existe também um intervalo mínimo entre golpes (hitCooldown) para
        /// que dois inimigos encostados ao mesmo tempo não tirem várias vidas.
        /// </summary>
        /// <param name="timePenalty">Segundos a descontar do cronômetro.</param>
        /// <returns>true se o golpe foi contabilizado; false se foi ignorado.</returns>
        public bool RegisterPlayerHit(float timePenalty)
        {
            if (levelFinished) return false;
            if (Time.time < _nextHitAllowed) return false;

            _nextHitAllowed = Time.time + hitCooldown;

            playerHits++;
            lives = Mathf.Max(0, lives - 1);
            timeLeft = Mathf.Max(0f, timeLeft - timePenalty);

            if (OnStatsChanged != null) OnStatsChanged();
            if (OnTimeChanged != null) OnTimeChanged(timeLeft);

            if (lives <= 0)
            {
                loseReason = "S.E.E.D. DESATIVADO";
                EndLevel(false);
            }
            else if (timeLeft <= 0f)
            {
                loseReason = "TEMPO ESGOTADO";
                EndLevel(false);
            }
            return true;
        }

        /// <summary>
        /// Alterna entre pausado e despausado.
        /// </summary>
        public void TogglePause()
        {
            SetPaused(!isPaused);
        }

        /// <summary>
        /// Define o estado de pausa, congelando o tempo do jogo.
        /// </summary>
        /// <param name="paused">true para pausar, false para continuar.</param>
        public void SetPaused(bool paused)
        {
            if (levelFinished) return;
            isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            if (OnStatsChanged != null) OnStatsChanged();
        }

        /// <summary>
        /// Encerra a fase informando se houve vitória.
        /// </summary>
        /// <param name="win">true = vitória, false = derrota.</param>
        public void EndLevel(bool win)
        {
            if (levelFinished) return;
            levelFinished = true;
            isPaused = false;
            Time.timeScale = 1f;

            if (AudioManager.Instance != null)
            {
                if (win) AudioManager.Instance.PlayWin();
                else AudioManager.Instance.PlayLose();
            }
            if (OnLevelEnded != null) OnLevelEnded(win);
        }

        /// <summary>
        /// Reinicia a fase. Se a cena atual estiver registrada no Build
        /// Settings, recarrega-a (o objeto persistente do Bootstrap remonta o
        /// jogo automaticamente). Caso contrário, remonta o jogo em memória,
        /// garantindo que o botão REINICIAR funcione SEMPRE.
        /// </summary>
        public void RestartLevel()
        {
            Time.timeScale = 1f;

            UnityEngine.SceneManagement.Scene scene =
                UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            if (scene.buildIndex >= 0)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(scene.buildIndex);
                return;
            }

            // Alternativa garantida (funciona mesmo sem Build Settings).
            Bootstrap.Rebuild();
        }

        /// <summary>
        /// Sai do jogo (ou para o Play Mode, quando estiver no Editor).
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Texto formatado do tempo restante no padrão MM:SS.
        /// </summary>
        /// <returns>String do tempo no formato MM:SS.</returns>
        public string GetTimeString()
        {
            int total = Mathf.Max(0, Mathf.CeilToInt(timeLeft));
            int minutes = total / 60;
            int seconds = total % 60;
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
