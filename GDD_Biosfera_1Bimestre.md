================================================================================
                GDD - GAME DESIGN DOCUMENT  |  1º BIMESTRE
                       BIOSFERA - A JORNADA DA RESTAURAÇÃO
================================================================================

  Motor:          Unity 6000.0.8f1 (versão exata do Laboratório 12)
  Render Pipeline: Built-in (shader "Standard"). O código detecta
                   automaticamente a URP e usa "Universal Render Pipeline/Lit"
                   caso ela esteja ativa - por isso o jogo nunca fica sem
                   material nem quebra por causa de pacote de pipeline.
  Modo:           3D, Terceira Pessoa
  Tema:           ECORRENOVAÇÃO
  Plataforma:     PC / Windows
  Equipe:         (preencher nome do grupo)

  IMPORTANTE: este GDD descreve SOMENTE o que foi ENTREGUE no 1º bimestre.
  Os números abaixo foram conferidos diretamente no código.

================================================================================
1. VISÃO GERAL (CONCEITO ORIGINAL - NÃO É DA INTERNET)
================================================================================

"BIOSFERA - A Jornada da Restauração" é um jogo 3D de ação e aventura em
terceira pessoa. O jogador controla S.E.E.D., um pequeno robô de tecnologia
verde enviado à "Floresta das Cinzas", uma região devastada por incêndio e
poluição.

Por ser um robô (e não um humano), o S.E.E.D. não usa ferramentas: ele
TRANSPORTA sementes no próprio corpo e as libera ao entrar nas Zonas de
Restauração. Cada semente liberada faz nascer vegetação real no terreno,
pintando o disco da zona de verde em tempo real. Enquanto isso, robôs
poluentes continuam soltando fumaça tóxica e precisam ser neutralizados.

  Missão da fase entregue (Nível 1):
    - Coletar SEMENTES brilhantes espalhadas pelo mapa.
    - Levar as sementes até as ZONAS DE RESTAURAÇÃO (plantio automático).
    - Plantar 5 sementes para restaurar a floresta  -> VITÓRIA.
    - Neutralizar robôs poluentes pulando na cabeça deles.
    - Concluir tudo antes de o cronômetro de 4:00 (240 s) zerar.

================================================================================
2. TEMA ECORRENOVAÇÃO (8 ELEMENTOS - o mínimo exigido é 5)
================================================================================

  1. SEMENTES - esferas verdes emissivas que flutuam, giram e pulsam brilho.
     Representam a esperança da regeneração.
  2. ZONAS DE RESTAURAÇÃO - discos marrons com coluna de luz verde. Conforme
     a semente é plantada, o disco muda de marrom (terra morta) para verde
     (vegetação) por interpolação de cor.
  3. VEGETAÇÃO GERADA POR RESTAURAÇÃO - a cada semente plantada nasce uma
     árvore pequena aleatória dentro da zona (Instantiate a partir de um vetor
     de prefabs GameObject[]).
  4. ROBÔS POLUENTES - inimigos com uma esfera vermelha (olho) que emitem
     FUMAÇA TÓXICA contínua (ParticleSystem em loop).
  5. EXPLOSÃO AO NEUTRALIZAR - ao ser pisado, o poluente explode em partículas
     de explosão + detritos (pedrinhas) e gira no ar por torque.
  6. DETRITOS / LIXO - latas e entulho espalhados pelo cenário devastado.
  7. COGUMELOS E FLORES - indicam o ambiente que pode voltar a viver.
  8. TUFOS DE CAPIM - preenchem o terreno e são a categoria mais numerosa.

================================================================================
3. GAMEPLAY
================================================================================

3.1 CONTROLES
  W A S D / Setas         Movimentar o robô S.E.E.D.
  SHIFT (segurar)         Correr
  ESPAÇO                  Pular
  MOUSE                   Girar a câmera em volta do robô (3ª pessoa)
  P  /  ESC  / botão      Pausar e continuar
  BOTÃO REINICIAR         Reinicia a fase (presente na Pausa, Vitória e Derrota)

3.2 LOOP PRINCIPAL
  a) O robô nasce no centro do mapa.
  b) O HUD mostra: sementes coletadas, sementes plantadas / meta, cronômetro
     e a dica de controle.
  c) O jogador percorre o mapa e coleta sementes (colisão em trigger).
  d) O jogador entra numa Zona de Restauração: o plantio acontece SOZINHO,
     1 semente a cada ~0,9 s, enquanto houver semente na mão.
  e) Plantar 5 sementes  ->  tela de VITÓRIA.
  f) Cronômetro chegar a 0  ->  tela de DERROTA.
  g) Os poluentes patrulham, PERSEGUEM quando o jogador se aproxima (ficam
     avermelhados) e ATRAPALHAM: encostar de lado causa dano (-5 s no
     cronômetro), empurrão (knockback), som de dano e partículas. Pular na
     cabeça deles os neutraliza e dá um impulso para cima no jogador.

