using UnityEngine;
using UnityEngine.Rendering;

namespace Biosfera
{
    /// <summary>
    /// Fábrica central de recursos criados 100% por código: materiais,
    /// primitivas, sistemas de partículas e clipes de áudio procedurais.
    /// Assim o jogo NÃO depende de nenhum asset importado da internet.
    /// A escolha do shader é feita conforme o pipeline ativo do projeto,
    /// o que evita materiais cor-de-rosa caso a URP não esteja habilitada.
    /// </summary>
    public static class Factory
    {
        /// <summary>
        /// Indica se o projeto está usando a Universal Render Pipeline.
        /// </summary>
        /// <returns>true se uma Render Pipeline Asset estiver atribuída.</returns>
        private static bool UsesScriptablePipeline()
        {
            return GraphicsSettings.currentRenderPipeline != null;
        }

        /// <summary>
        /// Cria um Material usando o shader correto para o pipeline ativo
        /// (URP/Lit quando a URP está ativa, Standard caso contrário).
        /// </summary>
        /// <param name="color">Cor base (albedo) do material.</param>
        /// <param name="emission">Intensidade do brilho emissivo (0 = sem brilho).</param>
        /// <returns>Um novo Material configurado.</returns>
        public static Material Material(Color color, float emission = 0f)
        {
            Shader shader = null;
            if (UsesScriptablePipeline()) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Hidden/InternalErrorShader");

            Material m = new Material(shader);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.15f);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.15f);

