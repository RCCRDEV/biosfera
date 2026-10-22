# Biosfera — A Jornada da Restauração

**Game Design Document — 1º Bimestre**

- Engine: Unity 6000.0.8f1 (a versão exata que usamos no laboratório)
- Pipeline de renderização: Built-in (shader "Standard")
- Estilo: 3D, terceira pessoa
- Tema: Ecorrenovação
- Plataforma: PC / Windows
- Autor: Rafael Rocha (trabalho individual)

> Este documento descreve só o que eu realmente entreguei neste bimestre.
> Todos os números que aparecem aqui eu fui conferindo no próprio código enquanto testava o jogo.

---

## 1. A ideia do jogo

A ideia nasceu de uma pergunta simples: e se, em vez de um herói humano, quem consertasse o mundo fosse um robô? No jogo você controla o **S.E.E.D.**, um pequeno robô de tecnologia verde que foi mandado para a "Floresta das Cinzas", uma área que queimou e ficou coberta de poluição. A missão dele é trazer a floresta de volta.

O detalhe que eu mais gostei de fazer é que o S.E.E.D. **não tem mãos nem ferramentas**. Como ele é um robô, a semente fica guardada dentro do próprio corpo dele e é liberada quando ele entra nas Zonas de Restauração. A cada semente liberada, nasce vegetação de verdade no chão e o pedaço de terra que estava morto vai ficando verde na frente do jogador. Isso dá uma sensação de progresso bem concreta: você literalmente vê a floresta voltando.

Enquanto isso, robôs poluentes ficam soltando fumaça tóxica pelo mapa e atrapalham o trabalho.

**A missão do nível que entreguei:**

- Andar pelo mapa e coletar as sementes brilhantes.
- Levar essas sementes até as Zonas de Restauração (o plantio acontece sozinho).
- Plantar 5 sementes para restaurar a floresta e vencer.
- Tome cuidado com os robôs poluentes: eles machucam, e se você perder as três vidas o S.E.E.D. é desativado.
- Fazer tudo isso antes do cronômetro de 4:00 zerar.

---

## 2. O tema Ecorrenovação

O trabalho pedia pelo menos 5 elementos do tema. Eu coloquei 8:

1. **Sementes** — esferas verdes que brilham, flutuam e giram sozinhas no ar. Elas são a esperança da regeneração.
2. **Zonas de Restauração** — discos de terra com uma coluna de luz verde no alto. Conforme você planta, o disco vai mudando de marrom (terra morta) para verde (vegetação viva).
3. **Vegetação que nasce da restauração** — cada semente plantada faz brotar uma arvorezinha aleatória dentro da zona, usando `Instantiate` em cima de um vetor de prefabs.
4. **Robôs poluentes** — os inimigos, com um "olho" vermelho brilhando e um jato contínuo de fumaça tóxica.
5. **Explosão ao neutralizar** — quando você pisa em cima de um poluente, ele explode em partículas e pedrinhas e sai girando pelo ar.
6. **Detritos e lixo** — latas e entulho jogados pelo cenário devastado.
7. **Cogumelos e flores** — os primeiros sinais de que aquele lugar ainda pode voltar a viver.
8. **Tufos de capim** — espalhados pelo terreno, são a categoria mais numerosa do cenário.

---

## 3. Como se joga

### Controles

| Tecla / Ação | O que faz |
| --- | --- |
| W A S D ou setas | Andar com o robô |
| SHIFT (segurar) | Correr |
| ESPAÇO | Pular |
| MOUSE | Girar a câmera em volta do robô |
| P, ESC ou botão | Pausar e continuar |
| Botão REINICIAR | Reinicia a fase (tem na Pausa, na Vitória e na Derrota) |

### O fluxo de uma partida

O robô começa no centro do mapa. No HUD fica sempre visível quantas sementes você já pegou, quantas já plantou da meta, e o relógio. Você sai andando, coleta as sementes espalhadas e leva elas para uma das três Zonas de Restauração. Ficar dentro da zona é o suficiente: o plantio é automático, uma semente a cada 0,9 segundo.

Chegando a 5 sementes plantadas, aparece a tela de vitória. Se o tempo acabar antes, aparece a derrota.