3.3 VALORES CONFIGURADOS (conferidos no código)
  Tempo da fase .................. 240 s (4:00)
  Sementes plantadas p/ vencer .... 5
  Sementes espalhadas no mapa ..... 8
  Zonas de Restauração ............ 3
  Inimigos poluentes .............. 4
  Intervalo de plantio ............ 0,9 s
  Velocidade andar / correr ....... 5 / 8,5 m por segundo
  Força do pulo ................... 7 (ForceMode.Impulse)

================================================================================
4. OS 14 ITENS EXIGIDOS - ONDE CADA UM ESTÁ NO PROJETO
================================================================================

  4.1 COLISÃO
      - Player  -> CapsuleCollider (não-trigger) + Rigidbody  [Bootstrap.cs]
      - Semente -> SphereCollider isTrigger  [LevelSpawner.BuildSeedTemplate]
                   coletada em SeedCollector.OnTriggerEnter
      - Zona    -> SphereCollider isTrigger + RestorationZone.OnTriggerEnter /
                   OnTriggerStay / OnTriggerExit (detecta o jogador dentro)
      - Inimigo -> SphereCollider + Rigidbody + PollutionEnemy.OnCollisionEnter
                   (colisão física: pisa em cima = neutraliza)
      - Árvores, pedras e lixo -> colliders físicos de verdade
        (GameObject.CreatePrimitive já vem com collider).

  4.2 MOVIMENTAÇÃO: TRANSLATE e AddForce
      - transform.Translate  -> Seed.Update (a semente flutua no ar)
      - Rigidbody.AddForce   -> PlayerController.FixedUpdate
                                (movimento horizontal, ForceMode.Acceleration)
      - Rigidbody.AddForce   -> PlayerController.Jump
                                (pulo, ForceMode.Impulse)
      - Rigidbody.AddForce   -> PollutionEnemy.FixedUpdate
                                (patrulha e perseguição)

  4.3 ROTAÇÃO
      - transform.Rotate      -> Seed.Update (giro contínuo da semente)
      - Quaternion.Slerp +
        Quaternion.LookRotation -> PlayerController.UpdateAnimator (o modelo
                                   vira para a direção do movimento)
      - Quaternion.Slerp      -> PollutionEnemy.FixedUpdate (vira para onde anda)
      - CameraFollow.LateUpdate -> yaw/pitch com o mouse (órbita da câmera)

  4.4 TORQUE
      - Rigidbody.AddTorque(ForceMode.Impulse) -> PollutionEnemy.Neutralize
        (o poluente gira descontrolado no ar ao ser derrotado).

  4.5 VETOR DE PREFABS (GameObject[])
      - LevelSpawner possui 8 vetores de categoria + vegetação + semente +
        inimigo:  treePrefabs, rockPrefabs, bushPrefabs, mushroomPrefabs,
        flowerPrefabs, grassPrefabs, debrisPrefabs, trashPrefabs,
        vegetationPrefabs, seedPrefab, enemyPrefab.
      - RestorationZone.vegetationPrefabs -> GameObject[] usado no plantio.

  4.6 SCRIPTS C# (14 arquivos)
      Em Assets/Scripts/Biosfera/ (13):
        1.  Bootstrap.cs        8.  Seed.cs
        2.  BiosferaRunner.cs   9.  PollutionEnemy.cs
        3.  Factory.cs          10. RestorationZone.cs
        4.  GameManager.cs      11. CameraFollow.cs
        5.  AudioManager.cs     12. UIManager.cs
        6.  LevelSpawner.cs     13. SeedCollector.cs
        7.  PlayerController.cs
      Em Assets/Editor/ (1):
        14. BiosferaEditorSetup.cs  (cria a cena, o AnimatorController com os
                                     5 estados, o Build Settings e os shaders)

  4.7 PARTÍCULAS (explosão, fumaça e detritos/pedrinhas)
      - Factory.ParticleTemplate(...) cria, por código:
          * explosão do inimigo (FX_ExplosaoInimigo)
          * detritos/pedrinhas (FX_DetritosInimigo)
          * plantio da semente (FX_Plantio)
          * coleta da semente (FX_ColetarSemente)
      - Factory.Smoke(...) -> fumaça tóxica CONTÍNUA (loop) presa ao inimigo.
      - Factory.SpawnBurst(...) -> Instantiate + Play + Destroy automático.

  4.8 SOM
      - AudioManager.cs (Singleton) com 2 AudioSource (música e efeitos).
      - Todos os clipes são gerados por código (Factory.Tone / Factory.Ambient):
          Trilha sonora em loop (BGM_Biosfera)
          SFX: coletar, plantar, neutralizar, pular, vitória, derrota.
      - Chamados por GameManager, PlayerController e AudioManager.

  4.9 ANIMAÇÃO COM OS 5 ESTADOS (Idle, Walk, Run, Jump, Fall)
      - BiosferaEditorSetup.EnsureAnimatorController cria o AnimatorController
        "SEEED07_Animator" com os 5 estados e os parâmetros
        Speed (Float), VerticalSpeed (Float) e Grounded (Bool).
      - PlayerController.UpdateAnimator calcula o estado atual e chama
        Animator.CrossFade(hash). Além disso, existe uma animação procedural
        (o modelo "respira"/balança conforme a velocidade), garantindo que o
        robô NUNCA pareça congelado, mesmo sem clipes importados.

  4.10 RANDOM PARA ESCOLHER POSIÇÃO (e outros sorteios)
      - LevelSpawner.RandomGroundPosition usa Random.Range para X e Z dentro
        da área jogável, respeitando um raio livre no centro (onde o jogador
        nasce). Também sorteia a rotação Y e a escala de cada asset.
      - RestorationZone.PlantOne sorteia o prefab do vetor, o ângulo e o raio.
      - PollutionEnemy sorteia os 4 waypoints de patrulha ao redor do spawn.
      - Factory.Tone usa UnityEngine.Random para o ruído dos sons.

  4.11 UI COM TEXTO E BOTÃO REINICIAR (obrigatório)
      - Bootstrap.CreateUserInterface monta tudo por código (UGUI, sem TMP):
          HUD: Texto de sementes, Texto de tempo, Texto de dica, botão PAUSAR
          3 painéis (Pausa / Vitória / Derrota), e CADA UM tem
          BOTÃO REINICIAR + BOTÃO SAIR.
      - UIManager.cs conecta os cliques: REINICIAR -> GameManager.RestartLevel.

  4.12 SPAWN DE OBJETOS COM Instantiate
      - LevelSpawner.SpawnCategory -> Instantiate(prefab, pos, rotação aleatória)
      - LevelSpawner.SpawnSeeds / SpawnEnemies -> Instantiate
      - RestorationZone.PlantOne -> Instantiate da vegetação
      - Factory.SpawnBurst -> Instantiate das partículas

  4.13 TAGS
      - Configuradas em ProjectSettings/TagManager.asset:
          Player, Seed, RestorationZone, Enemy, Ground
      - Aplicadas em tempo de execução por Factory.SetTag (de forma segura).

  4.14 DOCUMENTAÇÃO XML NO CÓDIGO
      - Todas as classes e funções têm /// <summary>, /// <param> e
        /// <returns> quando aplicável, no padrão do C#:
            /// <summary> O que a função faz. </summary>
            /// <param name="x"> O que é o argumento. </param>
            /// <returns> O que ela devolve. </returns>
      - Exemplos: Bootstrap.Build, PlayerController.CheckGrounded,
        RestorationZone.PlantOne, Factory.Tone, entre outras.