            if (emission > 0.001f)
            {
                m.EnableKeyword("_EMISSION");
                if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", color * emission);
            }
            return m;
        }

        /// <summary>
        /// Habilita o brilho emissivo de um material, funcionando tanto no
        /// shader Standard (Built-in) quanto no URP/Lit. Serve para destacar
        /// o jogador, sementes e inimigos em alerta.
        /// </summary>
        /// <param name="m">Material alvo (pode ser null).</param>
        public static void MarkEmissive(Material m)
        {
            if (m == null) return;
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        /// <summary>
        /// Define a cor de emissão de um material, se ele suportar o canal.
        /// </summary>
        /// <param name="m">Material alvo (pode ser null).</param>
        /// <param name="color">Cor a aplicar na emissão.</param>
        public static void SetEmission(Material m, Color color)
        {
            if (m == null) return;
            if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", color);
        }

        /// <summary>
        /// Atribui uma Tag a um GameObject de forma SEGURA: caso a Tag não
        /// exista no projeto, registra um aviso em vez de lançar exceção
        /// (o que interromperia a montagem do jogo).
        /// </summary>
        /// <param name="go">GameObject que receberá a Tag.</param>
        /// <param name="tag">Nome da Tag.</param>
        public static void SetTag(GameObject go, string tag)
        {
            if (go == null || string.IsNullOrEmpty(tag)) return;
            try
            {
                go.tag = tag;
            }
            catch (UnityException)
            {
                Debug.LogWarning("[BIOSFERA] A Tag '" + tag + "' não existe. " +
                    "Crie em Edit > Project Settings > Tags and Layers.");
            }
        }

        /// <summary>
        /// Compara a Tag de um GameObject de forma SEGURA, sem lançar
        /// exceção caso a Tag não exista no projeto.
        /// </summary>
        /// <param name="go">GameObject a testar.</param>
        /// <param name="tag">Nome da Tag.</param>
        /// <returns>true se o GameObject possui a Tag indicada.</returns>
        public static bool HasTag(GameObject go, string tag)
        {
            if (go == null) return false;
            try
            {
                return go.CompareTag(tag);
            }
            catch (UnityException)
            {
                return false;
            }
        }

        /// <summary>
        /// Cria uma primitiva (cubo, esfera, cilindro...) já posicionada,
        /// escalada e pintada, opcionalmente sem collider.
        /// </summary>
        /// <param name="type">Tipo da primitiva a criar.</param>
        /// <param name="name">Nome do GameObject.</param>
        /// <param name="parent">Transform pai (pode ser null).</param>
        /// <param name="localPos">Posição local em relação ao pai.</param>
        /// <param name="localScale">Escala local.</param>
        /// <param name="mat">Material aplicado ao MeshRenderer.</param>
        /// <param name="withCollider">Se falso, remove o collider padrão.</param>
        /// <returns>GameObject da primitiva criada.</returns>
        public static GameObject Primitive(PrimitiveType type, string name, Transform parent,
            Vector3 localPos, Vector3 localScale, Material mat, bool withCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = localScale;

            if (!withCollider)
            {
                Collider c = go.GetComponent<Collider>();
                if (c != null)
                {
                    // Remoção IMEDIATA: como o objeto acabou de ser criado e
                    // ainda não foi usado, isso garante que os templates não
                    // clonem colliders indesejados ao serem instanciados.
                    Object.DestroyImmediate(c);
                }
            }

            if (mat != null)
            {
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                if (mr != null) mr.sharedMaterial = mat;
            }
            return go;
        }

        /// <summary>
        /// Cria o material padrão de partículas, escolhendo o shader conforme
        /// o pipeline ativo (URP Particles/Unlit ou Particles/Standard Unlit).
        /// </summary>
        /// <param name="color">Cor base das partículas.</param>
        /// <returns>Material de partículas.</returns>
        public static Material ParticleMaterial(Color color)
        {
            Shader s = null;
            if (UsesScriptablePipeline()) s = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (s == null) s = Shader.Find("Particles/Standard Unlit");
            if (s == null) s = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            if (s == null) s = Shader.Find("Sprites/Default");

            Material m = new Material(s);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            return m;
        }

        /// <summary>
        /// Cria um template de explosão/rajada de partículas (one-shot).
        /// O objeto retornado fica DESATIVADO no playOnAwake; use SpawnBurst
        /// para instanciar e tocar.
        /// </summary>
        /// <param name="name">Nome do template.</param>
        /// <param name="color">Cor das partículas.</param>
        /// <param name="count">Quantidade emitida na rajada.</param>
        /// <param name="startSize">Tamanho inicial.</param>
        /// <param name="startSpeed">Velocidade inicial.</param>
        /// <param name="lifetime">Tempo de vida em segundos.</param>
        /// <param name="gravity">Gravidade aplicada (negativa cai).</param>
        /// <param name="radius">Raio da forma de emissão.</param>
        /// <returns>GameObject com ParticleSystem configurado.</returns>
        public static GameObject ParticleTemplate(string name, Color color, int count,
            float startSize, float startSpeed, float lifetime, float gravity, float radius)
        {
            GameObject go = new GameObject(name);
            ParticleSystem ps = go.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = lifetime;
            main.startSize = startSize;
            main.startSpeed = startSpeed;
            main.startColor = color;
            main.gravityModifier = gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 400;

            ParticleSystem.EmissionModule em = ps.emission;
            em.rateOverTime = 0f;
            em.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, (short)Mathf.Clamp(count, 1, 300))
            });

            ParticleSystem.ShapeModule sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = radius;

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.85f, 0.4f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = new ParticleSystem.MinMaxGradient(grad);

            ParticleSystem.SizeOverLifetimeModule sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.15f));

            ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.sharedMaterial = ParticleMaterial(color);
                psr.renderMode = ParticleSystemRenderMode.Billboard;
            }

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            go.SetActive(false);
            return go;
        }

        /// <summary>
        /// Cria uma fumaça contínua que acompanha um Transform (ex.: inimigo).
        /// </summary>
        /// <param name="parent">Transform ao qual a fumaça se prende.</param>
        /// <param name="color">Cor da fumaça.</param>
        /// <param name="rate">Partículas emitidas por segundo.</param>
        /// <returns>O ParticleSystem da fumaça.</returns>
        public static ParticleSystem Smoke(Transform parent, Color color, float rate)
        {
            GameObject go = new GameObject("SmokeFX");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 1.2f, 0f);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 1.6f;
            main.startSize = 1.4f;
            main.startSpeed = 0.7f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 120;

            ParticleSystem.EmissionModule em = ps.emission;
            em.rateOverTime = rate;

            ParticleSystem.ShapeModule sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Cone;
            sh.radius = 0.35f;
            sh.angle = 12f;

            ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color * 0.5f, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(grad);

            ParticleSystem.SizeOverLifetimeModule sz = ps.sizeOverLifetime;
            sz.enabled = true;
            sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.5f, 1f, 1.8f));

            ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
            if (psr != null) psr.sharedMaterial = ParticleMaterial(color);

            ps.Play();
            return ps;
        }

        /// <summary>
        /// Instancia um template de partículas numa posição, toca a rajada e
        /// agenda a destruição automática do objeto.
        /// </summary>
        /// <param name="template">Template criado por ParticleTemplate.</param>
        /// <param name="position">Posição mundial onde spawnar.</param>
        /// <param name="scale">Escala aplicada.</param>
        /// <param name="destroyAfter">Segundos até destruir o objeto.</param>
        public static void SpawnBurst(GameObject template, Vector3 position, float scale, float destroyAfter)
        {
            if (template == null) return;
            GameObject go = Object.Instantiate(template, position, Quaternion.identity);
            go.SetActive(true);
            go.transform.localScale = Vector3.one * scale;
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Object.Destroy(go, destroyAfter);
        }

        /// <summary>
        /// Gera um AudioClip procedural (seno com envelope de decaimento e
        /// ruído opcional). Usado para todos os sons do jogo, sem assets.
        /// </summary>
        /// <param name="name">Nome do clipe.</param>
        /// <param name="startFreq">Frequência inicial em Hz.</param>
        /// <param name="endFreq">Frequência final em Hz.</param>
        /// <param name="duration">Duração em segundos.</param>
        /// <param name="noise">Mistura de ruído (0 = puro tom).</param>
        /// <param name="volume">Volume final (0..1).</param>
        /// <returns>AudioClip pronto para tocar.</returns>
        public static AudioClip Tone(string name, float startFreq, float endFreq,
            float duration, float noise, float volume)
        {
            const int sampleRate = 22050;
            int samples = Mathf.Max(64, Mathf.RoundToInt(sampleRate * Mathf.Max(0.02f, duration)));
            float[] data = new float[samples];
            float phase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / (samples - 1);
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += 2f * Mathf.PI * freq / sampleRate;
                float env = (1f - t) * (1f - t);
                float s = Mathf.Sin(phase);
                if (noise > 0f) s = Mathf.Lerp(s, UnityEngine.Random.Range(-1f, 1f), noise);
                data[i] = s * env * volume;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Gera um AudioClip ambiente contínuo (drone suave com tremolo),
        /// adequado para trilha sonora em loop.
        /// </summary>
        /// <param name="name">Nome do clipe.</param>
        /// <param name="freq">Frequência base em Hz.</param>
        /// <param name="duration">Duração do loop em segundos.</param>
        /// <param name="volume">Volume final (0..1).</param>
        /// <returns>AudioClip em loop.</returns>
        public static AudioClip Ambient(string name, float freq, float duration, float volume)
        {
            const int sampleRate = 22050;
            int samples = Mathf.Max(256, Mathf.RoundToInt(sampleRate * duration));
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float tremolo = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * (t * 3f));
                float s = Mathf.Sin(2f * Mathf.PI * freq * t)
                        + 0.5f * Mathf.Sin(2f * Mathf.PI * (freq * 1.5f) * t)
                        + 0.25f * Mathf.Sin(2f * Mathf.PI * (freq * 2.0f) * t);
                data[i] = (s / 1.75f) * tremolo * volume;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
