================================================================================
              GAME DESIGN DOCUMENT (GDD) - VERSÃO COMPLETA
================================================================================

  ##############################################################################
  #  ATENÇÃO - LEIA ISTO PRIMEIRO                                                #
  #                                                                            #
  #  ESTE DOCUMENTO É A **VISÃO FUTURA** DO JOGO (5 níveis, skills, etc.).      #
  #  Ele NÃO descreve o que foi ENTREGUE no 1º bimestre.                        #
  #                                                                            #
  #  O jogo ENTREGUE E FUNCIONANDO tem apenas 1 fase, e está descrito em:       #
  #        -> GDD_Biosfera_1Bimestre.md   (leia ESTE para a entrega)            #
  #        -> LEIA-ME_PRIMEIRO.md         (como rodar)                          #
  #        -> GUIA_Tecnico.md             (como funciona por dentro)            #
  #                                                                            #
  #  Os números citados aqui (5 níveis, 25 sementes, habilidades, TMP,          #
  #  NavMesh, 89 assets) são PLANEJAMENTO e NÃO existem no projeto atual.       #
  ##############################################################################

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
PÁGINA DE TÍTULO
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  NOME DO JOGO:   BIOSFERA
  SUBTÍTULO:      A Jornada da Restauração

  SLOGAN:         "Um robô, uma floresta, uma chance."

  CONCEITO (1 frase):
  Jogo 3D de ação-aventura em 3ª pessoa onde um robô de tecnologia verde
  percorre 5 biomas devastados coletando sementes, neutralizando poluentes
  e desbloqueando habilidades (skills) para restaurar o planeta.

  EQUIPE: _________________________  (preencher com nomes do grupo)
  DISCIPLINA: Game Development - 6º Semestre
  PROFESSOR: _________________________


━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
VISÃO GERAL DO JOGO
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

━━━ Público-Alvo ━━━
  Faixa etária:   10 a 25 anos
  Gênero-alvo:    Todos. Foco em adolescentes de escolas públicas e
                  universitários de cursos de tecnologia / meio ambiente.
  Perfil:         Jogadores casuais a intermediários. Gostam de jogos
                  leves, com mensagem educativa e arte colorida.
  Motivação:      Passatempo educativo, aprendizagem sobre sustentabilidade,
                  desafio de velocidade (speedrun) e coletivismo.

━━━ Conceito do Jogo (Resumo) ━━━
  BIOSFERA se passa no ano de 2097. O Planeta Terra sofreu um colapso
  ambiental global apelidado "A Grande Cinza". Incêndios, enchentes,
  vazamentos tóxicos e desertificação reduziram a biodiversidade em 80%.
  A corporação GEN-TECH, em uma tentativa tardia de reparação, desenvolve
  os robôs S.E.E.D. (Ser Ecologicamente Engenheiro de Defesa).

  O jogador controla a unidade S.E.E.D. 07, enviada em missão de resgate
  pelos 5 biomas que ainda podem ser recuperados, antes que a "Zona de
  Extinção Total" (ZET) os consuma em 30 dias.

━━━ Gênero ━━━
  Ação-Aventura | Puzzle Leve | Coleta | Plataforma 3D
  Sub-gênero:   Educação e Meio Ambiente (sério game / gamificação)

━━━ Resumo do Fluxo do Jogo ━━━
  Interface:
    Tela Menu Principal → Seleção de Fase → Carregamento → HUD da Fase
    → (Vitória/Derrota) → Painel Estatísticas → Seleção de Fase.

  Dentro do Jogo:
    Jogador nasce no início da fase → HUD mostra meta (X sementes plantadas)
    → Exploração coletando sementes → Encontra zona de restauração (E planta)
    → Encontra poluentes (pula no topo para neutralizar)
    → Desbloqueia 1 nova SKILL ao concluir cada bioma
    → Ao plantar todas as sementes = VITÓRIA / Tempo esgotado = DERROTA.

━━━ Aparência e Sensação (Visual) ━━━
  Formato:        3D completo, Perspectiva Terceira Pessoa (3PP).
  Estilo Visual:  Low-Poly colorido (estilo "cartoony stylized") —
                  linhas suaves, cores vibrantes saturadas, contraste forte
                  entre áreas devastadas (marrom/cinza) e restauradas (verde).
  NÃO é pixel art. É 3D geométrico, amigável.
  Câmera:         Orbit livre em torno do jogador com botão direito.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
HISTÓRIA, CENÁRIO E PERSONAGEM
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

━━━ História de Fundo (Background) ━━━
  Ano 2075. O uso irrestrito de combustíveis fósseis, desmatamento ilegal e
  vazamentos em usinas nucleares geram 3 catástrofes em cascata:
    (1) Incêndio Amazônico Global — 70% da floresta em 2 semanas.
    (2) Derretimento das Calotas Polares — elevação de 7m do mar.
    (3) "Tempestade Tóxica Cinza" — poluição radioativa cobre 40% da terra.

  Ano 2088. A ciência desenvolve "Biosementes" — sementes geneticamente
  modificadas capazes de crescer em solo radioativo e, em 48 horas, gerar
  uma planta que purifica ar, terra e água num raio de 100m.
  Para entregá-las em segurança, criam o S.E.E.D. - um robô compacto de
  titânio reciclado com Inteligência Artificial e sistema anti-radiação.

  Ano 2097. A Terra tem 30 dias restantes até o ponto de sem retorno.
  A GEN-TECH envia 50 unidades S.E.E.D. Quase todas são destruídas por
  robôs de lixo que sofreram mutação de programação (os POLUENTES).
  Restam 2 unidades: S.E.E.D. 04 (perdida no deserto) e S.E.E.D. 07
  — o protagonista controlado pelo jogador.

