using UnityEngine;

namespace Biosfera
{
    /// <summary>
    /// Inimigo "Poluente". Patrulha por pontos (waypoints) usando AddForce,
    /// persegue o jogador quando ele se aproxima e é neutralizado quando o
    /// jogador pisa em cima dele (aplicando torque ao ser derrotado).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PollutionEnemy : MonoBehaviour
    {
        [Header("Movimento")]
        [Tooltip("Velocidade de patrulha.")]
        public float patrolSpeed = 2.2f;
        [Tooltip("Velocidade de perseguição.")]
        public float chaseSpeed = 4.2f;
        [Tooltip("Distância em que o inimigo percebe o jogador.")]
        public float detectRange = 10f;
        [Tooltip("Aceleração aplicada via AddForce.")]
        public float acceleration = 24f;
        [Tooltip("Distância para considerar que chegou ao waypoint.")]
        public float waypointReachDistance = 1.4f;

        [Header("Referências")]
        [Tooltip("Transform do jogador (preenchido pelo Bootstrap).")]
        public Transform player;

        [Header("Dano ao jogador")]
        [Tooltip("Segundos descontados do cronômetro cada vez que o poluente encosta no jogador.")]
        public float timePenalty = 5f;
        [Tooltip("Força do empurrão aplicado no jogador.")]
        public float knockbackForce = 9f;
        [Tooltip("Tempo mínimo entre dois golpes do mesmo inimigo, em segundos.")]
        public float damageCooldown = 1.3f;

        [Header("Visual")]
        [Tooltip("Cor da fumaça poluente.")]
        public Color smokeColor = new Color(0.40f, 0.38f, 0.34f);
        [Tooltip("Cor de alerta quando o inimigo está perseguindo o jogador.")]
        public Color alertColor = new Color(1f, 0.28f, 0.18f);

        private Rigidbody _rb;
        private Vector3[] _waypoints;
        private int _waypointIndex;
        private bool _neutralized;
        private ParticleSystem _smoke;
        private GameObject _explosionTemplate;
        private GameObject _debrisTemplate;
        private Vector3 _spawnPosition;
        private Material _bodyMaterial;
        private float _nextHitTime;      // instante (Time.time) em que pode golpear de novo
        private float _alertBlend;       // 0 = calmo, 1 = perseguindo (interpolação visual)

        /// <summary>
        /// Cria os waypoints ao redor do ponto de spawn e instancia os efeitos
        /// de fumaça/explosão/detritos usados pelo inimigo.
        /// </summary>
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _spawnPosition = transform.position;

            _waypoints = new Vector3[4];
            for (int i = 0; i < _waypoints.Length; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                float radius = 4f + Random.Range(0f, 3f);
                _waypoints[i] = _spawnPosition + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            }

            _smoke = Factory.Smoke(transform, smokeColor, 14f);
            _explosionTemplate = Factory.ParticleTemplate("FX_ExplosaoInimigo",
                new Color(0.5f, 0.45f, 0.35f), 40, 0.5f, 5f, 0.9f, 0.6f, 0.6f);
            _debrisTemplate = Factory.ParticleTemplate("FX_DetritosInimigo",
                new Color(0.25f, 0.22f, 0.18f), 24, 0.16f, 4.5f, 1.2f, 1.6f, 0.5f);

            // Material próprio (instância) para poder mudar a cor quando persegue.
            Renderer r = GetComponentInChildren<Renderer>();
            if (r != null)
            {
                _bodyMaterial = r.material;
                Factory.MarkEmissive(_bodyMaterial);
            }
        }

