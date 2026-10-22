================================================================================
              GUIA TÉCNICO - COMO O PROJETO FUNCIONA POR DENTRO
                BIOSFERA - A JORNADA DA RESTAURAÇÃO  |  1º BIMESTRE
                     Unity 6000.0.8f1  |  Render Pipeline: Built-in
================================================================================

  Este documento é para QUEM QUER ENTENDER o código.
  Para apenas JOGAR, leia o LEIA-ME_PRIMEIRO.md (são só 3 passos).

  IMPORTANTE: todo o guia antigo de "montar a mão no Editor" foi removido,
  porque NÃO existe nada para montar à mão. Se você seguisse o guia antigo
  (criar projeto URP novo, arrastar objetos, criar NavMesh, etc.) você
  QUEBRARIA este projeto. Aqui tudo é criado por código.

================================================================================
1. A IDEIA CENTRAL: "ZERO-CLIQUE"
================================================================================

  O projeto não tem nenhum objeto salvo "montado à mão" na cena.
  A cena Level1.unity está praticamente VAZIA de propósito.

  Tudo (chão, sol, câmera, jogador, inimigos, ~340 assets, HUD, botões)
  é CONSTRUÍDO POR CÓDIGO quando você aperta PLAY.

  Isso é o que garante o requisito do trabalho de "gerar a fase por
  código" e, ao mesmo tempo, evita erros de referência quebrada
  (aquele clássico "Missing (Mono Script)") no Inspector.

================================================================================
2. OS DOIS PONTOS DE ENTRADA AUTOMÁTICOS
================================================================================

  (A) NO EDITOR - Assets/Editor/BiosferaEditorSetup.cs
      Atributo: [InitializeOnLoad]
      Roda sozinho quando o Unity termina de compilar, UMA vez por sessão,
      e cria os arquivos que o projeto precisa ter no disco:

        1) Assets/Scenes/                     (pasta)
        2) Assets/Resources/                  (pasta)
        3) Assets/Resources/SEEED07_Animator.controller
             -> AnimatorController com os 5 estados obrigatórios:
                Idle, Walk, Run, Jump, Fall
             -> Parâmetros: Speed (float), VerticalSpeed (float),
                Grounded (bool)
        4) Assets/Scenes/Level1.unity         (a cena do jogo)
        5) Build Settings com a Level1 como primeira cena
        6) "Always Included Shaders": Standard, Particles/Standard Unlit,
           Sprites/Default e UI/Default (evita material rosa em build)

      Esses 6 itens JÁ ESTÃO CRIADOS na pasta que você recebeu.
      Se apagar algum, ele é recriado na próxima vez que o Unity abrir.

      Para forçar a recriação manualmente:
        Window > General > Console  (não é aqui)
        -> Use o menu: nenhum. Basta apagar o arquivo e reabrir o Unity.
      Ou, pela linha de comando (opcional):
        Unity.exe -batchmode -quit -projectPath "<pasta do projeto>"
                  -executeMethod Biosfera.EditorTools.BiosferaEditorSetup.RunOnce

  (B) NO PLAY - Assets/Scripts/Biosfera/Bootstrap.cs
      Atributo: [RuntimeInitializeOnLoadMethod(AfterSceneLoad)]
      Roda sozinho assim que a cena carrega, ANTES do primeiro frame.
      É aqui que o jogo inteiro é montado.

================================================================================
3. ORDEM DE MONTAGEM DO JOGO (Bootstrap.Build)
================================================================================

  O método Bootstrap.Build() cria, nesta ordem:

    1. Objeto raiz  "== BIOSFERA =="   (agrupa tudo na Hierarchy)
    2. SetupEnvironment()   -> luz ambiente (Trilight), névoa linear
                               65..320 m e a luz direcional "Sol"
    3. CreateGround()       -> Plane 90 x 90 m, Tag "Ground"
    4. CreateWorld()        -> o MUNDO AO REDOR (terreno distante 700x700,
                               floresta de fundo, montanhas no horizonte e
                               4 paredes invisíveis no limite - ver seção 5.1)
    5. SetupCamera()        -> reutiliza a Main Camera (ou cria uma), garante
                               um AudioListener e farClipPlane = 700 m
    6. CreateManager<GameManager>()  -> objeto "GameManager" (tempo, VIDAS)
    7. CreateManager<AudioManager>() -> objeto "AudioManager"
    8. CreatePlayer()       -> o robô S.E.E.D.07 (ver seção 4)
    9. WireCameraFollow()   -> liga a câmera ao jogador (3ª pessoa)
   10. CreateLevelSpawner() -> instancia os ~340 assets (ver seção 6)
   11. CreateRestorationZones() -> 3 Zonas de Restauração
   12. CreateUserInterface()    -> Canvas, HUD, painéis e botões

  Proteção contra duplicação: se o objeto raiz ou um GameManager já
  existir, Build() simplesmente não faz nada.