================================================================================
5. NÍVEIS
================================================================================

  O projeto completo preverá 5 níveis (2º bimestre). No 1º BIMESTRE é
  entregue SOMENTE o Nível 1 - "Floresta das Cinzas" -, que já contém todas
  as mecânicas exigidas e ~340 assets instanciados por código.

  Nível 1 - Floresta das Cinzas (entregue)
    * Terreno plano de 90 m x 90 m, cor de terra queimada.
    * Área de spawn dos assets: de -34 a +34 (raio livre de 4,5 m no centro).
    * Assets instanciados por código (Instantiate sobre vetores de prefabs):
        26 Manchas de cinza | Cerca viva (44 árvores na borda)
        20 Cogumelos        | 34 Flores
        78 Tufos de capim   | 34 Detritos
        14 Lixos            | 44 Pedras/Arbustos em aglomerados
        27 a 54 Árvores em aglomerados
        --------------------------
        + 8 Sementes coletáveis + 4 Inimigos poluentes
        = ~340 ASSETS (319 a 376, varia a cada partida)
    * Os assets são gerados em 104 MODELOS DIFERENTES entre si (variação
      procedural de forma, tamanho e cor) - bem acima do mínimo de 70.
    * Ainda são criados: 1 chão, 3 zonas de restauração (disco + baliza +
      luz = 9 objetos), o robô (4 partes de modelo) e a interface completa.
    * O mínimo exigido pelo professor é 70 assets - o projeto entrega ~340,
      em 104 MODELOS diferentes.

