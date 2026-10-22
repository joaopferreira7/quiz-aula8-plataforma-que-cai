using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Monta a cena do quiz com os sprites do pack Kenney Pixel Platformer (menu Quiz > Recriar cena).
/// A cena já vem pronta em Assets/Scenes/Main.unity; isto serve só para recriá-la do zero.
/// </summary>
public static class SceneBuilder
{
    private const string CaminhoCena = "Assets/Scenes/Main.unity";
    private const string PastaPack = "Assets/Kenney Pixel Platformer/Tiles";
    private const string CaminhoMaterialJogador = "Assets/Fisica/JogadorSemAtrito.physicsMaterial2D";

    // Índices dos tiles usados (ver Assets/Kenney Pixel Platformer/Preview.png)
    private const int GramaEsq = 21, GramaMeio = 22, GramaDir = 23;
    private const int TerraEsq = 121, TerraMeio = 122, TerraDir = 123;
    private const int MadeiraEsq = 48, MadeiraMeio = 49, MadeiraDir = 50;
    private const int Broto = 124, Planta = 125, Arvore = 126;

    [MenuItem("Quiz/Recriar cena")]
    public static void Build()
    {
        AssetDatabase.ImportAsset("Assets/Kenney Pixel Platformer", ImportAssetOptions.ImportRecursive);
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = CriarCamera();

        // Chão à esquerda, um buraco no meio e chão à direita.
        // A plataforma especial leva o jogador por cima do buraco e depois cai.
        CriarChao("Chao Esquerda", -14, 12, -3, 6);
        CriarChao("Chao Direita", 13, 14, -3, 6);
        CriarChao("Degrau Esquerda", -17, 3, 1, 10);
        CriarChao("Degrau Direita", 27, 3, 1, 10);

        // Plataforma que cai (requisito 1: collider + Rigidbody2D sem gravidade, parada)
        var plataforma = CriarPlataforma("Plataforma Que Cai", -1f, -1.5f, 3);
        var rbPlataforma = plataforma.AddComponent<Rigidbody2D>();
        rbPlataforma.bodyType = RigidbodyType2D.Kinematic;
        rbPlataforma.gravityScale = 0f;
        rbPlataforma.interpolation = RigidbodyInterpolation2D.Interpolate;
        rbPlataforma.freezeRotation = true;
        plataforma.AddComponent<PlataformaQueCai>();

        // Decoração (sem collider)
        CriarDecoracao(Arvore, -11.5f, -2.5f);
        CriarDecoracao(Broto, -8.5f, -2.5f);
        CriarDecoracao(Planta, -4.5f, -2.5f);
        CriarDecoracao(Arvore, 16.5f, -2.5f);
        CriarDecoracao(Broto, 20.5f, -2.5f);
        CriarDecoracao(Planta, 24.5f, -2.5f);

        var player = CriarJogador(new Vector2(-9f, -2.4f));
        camera.GetComponent<CameraFollow2D>().DefinirAlvo(player.transform);
        camera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1.5f, -10f);

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(cena, CaminhoCena);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(CaminhoCena, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("[Quiz] Cena criada em " + CaminhoCena);
    }

    // ---------- Peças da cena ----------

