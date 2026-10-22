using UnityEngine;

namespace Biosfera
{
    /// <summary>
    /// Câmera de terceira pessoa: orbita ao redor do jogador com o mouse,
    /// segue suavemente (SmoothDamp) e evita atravessar o cenário.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Alvo")]
        [Tooltip("Transform seguido pela câmera (jogador).")]
        public Transform target;

        [Header("Órbita")]
        [Tooltip("Distância da câmera até o alvo.")]
        public float distance = 7f;
        [Tooltip("Altura do ponto observado no alvo.")]
        public float lookHeight = 1.4f;
        [Tooltip("Sensibilidade do mouse.")]
        public float mouseSensitivity = 180f;
        [Tooltip("Ângulo mínimo de inclinação.")]
        public float minPitch = -5f;
        [Tooltip("Ângulo máximo de inclinação.")]
        public float maxPitch = 65f;
        [Tooltip("Tempo de suavização do seguimento.")]
        public float smoothTime = 0.06f;

        [Header("Cursor")]
        [Tooltip("Trava o cursor no centro da tela durante o jogo.")]
        public bool lockCursor = true;

        private float _yaw;
        private float _pitch = 20f;
        private Vector3 _velocity;

        /// <summary>
        /// Inicializa o ângulo horizontal conforme a rotação atual e trava o
        /// cursor.
        /// </summary>
        private void Start()
        {
            _yaw = transform.eulerAngles.y;
            ApplyCursorState();
        }

        /// <summary>
        /// Lê o mouse para orbitar e reposiciona a câmera suavemente.
        /// </summary>
        private void LateUpdate()
        {
            if (target == null)
            {
                GameObject found = GameObject.FindGameObjectWithTag("Player");
                if (found != null) target = found.transform;
                if (target == null) return;
            }

            ApplyCursorState();

            bool blocked = GameManager.Instance != null &&
                (GameManager.Instance.isPaused || GameManager.Instance.levelFinished);

            float dt = Time.unscaledDeltaTime;

            if (!blocked)
            {
                _yaw += Input.GetAxis("Mouse X") * mouseSensitivity * dt;
                _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * dt;
                _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
            }

            Vector3 focus = target.position + Vector3.up * lookHeight;
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 desired = focus - rotation * Vector3.forward * distance;

            RaycastHit hit;
            if (Physics.Linecast(focus, desired, out hit))
            {
                desired = hit.point + hit.normal * 0.35f;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired,
                ref _velocity, smoothTime, Mathf.Infinity, dt);
            transform.rotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
        }

        /// <summary>
        /// Trava/destrava o cursor conforme o estado do jogo e a tecla Escape.
        /// </summary>
        private void ApplyCursorState()
        {
            if (!lockCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            bool blocked = GameManager.Instance != null &&
                (GameManager.Instance.isPaused || GameManager.Instance.levelFinished);

            if (Input.GetKeyDown(KeyCode.Escape) || blocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Input.GetMouseButtonDown(0) || Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
