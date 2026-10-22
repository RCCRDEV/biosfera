using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Biosfera
{
    /// <summary>
    /// Orquestrador "ZERO-CLIQUE" do jogo Biosfera (1º Bimestre).
    /// Executa automaticamente ao entrar no Play Mode
    /// (RuntimeInitializeOnLoadMethod) e monta TODA a cena por código:
    /// iluminação, chão, jogador, câmera, gerenciadores, UI completa,
    /// zonas de restauração e o LevelSpawner (que instancia os 134 assets).
    /// Nada precisa ser arrastado ou configurado manualmente no Editor.
    /// </summary>
    public static class Bootstrap
    {
        /// <summary>Nome do objeto raiz que agrupa tudo o que foi criado.</summary>
        public const string RootName = "== BIOSFERA ==";

        /// <summary>Raio (em metros) de cada Zona de Restauração.</summary>
        private const float ZoneRadius = 5f;

        private static Font _font;

        /// <summary>Objeto persistente que remonta o jogo a cada recarga de cena.</summary>
        private static BiosferaRunner _runner;

        /// <summary>
        /// Ponto de entrada automático: o Unity chama este método assim que a
        /// cena termina de carregar, antes do primeiro Update. Isso permite
        /// rodar o jogo a partir de QUALQUER cena, inclusive vazia.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            EnsureRunner();
            Build();
        }

        /// <summary>
        /// Cria (uma única vez) o objeto persistente responsável por remontar
        /// o jogo sempre que a cena for recarregada pelo botão REINICIAR.
        /// </summary>
        private static void EnsureRunner()
        {
            if (_runner != null) return;

            GameObject runnerGO = new GameObject("BiosferaRunner");
            Object.DontDestroyOnLoad(runnerGO);
            _runner = runnerGO.AddComponent<BiosferaRunner>();
        }

        /// <summary>
        /// Constrói o jogo completo. Se já existir uma instância montada
        /// (objeto raiz presente ou um GameManager ativo), nada é feito.
        /// </summary>
        public static void Build()
        {
            if (GameObject.Find(RootName) != null) return;
            if (Object.FindFirstObjectByType<GameManager>() != null) return;

            Time.timeScale = 1f;

            GameObject root = new GameObject(RootName);

            SetupEnvironment();
            CreateGround(root.transform);
            CreateWorld(root.transform);

            // A câmera é criada ANTES do jogador para que "Camera.main" já
            // exista quando o PlayerController for inicializado.
            Camera camera = SetupCamera(root.transform);

            CreateManager<GameManager>("GameManager", root.transform);
            CreateManager<AudioManager>("AudioManager", root.transform);

            GameObject player = CreatePlayer(root.transform);
            WireCameraFollow(camera, player);

            LevelSpawner spawner = CreateLevelSpawner(root.transform, player);

            CreateRestorationZones(root.transform, spawner, player);

            CreateUserInterface(root.transform);

            Debug.Log("<color=green>[BIOSFERA]</color> Cena montada automaticamente pelo Bootstrap. " +
                "Total de assets instanciados: " + spawner.TotalSpawned +
                " (" + spawner.DistinctPrefabCount + " modelos diferentes).");
        }

        /// <summary>
        /// Reinicia a fase remontando o jogo do zero. Usa o objeto persistente
        /// para adiar a remontagem em um frame (seguro mesmo quando chamado no
        /// meio do clique de um botão). Serve de alternativa garantida caso a
        /// cena não esteja registrada no Build Settings.
        /// </summary>
        public static void Rebuild()
        {
            if (_runner != null)
            {
                _runner.RequestRebuild();
                return;
            }
            RebuildNow();
        }

        /// <summary>
        /// Destrói imediatamente a montagem atual e reconstrói o jogo inteiro.
        /// Deve ser chamado fora do processamento de eventos da interface
        /// (por isso o Rebuild o agenda para o próximo frame).
        /// </summary>
        public static void RebuildNow()
        {
            Time.timeScale = 1f;

            GameObject root = GameObject.Find(RootName);
            if (root != null) Object.DestroyImmediate(root);

            GameManager[] managers = Object.FindObjectsByType<GameManager>(FindObjectsSortMode.None);
            for (int i = 0; i < managers.Length; i++) Object.DestroyImmediate(managers[i].gameObject);

            AudioManager[] audios = Object.FindObjectsByType<AudioManager>(FindObjectsSortMode.None);
            for (int i = 0; i < audios.Length; i++) Object.DestroyImmediate(audios[i].gameObject);

            Build();
        }

        // ------------------------------------------------------------------
        // AMBIENTE
        // ------------------------------------------------------------------

        /// <summary>
        /// Ajusta a iluminação ambiente, névoa e cria a luz direcional (Sol)
        /// caso a cena não possua uma.
        /// </summary>
        private static void SetupEnvironment()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.52f, 0.58f, 0.55f);
            RenderSettings.ambientEquatorColor = new Color(0.38f, 0.42f, 0.38f);
            RenderSettings.ambientGroundColor = new Color(0.20f, 0.20f, 0.18f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.72f, 0.79f, 0.75f);
            RenderSettings.fogStartDistance = 65f;
            RenderSettings.fogEndDistance = 320f;

            Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional) return;
            }

            GameObject sun = new GameObject("Sol");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.86f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
        }

        /// <summary>
        /// Cria o chão jogável plano de 90x90 metros com a Tag "Ground".
        /// </summary>
        /// <param name="parent">Transform pai (raiz do jogo).</param>
        private static void CreateGround(Transform parent)
        {
            Material mat = Factory.Material(new Color(0.30f, 0.36f, 0.24f));
            GameObject ground = Factory.Primitive(PrimitiveType.Plane, "Chao", parent,
                Vector3.zero, new Vector3(9f, 1f, 9f), mat, true);

            Factory.SetTag(ground, "Ground");
        }

        // ------------------------------------------------------------------
        // MUNDO AO REDOR (cenário de fundo, para o mapa não ficar "no vazio")
        // ------------------------------------------------------------------

        /// <summary>
        /// Monta todo o mundo visível ao redor da área jogável: terreno
        /// distante, floresta de fundo, montanhas no horizonte e paredes
        /// invisíveis que impedem o jogador de cair para fora do mapa.
        /// </summary>
        /// <param name="parent">Transform pai (raiz do jogo).</param>
        private static void CreateWorld(Transform parent)
        {
            GameObject world = new GameObject("Mundo");
            world.transform.SetParent(parent, false);

            CreateDistantTerrain(world.transform);
            CreateDistantForest(world.transform);
            CreateDistantHills(world.transform);
            CreateBoundaryWalls(parent);
        }

        /// <summary>
        /// Cria um terreno gigante (700 x 700 m) bem abaixo do chão jogável,
        /// para que não exista "vazio" ao redor do mapa.
        /// </summary>
        /// <param name="parent">Transform pai (objeto "Mundo").</param>
        private static void CreateDistantTerrain(Transform parent)
        {
            Material far = Factory.Material(new Color(0.20f, 0.27f, 0.16f));
            Factory.Primitive(PrimitiveType.Plane, "TerrenoDistante", parent,
                new Vector3(0f, -0.06f, 0f), new Vector3(70f, 1f, 70f), far, false);
        }

        /// <summary>
        /// Cria um anel de árvores grandes ao redor do mapa (fora da área
        /// jogável), formando a "floresta" que fecha o horizonte.
        /// </summary>
        /// <param name="parent">Transform pai (objeto "Mundo").</param>
        private static void CreateDistantForest(Transform parent)
        {
            GameObject forest = new GameObject("FlorestaDistante");
            forest.transform.SetParent(parent, false);

            Material trunk = Factory.Material(new Color(0.25f, 0.18f, 0.12f));
            Material[] canopies = new Material[6];
            for (int i = 0; i < canopies.Length; i++)
            {
                float hue = 0.27f + i * 0.017f;
                canopies[i] = Factory.Material(Color.HSVToRGB(hue, 0.42f, 0.26f + i * 0.035f));
            }

            for (int i = 0; i < 150; i++)
            {
                float angle = (i / 150f) * Mathf.PI * 2f + Random.Range(-0.04f, 0.04f);
                float radius = Random.Range(48f, 108f);
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                float h = Random.Range(4f, 9.5f);

                Factory.Primitive(PrimitiveType.Cylinder, "Tronco", forest.transform,
                    new Vector3(x, h * 0.5f, z), new Vector3(0.55f, h * 0.5f, 0.55f), trunk, false);

                Factory.Primitive(PrimitiveType.Sphere, "Copa", forest.transform,
                    new Vector3(x, h + 1.5f, z),
                    new Vector3(Random.Range(3.6f, 5.4f), Random.Range(3.0f, 4.6f), Random.Range(3.6f, 5.4f)),
                    canopies[i % canopies.Length], false);
            }
        }

        /// <summary>
        /// Cria montanhas arredondadas bem distantes, no horizonte, para dar
        /// profundidade ao cenário de fundo.
        /// </summary>
        /// <param name="parent">Transform pai (objeto "Mundo").</param>
        private static void CreateDistantHills(Transform parent)
        {
            GameObject hills = new GameObject("MontanhasDistantes");
            hills.transform.SetParent(parent, false);

            Material[] mats = new Material[4];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = Factory.Material(Color.HSVToRGB(0.32f + i * 0.015f, 0.16f, 0.24f + i * 0.05f));

            for (int i = 0; i < 40; i++)
            {
                float angle = (i / 40f) * Mathf.PI * 2f + Random.Range(-0.06f, 0.06f);
                float radius = Random.Range(170f, 300f);
                float w = Random.Range(38f, 85f);
                float h = Random.Range(28f, 70f);

                Factory.Primitive(PrimitiveType.Sphere, "Monte", hills.transform,
                    new Vector3(Mathf.Cos(angle) * radius, -h * 0.12f, Mathf.Sin(angle) * radius),
                    new Vector3(w, h, w), mats[i % mats.Length], false);
            }
        }

        /// <summary>
        /// Cria quatro paredes invisíveis nos limites do chão jogável, para o
        /// jogador não cair para fora do mapa.
        /// </summary>
        /// <param name="parent">Transform pai (raiz do jogo).</param>
        private static void CreateBoundaryWalls(Transform parent)
        {
            GameObject walls = new GameObject("Limites");
            walls.transform.SetParent(parent, false);

            Vector3[] offsets =
            {
                new Vector3(0f, 4f, 44.5f),
                new Vector3(0f, 4f, -44.5f),
                new Vector3(44.5f, 4f, 0f),
                new Vector3(-44.5f, 4f, 0f)
            };
            Vector3[] sizes =
            {
                new Vector3(92f, 8f, 1f),
                new Vector3(92f, 8f, 1f),
                new Vector3(1f, 8f, 92f),
                new Vector3(1f, 8f, 92f)
            };

            for (int i = 0; i < offsets.Length; i++)
            {
                GameObject wall = new GameObject("Limite_" + i);
                wall.transform.SetParent(walls.transform, false);
                wall.transform.localPosition = offsets[i];

                BoxCollider box = wall.AddComponent<BoxCollider>();
                box.size = sizes[i];
            }
        }

        // ------------------------------------------------------------------
        // CÂMERA
        // ------------------------------------------------------------------

        /// <summary>
        /// Reutiliza a câmera principal da cena, ou cria uma nova caso não
        /// exista. Garante também a presença de um AudioListener.
        /// </summary>
        /// <param name="parent">Transform pai (raiz do jogo).</param>
        /// <returns>A câmera configurada.</returns>
        private static Camera SetupCamera(Transform parent)
        {
            Camera camera = Camera.main;

            if (camera == null)
            {
                GameObject camGO = new GameObject("Main Camera");
                camGO.transform.SetParent(parent, false);
                camGO.transform.position = new Vector3(0f, 6f, -9f);

                camera = camGO.AddComponent<Camera>();
                camera.fieldOfView = 62f;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 700f;
                camera.clearFlags = CameraClearFlags.Skybox;

                Factory.SetTag(camGO, "MainCamera");
            }

            if (Object.FindFirstObjectByType<AudioListener>() == null &&
                camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }

            return camera;
        }

        /// <summary>
        /// Adiciona (ou reaproveita) o CameraFollow na câmera e define o
        /// jogador como alvo.
        /// </summary>
        /// <param name="camera">Câmera a controlar.</param>
        /// <param name="player">Objeto do jogador.</param>
        /// <returns>O componente CameraFollow configurado.</returns>
        private static CameraFollow WireCameraFollow(Camera camera, GameObject player)
        {
            CameraFollow follow = camera.GetComponent<CameraFollow>();
            if (follow == null) follow = camera.gameObject.AddComponent<CameraFollow>();

            follow.target = player.transform;
            follow.distance = 8f;
            follow.lookHeight = 1.6f;
            follow.lockCursor = true;
            return follow;
        }

        // ------------------------------------------------------------------
        // GERENCIADORES
        // ------------------------------------------------------------------

        /// <summary>
        /// Cria um GameObject com um componente específico e o adiciona à raiz.
        /// </summary>
        /// <typeparam name="T">Tipo de MonoBehaviour a criar.</typeparam>
        /// <param name="name">Nome do GameObject.</param>
        /// <param name="parent">Transform pai (raiz do jogo).</param>
        /// <returns>O componente criado.</returns>
        private static T CreateManager<T>(string name, Transform parent) where T : MonoBehaviour
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }

        // ------------------------------------------------------------------
        // JOGADOR
        // ------------------------------------------------------------------

        /// <summary>
        /// Cria o robô S.E.E.D.07: Rigidbody + CapsuleCollider, modelo visual
        /// (corpo, cabeça e olho), Animator com os 5 estados, PlayerController
        /// e SeedCollector. A Tag "Player" é aplicada para as colisões.
        /// </summary>
        /// <param name="parent">Transform pai (raiz do jogo).</param>
        /// <returns>O GameObject do jogador.</returns>
        private static GameObject CreatePlayer(Transform parent)
        {
            GameObject player = new GameObject("Player");
            player.transform.SetParent(parent, false);
            player.transform.position = new Vector3(0f, 1.2f, 0f);
            Factory.SetTag(player, "Player");

            // --- Corpo físico -------------------------------------------------
            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.mass = 1.2f;
            rb.linearDamping = 0.15f;
            rb.angularDamping = 4f;
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            CapsuleCollider capsule = player.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 0.9f, 0f);
            capsule.height = 1.8f;
            capsule.radius = 0.42f;

            // --- Modelo visual ------------------------------------------------
            GameObject model = new GameObject("Modelo");
            model.transform.SetParent(player.transform, false);

            Material bodyMat = Factory.Material(new Color(0.86f, 0.87f, 0.90f));
            Material accentMat = Factory.Material(new Color(0.22f, 0.62f, 0.42f), 0.4f);
            Material eyeMat = Factory.Material(new Color(0.45f, 1f, 0.75f), 2.4f);

            Factory.Primitive(PrimitiveType.Capsule, "Corpo", model.transform,
                new Vector3(0f, 0.85f, 0f), new Vector3(0.85f, 0.85f, 0.85f), bodyMat, false);
            Factory.Primitive(PrimitiveType.Sphere, "Cabeca", model.transform,
                new Vector3(0f, 1.72f, 0f), Vector3.one * 0.62f, bodyMat, false);
            Factory.Primitive(PrimitiveType.Sphere, "Olho", model.transform,
                new Vector3(0f, 1.76f, 0.26f), Vector3.one * 0.22f, eyeMat, false);
            Factory.Primitive(PrimitiveType.Cylinder, "Anel", model.transform,
                new Vector3(0f, 1.30f, 0f), new Vector3(0.62f, 0.04f, 0.62f), accentMat, false);

            // --- Animator com 5 estados (Idle/Walk/Run/Jump/Fall) --------------
            Animator animator = player.AddComponent<Animator>();
            RuntimeAnimatorController controller =
                Resources.Load<RuntimeAnimatorController>("SEEED07_Animator");
            if (controller != null) animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            // --- Comportamentos ----------------------------------------------
            PlayerController controller3D = player.AddComponent<PlayerController>();
            controller3D.visualTransform = model.transform;
            controller3D.animator = animator;
            controller3D.flashMaterial = bodyMat;
            if (Camera.main != null) controller3D.cameraTransform = Camera.main.transform;

            player.AddComponent<SeedCollector>();

            return player;
        }

        // ------------------------------------------------------------------
        // SPAWNER E ZONAS
        // ------------------------------------------------------------------

        /// <summary>
        /// Cria o LevelSpawner e dispara a instanciação de todos os assets do
        /// ambiente, sementes e inimigos.
        /// </summary>
        /// <param name="parent">Transform pai (raiz do jogo).</param>
        /// <param name="player">Jogador usado como referência.</param>
        /// <returns>O LevelSpawner já executado.</returns>
        private static LevelSpawner CreateLevelSpawner(Transform parent, GameObject player)
        {
            LevelSpawner spawner = CreateManager<LevelSpawner>("LevelSpawner", parent);
            spawner.player = player.transform;
            spawner.SpawnAll();
            return spawner;
        }

        /// <summary>
        /// Cria três Zonas de Restauração com disco visível, coluna de luz
        /// (baliza) e collider de gatilho, ligadas ao coletor do jogador.
        /// </summary>
        /// <param name="parent">Transform pai (raiz do jogo).</param>
        /// <param name="spawner">Spawner, fonte do vetor de vegetação.</param>
        /// <param name="player">Jogador, fonte das sementes em mão.</param>
        private static void CreateRestorationZones(Transform parent, LevelSpawner spawner, GameObject player)
        {
            SeedCollector collector = player.GetComponent<SeedCollector>();
            Vector3[] positions =
            {
                new Vector3(0f, 0f, 20f),
                new Vector3(-22f, 0f, -16f),
                new Vector3(24f, 0f, -14f)
            };

            Material beaconMat = Factory.Material(new Color(0.45f, 1f, 0.60f), 2.6f);

            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 pos = positions[i];

                // Objeto raiz da zona: SEM escala, para que o collider de
                // gatilho tenha o tamanho real (a escala do disco esmagaria
                // o gatilho e o jogador nunca seria detectado).
                GameObject zoneGO = new GameObject("ZonaRestauracao_" + i);
                zoneGO.transform.SetParent(parent, false);
                zoneGO.transform.position = pos;

                // Disco fino, apenas visual (fica "pintado" no chão).
                GameObject disc = Factory.Primitive(PrimitiveType.Cylinder, "Disco", zoneGO.transform,
                    new Vector3(0f, 0.02f, 0f),
                    new Vector3(ZoneRadius * 2f, 0.02f, ZoneRadius * 2f),
                    Factory.Material(new Color(0.35f, 0.30f, 0.26f)), false);

                SphereCollider trigger = zoneGO.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = ZoneRadius;
                trigger.center = new Vector3(0f, 1.2f, 0f);

                Factory.SetTag(zoneGO, "RestorationZone");

                RestorationZone zone = zoneGO.AddComponent<RestorationZone>();
                zone.collector = collector;
                zone.growthRoot = spawner.AssetRoot;
                zone.zoneRenderer = disc.GetComponent<Renderer>();
                zone.plantRadius = ZoneRadius * 0.85f;
                zone.vegetationPrefabs = spawner.vegetationPrefabs;

                // Baliza luminosa vertical, ligada à zona.
                Factory.Primitive(PrimitiveType.Cylinder, "Baliza", zoneGO.transform,
                    new Vector3(0f, 6f, 0f), new Vector3(0.22f, 6f, 0.22f), beaconMat, false);

                GameObject lightGO = new GameObject("LuzZona");
                lightGO.transform.SetParent(zoneGO.transform, false);
                lightGO.transform.localPosition = new Vector3(0f, 2.5f, 0f);
                Light glow = lightGO.AddComponent<Light>();
                glow.type = LightType.Point;
                glow.color = new Color(0.45f, 1f, 0.60f);
                glow.range = 16f;
                glow.intensity = 2.2f;
                glow.shadows = LightShadows.None;
            }
        }

        // ------------------------------------------------------------------
        // INTERFACE
        // ------------------------------------------------------------------

        /// <summary>
        /// Cria o Canvas, o EventSystem e todos os textos e botões (HUD +
        /// painéis de Pausa, Vitória e Derrota, com o BOTÃO REINICIAR
        /// obrigatório) e conecta tudo ao UIManager.
        /// </summary>
        /// <param name="parent">Transform pai (raiz do jogo).</param>
        private static void CreateUserInterface(Transform parent)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(parent, false);

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject esGO = new GameObject("EventSystem",
                    typeof(EventSystem), typeof(StandaloneInputModule));
                esGO.transform.SetParent(parent, false);
            }

            Transform ui = canvasGO.transform;

            Text seedsText = CreateText(ui, "Texto_Sementes", "", 34, TextAnchor.MiddleLeft,
                new Color(0.92f, 1f, 0.94f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(40f, -30f), new Vector2(1000f, 50f));

            Text livesText = CreateText(ui, "Texto_Vidas", "", 38, TextAnchor.MiddleLeft,
                new Color(1f, 0.55f, 0.55f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(40f, -88f), new Vector2(1000f, 54f));

            Text timerText = CreateText(ui, "Texto_Tempo", "04:00", 52, TextAnchor.MiddleRight,
                Color.white,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-40f, -26f), new Vector2(320f, 70f));

            Text hintText = CreateText(ui, "Texto_Dica", "", 26, TextAnchor.MiddleCenter,
                new Color(0.95f, 0.98f, 0.92f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 34f), new Vector2(1500f, 50f));

            Button pauseButton = CreateButton(ui, "Botao_Pausar", "PAUSAR", 30,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-40f, -110f), new Vector2(220f, 62f),
                new Color(0.16f, 0.32f, 0.22f));

            Text pauseTitle;
            Text pauseStats;
            Button pauseRestart;
            Button pauseExit;
            GameObject pausePanel = CreatePanel(ui, "Painel_Pausa", "PAUSADO",
                new Color(0.75f, 0.95f, 0.80f), out pauseTitle, out pauseStats, out pauseRestart, out pauseExit);

            Text winTitle;
            Text winStats;
            Button winRestart;
            Button winExit;
            GameObject winPanel = CreatePanel(ui, "Painel_Vitoria", "FLORESTA RESTAURADA!",
                new Color(0.55f, 1f, 0.62f), out winTitle, out winStats, out winRestart, out winExit);

            Text loseTitle;
            Text loseStats;
            Button loseRestart;
            Button loseExit;
            GameObject losePanel = CreatePanel(ui, "Painel_Derrota", "TEMPO ESGOTADO",
                new Color(1f, 0.62f, 0.55f), out loseTitle, out loseStats, out loseRestart, out loseExit);

            Button pauseResume = CreateButton(pausePanel.transform, "Botao_Continuar", "CONTINUAR", 34,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -160f), new Vector2(300f, 90f),
                new Color(0.18f, 0.40f, 0.55f));

            UIManager uiManager = canvasGO.AddComponent<UIManager>();
            uiManager.seedsText = seedsText;
            uiManager.livesText = livesText;
            uiManager.timerText = timerText;
            uiManager.hintText = hintText;
            uiManager.pauseButton = pauseButton;

            uiManager.pausePanel = pausePanel;
            uiManager.pauseStatsText = pauseStats;
            uiManager.pauseResumeButton = pauseResume;
            uiManager.pauseRestartButton = pauseRestart;
            uiManager.pauseExitButton = pauseExit;

            uiManager.winPanel = winPanel;
            uiManager.winStatsText = winStats;
            uiManager.winRestartButton = winRestart;
            uiManager.winExitButton = winExit;

            uiManager.losePanel = losePanel;
            uiManager.loseTitleText = loseTitle;
            uiManager.loseStatsText = loseStats;
            uiManager.loseRestartButton = loseRestart;
            uiManager.loseExitButton = loseExit;
        }

        /// <summary>
        /// Cria um painel de tela cheia com título, texto de estatísticas,
        /// botão REINICIAR e botão SAIR.
        /// </summary>
        /// <param name="parent">Canvas (pai).</param>
        /// <param name="name">Nome do painel.</param>
        /// <param name="title">Título exibido.</param>
        /// <param name="titleColor">Cor do título.</param>
        /// <param name="titleText">Texto do título (saída).</param>
        /// <param name="statsText">Texto de estatísticas (saída).</param>
        /// <param name="restartButton">Botão REINICIAR (saída).</param>
        /// <param name="exitButton">Botão SAIR (saída).</param>
        /// <returns>O GameObject do painel.</returns>
        private static GameObject CreatePanel(Transform parent, string name, string title, Color titleColor,
            out Text titleText, out Text statsText, out Button restartButton, out Button exitButton)
        {
            GameObject panel = new GameObject(name, typeof(Image));
            panel.transform.SetParent(parent, false);

            Image bg = panel.GetComponent<Image>();
            bg.color = new Color(0.02f, 0.05f, 0.03f, 0.93f);

            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            titleText = CreateText(panel.transform, "Titulo", title, 66, TextAnchor.MiddleCenter, titleColor,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -140f), new Vector2(1300f, 100f));

            statsText = CreateText(panel.transform, "Estatisticas", "", 36, TextAnchor.UpperCenter,
                new Color(0.90f, 0.96f, 0.90f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f), new Vector2(1000f, 320f));

            restartButton = CreateButton(panel.transform, "Botao_Reiniciar", "REINICIAR", 34,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-170f, -160f), new Vector2(320f, 90f),
                new Color(0.16f, 0.45f, 0.24f));

            exitButton = CreateButton(panel.transform, "Botao_Sair", "SAIR", 34,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(170f, -160f), new Vector2(320f, 90f),
                new Color(0.45f, 0.22f, 0.20f));

            panel.SetActive(false);
            return panel;
        }

        /// <summary>
        /// Cria um elemento de texto da interface (UGUI Text).
        /// </summary>
        /// <param name="parent">Transform pai.</param>
        /// <param name="name">Nome do GameObject.</param>
        /// <param name="content">Conteúdo inicial.</param>
        /// <param name="fontSize">Tamanho da fonte.</param>
        /// <param name="alignment">Alinhamento do texto.</param>
        /// <param name="color">Cor do texto.</param>
        /// <param name="anchorMin">Âncora mínima.</param>
        /// <param name="anchorMax">Âncora máxima.</param>
        /// <param name="pivot">Pivô.</param>
        /// <param name="anchoredPosition">Posição ancorada.</param>
        /// <param name="sizeDelta">Tamanho.</param>
        /// <returns>O componente Text criado.</returns>
        private static Text CreateText(Transform parent, string name, string content, int fontSize,
            TextAnchor alignment, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);

            Text text = go.GetComponent<Text>();
            text.font = InterfaceFont;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform rt = text.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
            return text;
        }

        /// <summary>
        /// Cria um botão da interface com um rótulo centralizado.
        /// </summary>
        /// <param name="parent">Transform pai.</param>
        /// <param name="name">Nome do GameObject.</param>
        /// <param name="label">Texto do botão.</param>
        /// <param name="fontSize">Tamanho da fonte do rótulo.</param>
        /// <param name="anchorMin">Âncora mínima.</param>
        /// <param name="anchorMax">Âncora máxima.</param>
        /// <param name="pivot">Pivô.</param>
        /// <param name="anchoredPosition">Posição ancorada.</param>
        /// <param name="sizeDelta">Tamanho.</param>
        /// <param name="background">Cor de fundo do botão.</param>
        /// <returns>O componente Button criado.</returns>
        private static Button CreateButton(Transform parent, string name, string label, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition,
            Vector2 sizeDelta, Color background)
        {
            GameObject go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            Image image = go.GetComponent<Image>();
            image.color = background;

            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.18f, 1.18f, 1.18f, 1f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1f;
            button.colors = colors;

            CreateText(go.transform, "Rotulo", label, fontSize, TextAnchor.MiddleCenter,
                Color.white, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);

            return button;
        }

        /// <summary>
        /// Fonte usada por todos os textos da interface, obtida do próprio
        /// Unity (com alternativas, para nunca falhar).
        /// </summary>
        private static Font InterfaceFont
        {
            get
            {
                if (_font != null) return _font;

                try { _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
                catch (System.Exception) { _font = null; }

                if (_font == null)
                {
                    try { _font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                    catch (System.Exception) { _font = null; }
                }
                if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 16);
                return _font;
            }
        }
    }
}
