using BepInEx;
using R2API.Utils;
using RoR2;
using R2API;
using R2API.MiscHelpers;
using System.Reflection;
using static R2API.SoundAPI;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;

namespace MoistureUpset
{
    public static class UImods
    {
        private sealed class UIReplaceMarker : MonoBehaviour { }

        public static void ReplaceUIObject(string objectname, string path)
        {
            try
            {
                GameObject logo = GameObject.Find(objectname);
                if (logo)
                {
                    if (logo.GetComponent<UIReplaceMarker>())
                    {
                        return;
                    }

                    var image = logo.GetComponent<UnityEngine.UI.Image>();
                    if (!image)
                    {
                        return;
                    }

                    byte[] bytes = ByteReader.readbytes(path);
                    if (bytes == null || bytes.Length == 0)
                    {
                        return;
                    }

                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    tex.filterMode = FilterMode.Trilinear;
                    if (!tex.LoadImage(bytes))
                    {
                        UnityEngine.Object.Destroy(tex);
                        return;
                    }

                    image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100);
                    logo.AddComponent<UIReplaceMarker>();
                }
            }
            catch (Exception)
            {
            }
        }
        public static void ReplaceUIBetter(string path, string png)
        {
            try
            {
                var fab = Addressables.LoadAssetAsync<Sprite>(path).WaitForCompletion();
                byte[] bytes = ByteReader.readbytes(png);
                fab.texture.LoadImage(bytes);
            }
            catch (Exception e)
            {
                //Debug.Log($"Couldn't replace sprite: {e}");
            }
        }
        public static void ReplaceTexture2D(string path, string png)
        {
            try
            {
                var fab = Addressables.LoadAssetAsync<Texture2D>(path).WaitForCompletion();
                byte[] bytes = ByteReader.readbytes(png);
                fab.LoadImage(bytes);
            }
            catch (Exception e)
            {
                //Debug.Log($"Couldn't replace sprite: {e}");
            }
        }
    }
    //Choice (Difficulty.Easy)
    //Choice (Difficulty.Normal)
    //Choice (Difficulty.Hard)
}