================================================================================
4. O JOGADOR (robô S.E.E.D.07)
================================================================================

  Hierarquia criada por Bootstrap.CreatePlayer():
     Player
       |- Rigidbody          (mass 1.2, linearDamping 0.15, angularDamping 4)
       |- CapsuleCollider    (height 1.8, radius 0.42, center Y 0.9)
       |- Animator           (SEEED07_Animator, carregado de Resources)
       |- PlayerController   (movimento, pulo, animação)
       |- SeedCollector      (coleta de sementes)
       |- Modelo
            |- Corpo   (Capsule)
            |- Cabeca  (Sphere)
            |- Olho    (Sphere emissiva)
            |- Anel    (Cylinder, detalhe verde)

  Tag aplicada: "Player" (Factory.SetTag, à prova de exceção).

  OBS sobre a API do Unity 6 usada no projeto:
    - rb.linearVelocity      (e NÃO o antigo rb.velocity)
    - rb.linearDamping       (e NÃO o antigo rb.drag)
    - rb.angularDamping      (e NÃO o antigo rb.angularDrag)
    - FindFirstObjectByType  (e NÃO o obsoleto FindObjectOfType)

================================================================================
5. FLUXO DO JOGO E EVENTOS
================================================================================

  GameManager (Singleton, GameManager.Instance) é o "cérebro":

    Estado que ele guarda:
      levelTimeLimit = 240 s     seedsToWin = 5
      seedsCollected, seedsPlanted, enemiesNeutralized
      timeLeft, isPaused, levelFinished
      lives, maxLives = 3        hitCooldown = 1 s     loseReason

    Eventos que ele dispara:
      OnStatsChanged  -> UIManager.Refresh()      (atualiza o HUD)
      OnTimeChanged   -> UIManager.OnTimeChanged() (atualiza o relógio)
      OnLevelEnded    -> UIManager.OnLevelEnded()  (abre Vitória/Derrota)

  Ciclo de uma partida:
    1. SeedCollector.OnTriggerEnter encosta na semente
         -> Seed.MarkCollected() desativa a semente
         -> SeedsInHand++  e  GameManager.RegisterSeedCollected()
    2. RestorationZone.OnTriggerStay mantém _playerInside = true
         -> a cada 0.9 s chama PlantOne()
         -> consome 1 semente, Instantiate de vegetação aleatória,
            partículas de plantio e a cor do disco vai ficando verde
         -> GameManager.RegisterSeedPlanted()
    3. Ao chegar em 5 plantadas -> GameManager.EndLevel(true) -> Vitória
       Ao chegar em 0 s          -> GameManager.EndLevel(false) -> Derrota
    4. PollutionEnemy encosta de lado no jogador
         -> GameManager.RegisterPlayerHit(5f): tira 1 VIDA, desconta 5 s,
            aplica knockback, pisca o robô de vermelho e toca som de dano;
            respeita hitCooldown (1 s de invulnerabilidade entre golpes)
         -> lives == 0 -> loseReason = "S.E.E.D. DESATIVADO"
            -> GameManager.EndLevel(false) -> Derrota (PlayerController.Die)

  Isso é o Tema Ecorrenovação na prática: o robô TRANSPORTA a semente
  no corpo e a LIBERA na zona, que então reverdece em tempo real.

  --------------------------------------------------------------------------
  5.1 VIDAS DO ROBÔ, DANO E O MUNDO AO REDOR
  --------------------------------------------------------------------------

    VIDAS: o jogador tem 3 vidas (GameManager.maxLives). Cada encostão de
    lado em um robô poluente chama GameManager.RegisterPlayerHit(): perde
    1 vida, perde 5 s do cronômetro, leva knockback (PlayerController.
    ApplyKnockback) e pisca de vermelho. Com lives == 0, o robô é
    "desativado" (PlayerController.Die esconde o modelo e dispara uma
    explosão verde) e o painel de derrota abre com "S.E.E.D. DESATIVADO".
    Se o tempo acabar antes, o título é "TEMPO ESGOTADO". Pular NA CABEÇA
    do inimigo continua sendo o jeito de neutralizá-lo (não gasta vida).

    MUNDO AO REDOR: Bootstrap.CreateWorld() garante que a fase não fique
    num "vácuo", montando tudo por código:
      - CreateDistantTerrain -> Plane 700 x 700 m bem abaixo de tudo;
      - CreateDistantForest  -> 150 árvores em anel (raio de 48 a 108 m);
      - CreateDistantHills   -> 40 montanhas no horizonte (raio 170 a 300 m);
      - CreateBoundaryWalls  -> 4 BoxColliders invisíveis em +-44.5 m.
    A névoa linear (65..320 m) e o farClipPlane da câmera (700 m) fazem o
    horizonte se dissolver com profundidade, escondendo o fim do terreno.