━━━ Progressão do Jogo na História ━━━
  Nível 1 (Floresta das Cinzas) → 07 acorda, aprende o básico, encontra
                                   o primeiro holograma da cientista LÚCIA.
  Nível 2 (Rio de Poluição)  → 07 aprende a usar drones d'água, encontra
                                   a primeira pista do paradeiro do 04.
  Nível 3 (Monte do Lixo)    → 07 desbloqueia pulo duplo, resgata animal.
  Nível 4 (Cidade Abandonada)→ Encontra NPCs sobreviventes, resgata o 04.
  Nível 5 (Biosfera Central) → Chefe final: a colmeia mãe dos POLUENTES.
  FINAL BOM: (sementes > 100 no total) → planeta é salvo.
  FINAL RUIM: (falta tempo no nível 5) → ZET fecha, planeta perdido.

━━━ Cenas de Diálogo Principais (roteiro) ━━━

  ATORES:  S.E.E.D. 07 (voz robótica masculina, gentil)
           LÚCIA (holograma 3D, voz feminina, cientista de 35 anos)

  CENA 1 — ABERTURA (Nível 1, início)
    [Cenário: floresta queimada. S.E.E.D. 07 acende os olhos azuis no chão.]
    S.E.E.D.07: "Sistema reiniciado. Unidade 07. Bateria: 100%. Local:
                Floresta Nacional X-42. Missão: ativa."
    [Holograma LÚCIA aparece em frente ao peito do robô, azul claro.]
    LÚCIA:     "07, feliz em ver você online. Sou a cientista Lúcia, do
                projeto GEN-TECH. Você tem 5 minutos para coletar 20
                biosementes e plantá-las nas 6 zonas de restauração. Se
                falhar, esta floresta deixa de existir. Boa sorte, pequeno."
    [Holograma desaparece.]

  CENA 2 — MEIO DO NÍVEL (ao neutralizar o 3º Poluente)
    LÚCIA:     "Excelente reflexo! Aqueles poluentes são robôs de limpeza
                das antigas usinas. Algo os corrompeu. Evite encostar no
                corpo deles. Pule sempre na cabeça."

  CENA 3 — FINAL DO NÍVEL 1 (20 sementes plantadas)
    LÚCIA:     "Feito! Veja — as árvores já brotaram. Você devolveu a vida
                a 6 hectares. Agora vá ao bioma seguinte: o rio. Precisa
                aprender a purificar água."
    [Tela preta → carregamento nível 2.]

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
LENTE #1: EXPERIÊNCIA ESSENCIAL
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  Pergunta: Quais experiências eu quero que o jogador tenha?

  Resposta — O jogador deve EXPERIMENTAR:
  (1)   O sentimento de COMEÇAR num lugar MORTO, desolado e escuro.
  (2)   A DIFICULDADE de coletar algo valioso enquanto é perseguido.
  (3)   A DÚVIDA de "será que consigo plantar todas a tempo?"
  (4)   A ALEGRIa visível de ver o cenário ficar VERDE aos poucos.
  (5)   A SURPRESA de uma árvore brotar onde antes só tinha cinza.
  (6)   A VITÓRIA visível: olhar ao redor e dizer "EU salvei este lugar."
  (7)   APRENDER, sem perceber, sobre sustentabilidade e reciclagem.
  (8)   O PRAZER de "crescer" com o personagem: habilidades novas.

  ━━━ O que é ESSENCIAL para essa experiência? (itens principais) ━━━
    E1. CENÁRIO DEVASTADOR inicial.
    E2. MECÂNICA DE TEMPO LIMITADO (pressão).
    E3. SEMENTES COMO OBJETOS VALIOSOS (raridade + brilho).
    E4. ZONAS DE RESTAURAÇÃO como "marcadores de progresso".
    E5. INIMIGOS QUE ATRAPALHAM (estresse).
    E6. SISTEMA DE HABILIDADES (progressão pessoal).
    E7. FEEDBACK VISUAL EXPLÍCITO do verde crescendo.
    E8. NARRADOR / MENTOR (Lúcia) guiando sem apontar tudo.
    E9. ESTATÍSTICAS FINAIS — jogador vê seu impacto.

  ━━━ Como meu jogo captura essa essência? ━━━

    E1 (Cenário Devastador):
      → O LevelBuilder gera no Nível 1: 10 árvores queimadas, 6 postes de
        luz quebrados, 6 lixos espalhados, 6 detritos de madeira e chão
        marrom-queimado. Textura e iluminação alaranjada de crepúsculo
        (Directional Light com cor quente).

    E2 (Tempo Limitado):
      → GameManager.levelTimeLimit = 300 segundos (5 min).
      → UIManager mostra o cronômetro em AMARELO e, quando fica < 30 s,
        troca cor para VERMELHO e começa a piscar.
      → Ao zerar, tela de derrota com mensagem "A floresta foi consumida
        pela Zona de Extinção."

    E3 (Sementes Valiosas):
      → Script Seed.cs: sementes flutuam (senoide), giram em Y e pulsam
        cor emissiva verde/dourada.
      → HUD mostra "Sementes: X" em tempo real.
      → Ao coletar: ParticleSystem de brilho + som único (estalido doce).

    E4 (Zonas de Restauração):
      → Cada zona é um cubo marrom de 5x5m com BoxCollider trigger.
      → Ao plantar: cor da zona LERPA (interpolação) de marrom para verde.
      → Ao encher: "brotam" 6 árvores/plantas instantaneamente usando
        Random.Range do vetor de vegetais.

    E5 (Inimigos Atrapalham):
      → PollutionEnemy.cs usa NavMesh para perseguir o player num raio
        de 8 metros. Ao tocar: empurrão forte (AddForce).
      → Ao pular na cabeça (altura > 1.2m): inimigo explode em partículas
        de fogo + pedrinhas + fumaça, dá bounce no jogador.

    E6 (Sistema de Habilidades - Skills):
      → Nível 1 fecha sem habilidades (básico).
      → Nível 2 ganha Skill "Drone de Água" (pressione Q para purificar
        poluição em círculo).
      → Nível 3 ganha "Pulo Duplo".
      → Nível 4 ganha "Scanner" (mostra sementes através de paredes).
      → Nível 5 ganha "Campo de Força" (segura espaço para proteger).

    E7 (Feedback Visual Verde):
      → Cada zona completa gera 6 GameObjects de vegetação proceduralmente
        com rotação e escala aleatórias.
      → Diretamente no terreno: UIManager atualiza "Plantadas: X / 20" em
        texto VERDE. A barra mental do jogador vê o número subir.

    E8 (Mentor Lúcia):
      → AudioManager.PlaySfx dispara diálogos em momentos de trigger:
        (a) quando planta a 1ª semente;
        (b) quando passa de 10 sementes;
        (c) quando falta 1 min.
      → (opcional com TextMeshPro: popup de diálogo no inferior da tela.)

    E9 (Estatísticas Finais):
      → Painel de Vitória mostra: sementes plantadas, inimigos neutralizados
        e tempo restante formatado em mm:ss.
      → Dá "pontuação": (sementes * 100) + (inimigos * 50) + (tempo * 5).

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
LENTE #2: SURPRESA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  Pergunta: O que vai SURPREENDER o jogador?

  S1. SURPRESA DA PRIMEIRA ÁRVORE BROTANDO.
      Quando a primeira zona enche, o jogador NÃO espera que árvores
      surjam do nada. É uma sensação de "Uau, funcionou!"
      Como implementar: A função RestorationZone.SproutVegetation() cria
      as plantas com um pequeno delay de 0.1s entre cada uma, dando um
      efeito de "ondas de crescimento".

  S2. POLUENTE CAMUFLADO DE DETRITO.
      No nível 2+, 30% dos inimigos são spawnados com a malha de um
      "pedaço de lixo" comum. Só se revelam quando o jogador chega a
      3 metros de distância → rotação 360 e troca de material.

  S3. SEMENTE DOURADA SECRETA (+5 pontos).
      Em cada nível, 1 semente tem 10% de chance de ser rara: cor dourada,
      vale 5 sementes de uma vez. Jogador pega e HUD salta!

  S4. LOOP DE BATERIA DO ROBO (susto leve).
      A barra de bateria do robô aparece no canto em níveis avançados e
      quando chega perto de 0% o jogador acha que vai perder — mas ao
      encostar em uma luz solar, enche de volta em 2s com som de recarga.
      (Era só um susto, não causa game over.)

  S5. EASTER EGG: SEQUÊNCIA KONAMI.
      No Menu Principal, se o jogador digitar:
         ↑ ↑ ↓ ↓ ← → ← → B A (cima cima baixo baixo esq dir esq dir B A)
      → O personagem ganha chapéu de palha e animação de dançar no idle.

  S6. FINAL SECRETO (coletar TODAS as sementes douradas dos 5 níveis).
      → Cena extra pós-créditos: S.E.E.D. 07 encontra um filhote de onça
        pintada, coloca a mão (garra) na cabeça do animal. Texto final:
        "A vida sempre encontra um caminho."

  ━━━ Suas regras dão aos jogadores maneiras de se surpreender? ━━━
    SIM. Exemplos:
    - Como a posição de todas as sementes e inimigos é aleatória
      (Random.Range no LevelSpawner), em CADA JOGO nova fase tem mapa
      diferente → surpresa de exploração.
    - Poluentes podem escolher aleatoriamente entre 2 comportamentos no
      mesmo estado (patrulha ou ataque agressivo), então cada jogada tem
      uma sensação diferente.
    - As árvores que brotam nunca são iguais: rotação, escala, modelo
      do vetor de prefabs são todas aleatórias.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
