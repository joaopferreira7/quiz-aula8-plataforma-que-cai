using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CenaTests
{
    [UnityTest]
    public IEnumerator JogadorPousaNoChaoEPlataformaFuncionaNaCena()
    {
        SceneManager.LoadScene("Main");
        yield return null;

        var player = GameObject.FindWithTag("Player");
        var plataforma = Object.FindAnyObjectByType<PlataformaQueCai>();
        Assert.IsNotNull(player);
        Assert.IsNotNull(plataforma);

        // Jogador pousa no chão e a plataforma continua parada.
        yield return new WaitForSeconds(1.5f);
        Assert.IsTrue(player.GetComponent<PlayerController2D>().EstaNoChao(), "O jogador deveria estar no chão");
        Assert.IsFalse(plataforma.FoiAtivada);

        // Coloca o jogador em cima da plataforma: ela anda, carrega o jogador e depois cai.
        Vector3 inicio = plataforma.transform.position;
        player.transform.position = inicio + Vector3.up * 1.2f;
        yield return new WaitUntil(() => plataforma.FoiAtivada);
        yield return new WaitForSeconds(2f);
        Assert.Greater(plataforma.transform.position.x, inicio.x + 3f);
        Assert.Greater(player.transform.position.x, inicio.x + 2f, "O jogador deveria ser carregado pela plataforma");

        yield return new WaitUntil(() => plataforma.EstaCaindo);
        Assert.AreEqual(inicio.x + 10f, plataforma.transform.position.x, 0.05f);
    }
}