    private static GameObject CriarCamera()
    {
        var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow2D));
        cam.tag = "MainCamera";
        var camera = cam.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(127, 240, 165, 255);

        // Fundo do pack: colinas + chão ao longe, preso à câmera
        var fundo = new GameObject("Fundo");
        fundo.transform.SetParent(cam.transform, false);
        CriarFaixaFundo(fundo.transform, "Colinas", 14, -1.5f);
        CriarFaixaFundo(fundo.transform, "Base 1", 22, -4.5f);
        CriarFaixaFundo(fundo.transform, "Base 2", 22, -7.5f);
        return cam;
    }

    private static void CriarFaixaFundo(Transform pai, string nome, int tile, float y)
    {
        var go = new GameObject(nome, typeof(SpriteRenderer));
        go.transform.SetParent(pai, false);
        go.transform.localPosition = new Vector3(0f, y, 10f);
        go.transform.localScale = new Vector3(3f, 3f, 1f);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = Fundo(tile);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(14f, 1f);
        sr.sortingOrder = -10;
    }

    /// <summary>Bloco de chão: grama em cima e terra embaixo. Um único BoxCollider2D cobre tudo.</summary>
    private static GameObject CriarChao(string nome, int xInicio, int largura, int yTopo, int linhas)
    {
        var chao = new GameObject(nome);
        chao.transform.position = new Vector3(xInicio + largura / 2f, yTopo - linhas / 2f, 0f);
        var col = chao.AddComponent<BoxCollider2D>();
        col.size = new Vector2(largura, linhas);

        for (int linha = 0; linha < linhas; linha++)
        for (int i = 0; i < largura; i++)
        {
            int tile;
            if (linha == 0) tile = i == 0 ? GramaEsq : i == largura - 1 ? GramaDir : GramaMeio;
            else tile = i == 0 ? TerraEsq : i == largura - 1 ? TerraDir : TerraMeio;
            CriarTile(chao.transform, tile, new Vector2(xInicio + i + 0.5f, yTopo - linha - 0.5f), 0);
        }
        return chao;
    }

    /// <summary>Plataforma de madeira de 1 tile de altura.</summary>
    private static GameObject CriarPlataforma(string nome, float xInicio, float yTopo, int largura)
    {
        var plataforma = new GameObject(nome);
        plataforma.transform.position = new Vector3(xInicio + largura / 2f, yTopo - 0.5f, 0f);
        var col = plataforma.AddComponent<BoxCollider2D>();
        col.size = new Vector2(largura, 1f);

        for (int i = 0; i < largura; i++)
        {
            int tile = i == 0 ? MadeiraEsq : i == largura - 1 ? MadeiraDir : MadeiraMeio;
            CriarTile(plataforma.transform, tile, new Vector2(xInicio + i + 0.5f, yTopo - 0.5f), 0);
        }
        return plataforma;
    }

    private static void CriarDecoracao(int tile, float x, float y)
    {
        CriarTile(null, tile, new Vector2(x, y), -1).name = "Decoracao";
    }

    private static GameObject CriarJogador(Vector2 posicao)
    {
        var player = new GameObject("Player", typeof(SpriteRenderer));
        player.tag = "Player";
        player.transform.position = posicao;
        var sr = player.GetComponent<SpriteRenderer>();
        sr.sprite = Personagem(0);
        sr.sortingOrder = 5;

        // Collider do tamanho do desenho do personagem (20x23 px de 24x24)
        var col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(20f / 24f, 23f / 24f);
        col.offset = new Vector2(0f, -0.5f / 24f);
        col.sharedMaterial = MaterialSemAtrito();

        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        player.AddComponent<PlayerController2D>();
        return player;
    }

    // ---------- Utilitários ----------

    private static GameObject CriarTile(Transform pai, int tile, Vector2 posicaoMundo, int ordem)
    {
        var go = new GameObject("Tile " + tile, typeof(SpriteRenderer));
        go.transform.position = posicaoMundo;
        if (pai != null) go.transform.SetParent(pai, true);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = Tile(tile);
        sr.sortingOrder = ordem;
        return go;
    }

    // Sem atrito o jogador não "gruda" nas paredes ao pular encostado nelas.
    private static PhysicsMaterial2D MaterialSemAtrito()
    {
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(CaminhoMaterialJogador);
        if (material != null) return material;

        Directory.CreateDirectory(Path.GetDirectoryName(CaminhoMaterialJogador));
        material = new PhysicsMaterial2D("JogadorSemAtrito") { friction = 0f, bounciness = 0f };
        AssetDatabase.CreateAsset(material, CaminhoMaterialJogador);
        return material;
    }

    private static Sprite Tile(int i) => Carregar($"{PastaPack}/tile_{i:0000}.png");
    private static Sprite Personagem(int i) => Carregar($"{PastaPack}/Characters/tile_{i:0000}.png");
    private static Sprite Fundo(int i) => Carregar($"{PastaPack}/Backgrounds/tile_{i:0000}.png");

    private static Sprite Carregar(string caminho)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        if (sprite == null) Debug.LogError("[Quiz] Sprite não encontrado: " + caminho);
        return sprite;
    }
}
