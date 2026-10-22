using UnityEngine;

namespace Biosfera
{
    /// <summary>
    /// Semente coletável. Flutua (transform.Translate), gira (transform.Rotate)
    /// e pulsa brilho emissivo. A detecção de coleta é feita pelo SeedCollector.
    /// </summary>
    public class Seed : MonoBehaviour
    {
        [Header("Animação")]
        [Tooltip("Altura da flutuação, em metros.")]
        public float bobAmplitude = 0.28f;
        [Tooltip("Velocidade da flutuação.")]
        public float bobSpeed = 2f;
        [Tooltip("Velocidade de rotação, em graus por segundo.")]
        public float spinSpeed = 120f;

        [Header("Visual")]
        [Tooltip("Cor do brilho emissivo da semente.")]
        public Color glowColor = new Color(0.35f, 1f, 0.5f);

        private Vector3 _basePosition;
        private Material _materialInstance;
        private bool _collected;

        /// <summary>
        /// Guarda a posição inicial e cria uma cópia do material para poder
        /// animar o brilho emissivo sem afetar outras sementes.
        /// </summary>
        private void Awake()
        {
            _basePosition = transform.position;
            Renderer r = GetComponentInChildren<Renderer>();
            if (r != null)
            {
                _materialInstance = r.material;
                _materialInstance.EnableKeyword("_EMISSION");
            }
        }

        /// <summary>
        /// Anima a flutuação (Translate), a rotação (Rotate) e o brilho.
        /// </summary>
        private void Update()
        {
            if (_collected) return;

            float targetY = _basePosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            float deltaY = targetY - transform.position.y;
            transform.Translate(0f, deltaY, 0f, Space.World);
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

            if (_materialInstance != null && _materialInstance.HasProperty("_EmissionColor"))
            {
                float glow = 1.4f + Mathf.Sin(Time.time * 3f) * 0.5f;
                _materialInstance.SetColor("_EmissionColor", glowColor * glow);
            }
        }

        /// <summary>
        /// Marca a semente como coletada e a desativa.
        /// </summary>
        public void MarkCollected()
        {
            if (_collected) return;
            _collected = true;
            gameObject.SetActive(false);
        }
    }
}
