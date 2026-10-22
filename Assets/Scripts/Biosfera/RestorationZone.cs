using UnityEngine;

namespace Biosfera
{
    /// <summary>
    /// Zona de Restauração. Quando o jogador entra com sementes em mãos, o
    /// plantio acontece automaticamente em intervalos regulares: cada semente
    /// plantada instancia uma vegetação aleatória (Instantiate sobre um vetor
    /// de prefabs) e faz o "terreno" ficar mais verde.
    /// </summary>
    public class RestorationZone : MonoBehaviour
    {
        [Header("Referências")]
        [Tooltip("Coletor do jogador, fonte das sementes a plantar.")]
        public SeedCollector collector;
        [Tooltip("Raiz onde a vegetação plantada será criada (opcional).")]
        public Transform growthRoot;

        [Header("Plantio")]
        [Tooltip("Intervalo entre plantios, em segundos.")]
        public float plantInterval = 0.9f;
        [Tooltip("Raio em que a vegetação é posicionada.")]
        public float plantRadius = 6f;

        [Header("Vetor de prefabs de vegetação (GameObject[])")]
        [Tooltip("Prefabs usados para crescer a vegetação ao plantar.")]
        public GameObject[] vegetationPrefabs;

        [Header("Visual")]
        [Tooltip("Renderer do disco que representa a zona.")]
        public Renderer zoneRenderer;
        [Tooltip("Cor da zona quando totalmente restaurada.")]
        public Color restoredColor = new Color(0.30f, 0.85f, 0.35f);
        [Tooltip("Cor da zona quando degradada.")]
        public Color degradedColor = new Color(0.35f, 0.30f, 0.26f);

        private float _timer;
        private bool _playerInside;
        private Material _zoneMaterial;
        private GameObject _growthTemplate;
        private int _plantedCount;

        /// <summary>
        /// Cria o template de partículas usado no plantio.
        /// </summary>
        private void Awake()
        {
            _growthTemplate = Factory.ParticleTemplate("FX_Plantio",
                new Color(0.45f, 1f, 0.55f), 30, 0.2f, 2.8f, 0.9f, -0.3f, 0.5f);
        }

        /// <summary>
        /// Prepara o material da zona. Feito no Start (e não no Awake) porque
        /// o Bootstrap atribui o zoneRenderer logo após criar o componente.
        /// </summary>
        private void Start()
        {
            if (zoneRenderer != null && _zoneMaterial == null)
            {
                _zoneMaterial = zoneRenderer.material;
                ApplyZoneColor(0f);
            }
            _timer = plantInterval;
        }

        /// <summary>
        /// Marca que o jogador está dentro da zona.
        /// </summary>
        /// <param name="other">Collider que entrou no trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.CompareTag("Player")) _playerInside = true;
        }

        /// <summary>
        /// Mantém a marcação enquanto o jogador permanece na zona.
        /// </summary>
        /// <param name="other">Collider que permanece no trigger.</param>
        private void OnTriggerStay(Collider other)
        {
            if (other != null && other.CompareTag("Player")) _playerInside = true;
        }

        /// <summary>
        /// Desmarca quando o jogador sai da zona.
        /// </summary>
        /// <param name="other">Collider que saiu do trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.CompareTag("Player")) _playerInside = false;
        }

        /// <summary>
        /// Executa o plantio em intervalos quando há jogador dentro da zona
        /// com sementes disponíveis.
        /// </summary>
        private void Update()
        {
            if (GameManager.Instance != null &&
                (GameManager.Instance.levelFinished || GameManager.Instance.isPaused))
                return;

            if (!_playerInside)
            {
                _timer = plantInterval;
                return;
            }
            if (collector == null || collector.SeedsInHand <= 0) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = plantInterval;
                PlantOne();
            }
        }

        /// <summary>
        /// Consome uma semente da mão do jogador, instancia uma vegetação
        /// aleatória na zona, dispara partículas e atualiza a cor do terreno.
        /// </summary>
        private void PlantOne()
        {
            if (!collector.ConsumeSeed()) return;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(0.5f, plantRadius);
            Vector3 position = transform.position + new Vector3(
                Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

            if (vegetationPrefabs != null && vegetationPrefabs.Length > 0)
            {
                GameObject prefab = vegetationPrefabs[Random.Range(0, vegetationPrefabs.Length)];
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab, position,
                        Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                    if (growthRoot != null) go.transform.SetParent(growthRoot, true);
                    go.transform.localScale = prefab.transform.localScale * Random.Range(0.8f, 1.25f);
                    go.SetActive(true);
                }
            }

            Factory.SpawnBurst(_growthTemplate, position + Vector3.up * 0.7f, 1f, 2.2f);

            _plantedCount++;
            ApplyZoneColor(GetProgress());

            if (GameManager.Instance != null) GameManager.Instance.RegisterSeedPlanted();
        }

        /// <summary>
        /// Calcula o progresso de restauração (0..1) com base na meta de vitória.
        /// </summary>
        /// <returns>Fração entre 0 e 1.</returns>
        private float GetProgress()
        {
            int goal = (GameManager.Instance != null) ? GameManager.Instance.seedsToWin : 5;
            if (goal <= 0) goal = 1;
            return Mathf.Clamp01((float)_plantedCount / goal);
        }

        /// <summary>
        /// Aplica na cor do material a transição de degradado para restaurado.
        /// </summary>
        /// <param name="t">Progresso de 0 a 1.</param>
        private void ApplyZoneColor(float t)
        {
            if (_zoneMaterial == null) return;

            Color c = Color.Lerp(degradedColor, restoredColor, t);
            if (_zoneMaterial.HasProperty("_BaseColor")) _zoneMaterial.SetColor("_BaseColor", c);
            if (_zoneMaterial.HasProperty("_Color")) _zoneMaterial.SetColor("_Color", c);
            if (_zoneMaterial.HasProperty("_EmissionColor"))
            {
                _zoneMaterial.EnableKeyword("_EMISSION");
                _zoneMaterial.SetColor("_EmissionColor", restoredColor * (t * 0.6f));
            }
        }

        /// <summary>
        /// Quantidade de vegetações já plantadas nesta zona.
        /// </summary>
        public int PlantedCount { get { return _plantedCount; } }
    }
}
