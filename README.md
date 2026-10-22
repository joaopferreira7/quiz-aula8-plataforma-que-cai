# Quiz – Aula 8: Plataforma que se move ao ser tocada e depois cai

Unity **6000.4.1f1** · Input System · física 2D · assets **Kenney Pixel Platformer** (CC0)

## Como abrir
1. Unity Hub → **Add project from disk** → selecione esta pasta.
2. Abra `Assets/Scenes/Main.unity` e aperte **Play**.
3. Pule na plataforma de madeira (A/D para andar, Espaço para pular). Ela leva você por cima do buraco
   por 5 segundos e depois cai — pule para o chão da direita antes!

## Script principal: `Assets/Scripts/PlataformaQueCai.cs`
| Requisito | Como foi atendido |
|---|---|
| 1. Collider + Rigidbody2D, parada, sem gravidade | `BoxCollider2D` + `Rigidbody2D` **Kinematic** com `gravityScale = 0` |
| 2. Imóvel até o jogador tocar | `OnCollisionEnter2D` só reage a objetos com tag **Player** |
| 3. Linha reta, velocidade constante | `linearVelocity = direcao * velocidade` (padrão: direita, 2 u/s) |
| 4. Exatamente 5 segundos, via script | Tempo contado com `Time.fixedDeltaTime` no `FixedUpdate` |
| 5. Para e cai | Zera a velocidade, troca para **Dynamic**, liga a gravidade e trava o eixo X |
| 6. Ativa uma única vez | Máquina de estados `Parada → Movendo → Caindo`; só ativa no estado `Parada` |

Direção, velocidade e duração são editáveis no Inspector.

## Outros arquivos
- `PlayerController2D.cs` – movimentação + pulo (soma a velocidade da plataforma para o jogador ser carregado por ela).
- `CameraFollow2D.cs` – câmera que segue o jogador.
- `Editor/SceneBuilder.cs` – menu **Quiz → Recriar cena**.
- `Editor/KenneySpriteImporter.cs` – configura os PNGs do pack como pixel art.

## Assets
[Kenney Pixel Platformer](https://kenney.nl/assets/pixel-platformer) (licença CC0, ver `Assets/Kenney Pixel Platformer/License.txt`).

## Testes automáticos
`Assets/Tests/CenaTests.cs` carrega a cena real: o jogador pousa no chão, sobe na plataforma, é carregado por ela e ela cai após andar 10 unidades.
`Assets/Tests/PlataformaQueCaiTests.cs` (Window → General → Test Runner → PlayMode → Run All)
verifica que a plataforma fica parada sem contato, anda a 2 u/s, percorre exatamente 10 unidades (5 s),
para no X, cai e não reinicia o movimento.