================================================================================
6. OS ~340 ASSETS INSTANCIADOS (mínimo exigido pelo professor: 70)
================================================================================

  Todos criados por LevelSpawner.SpawnAll() com Instantiate sobre
  VETORES de prefabs (GameObject[]) e posição/rotação ALEATÓRIAS
  (Random.Range + RandomGroundPosition, área de -34 a +34 m).

    Categoria                 Quantidade   Configurável em (Inspector)
    ----------------------    ----------   ---------------------------
    Manchas de cinza               26      ashCount
    Árvores em aglomerados     27 a 54     treeClusters / treePerClusterMin / Max
    Pedras em aglomerados      14 a 28     rockClusters
    Arbustos em aglomerados    16 a 32     bushClusters
    Cerca viva (borda)             44      borderTreeCount
    Cogumelos                      20      mushroomCount
    Flores                         34      flowerCount
    Tufos de capim                 78      grassCount
    Detritos (pedrinhas)           34      debrisCount
    Lixo (latas)                   14      trashCount
    ----------------------    ----------
    Subtotal ambiente         307 a 364
    Sementes coletáveis             8      seedCount
    Robôs poluentes                 4      enemyCount
    ----------------------    ----------
    TOTAL                     319 a 376    (na última validação: 340)

  Os valores são aleatórios a cada partida (Random.Range nos aglomerados),
  por isso o total varia - e é impresso no Console pelo LevelSpawner.

  --------------------------------------------------------------------------
  6.1 OS 104 MODELOS **DIFERENTES ENTRE SI** (requisito "70 assets diferentes")
  --------------------------------------------------------------------------

  Não basta instanciar ~340 cópias: o professor pede 70 assets DIFERENTES.
  Por isso o LevelSpawner gera 104 MODELOS distintos, cada um com forma,
  tamanho e cor próprios (método BuildVariants + EnsureTemplates):

    Categoria      Modelos distintos   Campo no Inspector
    -------------  -----------------   ------------------
    Árvores               12           treeVariants
    Pedras                12           rockVariants
    Arbustos              10           bushVariants
    Cogumelos             10           mushroomVariants
    Flores                12           flowerVariants
    Capim                 10           grassVariants
    Detritos              10           debrisVariants
    Lixo                  10           trashVariants
    Manchas de cinza       8           ashVariants
    Vegetação (zona)       8           vegetationVariantCount
    Semente                1           (Seed)
    Inimigo poluente       1           (Enemy)
    -------------  -----------------
    TOTAL                104 modelos diferentes  (mínimo exigido: 70)

  Como cada variação é diferente: a cor sai de uma função de matiz/saturação/
  brilho (Hsv) e o formato/tamanho sai de Noise01(índice, tempero) - um valor
  estável por índice. Ou seja: a variação 3 é SEMPRE igual a si mesma, mas
  nunca igual à variação 4 (altura, tronco, nº de copas, nº de pétalas,
  nº de folhas, forma da pedra etc. mudam). O total aparece no Console:
      [BIOSFERA] LevelSpawner instanciou 340 assets ... em 104 modelos
      DIFERENTES entre si.
  E também é exposto em LevelSpawner.DistinctPrefabCount.

  Se algum vetor de prefabs estiver vazio, o próprio script cria os
  templates por código (EnsureTemplates) dentro de "_Templates"
  (uma raiz desativada). Assim o jogo NUNCA fica sem assets.

  Importante: como os templates são objetos DESATIVADOS, todo
  Instantiate é seguido de SetActive(true) para o clone aparecer.

