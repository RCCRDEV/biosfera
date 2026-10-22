using UnityEngine;

namespace Biosfera
{
    /// <summary>
    /// Popula a fase. Cria TODOS os assets de ambiente, sementes e inimigos
    /// usando a função Instantiate sobre VETORES de prefabs (GameObject[]),
    /// com posição e rotação aleatórias. Os templates são gerados por código
    /// caso os vetores estejam vazios (garantindo que o jogo sempre funcione).
    /// </summary>
    public class LevelSpawner : MonoBehaviour
    {
        [Header("Referências")]
        [Tooltip("Transform do jogador (evita spawnar em cima dele).")]
        public Transform player;

        [Header("Vetores de prefabs (GameObject[])")]
        public GameObject[] treePrefabs;
        public GameObject[] rockPrefabs;
        public GameObject[] bushPrefabs;
        public GameObject[] mushroomPrefabs;
        public GameObject[] flowerPrefabs;
        public GameObject[] grassPrefabs;
        public GameObject[] debrisPrefabs;
        public GameObject[] trashPrefabs;
        [Tooltip("Manchas de cinza/terra queimada espalhadas pelo chão.")]
        public GameObject[] ashPrefabs;
        [Tooltip("Vetor de prefabs de vegetação usado pela Zona de Restauração.")]
        public GameObject[] vegetationPrefabs;
        [Tooltip("Template (prefab) da semente coletável.")]
        public GameObject seedPrefab;
        [Tooltip("Template (prefab) do inimigo poluente.")]
        public GameObject enemyPrefab;

        [Header("Quantidades por categoria")]
        public int mushroomCount = 20;
        public int flowerCount = 34;
        public int grassCount = 78;
        public int debrisCount = 34;
        public int trashCount = 14;
        public int ashCount = 26;
        public int borderTreeCount = 44;
        public int seedCount = 8;
        public int enemyCount = 4;
        public int vegetationVariantCount = 8;

        [Header("Variações DISTINTAS de modelos (requisito: mínimo 70)")]
        [Tooltip("Quantidade de árvores DIFERENTES entre si.")]
        public int treeVariants = 12;
        [Tooltip("Quantidade de pedras DIFERENTES entre si.")]
        public int rockVariants = 12;
        [Tooltip("Quantidade de arbustos DIFERENTES entre si.")]
        public int bushVariants = 10;
        [Tooltip("Quantidade de cogumelos DIFERENTES entre si.")]
        public int mushroomVariants = 10;
        [Tooltip("Quantidade de flores DIFERENTES entre si.")]
        public int flowerVariants = 12;
        [Tooltip("Quantidade de tufos de capim DIFERENTES entre si.")]
        public int grassVariants = 10;
        [Tooltip("Quantidade de detritos DIFERENTES entre si.")]
        public int debrisVariants = 10;
        [Tooltip("Quantidade de lixos DIFERENTES entre si.")]
        public int trashVariants = 10;
        [Tooltip("Quantidade de manchas de cinza DIFERENTES entre si.")]
        public int ashVariants = 8;

        [Header("Aglomerados (clusters)")]
        [Tooltip("Quantidade de aglomerados de árvores.")]
        public int treeClusters = 9;
        [Tooltip("Árvores mínimas por aglomerado.")]
        public int treePerClusterMin = 3;
        [Tooltip("Árvores máximas por aglomerado.")]
        public int treePerClusterMax = 6;
        [Tooltip("Raio de espalhamento dentro de cada aglomerado.")]
        public float clusterSpread = 6.5f;
        [Tooltip("Quantidade de aglomerados de pedras.")]
        public int rockClusters = 7;
        [Tooltip("Quantidade de aglomerados de arbustos.")]
        public int bushClusters = 8;

        [Header("Área do mapa")]
        [Tooltip("Metade do lado do terreno (o terreno vai de -valor a +valor).")]
        public float areaHalfSize = 34f;
        [Tooltip("Raio livre ao redor do centro (onde o jogador nasce).")]
        public float safeRadius = 4.5f;

        /// <summary>Total de assets instanciados por este spawner.</summary>
        public int TotalSpawned { get; private set; }

        /// <summary>
        /// Quantidade de MODELOS DIFERENTES entre si (variações únicas de
        /// árvores, pedras, flores, etc.). É este número que atende ao
        /// requisito do professor de "70 assets diferentes".
        /// </summary>
        public int DistinctPrefabCount { get; private set; }

        /// <summary>Raiz onde os assets spawnados são organizados.</summary>
        public Transform AssetRoot { get { return _assetRoot; } }

        private Transform _assetRoot;
        private Transform _templateRoot;

        /// <summary>
        /// Cria as raízes (assets e templates) e garante os templates padrão.
        /// NÃO spawna automaticamente: o Bootstrap chama SpawnAll() após
        /// conectar todas as referências.
        /// </summary>
        private void Awake()
        {
            GameObject assetRootGO = new GameObject("_AssetsSpawnados");
            assetRootGO.transform.SetParent(transform, false);
            _assetRoot = assetRootGO.transform;

            GameObject templateRootGO = new GameObject("_Templates");
            templateRootGO.transform.SetParent(transform, false);
            _templateRoot = templateRootGO.transform;
            _templateRoot.gameObject.SetActive(false);
        }

