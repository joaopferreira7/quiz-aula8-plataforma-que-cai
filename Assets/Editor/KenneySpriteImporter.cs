using UnityEditor;
using UnityEngine;

/// <summary>
/// Configura automaticamente os PNGs do pack "Kenney Pixel Platformer" como sprites pixel art:
/// sem filtro (Point), sem compressão e com 1 tile = 1 unidade do mundo.
/// </summary>
public class KenneySpriteImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.Contains("Kenney Pixel Platformer/Tiles")) return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;

        // Tiles do cenário têm 18px; personagens e fundos têm 24px.
        bool tile24 = assetPath.Contains("/Characters/") || assetPath.Contains("/Backgrounds/");
        importer.spritePixelsPerUnit = tile24 ? 24 : 18;

        // FullRect permite usar o modo "Tiled" do SpriteRenderer (usado no fundo).
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteExtrude = 0;
        importer.SetTextureSettings(settings);
    }
}