Os quatro poluentes ficam patrulhando o mapa. Quando você chega perto, eles percebem (ficam avermelhados) e começam a te perseguir. Encostar de lado neles dói: você perde uma das três vidas, perde 5 segundos no relógio, leva um empurrão e o robô pisca de vermelho. Para vencer um poluente, o jeito é pular em cima da cabeça dele — ele explode, e você ganha um impulso para cima.

### Números que configurei

| Item | Valor |
| --- | --- |
| Tempo da fase | 240 s (4:00) |
| Sementes para vencer | 5 |
| Sementes espalhadas pelo mapa | 8 |
| Zonas de Restauração | 3 |
| Inimigos poluentes | 4 |
| Intervalo entre plantios | 0,9 s |
| Velocidade de andar / correr | 5 / 8,5 m por segundo |
| Força do pulo | 7 (ForceMode.Impulse) |

---

## 4. Os 14 itens pedidos no trabalho

O trabalho tinha uma lista de 14 coisas que o projeto precisava ter. Fui anotando onde cada uma ficou no código para facilitar a correção.

**1. Colisão.** O jogador tem um `CapsuleCollider` de verdade e um `Rigidbody`. A semente usa um `SphereCollider` em modo trigger, e a coleta acontece no `SeedCollector.OnTriggerEnter`. A zona também é um trigger, e o `RestorationZone` usa `OnTriggerEnter`, `OnTriggerStay` e `OnTriggerExit` para saber quando o robô está dentro dela. O inimigo tem colisão física de verdade, por isso dá para pisar em cima dele. Árvores, pedras e lixo também têm colisores normais (os primitivos do Unity já vêm com isso).

**2. Movimentação com Translate e AddForce.** A semente flutua com `transform.Translate` (no `Seed.Update`). O robô anda com `Rigidbody.AddForce` usando `ForceMode.Acceleration`, e o pulo também é `AddForce`, mas com `ForceMode.Impulse`. Os inimigos usam `AddForce` tanto para patrulhar quanto para perseguir.

**3. Rotação.** A semente gira com `transform.Rotate`. O modelo do robô vira para a direção em que você anda usando `Quaternion.Slerp` + `Quaternion.LookRotation`. Os inimigos também se viram para onde andam. E a câmera gira em órbita com o mouse no `CameraFollow.LateUpdate`.

**4. Torque.** Quando um poluente é neutralizado, ele leva um `AddTorque` com `ForceMode.Impulse` e sai girando descontrolado no ar.

**5. Vetor de prefabs (`GameObject[]`).** O `LevelSpawner` trabalha com vários vetores: árvores, pedras, arbustos, cogumelos, flores, capim, detritos, lixo, vegetação, semente e inimigo. A `RestorationZone` também tem o vetor dela, usado na hora de plantar.

**6. Scripts em C#.** Ao todo são 14 arquivos: 13 em `Assets/Scripts/Biosfera/` e 1 em `Assets/Editor/`.

- Em `Assets/Scripts/Biosfera/`: `Bootstrap.cs`, `BiosferaRunner.cs`, `Factory.cs`, `GameManager.cs`, `AudioManager.cs`, `LevelSpawner.cs`, `PlayerController.cs`, `Seed.cs`, `PollutionEnemy.cs`, `RestorationZone.cs`, `CameraFollow.cs`, `UIManager.cs` e `SeedCollector.cs`.
- Em `Assets/Editor/`: `BiosferaEditorSetup.cs`, que cria a cena, o AnimatorController com os 5 estados, o Build Settings e os shaders.

**7. Partículas.** Tudo criado por código na classe `Factory`: a explosão do inimigo, os detritos/pedrinhas, o efeito do plantio e o efeito de coletar a semente. A fumaça tóxica é um `ParticleSystem` em loop preso ao inimigo. O `Factory.SpawnBurst` dispara um efeito com `Instantiate`, toca e se destrói sozinho.

**8. Som.** O `AudioManager` tem dois `AudioSource`, um para a trilha e outro para os efeitos. Nenhum som foi baixado da internet: todos os clipes são gerados por código com `Factory.Tone` e `Factory.Ambient`. Tem uma música em loop e efeitos para coletar, plantar, neutralizar, pular, vencer e perder.