        /// <summary>
        /// Instancia todas as categorias. Deve ser chamado pelo Bootstrap
        /// depois que as referências estiverem prontas.
        /// </summary>
        public void SpawnAll()
        {
            EnsureTemplates();
            TotalSpawned = 0;

            // Manchas de terra queimada primeiro (ficam "debaixo" de tudo).
            SpawnCategory(ashPrefabs, ashCount, 0.9f, 1.6f);

            // Aglomerados: deixam o mapa com cara de floresta de verdade.
            SpawnClustered(treePrefabs, treeClusters, treePerClusterMin, treePerClusterMax,
                safeRadius, clusterSpread, 0.9f, 1.3f);
            SpawnClustered(rockPrefabs, rockClusters, 2, 4, safeRadius, clusterSpread * 0.7f, 0.8f, 1.3f);
            SpawnClustered(bushPrefabs, bushClusters, 2, 4, safeRadius, clusterSpread * 0.6f, 0.8f, 1.2f);

            // Detalhes espalhados (mais densos perto do chão).
            SpawnCategory(mushroomPrefabs, mushroomCount, 0.8f, 1.3f);
            SpawnCategory(flowerPrefabs, flowerCount, 0.9f, 1.4f);
            SpawnCategory(grassPrefabs, grassCount, 0.8f, 1.5f);
            SpawnCategory(debrisPrefabs, debrisCount, 0.8f, 1.3f);
            SpawnCategory(trashPrefabs, trashCount, 0.8f, 1.2f);

            // Cerca viva no limite do mapa: esconde a borda do terreno.
            SpawnBorderTrees();

            SpawnSeeds();
            SpawnEnemies();

            Debug.Log("<color=green>[BIOSFERA]</color> LevelSpawner instanciou " +
                TotalSpawned + " assets de ambiente pela função Instantiate, em " +
                DistinctPrefabCount + " modelos DIFERENTES entre si.");
        }

        /// <summary>
        /// Cria aglomerados: sorteia pontos centrais e espalha várias cópias
        /// aleatórias do vetor de prefabs ao redor de cada centro. Isso deixa
        /// o cenário com "moitas" de vegetação em vez de objetos isolados.
        /// </summary>
        /// <param name="pool">Vetor de prefabs (GameObject[]).</param>
        /// <param name="clusterCount">Quantidade de aglomerados.</param>
        /// <param name="perClusterMin">Itens mínimos por aglomerado.</param>
        /// <param name="perClusterMax">Itens máximos por aglomerado.</param>
        /// <param name="minRadius">Distância mínima do centro do mapa.</param>
        /// <param name="spread">Raio de espalhamento do aglomerado.</param>
        /// <param name="minScale">Fator mínimo de escala.</param>
        /// <param name="maxScale">Fator máximo de escala.</param>
        private void SpawnClustered(GameObject[] pool, int clusterCount, int perClusterMin,
            int perClusterMax, float minRadius, float spread, float minScale, float maxScale)
        {
            if (pool == null || pool.Length == 0) return;

            for (int c = 0; c < clusterCount; c++)
            {
                Vector2 center2D = RandomPoint(minRadius);
                int items = Random.Range(perClusterMin, perClusterMax + 1);

                for (int i = 0; i < items; i++)
                {
                    GameObject prefab = pool[Random.Range(0, pool.Length)];
                    if (prefab == null) continue;

                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    float radius = Random.Range(0.6f, spread);
                    Vector3 position = new Vector3(
                        center2D.x + Mathf.Cos(angle) * radius, 0f,
                        center2D.y + Mathf.Sin(angle) * radius);

                    SpawnOne(prefab, position, minScale, maxScale);
                }
            }
        }

        /// <summary>
        /// Planta uma faixa densa de árvores no limite do terreno para que a
        /// borda do plano não fique visível (sensação de floresta fechada).
        /// </summary>
        private void SpawnBorderTrees()
        {
            if (treePrefabs == null || treePrefabs.Length == 0) return;

            for (int i = 0; i < borderTreeCount; i++)
            {
                float angle = (i / (float)Mathf.Max(1, borderTreeCount)) * Mathf.PI * 2f;
                angle += Random.Range(-0.06f, 0.06f);

                float radius = areaHalfSize + Random.Range(-1.5f, 2.5f);
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

                GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                SpawnOne(prefab, position, 1.05f, 1.45f);
            }
        }

