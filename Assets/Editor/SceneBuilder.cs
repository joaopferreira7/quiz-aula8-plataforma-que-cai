using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Monta a cena do quiz automaticamente (menu Quiz > Recriar cena).
/// A cena já vem pronta em Assets/Scenes/Main.unity; isto serve só para recriá-la do zero.
/// </summary>
public static class SceneBuilder
{
    private const string CaminhoCena = "Assets/Scenes/Main.unity";
    private const string CaminhoSprite = "Assets/Sprites/Quadrado.png";

    [MenuItem("Quiz/Recriar cena")]
    public static void Build()
    {
        Sprite quadrado = CriarSpriteQuadrado();

        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Câmera
        var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow2D));
        cam.tag = "MainCamera";
        cam.transform.position = new Vector3(0f, 1.5f, -10f);
        var camera = cam.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 7f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.53f, 0.78f, 0.95f);

        // Cenário: chão à esquerda, um buraco no meio e chão à direita.
        // A plataforma especial leva o jogador por cima do buraco e depois cai.
        CriarBloco("Chao Esquerda", quadrado, new Vector2(-8f, -3f), new Vector2(12f, 1f), new Color(0.35f, 0.6f, 0.25f));
        CriarBloco("Chao Direita", quadrado, new Vector2(22f, -3f), new Vector2(16f, 1f), new Color(0.35f, 0.6f, 0.25f));
        CriarBloco("Parede Esquerda", quadrado, new Vector2(-14.5f, 2f), new Vector2(1f, 10f), Color.gray);

        // Plataforma que cai (requisito 1: collider + Rigidbody2D sem gravidade, parada)
        var plataforma = CriarBloco("Plataforma Que Cai", quadrado, new Vector2(1.5f, -1.5f), new Vector2(3f, 0.5f), new Color(0.85f, 0.6f, 0.15f));
        var rbPlataforma = plataforma.AddComponent<Rigidbody2D>();
        rbPlataforma.bodyType = RigidbodyType2D.Kinematic;
        rbPlataforma.gravityScale = 0f;
        rbPlataforma.interpolation = RigidbodyInterpolation2D.Interpolate;
        rbPlataforma.freezeRotation = true;
        plataforma.AddComponent<PlataformaQueCai>();

        // Jogador
        var player = CriarBloco("Player", quadrado, new Vector2(-6f, -1.5f), new Vector2(1f, 1f), new Color(0.9f, 0.3f, 0.3f));
        player.tag = "Player";
        player.GetComponent<SpriteRenderer>().sortingOrder = 1;
        var rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        player.AddComponent<PlayerController2D>();

        cam.GetComponent<CameraFollow2D>().DefinirAlvo(player.transform);

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(cena, CaminhoCena);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(CaminhoCena, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("[Quiz] Cena criada em " + CaminhoCena);
    }

    private static GameObject CriarBloco(string nome, Sprite sprite, Vector2 posicao, Vector2 tamanho, Color cor)
    {
        var go = new GameObject(nome, typeof(SpriteRenderer), typeof(BoxCollider2D));
        go.transform.position = posicao;
        go.transform.localScale = new Vector3(tamanho.x, tamanho.y, 1f);
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = cor;
        return go;
    }

    // Gera um sprite branco 32x32 (1 unidade do mundo). Troque por um sprite do seu asset pack se quiser.
    private static Sprite CriarSpriteQuadrado()
    {
        if (!File.Exists(CaminhoSprite))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CaminhoSprite));
            var tex = new Texture2D(32, 32);
            var pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            File.WriteAllBytes(CaminhoSprite, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(CaminhoSprite);
        }

        var importer = (TextureImporter)AssetImporter.GetAtPath(CaminhoSprite);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(CaminhoSprite);
    }
}