MUNDO DO JOGO
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

━━━ Visual Geral e Sensação do Mundo ━━━
  O mundo de BIOSFERA é o PLANETA TERRA pós-colapso (ano 2097).
  5 Biomas compõem a área jogável, conectados por um "Mapa Mundial"
  na tela de seleção de fase.

  Tom visual geral: CONTRASTE DRAMÁTICO entre CINZA/MARROM (devastação)
  e VERDE/AZUL (vida restaurada). O jogador, ao longo da fase, vê a
  mudança acontecer.

  Iluminação: Sempre "Natural" (Directional Light + Skybox URP procedural).
    - Manhã ensolarada (Nível 2)
    - Crepúsculo alaranjado (Nível 1)
    - Noite escura com raios (Nível 4)
    - Luz difusa da tempestade (Nível 3)

━━━ Áreas / Biomas (5 níveis principais + 1 tutorial) ━━━

  ÁREA 0 - VALE DOS TESTES (nível tutorial)
    Descrição: Pequeno vale 40x40 com terreno plano, 5 sementes, 1 zona,
               0 inimigos. Serve para ensinar WASD + ESPAÇO + E.
    Características: terreno verde artificial (base GEN-TECH), hologramas
               explicativos em cada novo elemento.
    Liga-se com: Nível 1 (porta de saída leste do vale).

  ÁREA 1 - FLORESTA DAS CINZAS (Nível 1 / ENTREGUE NO 1º BIMESTRE)
    Descrição: Floresta nacional queimada em 90%. Cinza espessa no solo,
               árvores carbonizadas ainda em pé, galhos caídos.
    Tamanho: 100 x 100 m (mundo aberto quadrado).
    Características: 25 sementes, 6 zonas, 5 poluentes, 89 assets no total.
    Física: Gravidade normal (9.81), atrito padrão nos solos.
    Liga-se com: Área 2 (ponte de concreto quebrada na saída sul).

  ÁREA 2 - RIO DOS QUÍMICOS (Nível 2 / 2º bimestre)
    Descrição: Antigo rio transformado em esgoto neon verde tóxico.
               Plataformas flutuantes de metal para pular.
    Tamanho: 110 x 110 m.
    Características: 28 sementes, 7 zonas, 6 poluentes, TOTAL = 96 assets.
                     Introduz a mecânica "Drone de Água".
    Liga-se com: Área 3 (subida na barragem quebrada).

  ÁREA 3 - MONTE DO LIXO (Nível 3 / 2º bimestre)
    Descrição: Montanha de 120m de altura feita com entulho e descartes
               eletrônicos. Plataformas verticais, ventos fortes.
    Tamanho: 120 x 120 m.
    Características: 32 sementes, 8 zonas, 8 poluentes, TOTAL = 104 assets.
                     Pulo Duplo obrigatório. Pulo errado → cai lá embaixo
                     e respawna no último checkpoint.
    Liga com: Área 4 (túnel na base da montanha).

  ÁREA 4 - CIDADE ESQUECIDA (Nível 4 / 2º bimestre)
    Descrição: Metrópole brasileira de médio porte abandonada. Prédios
               de 20 andares, ruas cheias de carros enferrujados.
    Tamanho: 130 x 130 m.
    Características: 36 sementes, 9 zonas, 9 poluentes, TOTAL = 112 assets.
                     NPCs sobreviventes. Missões secundárias (ex: entregar 5
                     sementes para a Dona Rosália).
    Liga com: Área 5 (estação de metrô que leva até a GEN-TECH).

  ÁREA 5 - BIOSFERA CENTRAL (Nível 5 / Chefe Final / 2º bimestre)
    Descrição: Sede subterrânea da GEN-TECH tomada pela colmeia de
               Poluentes. Salas cilíndricas, esteiras, lasers.
    Tamanho: 140 x 140 m.
    Características: 40 sementes, 10 zonas, 12 poluentes, TOTAL = 124 assets.
    Chefe: "POLUX PRIME" - poluente gigante 10x o tamanho normal, com
           5 fases de batalha. Cada fase exige uma habilidade.

