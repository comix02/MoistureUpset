using MoistureUpset.NetMessages;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MoistureUpset.Fixers
{
    class GolemRandomizer : MonoBehaviour
    {
        internal static List<Material> materials = new List<Material>();
        internal static List<Mesh> meshes = new List<Mesh>();
        internal int num;
        private GameObject shoop;
        private SkinnedMeshRenderer bodyRenderer;
        bool check = true;
        bool needToSetup = true;
        int reinforceFrames = 0;
        void Start()
        {
            if (NetworkServer.active)
            {
                if (materials.Count > 0)
                {
                    num = UnityEngine.Random.Range(0, materials.Count);
                }
                Setup();
            }
            else
            {
                var identity = gameObject.GetComponent<NetworkIdentity>();
                if (identity)
                {
                    new RequestRoblox(identity.netId).Send(R2API.Networking.NetworkDestination.Server);
                }
            }
        }
        internal void Setup()
        {
            if (!needToSetup)
            {
                return;
            }

            if (materials.Count == 0 || meshes.Count == 0)
            {
                return;
            }

            if (num < 0 || num >= materials.Count || num >= meshes.Count)
            {
                num = 0;
            }

            var modelLocator = GetComponent<ModelLocator>();
            if (!modelLocator || !modelLocator.modelTransform)
            {
                return;
            }

            var modelTransform = modelLocator.modelTransform.gameObject;
            var characterModel = modelTransform.GetComponentInChildren<CharacterModel>();
            if (!characterModel)
            {
                return;
            }

            bodyRenderer = modelTransform.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (!bodyRenderer)
            {
                return;
            }

            Material chosenMaterial = materials[num];
            Mesh chosenMesh = meshes[num];
            if (!chosenMaterial || !chosenMesh)
            {
                return;
            }

            bodyRenderer.sharedMesh = chosenMesh;
            var sharedMaterials = bodyRenderer.sharedMaterials;
            for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
            {
                sharedMaterials[materialIndex] = chosenMaterial;
            }
            bodyRenderer.sharedMaterials = sharedMaterials;

            if (characterModel.baseRendererInfos != null && characterModel.baseRendererInfos.Length > 0)
            {
                bool applied = false;
                for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
                {
                    if (characterModel.baseRendererInfos[i].renderer == bodyRenderer)
                    {
                        characterModel.baseRendererInfos[i].defaultMaterial = chosenMaterial;
                        applied = true;
                        break;
                    }
                }
                if (!applied)
                {
                    characterModel.baseRendererInfos[0].defaultMaterial = chosenMaterial;
                }
            }

            if (!shoop)
            {
                var head = characterModel.transform.Find("GolemArmature/ROOT/base/stomach/chest/head");
                if (head)
                {
                    shoop = GameObject.Instantiate(Assets.Load<GameObject>("@MoistureUpset_noob:assets/robloxcharacters/shoop.prefab"));
                    shoop.transform.parent = head;
                    shoop.transform.localScale = Vector3.one;
                    if (num == 5)
                    {
                        shoop.transform.localPosition = new Vector3(0, .39f, .51f);
                    }
                    else
                    {
                        shoop.transform.localPosition = new Vector3(0, .39f, .11f);
                    }
                    shoop.transform.localEulerAngles = new Vector3(0, 270, 0);
                    shoop.SetActive(false);
                }
            }

            ApplyShoopVisuals();
            reinforceFrames = 30;
            needToSetup = false;
        }
        void Update()
        {
            if (needToSetup)
            {
                Setup();
                return;
            }

            if (check)
            {
                var modelLocator = GetComponentInChildren<ModelLocator>();
                if (!modelLocator || !modelLocator.modelTransform || num < 0 || num >= materials.Count || num >= meshes.Count)
                {
                    return;
                }

                var characterModel = modelLocator.modelTransform.gameObject.GetComponentInChildren<CharacterModel>();
                if (!characterModel || characterModel.baseRendererInfos == null || characterModel.baseRendererInfos.Length == 0)
                {
                    return;
                }

                if (!bodyRenderer)
                {
                    bodyRenderer = modelLocator.modelTransform.GetComponentInChildren<SkinnedMeshRenderer>(true);
                }
                if (!bodyRenderer)
                {
                    return;
                }

                var body = GetComponent<CharacterBody>();
                if (body && body.healthComponent && !body.healthComponent.alive)
                {
                    check = false;
                    return;
                }

                if (reinforceFrames <= 0)
                {
                    check = false;
                    return;
                }

                Material chosenMaterial = materials[num];
                Mesh chosenMesh = meshes[num];

                if (bodyRenderer.sharedMesh != chosenMesh)
                {
                    bodyRenderer.sharedMesh = chosenMesh;
                }

                bool matchedRenderer = false;
                for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
                {
                    if (characterModel.baseRendererInfos[i].renderer == bodyRenderer)
                    {
                        matchedRenderer = true;
                        if (characterModel.baseRendererInfos[i].defaultMaterial != chosenMaterial)
                        {
                            characterModel.baseRendererInfos[i].defaultMaterial = chosenMaterial;
                        }
                        break;
                    }
                }
                if (!matchedRenderer && characterModel.baseRendererInfos[0].defaultMaterial != chosenMaterial)
                {
                    characterModel.baseRendererInfos[0].defaultMaterial = chosenMaterial;
                }

                reinforceFrames--;
                if (reinforceFrames <= 0)
                {
                    check = false;
                }
            }
        }
        private void ApplyShoopVisuals()
        {
            if (!shoop)
            {
                return;
            }

            Texture shoopTexture = TryLoadTexture("@MoistureUpset_noob:assets/robloxcharacters/shoop.png");
            if (!shoopTexture)
            {
                return;
            }

            var renderers = shoop.GetComponentsInChildren<Renderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                if (!renderer)
                {
                    continue;
                }

                var materials = renderer.materials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    var material = materials[materialIndex];
                    if (!material)
                    {
                        continue;
                    }

                    material.color = Color.white;
                    material.mainTexture = shoopTexture;
                    var texturePropertyNames = material.GetTexturePropertyNames();
                    for (int textureIndex = 0; textureIndex < texturePropertyNames.Length; textureIndex++)
                    {
                        material.SetTexture(texturePropertyNames[textureIndex], shoopTexture);
                    }
                }
                renderer.materials = materials;
            }
        }
        private Texture TryLoadTexture(string assetPath)
        {
            try
            {
                return Assets.Load<Texture>(assetPath);
            }
            catch (Exception)
            {
                return null;
            }
        }
        IEnumerator removeShaderTing()
        {
            yield return new WaitForSeconds(4f);
            Transform transform = GetComponentInChildren<ModelLocator>().modelTransform;
            if (transform)
            {
                transform.GetComponent<PrintController>().InvokeMethod("SetPrintThreshold", 12);
            }
            //string[] s = { "DITHER" };
            //GetComponentInChildren<ModelLocator>().modelTransform.gameObject.GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial.shaderKeywords = s;
        }
        internal void Charge()
        {
            if (shoop)
            {
                ApplyShoopVisuals();
                shoop.SetActive(true);
            }
        }
        internal void Shoot()
        {
            if (shoop)
            {
                shoop.SetActive(false);
            }
        }
    }
}
