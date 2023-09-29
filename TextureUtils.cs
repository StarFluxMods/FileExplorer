using System.Collections.Generic;
using UnityEngine;

namespace FileExplorer
{
    public class TextureUtils
    {
        private static Dictionary<(Color, (int, int)), Texture2D> cache = new Dictionary<(Color, (int, int)), Texture2D>();
        
        public static Texture2D GetTexture(Color color, int width, int height)
        {
            if (cache.ContainsKey((color, (width, height))))
            {
                return cache[(color, (width, height))];
            }
            
            var texture = new Texture2D(width, height, TextureFormat.RGBAFloat, false); 
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            cache.Add((color, (width, height)), texture);
            return texture;
        }
    }
}