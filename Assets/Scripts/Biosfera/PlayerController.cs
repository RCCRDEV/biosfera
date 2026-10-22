using UnityEngine;

namespace Biosfera
{
    /// <summary>
    /// Controla o jogador (robô S.E.E.D.07). A movimentação horizontal é feita
    /// com Rigidbody.AddForce e o pulo com AddForce (Impulse). A verificação de
    /// chão usa Physics.RaycastNonAlloc.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimento")]
        [Tooltip("Velocidade ao andar.")]
        public float walkSpeed = 5f;
        [Tooltip("Velocidade ao correr (Shift).")]
        public float runSpeed = 8.5f;
        [Tooltip("Força do pulo.")]
        public float jumpForce = 7f;
        [Tooltip("Multiplicador de aceleração do AddForce.")]
        public float acceleration = 18f;

        [Header("Chão")]
        [Tooltip("Distância do raycast de verificação de chão.")]
        public float groundCheckDistance = 0.6f;
        [Tooltip("Máscara de camadas consideradas chão.")]
        public LayerMask groundMask = ~0;

        [Header("Referências")]
        [Tooltip("Transform da câmera (direção relativa).")]
        public Transform cameraTransform;
        [Tooltip("Transform do modelo visual (para rotação/animacão).")]
        public Transform visualTransform;
        [Tooltip("Animator com os 5 estados de animação.")]
        public Animator animator;

        [Header("Feedback de dano e morte")]
        [Tooltip("Material do corpo do robô, piscado em vermelho ao levar dano.")]
        public Material flashMaterial;
        [Tooltip("Cor piscada quando o robô leva dano.")]
        public Color flashColor = new Color(1f, 0.22f, 0.18f);
        [Tooltip("Duração da piscada de dano, em segundos.")]
        public float flashDuration = 0.45f;

        private Rigidbody _rb;
        private bool _isGrounded;
        private float _moveInput;
        private float _turnInput;
        private bool _runInput;
        private string _currentState = "";
        private float _bobTimer;
        private readonly RaycastHit[] _groundHits = new RaycastHit[8];

        private Color _baseBodyColor = Color.white;
        private float _flashTimer;
        private bool _dead;
        private GameObject _deathBurstTemplate;

        /// <summary>
        /// Obtém o Rigidbody, faz fallback da câmera caso não tenha sido
        /// atribuída pelo Bootstrap e prepara o efeito de explosão da morte.
        /// </summary>
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            if (flashMaterial != null)
            {
                if (flashMaterial.HasProperty("_BaseColor"))
                    _baseBodyColor = flashMaterial.GetColor("_BaseColor");
                else if (flashMaterial.HasProperty("_Color"))
                    _baseBodyColor = flashMaterial.GetColor("_Color");
            }

