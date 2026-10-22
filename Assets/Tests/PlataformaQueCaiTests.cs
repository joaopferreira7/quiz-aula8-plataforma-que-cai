using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlataformaQueCaiTests
{
    private GameObject plataforma;
    private GameObject jogador;

    [TearDown]
    public void Limpar()
    {
        if (plataforma != null) Object.Destroy(plataforma);
        if (jogador != null) Object.Destroy(jogador);
    }

    [UnityTest]
    public IEnumerator PlataformaParaDepoisDe5SegundosECai()
    {
        plataforma = new GameObject("Plataforma", typeof(BoxCollider2D), typeof(Rigidbody2D));
        plataforma.transform.localScale = new Vector3(3f, 0.5f, 1f);
        var script = plataforma.AddComponent<PlataformaQueCai>();
        var rbPlataforma = plataforma.GetComponent<Rigidbody2D>();
        Vector2 inicio = rbPlataforma.position;

        // 1) Sem o jogador por perto, fica parada e sem gravidade.
        yield return new WaitForSeconds(1f);
        Assert.IsFalse(script.FoiAtivada);
        Assert.AreEqual(inicio, rbPlataforma.position);

        // 2) Solta o jogador em cima dela.
        jogador = new GameObject("Player", typeof(BoxCollider2D), typeof(Rigidbody2D));
        jogador.tag = "Player";
        jogador.transform.position = inicio + Vector2.up * 1.5f;
        yield return new WaitUntil(() => script.FoiAtivada);

        // 3) Move para a direita em velocidade constante.
        yield return new WaitForSeconds(2f);
        Assert.IsFalse(script.EstaCaindo);
        Assert.AreEqual(2f, rbPlataforma.linearVelocity.x, 0.001f);
        Assert.AreEqual(0f, rbPlataforma.linearVelocity.y, 0.001f);

        // 4) Depois de 5s: para no X e começa a cair.
        yield return new WaitUntil(() => script.EstaCaindo);
        float distancia = rbPlataforma.position.x - inicio.x;
        Assert.AreEqual(10f, distancia, 0.01f, "Deveria andar 2 u/s * 5 s = 10 unidades");

        float yAntes = rbPlataforma.position.y;
        yield return new WaitForSeconds(0.5f);
        Assert.AreEqual(inicio.x + distancia, rbPlataforma.position.x, 0.001f, "Não deve mais andar na horizontal");
        Assert.Less(rbPlataforma.position.y, yAntes - 0.5f, "Deveria estar caindo");

        // 5) Não reativa: continua caindo mesmo com o jogador encostando.
        Assert.IsTrue(script.EstaCaindo);
        Assert.AreEqual(RigidbodyType2D.Dynamic, rbPlataforma.bodyType);
    }
}
