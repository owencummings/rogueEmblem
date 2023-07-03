using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitAttributes
{
    // Eventually fill with starting unit stats
    public enum UnitType {
        None,
        Melee,
        Archer,
        Wizard,
        Lurker, 
        BigEnemy
    }

    public struct BirdPalette {
        public BirdPalette(Color lightColor, Color darkColor){
            LightColor = lightColor;
            DarkColor = darkColor;
        }

        public Color LightColor { get; }
        public Color DarkColor { get; }
    }

    public static class BirdPalettes {
        public static Dictionary<UnitType, BirdPalette> paletteMap = new Dictionary<UnitType, BirdPalette>();
        public static bool populated = false;
        
        public static void PopulatePalettes()
        {
            paletteMap[UnitType.Melee] = new BirdPalette(new Color(0.91f, 0.64f, 0.1f, 1f), 
                                                         new Color(0.97f, 0.55f, 0.03f, 1f));
            paletteMap[UnitType.Archer] = new BirdPalette(new Color(0.15f, 0.70f, 0.58f, 1f),
                                                          new Color(0.22f, 0.75f, 0.63f, 1f));
            paletteMap[UnitType.None] = new BirdPalette(Color.red, Color.blue);
            paletteMap[UnitType.Wizard] = new BirdPalette(Color.red, Color.blue);
            paletteMap[UnitType.Lurker] = new BirdPalette(Color.red, Color.blue);
            paletteMap[UnitType.BigEnemy] = new BirdPalette(Color.red, Color.blue);
        }

        public static void ColorizeTexture(Transform prefabTransform, UnitType unitType)
        {
            if (!populated){
                PopulatePalettes();
                populated = true;
            }

            if (unitType == UnitType.None) { return; }

            for (int i=1; i<prefabTransform.GetChild(0).childCount; i++){
                Renderer rend = prefabTransform.GetChild(0).GetChild(i).GetComponent<Renderer>();
                Texture2D tex = rend.material.mainTexture as Texture2D;
                Texture2D newTex = new Texture2D(tex.width, tex.height);
                rend.material.mainTexture = newTex;
                newTex.SetPixels(tex.GetPixels());

                // Light colors
                Color lite = paletteMap[unitType].LightColor;
                newTex.SetPixel(6, 7, lite);
                newTex.SetPixel(6, 7, lite);
                newTex.SetPixel(6, 7, lite);
                newTex.SetPixel(6, 7, lite);
                newTex.SetPixel(6, 7, lite);
                newTex.SetPixel(6, 7, lite);
                newTex.SetPixel(6, 7, lite);
                newTex.SetPixel(6, 7, lite);

                //Dark colors 
                Color dark = paletteMap[unitType].DarkColor;
                newTex.SetPixel(8, 0, dark);
                newTex.SetPixel(8, 1, dark);
                newTex.SetPixel(8, 2, dark);
                newTex.SetPixel(8, 3, dark);
                newTex.SetPixel(9, 0, dark);
                newTex.SetPixel(9, 1, dark);
                newTex.SetPixel(9, 2, dark);
                newTex.SetPixel(9, 3, dark);
                newTex.Apply();
                rend.material.mainTexture = newTex;
            }
        }
    }

}
