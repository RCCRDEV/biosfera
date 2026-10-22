================================================================================
        BIOSFERA - A JORNADA DA RESTAURAÇÃO  |  1º BIMESTRE
                     "ABRE O UNITY E APERTA PLAY"
================================================================================

  Jogo ORIGINAL (criado do zero, não é cópia de internet)
  Tema: ECORRENOVAÇÃO
  Unity: 6000.0.8f1 (exatamente a versão do Laboratório 12)
  14 scripts C# com DOCUMENTAÇÃO XML em todas as funções
  ~340 assets instanciados por código, em 104 MODELOS DIFERENTES entre si
     (o mínimo exigido pelo professor é 70 assets diferentes)
  Os 14 itens dos tutoriais estão todos implementados
  Botão REINICIAR obrigatório presente
  Você NÃO arrasta nada, NÃO cria cena, NÃO cria tag: é tudo automático

================================================================================
PASSO A PASSO (só isso, 3 passos)
================================================================================

PASSO 1 - ABRIR O PROJETO
  a) Abra o Unity Hub.
  b) Clique em "Open" (ou "Add" / "Add project from disk").
  c) Selecione ESTA pasta (a pasta que contém Assets, Packages e
     ProjectSettings - ou seja, a pasta onde está este arquivo).
  d) Se o Hub pedir a versão, escolha 6000.0.8f1.
     Se ela não aparecer:
        Installs -> Install Editor -> Archive -> procure 6000.0.8f1 e instale.

PASSO 2 - ESPERAR O UNITY TERMINAR A PRIMEIRA ABERTURA
  a) O Unity vai importar e compilar tudo. Isso demora um pouco (é normal).
  b) Espere a rodinha/barra de progresso do canto inferior direito
     DESAPARECER por completo.
  c) ESTES ARQUIVOS JÁ ESTÃO PRONTOS NA PASTA (você não precisa criar nada):
        - Assets/Scenes/Level1.unity                    (a cena do jogo)
        - Assets/Resources/SEEED07_Animator.controller  (Animator com os
          5 estados: Idle, Walk, Run, Jump, Fall)
        - a cena Level1 já registrada no Build Settings
     Se algum dia você apagar um deles, o script de Editor
     BiosferaEditorSetup recria tudo sozinho quando o Unity abrir.
  d) O Unity abre a cena Assets/Scenes/Level1.unity automaticamente.
  e) Confira na janela "Console" (menu Window -> General -> Console) que
     NÃO existe nenhuma linha vermelha.

PASSO 3 - APERTAR PLAY
  a) Clique no botão PLAY (triângulo preto, no topo central da janela).
  b) PRONTO. O jogo se monta sozinho na hora. No Console aparece:
        [BIOSFERA] Cena montada automaticamente pelo Bootstrap.
        Total de assets instanciados: ~340 (104 modelos diferentes).
  c) Clique na tela do jogo para travar o mouse e começar a jogar.

  > O jogo monta a partir de QUALQUER cena, até uma cena vazia. Se você
    apertar Play e estiver na cena errada, ainda assim funciona.

================================================================================
CONTROLES
================================================================================

  W A S D  (ou setas)      Andar com o robô S.E.E.D.
  SHIFT (segurar)          Correr
  ESPAÇO                   Pular  (pule NA CABEÇA do inimigo para neutralizá-lo)
  MOUSE                    Girar a câmera em volta do robô
  P   (ou ESC / botão)     Pausar / Continuar
  BOTÃO REINICIAR (na tela) Reinicia a fase inteira

================================================================================
VIDAS DO ROBÔ (o inimigo te dá dano E você pode PERDER)
================================================================================

  O robô S.E.E.D. começa com 3 VIDAS (aparecem no HUD, no canto superior
  esquerdo: "VIDA: 3 / 3 [##] [##] [##]").

  Cada encostão de lado em um robô poluente tira 1 VIDA, desconta 5 segundos
  do cronômetro, empurra você (knockback), pisca o robô de vermelho e sai
  som de dano. Existe 1 segundo de "invulnerabilidade" entre dois golpes,
  para dois inimigos colados não tirarem tudo de uma vez.

  Quando as 3 vidas acabam -> o robô é DESATIVADO (explosão verde) e aparece
  o PAINEL DE DERROTA com o título "S.E.E.D. DESATIVADO" e o botão REINICIAR.
  Se o tempo acabar antes disso, o título é "TEMPO ESGOTADO".

================================================================================
O MUNDO AO REDOR (nada de mapa "no vácuo")
================================================================================

  Ao redor da área jogável existe um cenário de verdade, todo gerado por
  código pelo Bootstrap:
    - Terreno distante gigante (700 x 700 m) embaixo de tudo;
    - Uma floresta de fundo com 150 árvores em anel (raio de 48 a 108 m);
    - 40 montanhas no horizonte (raio de 170 a 300 m);
    - Névoa linear (de 65 a 320 m) que dá profundidade ao horizonte;
    - 4 paredes invisíveis no limite do mapa, para o robô não cair para fora.
  Assim o cenário tem horizonte, profundidade e "cerca natural" em volta.