================================================================================
7. OS 14 ITENS DO CHECKLIST -> ONDE ESTÁ CADA UM NO CÓDIGO
================================================================================

   1) COLISÃO
      Rigidbody + CapsuleCollider no Player ................ Bootstrap.CreatePlayer
      SphereCollider (isTrigger) nas sementes/zona ......... LevelSpawner.BuildSeedTemplate
      SphereCollider (sólido) no inimigo ................... LevelSpawner.BuildEnemyTemplate
      OnCollisionEnter (pisar no inimigo) .................. PollutionEnemy.OnCollisionEnter

   2) MOVIMENTAÇÃO
      transform.Translate (flutuação da semente) ........... Seed.Update
      Rigidbody.AddForce (andar/correr) .................... PlayerController.FixedUpdate
      Rigidbody.AddForce ForceMode.Impulse (pulo) .......... PlayerController.Jump

   3) ROTAÇÃO
      transform.Rotate (a semente gira) .................... Seed.Update
      Quaternion.Slerp (o robô vira para onde anda) ........ PlayerController.UpdateAnimator

   4) TORQUE
      Rigidbody.AddTorque ForceMode.Impulse ................ PollutionEnemy.Neutralize

   5) VETOR DE PREFABS GameObject[]
      LevelSpawner: treePrefabs, rockPrefabs, bushPrefabs,
      mushroomPrefabs, flowerPrefabs, grassPrefabs,
      debrisPrefabs, trashPrefabs, vegetationPrefabs
      RestorationZone: vegetationPrefabs

   6) SCRIPTS C#  -> 13 em Assets/Scripts/Biosfera + 1 em Assets/Editor

   7) PARTÍCULAS (explosão / fumaça / detritos)
      Factory.ParticleTemplate + Factory.SpawnBurst
        - explosão .......... "FX_ExplosaoInimigo" (PollutionEnemy.Start)
        - detritos/pedrinhas  "FX_DetritosInimigo"  (PollutionEnemy.Start)
        - fumaça contínua ... Factory.Smoke           (PollutionEnemy.Start)
        - plantio ........... "FX_Plantio"            (RestorationZone.Awake)
        - coleta ............ "FX_ColetarSemente"     (SeedCollector.Awake)

   8) SOM
      AudioManager: 2 AudioSources (música em loop + efeitos).
      Clipes gerados por código (Factory.Tone / Factory.Ambient):
        BGM_Biosfera, SFX_Coletar, SFX_Plantar, SFX_Neutralizar,
        SFX_Pulo, SFX_Vitoria, SFX_Derrota.

   9) ANIMAÇÃO COM 5 ESTADOS (Idle, Walk, Run, Jump, Fall)
      Criados pelo BiosferaEditorSetup (SEEED07_Animator.controller).
      Quem aciona: PlayerController.UpdateAnimator
        (calcula o estado pela velocidade e usa Animator.CrossFade).
      OBS: como os estados não têm clipes (não usamos animação da
      internet), o PlayerController também faz uma ANIMAÇÃO PROCEDURAL
      (flutuação + inclinação do modelo) para o robô nunca ficar "duro".

  10) RANDOM (posição / rotação / escala)
      LevelSpawner.RandomGroundPosition (posição)
      LevelSpawner.SpawnCategory (rotação Random 0..360 e escala Random)
      RestorationZone.PlantOne (ângulo e raio aleatórios)

  11) UI COM TEXTO E **BOTÃO REINICIAR** (obrigatório)
      Bootstrap.CreateUserInterface + UIManager.
      Textos: sementes, cronômetro, dica.
      Botões: PAUSAR (HUD), CONTINUAR, REINICIAR, SAIR.
      O botão REINICIAR aparece nos 3 painéis: Pausa, Vitória e Derrota.
      A fonte vem do próprio Unity (Resources.GetBuiltinResource<Font>),
      então não é preciso importar TextMeshPro nem nenhum pacote extra.

  12) INSTANTIATE (geração em tempo de execução)
      LevelSpawner.SpawnCategory / SpawnSeeds / SpawnEnemies
      RestorationZone.PlantOne
      Factory.SpawnBurst (partículas)

  13) TAGS
      Definidas em ProjectSettings/TagManager.asset:
        Player, Seed, RestorationZone, Enemy, Ground
      Aplicadas por Factory.SetTag (com try/catch: uma tag ausente
      gera apenas um aviso, nunca derruba o jogo).

  14) DOCUMENTAÇÃO XML POR FUNÇÃO
      Todos os métodos têm /// <summary> com <param> e <returns>.
      Basta abrir qualquer .cs de Assets/Scripts/Biosfera.