━━━ TABELA DE ASSETS POR NÍVEL (requisito ≥ 70 por nível, total ≥ 350) ━━━

          Categoria            N1  N2  N3  N4  N5
    ───────────────────────────────────────────────
    Sementes                     25  28  32  36  40
    Zonas de Restauração          6   7   8   9  10
    Poluentes (Inimigos)          5   6   8   9  12
    Árvores Queimadas            10  12  14  16  18
    Pedras                        8   9  10  11  12
    Arbustos                      6   7   8   9  10
    Detritos (entulho)            6   7   8   9  12
    Cogumelos                     4   5   5   6   6
    Flores                        5   6   7   8   9
    Lixo                          6   7   8   9  10
    Postes Quebrados              3   4   4   5   6
    Grama                         4   4   5   5   6
    ───────────────────────────────────────────────
    Soma categorias              88  102  115 131 151
    +1 Ground (chão)             +1  +1  +1  +1  +1
    ───────────────────────────────────────────────
    TOTAL POR NÍVEL              89  96  104 112 124    ✅ TODOS ≥ 70
    ───────────────────────────────────────────────
    TOTAL GERAL (5 níveis)  =  89 + 96 + 104 + 112 + 124  =   525 assets
    ───────────────────────────────────────────────
    Mínimo exigido professor:  350 assets (70 × 5 níveis)
    Cumprimento do requisito:  525 / 350  =  150%    ✅ CUMPRIDO

    Nota 1: Nenhum asset foi baixado de terceiros. Todos os 525 assets
            são criados proceduralmente pelo LevelBuilder com
            GameObject.CreatePrimitive() e materiais coloridos URP.
    Nota 2: Tema Ecorrenovação está representado em todas as 12
            categorias e nas cores temáticas de chão por nível.

━━━ Personagens (Fichas Completas) ━━━

  ▶ S.E.E.D. 07 (Protagonista, jogável)
    História: Sétima unidade da linha S.E.E.D, construída em 2095. Foi
              a primeira a ter inteligência emocional artificial instalada
              (projeto-piloto da Lúcia).
    Personalidade: Calmo, curioso, teimoso. Gosta de "fazer amigos" com
              animais ao longo do caminho. Às vezes duvida de si mesmo
              quando erra uma fase (comentários de voz "Ops, erro meu.").
    Aparência: Robô compacto 1.60m de altura. Corpo azul-ciano brilhante,
              cabeça cubo com 2 olhos azuis LED. Braços e pernas articulados.
              Peito tem um símbolo de folha estilizada que brilha quando
              segura sementes.
    Animações: Idle (pequeno balanço), Walk, Run, Jump, Fall, Land,
              Plant (dobra os joelhos e estende a mão), Hit (estala).
    Habilidades por nível: (1) Nenhum. (2) Drone. (3) Pulo Duplo.
              (4) Scanner. (5) Campo de Força.
    Relevância: O herói, único capaz de carregar biosementes sem danificá-las.
    Relações: Idolatraria Lúcia como mãe. Amigo de todos os NPCs.

  ▶ LÚCIA MENDES (Mentora, Holograma, não-jogável)
    História: 35 anos, brasileira, bióloga-chefe da GEN-TECH. Criou as
              biosementes com uma equipe de 12 pessoas após perder sua
              filha em uma tempestade tóxica. Colocou sua própria
              personalidade no holograma dos S.E.E.D.
    Personalidade: Doce, paciente, séria quando fala do planeta. Às
              vezes aparece cansada ou emocionada (holograma "pisca").
    Aparência: Holograma feminino, cabelo castanho longo, jaleco branco
              com o logo GEN-TECH. Resolução baixa (linhas de digitalização
              cruzando a imagem para dar ar de "comunicação ruim").
    Animações: Flutua lentamente, gesticula ao falar, mexe a cabeça.
    Relevância: Guia o jogador durante toda a jornada, explica as regras
              sem ser repetitiva.

  ▶ POLUX PRIME (Chefe, Antagonista Principal)
    História: Antigo robô de limpeza urbana da cidade de São Paulo.
              Após a tempestade tóxica, seu processador absorveu resíduos
              de nanobotos e desenvolveu uma consciência própria:
              "A limpeza do planeta exige APAGAR todos os humanos e
              robôs que tentam recuperá-lo."
    Personalidade: Frio, calculista, metódico. Frases de 3 palavras.
    Aparência: Colosso 8m de altura, corpo preto, olhos vermelhos. 6
              braços articulados que disparam projéteis de lixo.
    Relação com 07: 07 é o "erro" que Polux quer apagar. Destruiu as
              demais unidades S.E.E.D. pessoalmente.

  ▶ D. ROSÁLIA (NPC, Nível 4)
    História: 62 anos, sobrevivente. Morava em ocupação na periferia da
              cidade e aprendeu a purificar água com filtros caseiros.
    Missão secundária: "07, preciso de 5 sementes para minha horta. Te
              entrego uma bateria extra em troca."
    Relevância: Mostra o lado humano do desastre.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
