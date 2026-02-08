using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoistureUpset
{
    public static class AddressableLoader
    {
        public static T LoadAsset<T>(string path, string context = null) where T : Object
        {
            var asset = Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
            if (!asset)
            {
                if (string.IsNullOrWhiteSpace(context))
                {
                    DebugClass.Log($"[Addressables] Missing asset at '{path}'.");
                }
                else
                {
                    DebugClass.Log($"[Addressables] Missing asset at '{path}' ({context}).");
                }
            }

            return asset;
        }
    }
}
