using UnityEngine;

/// <summary>
/// Plataforma que fica parada até o jogador encostar nela.
/// Ao ser tocada: anda em linha reta por 5 segundos com velocidade constante,
/// depois para o movimento horizontal e passa a cair pela gravidade.
/// O movimento só é ativado UMA vez.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlataformaQueCai : MonoBehaviour
{
    private enum Estado { Parada, Movendo, Caindo }

    [SerializeField] private Vector2 direcao = Vector2.right;
    [SerializeField] private float velocidade = 2f;
    [SerializeField] private float duracaoMovimento = 5f;
    [SerializeField] private float escalaGravidadeQueda = 1f;

    private Rigidbody2D rb;
    private Estado estado = Estado.Parada;
    private float tempoMovendo;

    public bool FoiAtivada => estado != Estado.Parada;
    public bool EstaCaindo => estado == Estado.Caindo;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Requisito 1: começa parada e sem gravidade.
        // Kinematic = não é afetada por gravidade nem empurrada pelo jogador,
        // mas continua colidindo com ele (o jogador é Dynamic).
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.freezeRotation = true;
    }

    private void OnCollisionEnter2D(Collision2D colisao)
    {
        // Requisitos 2 e 6: só ativa com o jogador, e só se ainda estiver parada.
        if (estado != Estado.Parada) return;
        if (!colisao.collider.CompareTag("Player")) return;

        estado = Estado.Movendo;
        tempoMovendo = 0f;
    }

    private void FixedUpdate()
    {
        if (estado != Estado.Movendo) return;

        // Requisito 4: o tempo é contado no passo fixo da física,
        // então a plataforma anda exatamente "duracaoMovimento" segundos.
        // (a pequena tolerância evita erro de arredondamento do float)
        if (tempoMovendo >= duracaoMovimento - 0.0001f)
        {
            ComecarQueda();
            return;
        }

        // Requisito 3: linha reta, velocidade constante.
        rb.linearVelocity = direcao.normalized * velocidade;
        tempoMovendo += Time.fixedDeltaTime;
    }

    private void ComecarQueda()
    {
        estado = Estado.Caindo;

        // Requisito 5: para o movimento horizontal e passa a sofrer gravidade.
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = escalaGravidadeQueda;
        // Trava X e rotação para cair reto, sem ser empurrada para os lados.
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        // Some depois de cair para fora da tela.
        Destroy(gameObject, 6f);
    }
}