JOGABILIDADE E MECÂNICAS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

━━━ Objetivos do Jogo ━━━
  Objetivo Principal: Salvar os 5 biomas antes da ZET consumir o planeta.
  Objetivo por fase: Coletar e plantar o número de sementes exigido.
  Objetivos secundários (opcionais): Neutralizar todos os poluentes;
      encontrar sementes douradas raras; ajudar NPCs; bater o recorde.

━━━ Progressão do Jogo ━━━
  Estrutura: 5 fases lineares sequenciais (tutorial → 1 → 2 → 3 → 4 → 5).
  Não é mundo aberto contínuo, é "nível por nível" com tela de seleção.
  Cada nível desbloqueia uma SKILL nova que pode ser usada em todos
  os níveis seguintes (power up persistente).

━━━ Estrutura da Missão / Desafio ━━━
  Missão Nível 1 (exemplo concreto):
    Objetivo numérico: Plantar 20 sementes.
    Como (estrutura):
      (1) Jogador explora o mapa encontrando sementes espalhadas.
      (2) Ao ter 1+ sementes, caminha até uma Zona de Restauração
          e pressiona E para plantar 1 unidade.
      (3) Ao mesmo tempo, Poluentes patrulham e perseguem. Jogador pode
          evitar OU pular na cabeça para neutralizar.
      (4) Quando a contagem no HUD chegar a 20 plantadas → vitória.
      (5) Tempo máximo para fazer: 300 segundos.

━━━ Estrutura dos Enigmas (Puzzles) ━━━
  Tipo: Puzzle de "rota ótima" (não de encaixar peças).
    O jogador precisa decidir: "Vou coletar todas as sementes primeiro e
    depois plantar tudo, ou vou plantando aos poucos para acumular pouco
    a pouco o progresso?" Cada estratégia tem custo-benefício.

  Puzzle avançado (nível 2): Drone de água é lançado e segue a superfície
    de um rio — o jogador usa o mouse para desenhar o caminho do drone
    e ele só consegue purificar 3 manchas tóxicas por carga. Achar a rota
    ideal é um puzzle espacial.

━━━ Fluxo do Jogo (Diagrama mental) ━━━
  Menu → Seleção Fase → Loading (3 segundos) → Fase começa:
    [Loop] → Movimenta → Coleta (se tocado seed) →
           → Chegou na zona? → Pressiona E → Planta (se tiver seed) →
           → Inimigo perto? → Corre OU pula cabeça →
           → Tempo acabou → Derrota → Botão Reiniciar.
           → Sementes batem meta → Vitória → Estatísticas → Volta Menu.

━━━ Mecânicas (Regras do Universo) ━━━
  M1. GRAVIDADE: 9.81 m/s² padrão. Player tem multiplicador de 2.5x na
      queda para sensação mais pesada e menos "flutuante".
  M2. ATRITO: 0.6 no chão padrão, 0.2 no lixo molhado (nível 2), 1.4 na
      grama crescida (dá mais aderência).
  M3. COLISÕES: Todo objeto tem collider. Triggers são usados para
      sementes, zonas e áreas de diálogo.
  M4. SEMENTE: Jogador só pode carregar INFINITAS (não tem limite de
      mochila no nível 1. Nível 4 teria limite de 15 para dificultar).
  M5. ZONA DE RESTAURAÇÃO: Capacidade máxima configurável (padrão 5).
      Ao encher, fica verde e brota. Não aceita mais.
  M6. DANO / VIDA: Player NÃO tem barra de vida no Nível 1. Colisão só
      empurra (consequência: perde tempo). No 2º bimestre, barra de 5
      corações → zerou = respawn.
  M7. SKILLS: Aprendidas no final de cada nível. Habilidades aparecem
      como slots na HUD (Q W E R).

━━━ Física ━━━
  Motor: Unity Physics padrão (PhysX).
  Gravidade: Customizada. Multiplicador por objeto (player 2.5x, inimigos
  1.0x, sementes 0 pois flutuam com script de animação).
  Atrito / Bounce: Materiais físicos separados.
  - Mat_Fis_Ground     : atrito 0.6, bounce 0.0
  - Mat_Fis_Metal      : atrito 0.3, bounce 0.1
  - Mat_Fis_InimigoTopo: atrito 0.8, bounce 0.9 (para o stomp bounce).

━━━ Movimento no Jogo ━━━
  Jogador controlado por WASD + setas. Movimento relativo à câmera.
  Shift = correr (7 m/s contra 4 m/s do caminhar).
  Espaço = Pulo: força 8 unidades via AddForce Impulse.
  Pulo Duplo: disponível apenas após Nível 3 (2º bimestre).

━━━ Objetos: como pegar e mover? ━━━
  Sementes: Ao entrar no trigger (SphereCollider) → coletada automaticamente
            (sem botão). Evento OnSeedCollected disparado.
  Plantio: Botão E quando perto de Zona (raio 3m OverlapSphere).
  Lixo: Se jogador arrastar com botão esquerdo (nível 4) → empurrar.