================================================================================
5.1 O MUNDO AO REDOR (a fase não fica "no vácuo")
================================================================================

  Além da área jogável, o Bootstrap monta por código um cenário completo:
    * Terreno distante de 700 m x 700 m, abaixo do chão jogável;
    * Floresta de fundo com 150 árvores em anel (raio de 48 a 108 m);
    * 40 montanhas no horizonte (raio de 170 a 300 m);
    * Névoa linear (65 m a 320 m) e farClipPlane de 700 m, dando profundidade;
    * 4 paredes invisíveis (BoxCollider) em ±44,5 m - cerca natural que
      impede o robô de cair para fora do mapa.

================================================================================
5.2 VIDAS DO ROBÔ E MORTE (o dano tem consequência)
================================================================================

  * O robô S.E.E.D. tem 3 VIDAS (GameManager.maxLives), mostradas no HUD
    ("VIDA: 3 / 3 [##] [##] [##]").
  * Encostar DE LADO num robô poluente tira 1 VIDA, desconta 5 segundos do
    cronômetro, aplica knockback, pisca o robô de vermelho e toca som de dano.
    Há 1 s de invulnerabilidade (hitCooldown) entre dois golpes.
  * Com as 3 vidas zeradas -> PlayerController.Die(): o modelo é escondido,
    dispara explosão verde e abre o PAINEL DE DERROTA com o título
    "S.E.E.D. DESATIVADO". Se o tempo acabar antes, o título é
    "TEMPO ESGOTADO".
  * Pular NA CABEÇA do inimigo continua sendo o jeito de neutralizá-lo (e
    não consome vida).

================================================================================
6. QUALIDADE DOS GRÁFICOS
================================================================================

  6.1 Todos os assets são GERADOS PROCEDURALMENTE por código
      (GameObject.CreatePrimitive: Cube, Sphere, Cylinder, Plane) com
      materiais coloridos, escala e rotação aleatórias -> variação visual.
  6.2 Nada é importado da internet: nenhum modelo, textura, som ou partícula
      veio de fora. Isso garante a ORIGINALIDADE exigida.
  6.3 Iluminação: luz direcional (Sol) com sombras suaves, luz ambiente
      Trilight e névoa linear (65 m a 320 m) para dar profundidade - reforçada
      pelo mundo ao redor (terreno distante, floresta e montanhas).
  6.4 As Zonas de Restauração têm luz pontual verde e baliza alta, para o
      jogador localizar o objetivo de longe.

================================================================================
7. GAME MECHANIC (MECÂNICA DIFERENCIAL ORIGINAL)
================================================================================

  "TRANSPORTE E LIBERAÇÃO (o robô não planta, ele libera)"
  - Diferente de jogos onde se clica para plantar, aqui o robô é um
    CONTÊINER: as sementes ficam guardadas nele e são liberadas
    automaticamente enquanto ele permanece sobre a Zona de Restauração.
    Isso cria uma decisão tática: ficar parado na zona é seguro mas consome
    tempo; andar pelo mapa é mais rápido, porém expõe você aos poluentes.
  - Por ser um robô de metal, o inimigo só pode ser vencido POR CIMA
    (pular na cabeça). Bater DE LADO tira 1 VIDA e empurra você (knockback),
    o que reforça a mecânica de plataforma em um jogo 3D.

================================================================================
8. PARTICIPAÇÃO DO GRUPO
================================================================================

  (PREENCHER PELO GRUPO)
    Nome 1: ______% (ex.: jogabilidade / scripts)
    Nome 2: ______% (ex.: UI / level design)
    Nome 3: ______% (ex.: som / partículas / GDD)
  TOTAL: 100%

================================================================================
9. CHECKLIST DE ENTREGA (1º BIMESTRE)
================================================================================

  [x] Tema Ecorrenovação com mais de 5 elementos (8 entregues)
  [x] Todos os 14 itens dos tutoriais implementados e documentados
  [x] Nível 1 com mais de 70 assets diferentes -> 104 MODELOS distintos
      (~340 assets instanciados no total)
  [x] GDD detalhado (este documento)
  [x] Documentação XML em todas as funções dos scripts
  [x] Gameplay funcional (coletar -> plantar -> vencer)
  [x] Sistema de dano com consequência: 3 vidas e morte do robô
  [x] Mundo ao redor: terreno distante, floresta de fundo e montanhas
  [x] Game Mechanic original ("Transporte e Liberação")
  [x] Partículas: explosão, fumaça e detritos/pedrinhas
  [x] Som: trilha sonora + efeitos, tudo gerado por código
  [x] Animação com os 5 estados (Idle, Walk, Run, Jump, Fall)
  [x] UI com TEXTO e BOTÃO REINICIAR obrigatório
  [x] Movimentação com Translate e AddForce
  [x] Rotação e Torque com Rigidbody
  [x] Vetores de prefabs (GameObject[]) e Random de posição
  [x] Tags obrigatórias configuradas
  [x] Rodar sem erro nenhum: apenas abrir o Unity e apertar Play

================================================================================
FIM DO GDD - 1º BIMESTRE
================================================================================