        /// <summary>
        /// Instancia uma única cópia de um prefab numa posição, com rotação e
        /// escala aleatórias, já parenteada e ativada.
        /// </summary>
        /// <param name="prefab">Prefab a instanciar.</param>
        /// <param name="position">Posição no mundo.</param>
        /// <param name="minScale">Fator mínimo de escala.</param>
        /// <param name="maxScale">Fator máximo de escala.</param>
        /// <returns>O GameObject instanciado.</returns>
        private GameObject SpawnOne(GameObject prefab, Vector3 position, float minScale, float maxScale)
        {
            if (prefab == null) return null;

            GameObject go = Instantiate(prefab, position,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            go.name = prefab.name + "_" + TotalSpawned;
            go.transform.SetParent(_assetRoot, true);
            go.transform.localScale = prefab.transform.localScale * Random.Range(minScale, maxScale);
            go.SetActive(true);
            TotalSpawned++;
            return go;
        }

        /// <summary>
        /// Instancia N cópias aleatórias de um vetor de prefabs em posições
        /// aleatórias dentro da área jogável.
        /// </summary>
        /// <param name="pool">Vetor de prefabs (GameObject[]).</param>
        /// <param name="count">Quantidade a instanciar.</param>
        /// <param name="minScale">Fator mínimo de escala aleatória.</param>
        /// <param name="maxScale">Fator máximo de escala aleatória.</param>
        private void SpawnCategory(GameObject[] pool, int count, float minScale, float maxScale)
        {
            if (pool == null || pool.Length == 0) return;

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = pool[Random.Range(0, pool.Length)];
                Vector3 position = RandomGroundPosition(safeRadius);
                SpawnOne(prefab, position, minScale, maxScale);
            }
        }

        /// <summary>
        /// Instancia as sementes coletáveis espalhadas pelo mapa.
        /// </summary>
        private void SpawnSeeds()
        {
            if (seedPrefab == null) return;

            for (int i = 0; i < seedCount; i++)
            {
                Vector3 position = RandomGroundPosition(safeRadius) + Vector3.up * 1.1f;
                GameObject go = Instantiate(seedPrefab, position, Quaternion.identity);
                go.name = "Semente_" + i;
                go.transform.SetParent(_assetRoot, true);
                go.SetActive(true);
                TotalSpawned++;
            }
        }

        /// <summary>
        /// Instancia os inimigos poluentes, cada um em uma região distinta.
        /// </summary>
        private void SpawnEnemies()
        {
            if (enemyPrefab == null) return;

            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 position = RandomGroundPosition(safeRadius + 6f) + Vector3.up * 1.6f;
                GameObject go = Instantiate(enemyPrefab, position, Quaternion.identity);
                go.name = "InimigoPoluente_" + i;
                go.transform.SetParent(_assetRoot, true);
                go.SetActive(true);

                PollutionEnemy enemy = go.GetComponent<PollutionEnemy>();
                if (enemy != null) enemy.player = player;

                TotalSpawned++;
            }
        }

        /// <summary>
        /// Sorteia um ponto no plano XZ dentro da área, respeitando um raio
        /// livre ao redor do centro.
        /// </summary>
        /// <param name="minRadius">Distância mínima do centro.</param>
        /// <returns>Ponto sorteado em XZ.</returns>
        private Vector2 RandomPoint(float minRadius)
        {
            for (int attempt = 0; attempt < 24; attempt++)
            {
                float x = Random.Range(-areaHalfSize, areaHalfSize);
                float z = Random.Range(-areaHalfSize, areaHalfSize);
                if (new Vector2(x, z).magnitude < minRadius) continue;
                return new Vector2(x, z);
            }
            return new Vector2(areaHalfSize * 0.7f, areaHalfSize * 0.7f);
        }

        /// <summary>
        /// Sorteia uma posição no plano XZ dentro da área, respeitando um
        /// raio livre ao redor do centro. O Y fica em 0 (terreno plano).
        /// </summary>
        /// <param name="minRadius">Distância mínima do centro.</param>
        /// <returns>Posição sorteada.</returns>
        private Vector3 RandomGroundPosition(float minRadius)
        {
            Vector2 p = RandomPoint(minRadius);
            return new Vector3(p.x, 0f, p.y);
        }

        /// <summary>
        /// Garante que todos os vetores de prefabs possuam conteúdo. Caso
        /// estejam vazios, cria templates procedurais (árvores, pedras, etc.)
        /// dentro de uma raiz desativada.
        /// </summary>
        private void EnsureTemplates()
        {
            // Cada categoria gera N MODELOS DIFERENTES entre si. A soma de
            // todas as variações (abaixo) é o que atende ao requisito do
            // professor de "no mínimo 70 assets diferentes".
            if (IsEmpty(treePrefabs))
                treePrefabs = BuildVariants(treeVariants, BuildTree);
            if (IsEmpty(rockPrefabs))
                rockPrefabs = BuildVariants(rockVariants, BuildRock);
            if (IsEmpty(bushPrefabs))
                bushPrefabs = BuildVariants(bushVariants, BuildBush);
            if (IsEmpty(mushroomPrefabs))
                mushroomPrefabs = BuildVariants(mushroomVariants, BuildMushroom);
            if (IsEmpty(flowerPrefabs))
                flowerPrefabs = BuildVariants(flowerVariants, BuildFlower);
            if (IsEmpty(grassPrefabs))
                grassPrefabs = BuildVariants(grassVariants, BuildGrass);
            if (IsEmpty(debrisPrefabs))
                debrisPrefabs = BuildVariants(debrisVariants, BuildDebris);
            if (IsEmpty(trashPrefabs))
                trashPrefabs = BuildVariants(trashVariants, BuildTrash);
            if (IsEmpty(ashPrefabs))
                ashPrefabs = BuildVariants(ashVariants, BuildAshPatch);
            if (IsEmpty(vegetationPrefabs))
                vegetationPrefabs = BuildVariants(vegetationVariantCount, BuildSmallTree);

            if (seedPrefab == null) seedPrefab = BuildSeedTemplate();
            if (enemyPrefab == null) enemyPrefab = BuildEnemyTemplate();

            DistinctPrefabCount = CountDistinctModels();
        }