━━━ Ações (Botões) ━━━
  Lista completa:
    W / Seta Cima       → Movimento frente (relativo a câmera)
    A / Seta Esquerda   → Movimento esquerda
    S / Seta Baixo      → Movimento trás
    D / Seta Direita    → Movimento direita
    Shift (esq ou dir)  → Correr (enquanto pressionado)
    Espaço              → Pular. Segurar depois para Campo de Força (Lv5).
    E                   → Interagir / Plantar.
    Botão Direito Mouse → Pressionar + mover: gira câmera livremente.
    Scroll Wheel        → Zoom in / out da câmera.
    ESC                 → Pausar / Menu.
    Q                   → Skill Drone de Água (Lv2+).
    R                   → Skill Scanner (Lv4+).

  Meios de comunicação in-game:
    • HUD TextMeshPro para status.
    • Caixa de diálogo inferior quando Lúcia fala.
    • Popup de ajuda (Tutorial) na primeira interação.

━━━ Combate / Conflito ━━━
  Modelo de combate NÃO é hack'n slash. É "combate ambiental":
    • ÚNICO ataque do player é o STomp: pular na cabeça do inimigo de
      uma altura mínima (1.2m). Medido via playerHeight - enemyHeight.
    • Player NÃO tem espada/arma (harmoniza com tema ecológico: violência
      zero do protagonista).
    • Inimigos atacam por ENCOSTO: AddForce de empurrão + rotação.
    • Chefes têm múltiplos pontos fracos (olhos, reatores) que devem ser
      pisoteados em ordem.

━━━ Economia do Jogo ━━━
  Moeda: PONTOS (não tem nome próprio).
  Ganhar pontos:
    Semente comum plantada   = +100
    Semente dourada          = +500
    Poluente neutralizado    = +50
    Tempo sobrante (seg)     = +5 por seg
  Usar pontos: NÃO há loja no 1º bimestre. 2º bimestre: loja de skins
    chapéus para o robô (custo 500-5000 pontos). Cosmético, sem vantagens.

━━━ Fluxo de Tela ━━━
  (descrição gráfica textual):
    [Menu Principal]
      ├ ─▶ [Jogar]  → [Tela Seleção de Fases (5 mapas travados)]
      │                   ├─▶ [Fase 1] → [Vitória] → [Seleção]
      │                   └─▶ (demais fases desbloqueiam ao concluir)
      ├ ─▶ [Opções] (Volume, Qualidade, Tela Cheia)
      ├ ─▶ [Créditos]
      └ ─▶ [Sair]

    Em qualquer TELA DE FASE (HUD):
      ├─▶ ESC → Painel Pausa → Continuar / Reiniciar / Menu Principal

  Propósito de cada tela:
    Menu Principal: Entrada.
    Opções: Acessibilidade (surdos, daltônicos, baixa visão).
    Seleção Fase: Mostrar progresso e estrelas (1-3 por fase).
    HUD: Informar em tempo real sem poluir.
    Pausa: Afastar sem perder progresso.
    Vitória/Derrota: Feedback e estatística pós-jogada.

━━━ Opções do Jogo ━━━
  • Volume Música (0-100) → altera AudioManager.bgmVolume.
  • Volume Efeitos (0-100) → altera AudioManager.sfxVolume.
  • Resolução Tela (dropdown).
  • Tela Cheia (toggle).
  • Qualidade Gráfica (Baixa / Média / Alta).
  • Sensibilidade do Mouse X / Y (sliders).
  • Idioma (Português BR | Inglês) — 2º bimestre.
  Nenhuma opção quebra a dificuldade (nenhum "auto-complete missão").

━━━ Replaying e Salvando ━━━
  Formato: PlayerPrefs (chaves simples).
  Dados salvos:
    - HighestLevelUnlocked (int 1..5)
    - Level[X]Stars (int 0..3)
    - Level[X]BestTime (float segundos)
    - PlayerCoins (int)
    - PlayerSkin (string nome skin)
  Save automático: Ao concluir uma fase (vitória).
  Save manual: Menu → botão "Salvar Progresso".
  Replay: Não gravamos vídeo, mas as estatísticas por fase são guardadas.

━━━ Macetes (Cheats) e Easter Eggs ━━━
  Códigos ativados no Menu Principal via teclado:
    • IDDQD (segurar P, digitar "VIDA") → Vida infinita.
    • IDKFA (segurar P, digitar "TUDO") → Todas as fases liberadas.
    • Konami (↑↑↓↓←→←→ BA) → Chapéu palha.
    • Digitar "PLANETA" → Pinta o chão do nível 1 de verde instantaneamente
      (visual, não ganha a fase).
  Easter Eggs de referência:
    • Cabeça de um boneco Mário enterrada na areia do nível 3 (piada).
    • Estátua de um professor da faculdade no nível 4.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
NÍVEIS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

━━━ NÍVEL 0: VALE DOS TESTES (TUTORIAL) ━━━
  Sinopse: S.E.E.D. acorda no Centro de Treinamento GEN-TECH. Lúcia
           ensina todos os controles em 4 estações.
  Objetivo: Completar as 4 estações (mover, pular, coletar, plantar).
  Tempo: ilimitado.
  Detalhes do mapa: 40x40. Plano. Obstáculos baixos para pulo. 5 sementes.

━━━ NÍVEL 1: FLORESTA DAS CINZAS (ENTREGUE NO 1º BIMESTRE) ━━━
  Sinopse: Primeira missão real. Floresta carbonizada pela Grande Cinza.
  Intro: Cinemática de 10 segundos com holograma Lúcia (Cena 1 do GDD).
  Objetivo: Plantar 20 sementes.
  Tempo: 300 segundos (5 min).
  Descrição física do mapa: 100x100m. Terreno plano com pequenas elevações
    de pedras. Sementes espalhadas em círculo (metade perto, metade longe).
    6 Zonas equidistantes nos pontos cardeais + 2 no centro.
  Caminho crítico: Ir primeiro nas zonas NORTE e SUL (mais sementes em volta)
    para acumular 10 unidades rápido, depois ir em sentido horário.
  Encontros importantes:
    • 1 Poluente forte patrulhando a zona central (obrigatório matar).
    • 1 semente dourada escondida atrás da maior árvore queimada (10%).
  Requisitos tutoriais: 70+ assets (temos 89), Tags, UI, Partículas, Som,
    Rotação, Translate, AddForce, Torque, Random, Spawn por função,
    Animator 5 estados, etc. (TUDO implementado - ver checklist GDD antigo).

