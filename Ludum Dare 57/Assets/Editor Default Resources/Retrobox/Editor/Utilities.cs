using System.Collections;
using System.Collections.Generic;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEditor;
using Retro;
using UnityEditor.U2D.Sprites;
using System.Linq;

namespace RetroEditor {
    public static class Utilities {
        public static void GeneratePreferences() {

            RetroboxPrefs asset = ScriptableObject.CreateInstance<RetroboxPrefs>();

            asset.boxDictionary = new BoxDataDictionary();
            asset.pointDictionary = new BoxDataDictionary();

            asset.propsDictionary = new BoxPropertiesDictionary();
            asset.framePropsDictionary = new BoxPropertiesDictionary();

            asset.cachedZoomSetting = 0;
            asset.cachedGridSetting = true;

            //default box types
            asset.boxDictionary.Add("Physics", new BoxData(new Color(28 / 256f, 219 / 256f, 149 / 256f), "Physics", 0, true, Retro.Shape.Box));
            asset.boxDictionary.Add("Hurt", new BoxData(new Color(242 / 256f, 206 / 256f, 4 / 256f), "Hurt", 8, false, Retro.Shape.Box));
            asset.boxDictionary.Add("Hit", new BoxData(new Color(255 / 256f, 58 / 256f, 48 / 256f), "Hit", 9, false, Retro.Shape.Box));

            AssetDatabase.CreateAsset(asset, "Assets/Editor Default Resources/Retrobox/Resources/Retrobox Preferences.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        //Fill an image with a colour...
        public static Texture2D FillImage(Texture2D img, Color col) {
            for (int i = 0; i < img.width; i++) {
                for (int j = 0; j < img.height; j++) {
                    img.SetPixel(i, j, col);
                }
            }
            return img;
        }


        public static Sprite[] SliceTexture(Texture2D texture, Vector2Int size, Vector2 pivot) {
            List<Sprite> slicedSprites = new List<Sprite>();

            string assetPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter == null) {
                Debug.LogError("Failed to get TextureImporter for texture");
            } else {
                // FIRST STEP:
                // Setup the texture asset to have the correct import settings
                // Eg, Ensure that the texture is set to 'multiple' mode
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Multiple;
                textureImporter.mipmapEnabled = false; // Mipmaps are unnecessary for sprites
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.maxTextureSize = 8192; // Im setting this much larger incase we have very large spritesheets

                // Reimport the texture with updated settings
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                //-----------------------------------

                // SECOND STEP:
                // Slice the texture and create SpriteRects that represent each sliced sprite
                // Note: TextureImporter.spritesheet no longer works, it seems to get overridden
                var factory = new SpriteDataProviderFactories();
                factory.Init();
                ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(texture);
                dataProvider.InitSpriteEditorDataProvider();
                dataProvider.SetSpriteRects(GenerateSpriteRectData(texture, texture.width, texture.height, size, pivot));
                dataProvider.Apply();

                var assetImporter = dataProvider.targetObject as AssetImporter;
                assetImporter.SaveAndReimport();
                //------------------------------------

                // THIRD STEP:
                // I use this to load all of the sliced sprites into a sprite array that I use in a scriptable object
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (Object asset in assets) {
                    if (asset is Sprite sprite && sprite.name != texture.name) {
                        slicedSprites.Add(sprite);
                    }
                }
            }

            return slicedSprites.ToArray();
        }

        private static SpriteRect[] GenerateSpriteRectData(Texture2D tex, int textureWidth, int textureHeight, Vector2Int size, Vector2 pivot) {
            List<SpriteRect> spriteRects = new List<SpriteRect>();
            for (int y = textureHeight; y > 0; y -= size.y) {
                for (int x = 0; x < textureWidth; x += size.x) {
                    SpriteRect spriteRect = new SpriteRect();
                    spriteRect.rect = new Rect(x, y - size.y, size.x, size.y);
                    spriteRect.name = tex.name + "_" + spriteRects.Count;
                    spriteRect.alignment = SpriteAlignment.Custom;
                    spriteRect.pivot = pivot;
                    spriteRect.border = new Vector4(0, 0, 0, 0);

                    spriteRects.Add(spriteRect);
                }
            }

            return spriteRects.ToArray();
        }

    }
}