using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Biosfera
{
    /// <summary>
    /// Objeto PERSISTENTE (DontDestroyOnLoad) criado pelo Bootstrap.
    /// A sua única função é remontar todo o jogo sempre que uma cena for
    /// carregada/recarregada — por exemplo, quando o jogador clica em
    /// REINICIAR.
    /// Isso é necessário porque o atributo [RuntimeInitializeOnLoadMethod]
    /// do Bootstrap executa UMA ÚNICA VEZ, no início do jogo; sem este
    /// objeto, o reinício deixaria a cena vazia.
    /// </summary>
    public class BiosferaRunner : MonoBehaviour
    {
        /// <summary>
        /// Assina o evento de cena carregada.
        /// </summary>
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Cancela a assinatura ao ser desativado/destruído.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Remonta o jogo após o carregamento de qualquer cena.
        /// </summary>
        /// <param name="scene">Cena que terminou de carregar.</param>
        /// <param name="mode">Modo de carregamento utilizado.</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Bootstrap.Build();
        }

        /// <summary>
        /// Pede a remontagem do jogo no PRÓXIMO frame. É feito de forma
        /// adiada para nunca destruir objetos no meio do processamento de um
        /// clique da interface (o que poderia gerar erros).
        /// </summary>
        public void RequestRebuild()
        {
            StopAllCoroutines();
            StartCoroutine(DelayedRebuild());
        }

        /// <summary>
        /// Aguarda um frame e então reconstrói o jogo do zero.
        /// </summary>
        /// <returns>Instrução para a coroutine do Unity.</returns>
        private IEnumerator DelayedRebuild()
        {
            yield return null;
            Bootstrap.RebuildNow();
        }
    }
}