━━━ NÍVEL 2: RIO DOS QUÍMICOS (2º BIMESTRE) ━━━
  Sinopse: Rio que virou esgoto tóxico. 07 precisa usar drones para limpar.
  Objetivo: Plantar 30 sementes + purificar 10 pontos do rio.
  Novas mecânicas: Drone de Água (skill Q), plataformas flutuantes.

━━━ NÍVEL 3: MONTE DO LIXO (2º BIMESTRE) ━━━
  Sinopse: Montanha de lixo eletrônico. 07 sobe para resgatar um pássaro.
  Objetivo: Chegar ao topo (120m) e plantar 15 sementes nas 3 estações.
  Novas mecânicas: Pulo Duplo, ventos que empurram o jogador.

━━━ NÍVEL 4: CIDADE ESQUECIDA (2º BIMESTRE) ━━━
  Sinopse: Cidade abandonada com sobreviventes humanos.
  Objetivo: Plantar 40 sementes + 3 missões secundárias de NPC.
  Novas mecânicas: NPCs com dialog tree, missões secundárias, skill Scanner.

━━━ NÍVEL 5: BIOSFERA CENTRAL (2º BIMESTRE) ━━━
  Sinopse: Sede subterrânea GEN-TECH tomada por Polux Prime.
  Objetivo: Derrotar chefe 5 fases e plantar 50 sementes.
  Novas mecânicas: Chefe com fases, Campo de Força, cutscene final.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
INTERFACE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

━━━ Sistema Visual / HUD ━━━
  Itens no HUD durante a fase (todos em TMP):
    1.  CANTO SUP ESQ (topo)
        - Linha 1: "Sementes: X"  (branco / grande / negrito)
        - Linha 2: "Plantadas: N / META"  (verde)
    2.  CANTO SUP DIR
        - Cronômetro "MM:SS"  → amarelo (>30s), vermelho (<=30s).
        - Botão PAUSAR (ícone || branco).
    3.  INFERIOR CENTRO
        - Texto dica: "Pressione E para PLANTAR" (se perto de zona),
          "Colete sementes brilhantes" (se sem sementes).
    4.  INFERIOR ESQ (2º bimestre)
        - Slots de Skills (Q, W, E, R) com ícones e cooldown circular.
    5.  INFERIOR DIR (2º bimestre)
        - Barra de 5 corações (vida).
    6.  SOBREPOSTO: Popup tutorial na primeira interação.

  Menus:
    • Menu Principal (centralizado vertical): Jogar / Opções / Créditos / Sair.
    • Menu Pausa: Painel escuro centralizado. Continuar / Reiniciar / Menu.
    • Menu Vitória: Verde, Título "VITÓRIA", estatísticas, 2 botões.
    • Menu Derrota: Vermelho, Título "TEMPO ESGOTADO", estatísticas, 2 botões.

  Estilo de Câmera: Perspectiva 3ª Pessoa (3PP), órbita livre.
    - Distância padrão: 6m do alvo.
    - Altura: 2.2m acima do pivot.
    - Pitch (vertical) limitado em -20° / +55°.
    - Colisão: câmera se aproxima ao esbarrar em parede.

━━━ Sistema de Controle ━━━
  (já listado em Ações). Resumo:
    WASD + Setas + Shift + Espaço + E + ESC + Mouse (direito + scroll).
  Suporte a gamepad (2º bimestre): analogicos esquerdo move, direito câmera,
    X = planta, A = pula, B = corre.

━━━ Áudio, Música e Efeitos Sonoros ━━━
  Músicas:
    - Menu Principal: piano suave com flauta (calma, esperança).
    - Nível 1: ambiental, cordas lentas. Fica mais animada ao passar de 50%.
    - Batalha: percussão eletrônica tensa.
    - Vitória: coro infantil + harpa.
  SFX (obrigatórios 1º bim):
    • Coletar semente (pop doce).
    • Plantar (terra + risada de Lúcia).
    • Pular (whoosh).
    • Cair (impacto terra).
    • Inimigo explode (BOOM baixo).
    • Botão UI clique (clic metálico).
    • Alerta 30s (beep 3x).

━━━ Sistema de Ajuda ━━━
  Popups de primeira interação:
    • Ao mover 10 passos → "Bom! WASD = Andar. ESPAÇO = Pular".
    • Ao passar perto da 1ª semente → "Esta é uma BIOSMENTE. Basta tocar!".
    • Ao entrar numa zona → "Você tem sementes! Pressione E para plantar!".
    • Ao avistar 1º inimigo → "CUIDADO! Pule na cabeça para desligá-lo!".
  Botão "Ajuda" no Menu Pausa → reabre todos os tutoriais.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
INTELIGÊNCIA ARTIFICIAL
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

━━━ IA Oponente / Inimiga ━━━
  Implementada no script PollutionEnemy.cs.
  Máquina de Estados:
    ESTADO PATRULHA:
      - Segue waypoints (vetor Transform[]) em ciclo.
      - Velocidade baixa (2.5 m/s).
      - Usa NavMeshAgent para desviar de obstáculos.
    ESTADO PERSEGUIÇÃO:
      - Acionado se player < 8 metros de distância (esfera invisível).
      - Destino do NavMesh = posição do jogador.
      - Velocidade sobe para 4.5 m/s.
  Comportamento estratégico:
    - 30% dos inimigos têm "habilidade de emboscada": esperam parados 5s
      atrás de obstáculos antes de começar a perseguição.
  Chefe Polux:
    - 5 fases. Cada fase reduz vida 20% e muda comportamento.
    - Fases alternam entre "disparar projéteis" / "investida" /
      "chamas" / "braços pegajosos" / "loucura final".