        /// <summary>
        /// Constrói um vetor de prefabs chamando a função criadora para cada
        /// índice (0..count-1). Cada índice produz um modelo DIFERENTE.
        /// </summary>
        /// <param name="count">Quantidade de variações desejadas.</param>
        /// <param name="builder">Função que cria a variação pelo índice.</param>
        /// <returns>Vetor de prefabs distintos.</returns>
        private GameObject[] BuildVariants(int count, System.Func<int, GameObject> builder)
        {
            int total = Mathf.Max(1, count);
            GameObject[] result = new GameObject[total];
            for (int i = 0; i < total; i++)
                result[i] = builder(i);
            return result;
        }

        /// <summary>
        /// Soma quantos modelos diferentes entre si existem no jogo (todas as
        /// variações de árvore, pedra, flor, semente, inimigo, etc.).
        /// </summary>
        /// <returns>Quantidade total de prefabs distintos.</returns>
        private int CountDistinctModels()
        {
            int total = 0;
            total += Length(treePrefabs);
            total += Length(rockPrefabs);
            total += Length(bushPrefabs);
            total += Length(mushroomPrefabs);
            total += Length(flowerPrefabs);
            total += Length(grassPrefabs);
            total += Length(debrisPrefabs);
            total += Length(trashPrefabs);
            total += Length(ashPrefabs);
            total += Length(vegetationPrefabs);
            if (seedPrefab != null) total++;
            if (enemyPrefab != null) total++;
            return total;
        }

        /// <summary>
        /// Retorna o tamanho de um vetor de GameObjects, tolerando nulos.
        /// </summary>
        /// <param name="array">Vetor a medir.</param>
        /// <returns>Tamanho do vetor (0 se nulo).</returns>
        private static int Length(GameObject[] array)
        {
            return array != null ? array.Length : 0;
        }

        /// <summary>
        /// Gera um valor pseudoaleatório estável (0..1) a partir do índice da
        /// variação e de um "tempero". Diferente de Random.Range, o mesmo
        /// índice sempre devolve o mesmo valor — o que garante variações
        /// distintas e reproduzíveis.
        /// </summary>
        /// <param name="variant">Índice da variação.</param>
        /// <param name="salt">Tempero para gerar números independentes.</param>
        /// <returns>Valor entre 0 e 1.</returns>
        private static float Noise01(int variant, float salt)
        {
            float v = Mathf.Sin((variant + 1) * 12.9898f + salt * 78.233f) * 43758.5453f;
            return v - Mathf.Floor(v);
        }

        /// <summary>
        /// Cria uma cor a partir de matiz/saturação/valor, com o matiz
        /// "dando a volta" (0..1) para nunca sair da faixa válida.
        /// </summary>
        /// <param name="hue">Matiz (0..1).</param>
        /// <param name="saturation">Saturação (0..1).</param>
        /// <param name="value">Brilho (0..1).</param>
        /// <returns>Cor resultante.</returns>
        private static Color Hsv(float hue, float saturation, float value)
        {
            return Color.HSVToRGB(Mathf.Repeat(hue, 1f), Mathf.Clamp01(saturation), Mathf.Clamp01(value));
        }