**9. Animação com 5 estados.** O `SEEED07_Animator` é montado pelo `BiosferaEditorSetup` com os estados Idle, Walk, Run, Jump e Fall, e os parâmetros `Speed`, `VerticalSpeed` e `Grounded`. O `PlayerController` calcula o estado atual e troca com `Animator.CrossFade`. Além disso eu fiz uma animação procedural (o robô "respira" e balança conforme a velocidade), então ele nunca fica parecendo congelado.

**10. Posição aleatória.** O `LevelSpawner.RandomGroundPosition` sorteia a posição X e Z de cada asset dentro da área jogável, sempre respeitando um espaço livre no centro onde o robô nasce. Também são sorteadas a rotação e a escala. A zona sorteia qual prefab usar, e os inimigos sorteiam seus pontos de patrulha.

**11. UI com texto e botão REINICIAR.** Toda a interface é montada por código pelo `Bootstrap`, usando o UGUI comum (Text e Button, sem TextMeshPro). Tem o HUD com o texto das sementes, o texto do tempo e a dica de controle, mais três painéis (Pausa, Vitória e Derrota) — e cada painel tem o botão REINICIAR e um botão de sair. Os cliques são ligados no `UIManager`.

**12. Spawn com Instantiate.** Usado em vários lugares: ao espalhar as categorias do cenário, ao criar as sementes e inimigos, ao plantar a vegetação na zona e ao disparar as partículas.

**13. Tags.** Configurei as cinco tags no `TagManager.asset`: `Player`, `Seed`, `RestorationZone`, `Enemy` e `Ground`. Elas são aplicadas em tempo de execução pela `Factory.SetTag`, de um jeito seguro para não quebrar se a tag não existir.

**14. Documentação XML.** Todas as classes e funções estão comentadas no padrão do C#, com `/// <summary>`, `/// <param>` e `/// <returns>` quando faz sentido.

---

## 5. O nível que entreguei

Neste bimestre eu entreguei **só o Nível 1**, chamado "Floresta das Cinzas". Os outros níveis ficam para o segundo bimestre. Mesmo sendo um só, ele já tem todas as mecânicas pedidas.

O chão é um plano de 90 x 90 metros, com cor de terra queimada. Os assets são espalhados numa área de -34 a +34 metros, deixando um espaço livre no centro para o robô nascer.

No total, o jogo cria por código cerca de **340 objetos** (esse número varia um pouco a cada partida, porque algumas coisas são sorteadas). São manchas de cinza, a cerca de árvores na borda, cogumelos, flores, tufos de capim, detritos, lixo, pedras, arbustos e árvores em aglomerados, além das 8 sementes e dos 4 inimigos.

O que me deixou mais satisfeito foi ter resolvido o pedido de "70 assets **diferentes**". Como eu gero tudo por código, eu poderia acabar fazendo 340 cópias do mesmo modelo, e isso não valeria. Então criei uma variação procedural: a cor sai de uma conversão de matiz/saturação/brilho (`Hsv`) e o formato e o tamanho saem de um valor estável por índice (`Noise01`). O resultado é que cada modelo tem forma, tamanho e cor próprios e nunca é igual ao outro. No total, saem **104 modelos distintos**, bem acima dos 70 exigidos.

Além dos assets, ainda são criados o chão, as 3 zonas de restauração, o robô e a interface inteira.

---

## 6. O mundo ao redor

Um dos problemas que eu tive no começo foi que a fase parecia estar flutuando no vazio, sem nada em volta. Para resolver, eu montei um cenário de verdade ao redor da área jogável:

- Um terreno distante de 700 x 700 metros, bem abaixo de tudo, para não aparecer o "fim do mundo".
- Uma floresta de fundo com 150 árvores em anel, entre 48 e 108 metros de distância.
- 40 montanhas lá no horizonte, entre 170 e 300 metros.
- Névoa linear (de 65 a 320 metros) e o alcance da câmera em 700 metros, o que dá aquela sensação de profundidade.
- Quatro paredes invisíveis no limite do mapa, para o robô não cair para fora.

Assim o cenário tem horizonte, profundidade e uma "cerca natural" em volta.

---

## 7. Vidas do robô e dano

