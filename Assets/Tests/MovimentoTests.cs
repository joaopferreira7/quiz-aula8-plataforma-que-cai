#if ENABLE_INPUT_SYSTEM
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Simula o teclado (Input System) e confere que o personagem anda e pula na cena real.
/// </summary>
public class MovimentoTests : InputTestFixture
{
    private Keyboard teclado;

    public override void Setup()
    {
        base.Setup();
        teclado = InputSystem.AddDevice<Keyboard>();
    }

    [UnityTest]
    public IEnumerator PersonagemAndaParaOsDoisLadosEPula()
    {
        SceneManager.LoadScene("Main");
        yield return null;
        var player = GameObject.FindWithTag("Player");
        yield return new WaitForSeconds(1f); // espera pousar no chão

        // Anda para a direita segurando D
        float x0 = player.transform.position.x;
        Press(teclado.dKey);
        yield return new WaitForSeconds(0.5f);
        Release(teclado.dKey);
        yield return null; // deixa o sistema processar a tecla solta antes da próxima
        Assert.Greater(player.transform.position.x, x0 + 2f, "Deveria andar para a direita com D");

        // Anda para a esquerda segurando a seta esquerda
        float x1 = player.transform.position.x;
        Press(teclado.leftArrowKey);
        yield return new WaitForSeconds(0.3f);
        Release(teclado.leftArrowKey);
        Assert.Less(player.transform.position.x, x1 - 1f, "Deveria andar para a esquerda com a seta");
        Assert.IsTrue(player.GetComponent<SpriteRenderer>().flipX, "Deveria virar para a esquerda");

        // Pula com Espaço
        yield return new WaitForSeconds(0.2f);
        float y0 = player.transform.position.y;
        Press(teclado.spaceKey);
        yield return null;
        yield return null;
        Release(teclado.spaceKey);
        yield return new WaitForSeconds(0.3f);
        Assert.Greater(player.transform.position.y, y0 + 1.5f, "Deveria pular com Espaço");
    }
}
#endif