================================================================================
8. COMO O BOTÃO REINICIAR REALMENTE FUNCIONA
================================================================================

  Esse foi o ponto mais delicado do projeto. Explicando:

  - O [RuntimeInitializeOnLoadMethod] roda UMA ÚNICA VEZ em todo o Play.
    Se o REINICIAR apenas recarregasse a cena, nada seria remontado
    (a cena está vazia) e o jogo apareceria em branco.

  - Solução: BiosferaRunner é um objeto com DontDestroyOnLoad que
    assina SceneManager.sceneLoaded. Sempre que QUALQUER cena termina
    de carregar, ele chama Bootstrap.Build() de novo.

  - Além disso, GameManager.RestartLevel() tem DUAS rotas:
      a) se a cena tem buildIndex (está no Build Settings) -> LoadScene
         (e o BiosferaRunner remonta o jogo);
      b) se não tem (ex.: você está numa cena qualquer sem Build
         Settings) -> Bootstrap.Rebuild() remonta tudo em memória.

  Resultado: o botão REINICIAR funciona SEMPRE, em qualquer situação.

  Detalhe importante: as raízes "_AssetsSpawnados" e "_Templates" do
  LevelSpawner são "presas" ao próprio spawner no Awake(). Sem isso,
  os objetos recriados se acumulariam (vazamento) no reinício em memória.

================================================================================
9. DECISÕES TÉCNICAS (e o porquê de cada uma)
================================================================================

  * SEM URP (Built-in) -> o Packages/manifest.json só tem o uGUI e os
    módulos internos do Unity. Isso elimina a família de erros
    "Missing types referenced from UniversalRenderPipelineGlobalSettings"
    que acontece quando uma URP de versão incompatível é instalada.
    Factory.Material() ainda assim detecta o pipeline ativo
    (GraphicsSettings.currentRenderPipeline) e escolhe automaticamente
    "Universal Render Pipeline/Lit" se você um dia ativar a URP.
    Ou seja: funciona nos dois casos, sem material rosa.

  * SEM TextMeshPro -> a UI usa o Text/Button clássicos (UGUI). Evita o
    passo manual "Import TMP Essentials" e evita dependência de pacote.

  * SEM NavMesh -> os inimigos usam Rigidbody.AddForce + waypoints
    calculados em código. Nada para "bakear" no Editor.

  * SEM assets externos -> materiais, meshes (primitivas), partículas e
    sons são gerados 100% por código (Factory). Nada foi baixado da
    internet, o que atende ao requisito de ideia/arte original.

  * SEM reflection -> todos os campos são public e são preenchidos
    diretamente pelo Bootstrap. Código simples, fácil de ler e de
    depurar (reflection era a origem de erros difíceis na versão antiga).

  * Partículas: as partículas usam simulacao em World Space e são
    destruídas sozinhas (Object.Destroy(go, segundos)).

================================================================================
10. SOLUÇÃO DE PROBLEMAS
================================================================================

  * Tela vazia ao apertar Play
    -> Aperte Play novamente. O jogo monta em QUALQUER cena.
    -> Para garantir, abra Assets/Scenes/Level1.unity antes do Play.

  * Materiais rosa / roxo
    -> Só ocorreria se o shader "Standard" não existisse. Ele existe em
       qualquer instalação do Unity (Built-in). Se você ativou a URP,
       o Factory troca sozinho para "Universal Render Pipeline/Lit".

  * Erro "Missing types ... UniversalRenderPipelineGlobalSettings"
    -> URP desatualizada no projeto. Feche o Unity, apague as pastas
       "Library" e "Temp" e abra de novo. Este projeto não depende de URP.

  * Erro de compilação em algum script
    -> Feche o Unity, apague "Library", "Temp" e "obj" e abra de novo.
       O Unity recompila tudo do zero.

  * "The type or namespace name 'InputSystem' could not be found"
    -> Não acontece aqui: o projeto usa o Input Manager antigo
       (ProjectSettings > Player > Active Input Handling = Input Manager).
       Não instale o pacote Input System para este projeto.

  * Console mostra avisos amarelos sobre Tags
    -> Só aconteceria se alguém apagasse as tags em
       Edit > Project Settings > Tags and Layers. As 5 tags
       (Player, Seed, RestorationZone, Enemy, Ground) já estão criadas.

  * Números no Console (é normal, não é erro):
      [BIOSFERA] Cena Assets/Scenes/Level1.unity criada automaticamente.
      [BIOSFERA] AnimatorController criado com os 5 estados de animação.
      [BIOSFERA] LevelSpawner instanciou 340 assets de ambiente ...
      [BIOSFERA] Cena montada automaticamente pelo Bootstrap.

================================================================================
FIM DO GUIA TÉCNICO
================================================================================