            _deathBurstTemplate = Factory.ParticleTemplate("FX_MorteSeed",
                new Color(0.55f, 0.92f, 0.62f), 70, 0.55f, 6.5f, 1.3f, 0.9f, 0.6f);
        }

        /// <summary>
        /// Assina o fim da fase para tocar a animação de derrota do robô.
        /// </summary>
        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnLevelEnded += HandleLevelEnded;
        }

        /// <summary>
        /// Cancela a assinatura do evento ao destruir o objeto.
        /// </summary>
        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnLevelEnded -= HandleLevelEnded;
        }

        /// <summary>
        /// Reage ao fim da fase: se a derrota foi por perda total de vidas,
        /// o robô S.E.E.D. é desativado (explosão + modelo escondido).
        /// </summary>
        /// <param name="win">true = vitória.</param>
        private void HandleLevelEnded(bool win)
        {
            if (win) return;
            if (GameManager.Instance != null && GameManager.Instance.lives > 0) return; // derrota por tempo
            Die();
        }

        /// <summary>
        /// Desativa o robô: some com o modelo, dispara partículas e bloqueia
        /// o controle. É a consequência de perder todas as vidas.
        /// </summary>
        public void Die()
        {
            if (_dead) return;
            _dead = true;

            if (visualTransform != null) visualTransform.gameObject.SetActive(false);
            Factory.SpawnBurst(_deathBurstTemplate, transform.position + Vector3.up, 1.3f, 2.6f);
        }

        /// <summary>
        /// Lê o input, verifica o chão, trata o pulo e atualiza a animação.
        /// </summary>
        private void Update()
        {
            UpdateDamageFlash();

            if (_dead) return;

            // Atalho de teclado para pausar/despausar. Funciona sempre, mesmo
            // com o cursor travado (é o jeito garantido de pausar sem mouse).
            if ((Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape)) &&
                GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
            }

            if (IsGameplayBlocked()) return;

            _moveInput = Input.GetAxisRaw("Vertical");
            _turnInput = Input.GetAxisRaw("Horizontal");
            _runInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            _isGrounded = CheckGrounded();

            if (_isGrounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")))
                Jump();

            UpdateAnimator();
        }

        /// <summary>
        /// Aplica a movimentação horizontal via AddForce (física).
        /// </summary>
        private void FixedUpdate()
        {
            if (IsGameplayBlocked()) return;

            Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 direction = forward * _moveInput + right * _turnInput;
            if (direction.sqrMagnitude > 1f) direction.Normalize();

            float speed = _runInput ? runSpeed : walkSpeed;
            Vector3 desired = direction * speed;

            Vector3 velocity = _rb.linearVelocity;
            Vector3 delta = new Vector3(desired.x - velocity.x, 0f, desired.z - velocity.z);
            _rb.AddForce(delta * acceleration, ForceMode.Acceleration);
        }

        /// <summary>
        /// Aplica o impulso do pulo (AddForce com ForceMode.Impulse).
        /// </summary>
        private void Jump()
        {
            Vector3 velocity = _rb.linearVelocity;
            velocity.y = 0f;
            _rb.linearVelocity = velocity;
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayJump();
        }

        /// <summary>
        /// Verifica se existe chão logo abaixo do jogador, ignorando os
        /// colliders do PRÓPRIO jogador (e as sementes, que são triggers).
        /// </summary>
        /// <returns>true se estiver apoiado no chão.</returns>
        private bool CheckGrounded()
        {
            Vector3 origin = transform.position + Vector3.up * 0.2f;
            int count = Physics.RaycastNonAlloc(origin, Vector3.down, _groundHits,
                groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider c = _groundHits[i].collider;
                if (c == null) continue;

                // Ignora o próprio jogador e qualquer objeto que seja filho dele.
                // (NÃO se pode comparar "root", porque o chão e o jogador são
                // ambos filhos da raiz "== BIOSFERA ==" e ficariam iguais.)
                if (c.transform == transform) continue;
                if (c.transform.IsChildOf(transform)) continue;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Atualiza o Animator com os 5 estados (Idle, Walk, Run, Jump, Fall)
        /// e aplica a animação procedural do modelo (movimento de flutuação),
        /// garantindo que o robô nunca pareça "congelado".
        /// </summary>
        private void UpdateAnimator()
        {
            float horizontalSpeed = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z).magnitude;

            string state;
            if (!_isGrounded)
                state = _rb.linearVelocity.y > 0.1f ? "Jump" : "Fall";
            else if (horizontalSpeed > runSpeed * 0.75f)
                state = "Run";
            else if (horizontalSpeed > 0.4f)
                state = "Walk";
            else
                state = "Idle";

            if (state != _currentState) _currentState = state;

            // Aciona o Animator (os 5 estados Idle/Walk/Run/Jump/Fall existem
            // no AnimatorController gerado pelo script de Editor).
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                int hash = Animator.StringToHash(_currentState);
                if (animator.HasState(0, hash)) animator.CrossFade(hash, 0.12f);
            }

            // Animação procedural do modelo (sempre ativa: dá vida ao robô
            // mesmo quando os estados do Animator não possuem clipes).
            if (visualTransform != null)
            {
                float speedFactor = Mathf.Clamp01(horizontalSpeed / runSpeed);
                _bobTimer += Time.deltaTime * (2f + speedFactor * 10f);
                float bob = Mathf.Sin(_bobTimer) * (0.06f + 0.12f * speedFactor);
                Vector3 p = visualTransform.localPosition;
                p.y = bob;
                visualTransform.localPosition = p;
            }

            // Rotação do modelo na direção do movimento (requisito "rotação").
            if (visualTransform != null && horizontalSpeed > 0.2f)
            {
                Vector3 dir = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion target = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    visualTransform.rotation = Quaternion.Slerp(visualTransform.rotation, target, 12f * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Aplica um empurrão/repulsão no jogador (usado quando um inimigo
        /// poluente o atinge). Empurra para longe da origem do golpe e
        /// interrompe o movimento atual com um leve ganho de altura.
        /// </summary>
        /// <param name="sourcePosition">Posição de quem causou o golpe.</param>
        /// <param name="force">Intensidade do empurrão.</param>
        public void ApplyKnockback(Vector3 sourcePosition, float force)
        {
            if (_rb == null) return;

            Vector3 away = transform.position - sourcePosition;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f) away = -transform.forward;
            away.Normalize();

            _rb.linearVelocity = new Vector3(0f, _rb.linearVelocity.y, 0f);
            _rb.AddForce(away * force + Vector3.up * (force * 0.55f), ForceMode.Impulse);

            // Pisca o robô de vermelho para o dano ficar bem visível.
            _flashTimer = flashDuration;
        }

        /// <summary>
        /// Faz o corpo do robô piscar na cor de dano e voltar suavemente à
        /// cor original (feedback visual de que levou um golpe).
        /// </summary>
        private void UpdateDamageFlash()
        {
            if (flashMaterial == null) return;

            if (_flashTimer > 0f) _flashTimer -= Time.deltaTime;

            float k = flashDuration > 0f ? Mathf.Clamp01(_flashTimer / flashDuration) : 0f;
            Color c = Color.Lerp(_baseBodyColor, flashColor, k);
            if (flashMaterial.HasProperty("_BaseColor")) flashMaterial.SetColor("_BaseColor", c);
            if (flashMaterial.HasProperty("_Color")) flashMaterial.SetColor("_Color", c);
        }

        /// <summary>
        /// Informa se o jogo está pausado ou finalizado (bloqueando o input).
        /// </summary>
        /// <returns>true se o gameplay está bloqueado.</returns>
        private bool IsGameplayBlocked()
        {
            return GameManager.Instance != null &&
                (GameManager.Instance.levelFinished || GameManager.Instance.isPaused);
        }
    }
}
