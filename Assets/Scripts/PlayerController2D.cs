using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Movimentação básica 2D: andar para os lados e pular.
/// Teclado: A/D ou setas para andar, Espaço/W/seta para cima para pular. Controle: analógico esquerdo + botão A.
/// Toda a movimentação é feita pela física (Rigidbody2D), sem animação.
/// Funciona tanto com o Input System (novo) quanto com o Input Manager (antigo),
/// dependendo do que estiver ativo em Project Settings > Player > Active Input Handling.
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
    private SpriteRenderer sprite;
    private ContactFilter2D filtroChao;
    private readonly ContactPoint2D[] contatos = new ContactPoint2D[8];
    private Vector3 posicaoInicial;

    private float inputHorizontal;
    private bool pediuPulo;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
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
        inputHorizontal = LerHorizontal();
        if (LerPulo())
            pediuPulo = true;

        // Vira o sprite para o lado em que está andando.
        if (sprite != null && Mathf.Abs(inputHorizontal) > 0.01f)
            sprite.flipX = inputHorizontal < 0f;

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

    private static float LerHorizontal()
    {
        float h = 0f;
#if ENABLE_INPUT_SYSTEM
        // Input System (novo): lê o teclado e o controle diretamente.
        var teclado = Keyboard.current;
        if (teclado != null)
        {
            if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) h -= 1f;
            if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) h += 1f;
        }
        var controle = Gamepad.current;
        if (controle != null && Mathf.Abs(controle.leftStick.x.ReadValue()) > 0.2f)
            h = controle.leftStick.x.ReadValue();
#else
        // Input Manager (antigo)
        h = Input.GetAxisRaw("Horizontal");
#endif
        return Mathf.Clamp(h, -1f, 1f);
    }

    private static bool LerPulo()
    {
#if ENABLE_INPUT_SYSTEM
        var teclado = Keyboard.current;
        var controle = Gamepad.current;
        return (teclado != null && (teclado.spaceKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame || teclado.upArrowKey.wasPressedThisFrame))
            || (controle != null && controle.buttonSouth.wasPressedThisFrame);
#else
        return Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
#endif
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
