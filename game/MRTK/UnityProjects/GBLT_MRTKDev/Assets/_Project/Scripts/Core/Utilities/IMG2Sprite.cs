using Core.Extension;
using Cysharp.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.Utility
{
    public static class IMG2Sprite
    {
        //Static class instead of _instance
        // Usage from any other script:
        // MySprite = IMG2Sprite.LoadNewSprite(FilePath, [pixelsPerUnit (optional)], [spriteType(optional)])

        #region Network File

        public static async UniTask<Texture2D> FetchImageTexture(string uri, float pixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            const int kMaxRetries = 3;

            for (int retries = 0; retries < kMaxRetries; retries++)
            {
                using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(uri);
                await webRequest.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
#else
                    if (webRequest.isHttpError || webRequest.isNetworkError)
#endif
                {
                    Debug.LogError("SendWebRequest error: " + webRequest.error + " for URL " + uri);
                }
                else
                {
#if UNITY_2019_1_OR_NEWER
                    bool saveAllowThreaded = Texture.allowThreadedTextureCreation;
                    Texture.allowThreadedTextureCreation = true;
#endif
                    Texture2D tex = DownloadHandlerTexture.GetContent(webRequest);

#if UNITY_2019_1_OR_NEWER
                    Texture.allowThreadedTextureCreation = saveAllowThreaded;
#endif

                    return tex;
                }
            }
            return null;
        }

        public static async UniTask<Sprite> FetchImageSprite(string uri, float pixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            Texture2D tex = await FetchImageTexture(uri, pixelsPerUnit, spriteType);
            return tex == null ? null : tex.TexToSprite(pixelsPerUnit, spriteType);
        }

        #endregion Network File

        #region Local File

        public static Texture2D LoadTexture(string filePath)
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            Texture2D tex;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);           // Create new "empty" texture
                if (tex.LoadImage(fileData))           // Load the imagedata into the texture (size is set automatically)
                    return tex;                 // If data = readable -> return texture
            }
            return null;                     // Return null if load failed
        }

        public static Sprite LoadSpriteFromDir(string filePath, float pixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

            Texture2D tex = LoadTexture(filePath);
            return tex.TexToSprite(pixelsPerUnit, spriteType);
        }

        #endregion Local File
    }
}