No começo o inimigo dava dano mas nada acontecia de verdade — você podia ser atropelado à vontade. Eu arrumei isso colocando um sistema de vidas.

O S.E.E.D. tem **3 vidas**, mostradas no HUD (tipo `VIDA: 3 / 3 [##] [##] [##]`). Cada vez que um poluente encosta de lado em você, perde-se 1 vida, o relógio cai 5 segundos, você leva um empurrão e o robô pisca de vermelho. Existe 1 segundo de invulnerabilidade entre dois golpes, para dois inimigos juntos não tirarem tudo de uma vez.

Se as três vidas acabarem, o robô é desativado (explosão verde) e aparece o painel de derrota com o título **"S.E.E.D. DESATIVADO"**. Se o tempo acabar antes, o título é **"TEMPO ESGOTADO"**. E pular na cabeça do inimigo continua sendo o jeito de derrotá-lo, sem gastar vida.

---

## 8. A mecânica que deixa o jogo diferente

A mecânica principal do jogo é o **"transporte e liberação"**. Em muitos jogos você clica para plantar; aqui não. O robô é um contêiner: ele carrega as sementes no corpo e as libera sozinho enquanto permanece em cima da Zona de Restauração.

Isso cria uma decisão tática de verdade: ficar paradinho na zona é seguro, mas consome tempo; sair correndo pelo mapa é mais rápido, só que te expõe aos poluentes. E como o inimigo é de metal, ele só é derrotado por cima — bater de lado só te machuca e te empurra. Isso reforça o lado de plataforma em um jogo 3D.

---

## 9. Como o jogo foi feito por dentro

**Gráficos.** Todo asset é gerado proceduralmente, usando os primitivos do Unity (`CreatePrimitive` com Cube, Sphere, Cylinder e Plane), com materiais coloridos e escala e rotação sorteadas.

**Originalidade.** Nada aqui veio da internet: nenhum modelo, textura, som ou partícula foi baixado. Tudo é criado em tempo de execução por código, o que garante que o jogo é original.

**Iluminação.** Usei uma luz direcional (o "sol") com sombras suaves, luz ambiente do tipo Trilight e a névoa para dar profundidade, que é reforçada pelo mundo ao redor que descrevi acima.

**Um detalhe que ajuda o jogador.** As Zonas de Restauração têm uma luz verde no alto e uma baliza, então dá para enxergar de longe para onde você precisa levar as sementes.

**O jeito "zero-clique".** Praticamente não existe nada montado à mão na cena do jogo. A cena `Level1` está quase vazia de propósito: tudo é construído por código quando você aperta Play. Isso evita aquele erro chato de "Missing (Mono Script)" e garante que o projeto roda em qualquer máquina, sem eu precisar arrastar nada.

---

## 10. Participação

Este trabalho foi feito **individualmente**.

- Rafael Rocha — 100% (ideia, programação, level design, UI, som, partículas e este GDD)

---

## 11. Checklist de entrega

- [x] Tema Ecorrenovação com mais de 5 elementos (entreguei 8)
- [x] Os 14 itens pedidos no trabalho, todos implementados e documentados
- [x] Nível 1 com mais de 70 assets diferentes (entreguei 104 modelos distintos e cerca de 340 objetos no total)
- [x] GDD detalhado (este documento)
- [x] Documentação XML nas funções dos scripts
- [x] Gameplay funcional (coletar, plantar e vencer)
- [x] Sistema de dano com consequência: 3 vidas e a morte do robô
- [x] Mundo ao redor: terreno distante, floresta de fundo e montanhas
- [x] Mecânica original ("transporte e liberação")
- [x] Partículas: explosão, fumaça e detritos/pedrinhas
- [x] Som: trilha sonora e efeitos, tudo gerado por código
- [x] Animação com os 5 estados (Idle, Walk, Run, Jump, Fall)
- [x] UI com texto e o botão REINICIAR obrigatório
- [x] Movimentação com Translate e AddForce
- [x] Rotação e torque com Rigidbody
- [x] Vetores de prefabs (`GameObject[]`) e posição aleatória
- [x] Tags obrigatórias configuradas
- [x] Roda sem erro nenhum: é só abrir o Unity e apertar Play