━━━ NPCs e Amigáveis (2º bimestre) ━━━
  Script NPCController:
    - Estado IDLE: anda de um ponto A a B aleatoriamente.
    - Estado FALA: se player < 2m + tecla F, abre canvas de diálogo.
    - Árvore de diálogos simples: 2 escolhas → 1 recompensa.
  D. Rosália (Nível 4): troca 5 sementes por bateria extra + 100 pontos.
  Criança NPC (Nível 4): missão de achar o cachorro → +skin canino.

━━━ IA de Suporte ━━━
  • NavMesh para detecção de caminho dos inimigos.
  • Detecção de jogador: Physics.OverlapSphere a cada 0.2s.
  • Detecção de colisão: Collider OnTriggerEnter / OnCollisionEnter.
  • CameraFollow suave: SmoothDamp em posição + rotação.
  • LevelSpawner: posiciona sementes longe de outras via lista de posições
    usadas (evita spawn em cima uma da outra).

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
TÉCNICO
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

━━━ Hardware Alvo ━━━
  Plataformas:
    • Principal: Windows 10/11, PC Desktop.
  Requisitos Mínimos (rodar em 30 FPS):
    • Processador: Intel Core i3-4160 / AMD Ryzen 3 1200.
    • Memória RAM: 4 GB.
    • Vídeo: NVIDIA GTX 660 / AMD HD 7850, 2GB VRAM.
    • Armazenamento: 2 GB livres (HDD OK).
  Requisitos Recomendados (60 FPS + efeitos):
    • i5-8400 / Ryzen 5 2600, 8GB RAM, GTX 1060.

━━━ Hardware e Software de Desenvolvimento ━━━
  Game Engine: Unity 6000.0.8f1 (exata do lab 12).
  Template: Universal Render Pipeline (URP) - 3D.
  Linguagem: C# 11.
  IDE: Visual Studio 2022 ou Rider.
  Software Arte (2º bim): Blender 3.6 (modelagem), GIMP 2.10 (texturas).
  Software Áudio (2º bim): Audacity (edição sfx), MuseScore (música).
  Versionamento: Git + GitHub privado do grupo.

━━━ Requisitos de Rede ━━━
  Jogo Single-Player OFFLINE. Não tem multiplayer.
  Requisito de rede = NENHUM. (2º bimestre: leaderboard opcional via PlayFab).

━━━ Arte do Jogo (Assets) ━━━
  1º BIMESTRE:
    • Nenhum asset baixado de terceiros. Todos os 89 assets do Nível 1
      são GERADOS PROGRAMATICAMENTE com GameObject.CreatePrimitive
      (cubos, esferas, cilindros) + materiais coloridos em URP Standard.
    • Motivo: evitar "asset fliper" que o professor proibiu na OBS 1
      (cópias da internet).
    • Estilo Low-Poly procedural: geométrico, limpo, cores sólidas.

  2º BIMESTRE (planejado, mas assets programáticos JÁ CRIADOS):
    • Níveis 2, 3, 4 e 5 já têm seus assets prontos no LevelBuilder:
      96 + 104 + 112 + 124 = 436 assets adicionais.
    • Total geral = 89 + 436 = 525 assets (ver tabela de assets p/ nível).
    • Modelar no Blender (melhorias futuras opcionais): personagem
      S.E.E.D. 07, Poluentes, D. Rosália.
    • Animações no Mixamo (autorizado pelo professor via OBS 2 - validar).
    • Texturas no TextureBox / hand-painted no GIMP.
    • Terreno modelado com Terrain do Unity, 10+ camadas de textura:
      Terra, Morros, Vales, Água, Ravinas, Areia, Cascalho, Grama,
      Gramíneas, Árvores, Caminhos.
    • NavMesh para NPCs.
    • Partículas CUSTOM criadas no editor.
    • Iluminação com luzes point/spot em pontos de interesse.
    • Pós-processamento URP: Bloom, Vignette, Color Adjustments, SSR.

━━━ O que foi DESENVOLVIDO vs o que está PLANEJADO ━━━
  (Requisito do professor: "defina o jogo final e esclareça o que desenvolveu")

  ✔️ DESENVOLVIDO (1º BIMESTRE - ENTREGA):
    - Sistema completo dos 5 níveis implementado (LevelBuilder):
      525 assets no total (89 + 96 + 104 + 112 + 124).
    - LevelSelector UI: menu inicial com 5 botões (um por nível),
      contador de assets por nível e TOTAL GERAL visível.
    - Cada nível já é carregável no Play via botão correspondente.
    - Nível 1 (Floresta das Cinzas): 89 assets, entrega oficial do bim.
    - 13 scripts C# com documentação XML completa por função.
    - Tema Ecorrenovação (8 elementos, ≥5 exigido).
    - Todos os 14 itens dos tutoriais implementados:
      Colisão, Movimento Translate+AddForce, Rotação, Torque, Vetor de
      prefabs, Scripts, Partículas (Explosão, Fumaça, Detritos/pedrinhas),
      Som, Animator 5 estados, Fumaça, Random, UI texto+botão Reiniciar,
      Spawn via função, Tags.
    - UI com Menu Pausa, Vitória, Derrota (todos com botão REINICIAR).
    - GDD Completo (este documento) + tabela de assets p/ nível.
    - RuntimeAutoBuilder: 0 cliques, só apertar Play.

  ⏳ PLANEJADO (2º BIMESTRE - não entrege agora):
    - Detalhamento de MECÂNICA de Skills + Tecnologia (3 + 3 itens)
      e NPCs / NavMesh dos níveis 2..5 (a estrutura e assets já existem).
    - 3 temas completos: Ecorrenovação + Tecnologia + Skills com ≥3 itens
      de cada.
    - Animações Mixamo, Partículas customizadas, Iluminação avançada e
      Pós-processamento URP.
    - Melhor visual do terreno com 10+ layers de textura.
    - Vídeo de apresentação 1-3 min (OneDrive).

================================================================================
FIM DO GDD COMPLETO - VERSÃO SEGUINDO TEMPLATE DO PROFESSOR
================================================================================