        /// <summary>
        /// Verifica se um vetor de GameObjects está vazio ou nulo.
        /// </summary>
        /// <param name="array">Vetor a verificar.</param>
        /// <returns>true se estiver vazio.</returns>
        private static bool IsEmpty(GameObject[] array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// Cria um template de árvore (tronco + copa) desativado. Altura,
        /// espessura do tronco, cor da madeira/folhas e quantidade de copas
        /// (1 a 3 esferas) mudam a cada variação.
        /// </summary>
        /// <param name="variant">Índice da variação.</param>
        /// <returns>GameObject template.</returns>
        private GameObject BuildTree(int variant)
        {
            GameObject root = new GameObject("Prefab_Arvore_" + variant);
            root.transform.SetParent(_templateRoot, false);

            float height = 2.6f + (variant % 6) * 0.75f + Noise01(variant, 3.1f) * 0.8f;
            float trunkRadius = 0.22f + Noise01(variant, 7.7f) * 0.22f;

            Material trunkMat = Factory.Material(Hsv(0.07f + Noise01(variant, 5f) * 0.04f,
                0.45f + Noise01(variant, 8f) * 0.25f, 0.18f + Noise01(variant, 9f) * 0.16f));

            Factory.Primitive(PrimitiveType.Cylinder, "Tronco", root.transform,
                new Vector3(0f, height * 0.5f, 0f),
                new Vector3(trunkRadius, height * 0.5f, trunkRadius), trunkMat, true);

            Material leafMat = Factory.Material(Hsv(0.24f + Noise01(variant, 11f) * 0.13f,
                0.34f + Noise01(variant, 13f) * 0.32f, 0.20f + Noise01(variant, 17f) * 0.24f),
                Noise01(variant, 19f) * 0.35f);

            // Copa: 1 a 3 esferas empilhadas/deslocadas — formato único por variação.
            int canopyCount = 1 + (variant % 3);
            for (int c = 0; c < canopyCount; c++)
            {
                float cw = (2.7f - c * 0.55f) * (0.7f + Noise01(variant, 23f + c) * 0.6f);
                float ch = cw * (0.70f + Noise01(variant, 29f + c) * 0.38f);
                Vector3 pos = new Vector3(
                    (Noise01(variant, 31f + c) - 0.5f) * 1.1f,
                    height + 0.55f + c * ch * 0.55f,
                    (Noise01(variant, 37f + c) - 0.5f) * 1.1f);

                Factory.Primitive(PrimitiveType.Sphere, "Copa" + c, root.transform,
                    pos, new Vector3(cw, ch, cw), leafMat, false);
            }

            root.SetActive(false);
            return root;
        }

        /// <summary>
        /// Cria um template de árvore pequena (usada na vegetação da zona).
        /// Cada variação tem altura, cor, copa e inclinação diferentes.
        /// </summary>
        /// <param name="variant">Índice da variação.</param>
        /// <returns>GameObject template.</returns>
        private GameObject BuildSmallTree(int variant)
        {
            GameObject root = new GameObject("Prefab_Vegetacao_" + variant);
            root.transform.SetParent(_templateRoot, false);

            float height = 1.2f + (variant % 5) * 0.42f + Noise01(variant, 101f) * 0.5f;
            float trunkRadius = 0.12f + Noise01(variant, 103f) * 0.12f;

            Material trunkMat = Factory.Material(Hsv(0.08f + Noise01(variant, 107f) * 0.03f,
                0.5f, 0.20f + Noise01(variant, 109f) * 0.12f));
            Material leafMat = Factory.Material(Hsv(0.26f + Noise01(variant, 113f) * 0.12f,
                0.40f + Noise01(variant, 127f) * 0.32f, 0.26f + Noise01(variant, 131f) * 0.26f),
                0.15f + Noise01(variant, 137f) * 0.4f);

            GameObject trunk = Factory.Primitive(PrimitiveType.Cylinder, "Tronco", root.transform,
                new Vector3(0f, height * 0.5f, 0f),
                new Vector3(trunkRadius, height * 0.5f, trunkRadius), trunkMat, true);
            trunk.transform.localRotation = Quaternion.Euler(
                (Noise01(variant, 139f) - 0.5f) * 16f, Noise01(variant, 149f) * 360f,
                (Noise01(variant, 151f) - 0.5f) * 16f);

            float cw = 0.85f + Noise01(variant, 157f) * 0.9f;
            Factory.Primitive(PrimitiveType.Sphere, "Copa", root.transform,
                new Vector3((Noise01(variant, 163f) - 0.5f) * 0.5f, height + 0.3f, 0f),
                new Vector3(cw, cw * (0.7f + Noise01(variant, 167f) * 0.4f), cw), leafMat, false);

            root.SetActive(false);
            return root;
        }

        /// <summary>
        /// Cria um template de pedra. A forma (esfera/cubo/cilindro), o
        /// tamanho, a cor e a inclinação mudam a cada variação; metade das
        /// variações ganha uma pedra menor ao lado.
        /// </summary>
        /// <param name="variant">Índice da variação.</param>
        /// <returns>GameObject template.</returns>
        private GameObject BuildRock(int variant)
        {
            GameObject root = new GameObject("Prefab_Pedra_" + variant);
            root.transform.SetParent(_templateRoot, false);

            Material mat = Factory.Material(Hsv(0.07f + Noise01(variant, 41f) * 0.07f,
                0.02f + Noise01(variant, 43f) * 0.14f, 0.24f + Noise01(variant, 47f) * 0.28f));

            int shape = variant % 3;
            PrimitiveType type = shape == 0 ? PrimitiveType.Sphere
                : (shape == 1 ? PrimitiveType.Cube : PrimitiveType.Cylinder);

            float sx = 0.75f + Noise01(variant, 53f) * 1.4f;
            float sy = 0.5f + Noise01(variant, 59f) * 1.0f;
            float sz = 0.75f + Noise01(variant, 61f) * 1.4f;

            GameObject rock = Factory.Primitive(type, "Pedra", root.transform,
                new Vector3(0f, sy * 0.45f, 0f), new Vector3(sx, sy, sz), mat, true);
            rock.transform.localRotation = Quaternion.Euler(
                (Noise01(variant, 67f) - 0.5f) * 26f, Noise01(variant, 71f) * 360f,
                (Noise01(variant, 73f) - 0.5f) * 26f);

            if (variant % 2 == 0)
            {
                float s2 = 0.3f + Noise01(variant, 79f) * 0.45f;
                GameObject extra = Factory.Primitive(PrimitiveType.Sphere, "PedraB", root.transform,
                    new Vector3(0.65f + Noise01(variant, 83f) * 0.6f, s2 * 0.4f,
                        (Noise01(variant, 89f) - 0.5f) * 0.9f),
                    new Vector3(s2, s2 * 0.8f, s2), mat, true);
                extra.transform.localRotation = Quaternion.Euler(0f, Noise01(variant, 97f) * 360f, 0f);
            }

            root.SetActive(false);
            return root;
        }

        /// <summary>
        /// Cria um template de arbusto com 2 a 4 "moitas" (esferas) de
        /// tamanhos e posições diferentes, variando também a cor.
        /// </summary>
        /// <param name="variant">Índice da variação.</param>
        /// <returns>GameObject template.</returns>
        private GameObject BuildBush(int variant)
        {
            GameObject root = new GameObject("Prefab_Arbusto_" + variant);
            root.transform.SetParent(_templateRoot, false);

            Material mat = Factory.Material(Hsv(0.26f + Noise01(variant, 173f) * 0.11f,
                0.36f + Noise01(variant, 179f) * 0.34f, 0.22f + Noise01(variant, 181f) * 0.20f));

            int lumps = 2 + (variant % 3);
            for (int i = 0; i < lumps; i++)
            {
                float s = 0.85f + Noise01(variant, 191f + i) * 0.7f;
                Vector3 pos = new Vector3(
                    (Noise01(variant, 193f + i) - 0.5f) * 1.3f,
                    s * 0.42f,
                    (Noise01(variant, 197f + i) - 0.5f) * 1.3f);

                Factory.Primitive(PrimitiveType.Sphere, "Moita" + i, root.transform, pos,
                    new Vector3(s * 1.2f, s * 0.85f, s * 1.2f), mat, false);
            }

            root.SetActive(false);
            return root;
        }

        /// <summary>
        /// Cria um template de cogumelo com altura do caule, tamanho/cor do
        /// chapéu e quantidade de cogumelos diferentes por variação.
        /// </summary>
        /// <param name="variant">Índice da variação.</param>
        /// <returns>GameObject template.</returns>
        private GameObject BuildMushroom(int variant)
        {
            GameObject root = new GameObject("Prefab_Cogumelo_" + variant);
            root.transform.SetParent(_templateRoot, false);

            Material stemMat = Factory.Material(Hsv(0.10f, 0.05f, 0.78f + Noise01(variant, 199f) * 0.18f));
            Material capMat = Factory.Material(Hsv(Noise01(variant, 211f) * 0.14f,
                0.45f + Noise01(variant, 223f) * 0.4f, 0.65f + Noise01(variant, 227f) * 0.3f),
                0.1f + Noise01(variant, 229f) * 0.9f);

            int shrooms = 1 + (variant % 3);
            for (int i = 0; i < shrooms; i++)
            {
                float scale = 0.6f + Noise01(variant, 233f + i) * 0.8f;
                float ox = i == 0 ? 0f : (Noise01(variant, 239f + i) - 0.5f) * 0.7f;
                float oz = i == 0 ? 0f : (Noise01(variant, 241f + i) - 0.5f) * 0.7f;
                float stemH = 0.3f + Noise01(variant, 251f + i) * 0.4f;

                Factory.Primitive(PrimitiveType.Cylinder, "Caule" + i, root.transform,
                    new Vector3(ox, stemH * 0.5f, oz), new Vector3(0.11f, stemH * 0.5f, 0.11f) * scale,
                    stemMat, false);
                Factory.Primitive(PrimitiveType.Sphere, "Chapeu" + i, root.transform,
                    new Vector3(ox, stemH + 0.05f, oz),
                    new Vector3(0.55f, 0.34f, 0.55f) * scale, capMat, false);
            }

            root.SetActive(false);
            return root;
        }

        /// <summary>
        /// Cria um template de flor com altura do caule, cor, brilho e
        /// quantidade de pétalas (1 a 6) diferentes por variação.
        /// </summary>
        /// <param name="variant">Índice da variação.</param>
        /// <returns>GameObject template.</returns>
        private GameObject BuildFlower(int variant)
        {
            GameObject root = new GameObject("Prefab_Flor_" + variant);
            root.transform.SetParent(_templateRoot, false);

            Material stemMat = Factory.Material(Hsv(0.28f + Noise01(variant, 257f) * 0.06f, 0.5f,
                0.22f + Noise01(variant, 263f) * 0.14f));
            Color petalColor = Hsv(Noise01(variant, 269f), 0.55f + Noise01(variant, 271f) * 0.4f,
                0.75f + Noise01(variant, 277f) * 0.25f);
            Material petalMat = Factory.Material(petalColor, 0.25f + Noise01(variant, 281f) * 0.6f);

            float stemH = 0.28f + Noise01(variant, 283f) * 0.5f;
            Factory.Primitive(PrimitiveType.Cylinder, "Caule", root.transform,
                new Vector3(0f, stemH * 0.5f, 0f), new Vector3(0.05f, stemH * 0.5f, 0.05f), stemMat, false);

            float headY = stemH + 0.06f;
            Factory.Primitive(PrimitiveType.Sphere, "Miolo", root.transform,
                new Vector3(0f, headY, 0f), Vector3.one * 0.12f,
                Factory.Material(Hsv(0.13f, 0.7f, 0.85f), 0.3f), false);

            int petals = 1 + (variant % 6);
            float petalDist = 0.14f + Noise01(variant, 293f) * 0.1f;
            for (int p = 0; p < petals; p++)
            {
                float a = (p / (float)petals) * Mathf.PI * 2f;
                Factory.Primitive(PrimitiveType.Sphere, "Petala" + p, root.transform,
                    new Vector3(Mathf.Cos(a) * petalDist, headY, Mathf.Sin(a) * petalDist),
                    new Vector3(0.22f, 0.13f, 0.22f), petalMat, false);
            }

            root.SetActive(false);
            return root;
        }

        /// <summary>
        /// Cria um template de tufo de capim com quantidade de folhas (3 a 8),
        /// altura, inclinação e cor diferentes por variação.
        /// </summary>
        /// <param name="variant">Índice da variação.</param>
        /// <returns>GameObject template.</returns>
        private GameObject BuildGrass(int variant)
        {
            GameObject root = new GameObject("Prefab_Capim_" + variant);
            root.transform.SetParent(_templateRoot, false);

            Material mat = Factory.Material(Hsv(0.24f + Noise01(variant, 307f) * 0.12f,
                0.42f + Noise01(variant, 311f) * 0.3f, 0.30f + Noise01(variant, 313f) * 0.26f));

            int blades = 3 + (variant % 6);
            for (int i = 0; i < blades; i++)
            {
                float h = 0.3f + Noise01(variant, 317f + i) * 0.45f;
                float a = (i / (float)blades) * Mathf.PI * 2f + Noise01(variant, 331f + i) * 0.6f;
                float r = 0.06f + Noise01(variant, 337f + i) * 0.16f;

                GameObject blade = Factory.Primitive(PrimitiveType.Cube, "Folha" + i, root.transform,
                    new Vector3(Mathf.Cos(a) * r, h * 0.5f, Mathf.Sin(a) * r),
                    new Vector3(0.06f, h * 0.5f, 0.06f), mat, false);
                blade.transform.localRotation = Quaternion.Euler(
                    (Noise01(variant, 347f + i) - 0.5f) * 34f, Noise01(variant, 349f + i) * 360f,
                    (Noise01(variant, 353f + i) - 0.5f) * 34f);
            }

            root.SetActive(false);
            return root;
        }

        /// <summary>
        /// Cria um template de detrito/pedrinha com 2 a 6 fragmentos de
        /// formas (cubo/esfera), tamanhos e cores diferentes por variação.
        /// </summary>
        /// <param name="variant">Índice da variação.</param>
        /// <returns>GameObject template.</returns>
        private GameObject BuildDebris(int variant)
        {
            GameObject root = new GameObject("Prefab_Detrito_" + variant);
            root.transform.SetParent(_templateRoot, false);

            Material mat = Factory.Material(Hsv(0.06f + Noise01(variant, 359f) * 0.07f,
                0.03f + Noise01(variant, 367f) * 0.12f, 0.16f + Noise01(variant, 373f) * 0.22f));

            int pieces = 2 + (variant % 5);
            for (int i = 0; i < pieces; i++)
            {
                PrimitiveType type = (i % 2 == 0) ? PrimitiveType.Cube : PrimitiveType.Sphere;
                float s = 0.08f + Noise01(variant, 379f + i) * 0.22f;
                GameObject piece = Factory.Primitive(type, "Lasca" + i, root.transform,
                    new Vector3((Noise01(variant, 383f + i) - 0.5f) * 0.9f, s * 0.5f,
                        (Noise01(variant, 389f + i) - 0.5f) * 0.9f),
                    new Vector3(s * 1.4f, s, s * 1.2f), mat, false);
                piece.transform.localRotation = Quaternion.Euler(
                    Noise01(variant, 397f + i) * 360f, Noise01(variant, 401f + i) * 360f,
                    Noise01(variant, 409f + i) * 360f);
            }

            root.SetActive(false);
            return root;
        }

        /// <summary>
        /// Cria um template de lixo/poluição. A forma (lata, caixa, sacola),
        /// o tamanho e a cor mudam a cada variação.
        /// </summary>
        /// <param name="variant">Índice da variação.</param>
        /// <returns>GameObject template.</returns>
        private GameObject BuildTrash(int variant)
        {
            GameObject root = new GameObject("Prefab_Lixo_" + variant);
            root.transform.SetParent(_templateRoot, false);

            Material mat = Factory.Material(Hsv(Noise01(variant, 419f), 0.25f + Noise01(variant, 421f) * 0.45f,
                0.30f + Noise01(variant, 431f) * 0.35f));

            int shape = variant % 3;
            PrimitiveType type = shape == 0 ? PrimitiveType.Cylinder
                : (shape == 1 ? PrimitiveType.Cube : PrimitiveType.Sphere);
            float s = 0.5f + Noise01(variant, 433f) * 0.7f;

            GameObject body = Factory.Primitive(type, "Lixo", root.transform,
                new Vector3(0f, s * 0.28f, 0f), new Vector3(0.3f, 0.3f, 0.3f) * s, mat, true);
            body.transform.localRotation = Quaternion.Euler(
                (Noise01(variant, 439f) - 0.5f) * 40f, Noise01(variant, 443f) * 360f,
                (Noise01(variant, 449f) - 0.5f) * 40f);

            root.SetActive(false);
            return root;
        }

        /// <summary>
        /// Cria um template de mancha de cinza/terra queimada: um disco fino
        /// e escuro colado ao chão, que dá o aspecto de área degradada.
        /// Tamanho e tom variam; algumas variações têm duas manchas sobrepostas.
        /// </summary>
        /// <param name="variant">Índice da variação.</param>
        /// <returns>GameObject template.</returns>
        private GameObject BuildAshPatch(int variant)
        {
            GameObject root = new GameObject("Prefab_Cinza_" + variant);
            root.transform.SetParent(_templateRoot, false);

            Material mat = Factory.Material(Hsv(0.05f + Noise01(variant, 457f) * 0.06f,
                0.05f + Noise01(variant, 461f) * 0.14f, 0.10f + Noise01(variant, 463f) * 0.16f));

            float r = 1.4f + Noise01(variant, 467f) * 2.0f;
            Factory.Primitive(PrimitiveType.Cylinder, "Mancha", root.transform,
                new Vector3(0f, 0.012f, 0f), new Vector3(r, 0.012f, r), mat, false);

            if (variant % 2 == 1)
            {
                float r2 = r * (0.45f + Noise01(variant, 479f) * 0.4f);
                Factory.Primitive(PrimitiveType.Cylinder, "ManchaB", root.transform,
                    new Vector3((Noise01(variant, 487f) - 0.5f) * r, 0.014f,
                        (Noise01(variant, 491f) - 0.5f) * r),
                    new Vector3(r2, 0.012f, r2), mat, false);
            }

            root.SetActive(false);
            return root;
        }

        /// <summary>
        /// Cria o template da semente coletável (esfera emissiva + trigger).
        /// </summary>
        /// <returns>GameObject template.</returns>
        private GameObject BuildSeedTemplate()
        {
            GameObject root = new GameObject("Prefab_Semente");
            root.transform.SetParent(_templateRoot, false);

            Material mat = Factory.Material(new Color(0.35f, 1f, 0.45f), 2.2f);
            Factory.Primitive(PrimitiveType.Sphere, "Corpo", root.transform,
                Vector3.zero, Vector3.one * 0.45f, mat, false);

            SphereCollider trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.9f;

            root.AddComponent<Seed>();
            Factory.SetTag(root, "Seed");

            root.SetActive(false);
            return root;
        }

        /// <summary>
        /// Cria o template do inimigo poluente (esfera escura + Rigidbody).
        /// </summary>
        /// <returns>GameObject template.</returns>
        private GameObject BuildEnemyTemplate()
        {
            GameObject root = new GameObject("Prefab_Inimigo");
            root.transform.SetParent(_templateRoot, false);

            Material mat = Factory.Material(new Color(0.30f, 0.28f, 0.26f));
            Factory.Primitive(PrimitiveType.Sphere, "Corpo", root.transform,
                Vector3.zero, new Vector3(1.4f, 1.2f, 1.4f), mat, false);
            Factory.Primitive(PrimitiveType.Sphere, "Olho", root.transform,
                new Vector3(0f, 0.15f, 0.55f), Vector3.one * 0.28f,
                Factory.Material(new Color(1f, 0.35f, 0.2f), 1.5f), false);

            SphereCollider collider = root.AddComponent<SphereCollider>();
            collider.radius = 0.75f;

            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = 6f;
            rb.linearDamping = 2.5f;
            rb.angularDamping = 1.5f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            root.AddComponent<PollutionEnemy>();

            root.SetActive(false);
            return root;
        }
    }
}
