using System.Collections.Generic;
using UnityEngine;

namespace DigitPark.UI.Components
{
    /// <summary>
    /// Genera avatares estilo Google/Discord con la inicial del username
    /// sobre un fondo de color determinístico basado en el userId.
    /// Incluye cache en memoria para evitar regeneración.
    /// </summary>
    public static class AvatarInitialGenerator
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        private const int TEXTURE_SIZE = 256;

        private static readonly Color[] AvatarColors = {
            new Color(0f, 1f, 1f),          // Cyan
            new Color(1f, 0f, 0.6f),         // Magenta/Pink
            new Color(0.2f, 1f, 0.4f),       // Verde neon
            new Color(1f, 0.5f, 0f),          // Naranja
            new Color(0.6f, 0.3f, 1f),        // Purpura
            new Color(0.3f, 0.6f, 1f),        // Azul
            new Color(1f, 0.9f, 0f),          // Amarillo
            new Color(1f, 0.3f, 0.3f),        // Rojo
            new Color(0f, 0.8f, 0.8f),        // Teal
            new Color(0.9f, 0.5f, 1f),        // Lavanda
        };

        /// <summary>
        /// Genera un avatar con la inicial del username y color basado en userId.
        /// Usa cache para devolver el mismo sprite si ya fue generado.
        /// </summary>
        public static Sprite GenerateAvatar(string username, string userId)
        {
            if (string.IsNullOrEmpty(userId))
                userId = "default";

            if (_cache.TryGetValue(userId, out Sprite cached))
                return cached;

            string initial = GetInitial(username);
            Color bgColor = GetColorForUserId(userId);

            Texture2D texture = CreateAvatarTexture(initial, bgColor);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, TEXTURE_SIZE, TEXTURE_SIZE),
                new Vector2(0.5f, 0.5f),
                100f
            );
            sprite.name = $"Avatar_{initial}_{userId}";

            _cache[userId] = sprite;
            return sprite;
        }

        /// <summary>
        /// Elimina un avatar del cache (por ejemplo al subir foto nueva)
        /// </summary>
        public static void InvalidateCache(string userId)
        {
            if (_cache.ContainsKey(userId))
            {
                if (_cache[userId] != null && _cache[userId].texture != null)
                {
                    Object.Destroy(_cache[userId].texture);
                    Object.Destroy(_cache[userId]);
                }
                _cache.Remove(userId);
            }
        }

        /// <summary>
        /// Limpia todo el cache de avatares generados
        /// </summary>
        public static void ClearCache()
        {
            foreach (var kvp in _cache)
            {
                if (kvp.Value != null && kvp.Value.texture != null)
                {
                    Object.Destroy(kvp.Value.texture);
                    Object.Destroy(kvp.Value);
                }
            }
            _cache.Clear();
        }

        private static string GetInitial(string username)
        {
            if (string.IsNullOrEmpty(username) || username.Length == 0)
                return "?";

            // Ignorar @ si el username empieza con @
            string clean = username.TrimStart('@');
            if (clean.Length == 0)
                return "?";

            return clean[0].ToString().ToUpper();
        }

        private static Color GetColorForUserId(string userId)
        {
            int hash = Mathf.Abs(userId.GetHashCode());
            return AvatarColors[hash % AvatarColors.Length];
        }

        private static Texture2D CreateAvatarTexture(string initial, Color bgColor)
        {
            Texture2D texture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;

            // Rellenar fondo con el color
            Color[] pixels = new Color[TEXTURE_SIZE * TEXTURE_SIZE];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = bgColor;
            }
            texture.SetPixels(pixels);

            // Dibujar la inicial centrada (fuente bitmap simple)
            DrawInitial(texture, initial, Color.white);

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Dibuja una letra centrada en la textura usando renderizado bitmap
        /// con trazos gruesos para buena legibilidad
        /// </summary>
        private static void DrawInitial(Texture2D texture, string initial, Color color)
        {
            if (string.IsNullOrEmpty(initial)) return;

            char c = initial[0];
            bool[,] glyph = GetGlyph(c);
            if (glyph == null) return;

            int glyphW = glyph.GetLength(1);
            int glyphH = glyph.GetLength(0);

            // Escalar el glyph para que ocupe ~60% del texture
            float targetSize = TEXTURE_SIZE * 0.55f;
            float scale = targetSize / Mathf.Max(glyphW, glyphH);

            int scaledW = Mathf.RoundToInt(glyphW * scale);
            int scaledH = Mathf.RoundToInt(glyphH * scale);

            int offsetX = (TEXTURE_SIZE - scaledW) / 2;
            int offsetY = (TEXTURE_SIZE - scaledH) / 2;

            // Dibujar con anti-aliasing simple (oversample)
            for (int y = 0; y < scaledH; y++)
            {
                for (int x = 0; x < scaledW; x++)
                {
                    // Mapear pixel de vuelta al glyph
                    int gx = Mathf.FloorToInt(x / scale);
                    int gy = Mathf.FloorToInt(y / scale);

                    gx = Mathf.Clamp(gx, 0, glyphW - 1);
                    gy = Mathf.Clamp(gy, 0, glyphH - 1);

                    if (glyph[gy, gx])
                    {
                        int px = offsetX + x;
                        // Invertir Y porque Texture2D tiene Y=0 abajo
                        int py = offsetY + (scaledH - 1 - y);

                        if (px >= 0 && px < TEXTURE_SIZE && py >= 0 && py < TEXTURE_SIZE)
                        {
                            texture.SetPixel(px, py, color);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Devuelve un bitmap 7x5 para cada caracter soportado (A-Z, 0-9, ?)
        /// Formato: [fila, columna], true = pixel encendido
        /// </summary>
        private static bool[,] GetGlyph(char c)
        {
            c = char.ToUpper(c);
            return c switch
            {
                'A' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,true,true,true,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true}
                },
                'B' => new bool[,] {
                    {true,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,true,true,true,false}
                },
                'C' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                },
                'D' => new bool[,] {
                    {true,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,true,true,true,false}
                },
                'E' => new bool[,] {
                    {true,true,true,true,true},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,true,true,true,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,true,true,true,true}
                },
                'F' => new bool[,] {
                    {true,true,true,true,true},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,true,true,true,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false}
                },
                'G' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,false},
                    {true,false,true,true,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                },
                'H' => new bool[,] {
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,true,true,true,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true}
                },
                'I' => new bool[,] {
                    {true,true,true,true,true},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {true,true,true,true,true}
                },
                'J' => new bool[,] {
                    {false,false,true,true,true},
                    {false,false,false,true,false},
                    {false,false,false,true,false},
                    {false,false,false,true,false},
                    {false,false,false,true,false},
                    {true,false,false,true,false},
                    {false,true,true,false,false}
                },
                'K' => new bool[,] {
                    {true,false,false,false,true},
                    {true,false,false,true,false},
                    {true,false,true,false,false},
                    {true,true,false,false,false},
                    {true,false,true,false,false},
                    {true,false,false,true,false},
                    {true,false,false,false,true}
                },
                'L' => new bool[,] {
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,true,true,true,true}
                },
                'M' => new bool[,] {
                    {true,false,false,false,true},
                    {true,true,false,true,true},
                    {true,false,true,false,true},
                    {true,false,true,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true}
                },
                'N' => new bool[,] {
                    {true,false,false,false,true},
                    {true,true,false,false,true},
                    {true,false,true,false,true},
                    {true,false,true,false,true},
                    {true,false,false,true,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true}
                },
                'O' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                },
                'P' => new bool[,] {
                    {true,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,true,true,true,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false}
                },
                'Q' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,true,false,true},
                    {true,false,false,true,false},
                    {false,true,true,false,true}
                },
                'R' => new bool[,] {
                    {true,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,true,true,true,false},
                    {true,false,true,false,false},
                    {true,false,false,true,false},
                    {true,false,false,false,true}
                },
                'S' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,false},
                    {false,true,true,true,false},
                    {false,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                },
                'T' => new bool[,] {
                    {true,true,true,true,true},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false}
                },
                'U' => new bool[,] {
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                },
                'V' => new bool[,] {
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,false,true,false},
                    {false,true,false,true,false},
                    {false,false,true,false,false}
                },
                'W' => new bool[,] {
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {true,false,true,false,true},
                    {true,false,true,false,true},
                    {true,true,false,true,true},
                    {true,false,false,false,true}
                },
                'X' => new bool[,] {
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,false,true,false},
                    {false,false,true,false,false},
                    {false,true,false,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true}
                },
                'Y' => new bool[,] {
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,false,true,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false}
                },
                'Z' => new bool[,] {
                    {true,true,true,true,true},
                    {false,false,false,false,true},
                    {false,false,false,true,false},
                    {false,false,true,false,false},
                    {false,true,false,false,false},
                    {true,false,false,false,false},
                    {true,true,true,true,true}
                },
                '0' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,true,true},
                    {true,false,true,false,true},
                    {true,true,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                },
                '1' => new bool[,] {
                    {false,false,true,false,false},
                    {false,true,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,true,true,true,false}
                },
                '2' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {false,false,false,false,true},
                    {false,false,false,true,false},
                    {false,false,true,false,false},
                    {false,true,false,false,false},
                    {true,true,true,true,true}
                },
                '3' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {false,false,false,false,true},
                    {false,false,true,true,false},
                    {false,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                },
                '4' => new bool[,] {
                    {false,false,false,true,false},
                    {false,false,true,true,false},
                    {false,true,false,true,false},
                    {true,false,false,true,false},
                    {true,true,true,true,true},
                    {false,false,false,true,false},
                    {false,false,false,true,false}
                },
                '5' => new bool[,] {
                    {true,true,true,true,true},
                    {true,false,false,false,false},
                    {true,true,true,true,false},
                    {false,false,false,false,true},
                    {false,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                },
                '6' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,false},
                    {true,false,false,false,false},
                    {true,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                },
                '7' => new bool[,] {
                    {true,true,true,true,true},
                    {false,false,false,false,true},
                    {false,false,false,true,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false},
                    {false,false,true,false,false}
                },
                '8' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,false}
                },
                '9' => new bool[,] {
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {true,false,false,false,true},
                    {false,true,true,true,true},
                    {false,false,false,false,true},
                    {false,false,false,false,true},
                    {false,true,true,true,false}
                },
                _ => new bool[,] { // ? por defecto
                    {false,true,true,true,false},
                    {true,false,false,false,true},
                    {false,false,false,false,true},
                    {false,false,false,true,false},
                    {false,false,true,false,false},
                    {false,false,false,false,false},
                    {false,false,true,false,false}
                }
            };
        }
    }
}
