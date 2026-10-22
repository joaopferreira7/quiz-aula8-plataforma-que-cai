using UnityEngine;

/// <summary>
/// Movimentação básica 2D: andar para os lados (A/D ou setas) e pular (Espaço / W / seta para cima).
/// Toda a movimentação é feita pela física (Rigidbody2D), sem animação.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidade = 6f;

    [Header("Pulo")]
    [SerializeField] private float forcaPulo = 14f;

    [Header("Respawn")]
    [Tooltip("Se o jogador cair abaixo desta altura, volta para a posição inicial.")]
    [SerializeField] private float alturaMinima = -15f;

    private Rigidbody2D rb;
    private ContactFilter2D filtroChao;
    private readonly ContactPoint2D[] contatos = new ContactPoint2D[8];
    private Vector3 posicaoInicial;
    private float inputHorizontal;
    private bool pediuPulo;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        posicaoInicial = transform.position;

        // Considera "chão" qualquer contato cuja normal aponte para cima (entre 45° e 135°).
        // Assim não precisamos de layers nem de objetos auxiliares para checar o chão.
        filtroChao = new ContactFilter2D();
        filtroChao.SetNormalAngle(45f, 135f);
    }

    private void Update()
    {
        // Inputs são lidos no Update (a cada frame) para não perder nenhum aperto de tecla.
        inputHorizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            pediuPulo = true;

        if (transform.position.y < alturaMinima)
            Respawn();
    }

    private void FixedUpdate()
    {
        // A física é aplicada no FixedUpdate (passo fixo da física).
        Vector2 v = rb.linearVelocity;
        // Soma a velocidade do chão: assim o jogador é "carregado" por plataformas que se movem.
        v.x = inputHorizontal * velocidade + VelocidadeDoChao().x;

        if (pediuPulo && EstaNoChao())
            v.y = forcaPulo;

        pediuPulo = false;
        rb.linearVelocity = v;
    }

    public bool EstaNoChao()
    {
        return rb.IsTouching(filtroChao);
    }

    private Vector2 VelocidadeDoChao()
    {
        int n = rb.GetContacts(filtroChao, contatos);
        for (int i = 0; i < n; i++)
        {
            Rigidbody2D outro = contatos[i].collider.attachedRigidbody;
            if (outro == rb) outro = contatos[i].otherCollider.attachedRigidbody;
            if (outro != null && outro != rb && outro.bodyType == RigidbodyType2D.Kinematic)
                return outro.linearVelocity;
        }
        return Vector2.zero;
    }

    private void Respawn()
    {
        transform.position = posicaoInicial;
        rb.linearVelocity = Vector2.zero;
    }
}
