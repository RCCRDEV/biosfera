using UnityEngine;

namespace Biosfera
{
    /// <summary>
    /// Coletor de sementes do jogador. Usa detecção de colisão em trigger
    /// (OnTriggerEnter) para coletar sementes e guarda quantas estão na mão,
    /// prontas para serem plantadas nas zonas de restauração.
    /// </summary>
    public class SeedCollector : MonoBehaviour
    {
        /// <summary>Quantidade de sementes coletadas e ainda não plantadas.</summary>
        public int SeedsInHand { get; private set; }

        [Tooltip("Prefab/template de partículas usado ao coletar.")]
        public GameObject pickupParticles;

        /// <summary>
        /// Cria o template de partículas de coleta.
        /// </summary>
        private void Awake()
        {
            if (pickupParticles == null)
            {
                pickupParticles = Factory.ParticleTemplate("FX_ColetarSemente",
                    new Color(0.45f, 1f, 0.55f), 28, 0.18f, 3.4f, 0.7f, 0.4f, 0.4f);
            }
        }

        /// <summary>
        /// Detecta a entrada em um trigger; se for uma semente, coleta.
        /// </summary>
        /// <param name="other">Collider que entrou no trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;

            Seed seed = other.GetComponentInParent<Seed>();
            if (seed == null) return;

            Vector3 position = seed.transform.position;
            seed.MarkCollected();

            SeedsInHand++;
            Factory.SpawnBurst(pickupParticles, position, 1f, 1.6f);

            if (GameManager.Instance != null) GameManager.Instance.RegisterSeedCollected();
        }

        /// <summary>
        /// Consome uma semente da mão do jogador (usado ao plantar).
        /// </summary>
        /// <returns>true se havia semente disponível.</returns>
        public bool ConsumeSeed()
        {
            if (SeedsInHand <= 0) return false;
            SeedsInHand--;
            return true;
        }
    }
}