        /// <summary>
        /// Move o inimigo com AddForce: persegue o jogador se estiver perto,
        /// caso contrário patrulha entre os waypoints.
        /// </summary>
        private void FixedUpdate()
        {
            if (_neutralized || _rb == null) return;

            Vector3 target;
            float speed;

            float distanceToPlayer = player != null
                ? Vector3.Distance(Flat(transform.position), Flat(player.position))
                : float.MaxValue;

            bool chasing = player != null && distanceToPlayer < detectRange;
            _alertBlend = Mathf.MoveTowards(_alertBlend, chasing ? 1f : 0f, 2.5f * Time.fixedDeltaTime);

            if (chasing)
            {
                target = player.position;
                speed = chaseSpeed;
            }
            else
            {
                target = _waypoints[_waypointIndex];
                speed = patrolSpeed;
                if (Vector3.Distance(Flat(transform.position), Flat(target)) < waypointReachDistance)
                {
                    _waypointIndex = (_waypointIndex + 1) % _waypoints.Length;
                }
            }

            Vector3 direction = Flat(target - transform.position);
            if (direction.sqrMagnitude > 0.001f)
            {
                direction.Normalize();

                Vector3 velocity = _rb.linearVelocity;
                Vector3 desired = direction * speed;
                Vector3 delta = new Vector3(desired.x - velocity.x, 0f, desired.z - velocity.z);
                _rb.AddForce(delta * acceleration, ForceMode.Acceleration);

                // Rotação suave na direção do movimento.
                Quaternion look = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, 8f * Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Aplica a coloração de alerta (fica avermelhado ao perseguir) e faz
        /// a fumaça crescer. Roda em Update para ser suave.
        /// </summary>
        private void Update()
        {
            if (_neutralized || _bodyMaterial == null) return;

            Color baseColor = Color.Lerp(new Color(0.35f, 0.33f, 0.30f), alertColor, _alertBlend);
            Factory.SetEmission(_bodyMaterial, baseColor * (0.25f + _alertBlend * 1.1f));
            if (_bodyMaterial.HasProperty("_Color")) _bodyMaterial.SetColor("_Color", baseColor);
            if (_bodyMaterial.HasProperty("_BaseColor")) _bodyMaterial.SetColor("_BaseColor", baseColor);
        }

        /// <summary>
        /// Detecta o contato com o jogador: se ele estiver por cima (pisando)
        /// o inimigo é neutralizado; caso contrário o jogador leva o golpe.
        /// </summary>
        /// <param name="collision">Dados da colisão.</param>
        private void OnCollisionEnter(Collision collision)
        {
            HandlePlayerContact(collision);
        }

        /// <summary>
        /// Mantém o contato enquanto o jogador permanece encostado, para que
        /// o inimigo tenha um efeito perceptível (persegue e empurra).
        /// </summary>
        /// <param name="collision">Dados da colisão.</param>
        private void OnCollisionStay(Collision collision)
        {
            HandlePlayerContact(collision);
        }

        /// <summary>
        /// Decide o que acontece no contato com o jogador: pisada neutraliza,
        /// encostar de lado causa dano e empurrão.
        /// </summary>
        /// <param name="collision">Dados da colisão.</param>
        private void HandlePlayerContact(Collision collision)
        {
            if (_neutralized || collision == null) return;
            if (!Factory.HasTag(collision.gameObject, "Player")) return;

            bool playerAbove = collision.transform.position.y > transform.position.y + 0.5f;
            bool falling = collision.rigidbody != null && collision.rigidbody.linearVelocity.y < -0.5f;

            if (playerAbove || falling) Neutralize(collision);
            else HitPlayer(collision);
        }

        /// <summary>
        /// Causa dano ao jogador: tira 1 vida, desconta tempo do cronômetro,
        /// aplica empurrão (knockback), dispara partículas/som e recua o
        /// próprio inimigo. Respeita um intervalo mínimo entre golpes.
        /// </summary>
        /// <param name="collision">Colisão que originou o golpe.</param>
        private void HitPlayer(Collision collision)
        {
            if (Time.time < _nextHitTime) return;

            // O GameManager decide se o golpe conta (vida, tempo e cooldown
            // global). Se não contar, o inimigo não empurra nem gasta o golpe.
            bool counted = true;
            if (GameManager.Instance != null)
                counted = GameManager.Instance.RegisterPlayerHit(timePenalty);
            if (!counted) return;

            _nextHitTime = Time.time + damageCooldown;

            PlayerController pc = collision.gameObject.GetComponentInParent<PlayerController>();
            if (pc != null) pc.ApplyKnockback(transform.position, knockbackForce);

            if (AudioManager.Instance != null) AudioManager.Instance.PlayHurt();

            Factory.SpawnBurst(_debrisTemplate, transform.position + Vector3.up, 1f, 1.6f);

            // O próprio inimigo também recua um pouco ao golpear.
            if (_rb != null)
            {
                Vector3 away = Flat(transform.position - collision.transform.position);
                if (away.sqrMagnitude > 0.0001f)
                    _rb.AddForce(away.normalized * 3.5f, ForceMode.Impulse);
            }
        }

        /// <summary>
        /// Neutraliza o inimigo: aplica torque (giro), dispara partículas de
        /// explosão/fumaça/detritos, registra no GameManager e o destrói.
        /// </summary>
        /// <param name="collision">Colisão opcional (para empurrar o jogador).</param>
        public void Neutralize(Collision collision)
        {
            if (_neutralized) return;
            _neutralized = true;

            if (_rb != null)
            {
                _rb.constraints = RigidbodyConstraints.None;
                _rb.useGravity = false;
                // Torque: gira o inimigo no ar ao ser derrotado (requisito do trabalho).
                _rb.AddTorque(new Vector3(
                    Random.Range(-10f, 10f),
                    Random.Range(-6f, 6f),
                    Random.Range(-10f, 10f)), ForceMode.Impulse);
            }

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Factory.SpawnBurst(_explosionTemplate, transform.position + Vector3.up, 1.3f, 2.6f);
            Factory.SpawnBurst(_debrisTemplate, transform.position + Vector3.up * 0.6f, 1f, 3f);

            if (_smoke != null) _smoke.Stop();

            if (collision != null && collision.rigidbody != null)
            {
                Vector3 v = collision.rigidbody.linearVelocity;
                v.y = 7.5f;
                collision.rigidbody.linearVelocity = v;
            }

            if (GameManager.Instance != null) GameManager.Instance.RegisterEnemyNeutralized();

            Destroy(gameObject, 0.7f);
        }

        /// <summary>
        /// Remove a componente Y de um vetor (trabalha apenas no plano XZ).
        /// </summary>
        /// <param name="v">Vetor de entrada.</param>
        /// <returns>Vetor com Y zerado.</returns>
        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v;
        }
    }
}
