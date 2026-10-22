using UnityEngine;
using UnityEngine.UI;

namespace Biosfera
{
    /// <summary>
    /// Gerencia toda a interface: HUD (textos informativos + botão de pausa)
    /// e os painéis de Pausa, Vitória e Derrota — todos com o BOTÃO
    /// REINICIAR obrigatório. Conecta-se ao GameManager por eventos.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [Tooltip("Texto com as sementes coletadas e plantadas.")]
        public Text seedsText;
        [Tooltip("Texto com as vidas (corações) restantes do robô.")]
        public Text livesText;
        [Tooltip("Texto do cronômetro.")]
        public Text timerText;
        [Tooltip("Texto de dica/objetivo.")]
        public Text hintText;
        [Tooltip("Botão de pausa do HUD.")]
        public Button pauseButton;

        [Header("Painel de Pausa")]
        public GameObject pausePanel;
        public Text pauseStatsText;
        public Button pauseResumeButton;
        public Button pauseRestartButton;
        public Button pauseExitButton;

        [Header("Painel de Vitória")]
        public GameObject winPanel;
        public Text winStatsText;
        public Button winRestartButton;
        public Button winExitButton;

        [Header("Painel de Derrota")]
        public GameObject losePanel;
        public Text loseTitleText;
        public Text loseStatsText;
        public Button loseRestartButton;
        public Button loseExitButton;

        /// <summary>
        /// Conecta os cliques dos botões e assina os eventos do GameManager.
        /// Feito no Start (e não no Awake) porque o Bootstrap atribui as
        /// referências logo após criar o componente.
        /// </summary>
        private void Start()
        {
            WireButton(pauseButton, TogglePause);
            WireButton(pauseResumeButton, Resume);
            WireButton(pauseRestartButton, Restart);
            WireButton(pauseExitButton, Quit);
            WireButton(winRestartButton, Restart);
            WireButton(winExitButton, Quit);
            WireButton(loseRestartButton, Restart);
            WireButton(loseExitButton, Quit);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStatsChanged += Refresh;
                GameManager.Instance.OnTimeChanged += OnTimeChanged;
                GameManager.Instance.OnLevelEnded += OnLevelEnded;
            }
            HidePanels();
            Refresh();
        }

        /// <summary>
        /// Cancela as assinaturas ao destruir o objeto.
        /// </summary>
        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStatsChanged -= Refresh;
                GameManager.Instance.OnTimeChanged -= OnTimeChanged;
                GameManager.Instance.OnLevelEnded -= OnLevelEnded;
            }
        }

        /// <summary>
        /// Conecta um botão a uma ação, ignorando botões nulos.
        /// </summary>
        /// <param name="button">Botão a conectar.</param>
        /// <param name="action">Ação disparada no clique.</param>
        private static void WireButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        /// <summary>Alterna a pausa do jogo.</summary>
        private void TogglePause()
        {
            if (GameManager.Instance != null) GameManager.Instance.TogglePause();
            Refresh();
        }

        /// <summary>Despausa o jogo.</summary>
        private void Resume()
        {
            if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
            Refresh();
        }

        /// <summary>Reinicia a fase atual.</summary>
        private void Restart()
        {
            if (GameManager.Instance != null) GameManager.Instance.RestartLevel();
        }

        /// <summary>Sai do jogo.</summary>
        private void Quit()
        {
            if (GameManager.Instance != null) GameManager.Instance.QuitGame();
        }

        /// <summary>
        /// Atualiza os textos do HUD e o estado dos painéis.
        /// </summary>
        public void Refresh()
        {
            if (GameManager.Instance == null) return;
            GameManager g = GameManager.Instance;

            if (seedsText != null)
                seedsText.text = "Sementes: " + g.seedsCollected + " coletadas | " +
                    g.seedsPlanted + " / " + g.seedsToWin + " plantadas";

            if (livesText != null)
                livesText.text = "VIDA: " + g.lives + " / " + g.maxLives + BuildHearts(g.lives, g.maxLives);

            if (timerText != null)
                timerText.text = g.GetTimeString();

            if (hintText != null)
                hintText.text = "WASD mover | Shift correr | Espaco pular | P ou ESC pausar | " +
                    "Colete as sementes e leve ate a Zona de Restauracao";

            bool ended = g.levelFinished;
            if (pausePanel != null) pausePanel.SetActive(g.isPaused && !ended);
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);
        }

        /// <summary>
        /// Monta a representação visual das vidas em blocos cheios/vazios
        /// (evita depender de glifos especiais que a fonte padrão pode não ter).
        /// </summary>
        /// <param name="lives">Vidas atuais.</param>
        /// <param name="max">Vidas máximas.</param>
        /// <returns>String no formato " [###] [---] ".</returns>
        private static string BuildHearts(int lives, int max)
        {
            if (max <= 0) return "";
            string s = "   ";
            for (int i = 0; i < max; i++)
                s += (i < lives) ? "[##]" : "[  ]";
            return s;
        }

        /// <summary>
        /// Atualiza apenas o texto do cronômetro.
        /// </summary>
        /// <param name="timeLeft">Tempo restante em segundos.</param>
        private void OnTimeChanged(float timeLeft)
        {
            if (timerText != null && GameManager.Instance != null)
                timerText.text = GameManager.Instance.GetTimeString();
        }

        /// <summary>
        /// Exibe o painel final de vitória ou derrota com as estatísticas.
        /// </summary>
        /// <param name="win">true = vitória.</param>
        private void OnLevelEnded(bool win)
        {
            HidePanels();
            if (GameManager.Instance == null) return;
            GameManager g = GameManager.Instance;

            string stats = "Sementes coletadas: " + g.seedsCollected +
                "\nSementes plantadas: " + g.seedsPlanted + " / " + g.seedsToWin +
                "\nInimigos neutralizados: " + g.enemiesNeutralized +
                "\nGolpes recebidos: " + g.playerHits +
                "\nTempo restante: " + g.GetTimeString();

            if (win)
            {
                if (winPanel != null) winPanel.SetActive(true);
                if (winStatsText != null) winStatsText.text = stats;
            }
            else
            {
                if (losePanel != null) losePanel.SetActive(true);
                if (loseTitleText != null) loseTitleText.text = g.loseReason;
                if (loseStatsText != null)
                    loseStatsText.text = "Vidas restantes: " + g.lives + " / " + g.maxLives + "\n" + stats;
            }
        }

        /// <summary>Esconde todos os painéis de estado.</summary>
        private void HidePanels()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (winPanel != null) winPanel.SetActive(false);
            if (losePanel != null) losePanel.SetActive(false);
        }
    }
}