================================================================================
COMO SE JOGA (objetivo da fase)
================================================================================

  1. O robô nasce no centro do mapa (uma floresta devastada).
  2. Existem 8 SEMENTES brilhantes (verdes) espalhadas pelo terreno.
     Encostar nelas coleta (som + partículas de faísca verde).
  3. Existem 3 ZONAS DE RESTAURAÇÃO (discos marrons com uma coluna de luz
     verde no alto). Ao ENTRAR na zona com sementes na mão, o plantio é
     AUTOMÁTICO: a cada ~1 segundo o robô planta 1 semente, nasce vegetação
     de verdade no disco e ele vai ficando verde.
  4. Plante 5 SEMENTES  ->  PAINEL DE VITÓRIA (com o botão REINICIAR).
  5. O tempo começa em 4:00 (240 segundos). Se zerar -> PAINEL DE DERROTA
     (também com o botão REINICIAR).
  6. 4 robôs POLUENTES andam pelo mapa soltando FUMAÇA TÓXICA. Eles
     PERCEBEM o robô de longe (ficam avermelhados) e PERSEGUEM você.
     Se encostar de lado, você leva DANO: perde 1 VIDA, perde 5 segundos do
     cronômetro, é empurrado para longe (knockback), o robô pisca de
     vermelho e sai som de dano. Perdendo as 3 vidas -> PAINEL DE DERROTA.
     Pule em cima da cabeça deles: o inimigo explode em partículas de
     explosão e detritos/pedrinhas, gira com torque no ar e desaparece.

================================================================================
SOLUÇÃO DE PROBLEMAS
================================================================================

  * "Não aparece a cena / a tela do jogo está vazia no Play"
    -> Aperte Play novamente. O jogo se monta sozinho em qualquer cena.
    -> Se quiser garantir, abra Assets/Scenes/Level1.unity antes de dar Play.

  * "Os materiais estão rosa / roxos"
    -> Só aconteceria se o shader não fosse encontrado. O jogo procura o
       shader "Standard" (Built-in). Se o seu projeto estiver com a URP
       ativa, ele usa automaticamente "Universal Render Pipeline/Lit".
       Ou seja: funciona nos dois casos.

  * "Apareceu erro Missing types ... UniversalRenderPipelineGlobalSettings"
    -> Isso vem de uma URP desatualizada instalada no projeto. Este projeto
       NÃO usa pacotes de URP (veja Packages/manifest.json: só uGUI e os
       módulos internos do Unity). Se aparecer, feche o Unity, apague as
       pastas "Library" e "Temp" desta pasta e abra de novo.

  * "Apareceu erro em algum script"
    -> Feche o Unity, apague as pastas "Library", "Temp" e "obj" desta
       pasta, e abra de novo. O Unity recompila tudo do zero.

  * "Package com.unity.modules. ... cannot be found" / o projeto não abre
    -> Isso acontece quando o Unity Hub tenta abrir o projeto com uma
       versão MAIS NOVA do Unity e reescreve o arquivo
       ProjectSettings/ProjectVersion.txt. Este projeto é para a versão
       6000.0.8f1. Para corrigir:
         1) Feche o Unity.
         2) Abra o arquivo ProjectSettings/ProjectVersion.txt no Bloco de
            Notas e deixe exatamente estas 2 linhas:
              m_EditorVersion: 6000.0.8f1
              m_EditorVersionWithRevision: 6000.0.8f1 (fa7102f01711)
         3) Apague as pastas "Library", "Temp", "obj" e o arquivo
            Packages/packages-lock.json.
         4) Abra de novo pelo Hub escolhendo 6000.0.8f1.
    -> NUNCA aceite o botão "Upgrade" do Unity Hub: mantenha 6000.0.8f1.

================================================================================
COMO ZIPAR PARA ENTREGAR AO PROFESSOR
================================================================================

  1. FECHE O UNITY.
  2. Abra a pasta do projeto.
  3. Selecione SOMENTE estas 3 pastas:  [Assets] [Packages] [ProjectSettings]
     (não precisa mandar Library, Logs, obj, Temp, UserSettings)
  4. Botão direito -> Enviar para -> Pasta compactada (zip).
  5. Renomeie para:  Biosfera_GrupoX_1Bimestre.zip

================================================================================
ARQUIVOS DA ENTREGA
================================================================================

  SCRIPTS DE JOGO (13) - Assets/Scripts/Biosfera/
     Bootstrap.cs          Monta TODO o jogo automaticamente ao dar Play
                           (inclui o MUNDO ao redor: terreno, floresta,
                            montanhas, névoa e limites invisíveis)
     BiosferaRunner.cs     Objeto persistente: remonta o jogo ao REINICIAR
     Factory.cs            Cria materiais, primitivas, partículas e sons
     GameManager.cs        Tempo, vidas, contadores, pausa, vitória e derrota
     AudioManager.cs       Trilha sonora + efeitos (gerados por código)
     LevelSpawner.cs       Instancia os ~340 assets em 104 MODELOS diferentes
     PlayerController.cs   Movimento (AddForce), pulo, 5 estados, dano e morte
     SeedCollector.cs      Coleta de sementes (OnTriggerEnter)
     Seed.cs               Semente flutuante (Translate + Rotate)
     PollutionEnemy.cs     Inimigo: patrulha, persegue, fumaça, torque
     RestorationZone.cs    Zona: plantio automático + vegetação aleatória
     CameraFollow.cs       Câmera 3ª pessoa (órbita, suavizada, anti-clipping)
     UIManager.cs          HUD + painéis de Pausa/Vitória/Derrota + REINICIAR

  SCRIPT DE EDITOR (1) - Assets/Editor/
     BiosferaEditorSetup.cs  Cria a cena, o AnimatorController (5 estados),
                             o Build Settings e os shaders sempre incluídos

  DOCUMENTAÇÃO
     LEIA-ME_PRIMEIRO.md          (este arquivo - como rodar)
     GDD_Biosfera_1Bimestre.md    (documento de design do 1º bimestre)
     GUIA_Tecnico.md              (como o projeto foi montado por dentro)

================================================================================
FIM - Resumindo: abra no Unity 6000.0.8f1, espere compilar, aperte Play.
================================================================================
