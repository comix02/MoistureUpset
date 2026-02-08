using System;
using System.Collections;
using System.Reflection;
using R2API.Utils;
using RoR2;
using UnityEngine;

namespace MoistureUpset.Skins
{
    public static class AnimationReplacements
    {
        private static readonly string[] NonAnimatingEmotes =
        {
            "firework",
            "debug time",
            "kill stuff",
            "end me",
            "50 golems",
            "getSongName",
            "god",
            "noclip",
            "golemPlains"
        };

        private static Type customEmotesApiType;
        private static object bonemap;
        private static Delegate animChangedDelegate;

        public static void RunAll()
        {
            GameObject manager = new GameObject();
            manager.AddComponent<EffectManager>();
            UnityEngine.Object.DontDestroyOnLoad(manager);

            if (!TryResolveCustomEmotesApi())
            {
                DebugClass.Log("CustomEmotesAPI not found, skipping animation replacements.");
                return;
            }

            for (int i = 0; i < NonAnimatingEmotes.Length; i++)
            {
                TryInvokeStatic("AddNonAnimatingEmote", NonAnimatingEmotes[i]);
            }

            EnemyReplacements.LoadResource("kazotskykick");
            TryAddKazotskyAnimation();
            TrySubscribeAnimChanged();
        }

        private static bool TryResolveCustomEmotesApi()
        {
            if (customEmotesApiType != null)
            {
                return true;
            }

            customEmotesApiType = Type.GetType("EmotesAPI.CustomEmotesAPI, CustomEmotesAPI");
            return customEmotesApiType != null;
        }

        private static void TryAddKazotskyAnimation()
        {
            try
            {
                var startAnimation = Assets.Load<AnimationClip>("@MoistureUpset_kazotskykick:assets/kazotsky kick/Engineer Kazotsky Kick Start.anim");
                var loopAnimation = Assets.Load<AnimationClip>("@MoistureUpset_kazotskykick:assets/kazotsky kick/Engineer Kazotsky Kick Loop.anim");

                MethodInfo addCustomAnimation = null;
                foreach (var candidate in customEmotesApiType.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (candidate.Name == "AddCustomAnimation")
                    {
                        addCustomAnimation = candidate;
                        break;
                    }
                }

                if (addCustomAnimation == null)
                {
                    return;
                }

                var parameters = addCustomAnimation.GetParameters();
                var args = BuildDefaultArgs(parameters);

                if (args.Length > 0)
                {
                    args[0] = startAnimation;
                }
                if (args.Length > 1)
                {
                    args[1] = false;
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (string.Equals(parameters[i].Name, "secondaryAnimation", StringComparison.OrdinalIgnoreCase))
                    {
                        args[i] = loopAnimation;
                    }
                    else if (string.Equals(parameters[i].Name, "syncAnim", StringComparison.OrdinalIgnoreCase))
                    {
                        args[i] = true;
                    }
                }

                addCustomAnimation.Invoke(null, args);
            }
            catch (Exception e)
            {
                DebugClass.Log($"Failed to register kazotsky animation: {e.Message}");
            }
        }

        private static object[] BuildDefaultArgs(ParameterInfo[] parameters)
        {
            var args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].HasDefaultValue)
                {
                    args[i] = parameters[i].DefaultValue;
                    continue;
                }

                if (parameters[i].ParameterType.IsValueType)
                {
                    args[i] = Activator.CreateInstance(parameters[i].ParameterType);
                }
                else
                {
                    args[i] = null;
                }
            }
            return args;
        }

        private static void TrySubscribeAnimChanged()
        {
            try
            {
                var eventInfo = customEmotesApiType.GetEvent("animChanged", BindingFlags.Public | BindingFlags.Static);
                if (eventInfo == null || eventInfo.EventHandlerType == null)
                {
                    return;
                }

                var method = typeof(AnimationReplacements).GetMethod(nameof(CustomEmotesAPI_animChanged), BindingFlags.NonPublic | BindingFlags.Static);
                if (method == null)
                {
                    return;
                }

                animChangedDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, method);
                eventInfo.AddEventHandler(null, animChangedDelegate);
            }
            catch (Exception e)
            {
                DebugClass.Log($"Failed to subscribe to CustomEmotesAPI.animChanged: {e.Message}");
            }
        }

        private static void TryInvokeStatic(string methodName, params object[] args)
        {
            try
            {
                var method = customEmotesApiType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
                method?.Invoke(null, args);
            }
            catch (Exception e)
            {
                DebugClass.Log($"CustomEmotesAPI.{methodName} failed: {e.Message}");
            }
        }

        private static void SubmitLocalCommand(string cmd)
        {
            if (RoR2.Console.instance == null || NetworkUser.readOnlyLocalPlayersList.Count == 0)
            {
                return;
            }

            RoR2.Console.instance.SubmitCmd(NetworkUser.readOnlyLocalPlayersList[0], cmd);
        }

        private static void CustomEmotesAPI_animChanged(string newAnimation, object mapper)
        {
            bonemap = mapper;
            if (EffectManager.instance != null)
            {
                EffectManager.instance.mapper = bonemap;
            }

            if (newAnimation == "firework")
            {
                EffectManager.instance?.LaunchFirework();
            }
            else if (newAnimation == "debug time")
            {
                SubmitLocalCommand("god");
                SubmitLocalCommand("noclip");
                SubmitLocalCommand("no_enemies");
                SubmitLocalCommand("kill_all");
                SubmitLocalCommand("give_money 1000000");
                SubmitLocalCommand("give_item SoldiersSyringe 100");
                SubmitLocalCommand("give_item AlienHead 100");
                SubmitLocalCommand("give_item ShapedGlass 10");
            }
            else if (newAnimation == "kill stuff")
            {
                SubmitLocalCommand("god");
                SubmitLocalCommand("noclip");
                SubmitLocalCommand("give_item SoldiersSyringe 100");
                SubmitLocalCommand("give_item AlienHead 100");
                SubmitLocalCommand("give_item ShapedGlass 10");
            }
            else if (newAnimation == "end me")
            {
                SubmitLocalCommand("give_item ShapedGlass 10");
                SubmitLocalCommand("spawn_ai GolemMaster 10");
            }
            else if (newAnimation == "50 golems")
            {
                SubmitLocalCommand("spawn_ai GolemMaster 50");
            }
            else if (newAnimation == "noclip")
            {
                SubmitLocalCommand("noclip");
            }
            else if (newAnimation == "god")
            {
                SubmitLocalCommand("god");
            }
            else if (newAnimation == "getSongName")
            {
                try
                {
                    DebugClass.Log($"song name: {MoistureUpsetMod.musicController.GetPropertyValue<MusicTrackDef>("currentTrack").cachedName}");
                }
                catch (Exception)
                {
                }
            }
            else if (newAnimation == "golemPlains")
            {
                SubmitLocalCommand("next_stage golemplains");
            }
        }
    }

    public class EffectManager : MonoBehaviour
    {
        public static EffectManager instance;
        internal object mapper;
        internal IEnumerator clapRoutine = null;

        public EffectManager()
        {
            instance = this;
        }

        public void DefaultClap()
        {
            StopDefaultClap();
            clapRoutine = DefaultClapFX();
            StartCoroutine(clapRoutine);
        }

        public void StopDefaultClap()
        {
            if (clapRoutine != null)
            {
                StopCoroutine(clapRoutine);
            }
        }

        public IEnumerator DefaultClapFX()
        {
            yield return new WaitForSeconds(.3f);
            var trans = GetLeftHandTransform();
            if (trans == null)
            {
                yield break;
            }

            var obj = Assets.Load<GameObject>("@MoistureUpset_moisture_animationreplacements:assets/animationreplacements/testeffects/clap.prefab");
            obj.transform.position = trans.position;
            GameObject.Instantiate(obj);
            yield return new WaitForSeconds(2.06666f);
            obj = Assets.Load<GameObject>("@MoistureUpset_moisture_animationreplacements:assets/animationreplacements/testeffects/clap.prefab");
            obj.transform.position = trans.position;
            GameObject.Instantiate(obj);
        }
        public void LaunchFirework()
        {
            var mapperTransform = (mapper as Component)?.transform;
            if (mapperTransform == null)
            {
                return;
            }

            var obj = Assets.Load<GameObject>("@MoistureUpset_moisture_animationreplacements:assets/animationreplacements/testeffects/firework.prefab");
            obj.transform.position = mapperTransform.position;
            if (!obj.GetComponent<Firework>())
            {
                obj.AddComponent<Firework>();
            }
            GameObject.Instantiate(obj);
        }

        private Transform GetLeftHandTransform()
        {
            var mapperComponent = mapper as Component;
            if (mapperComponent == null)
            {
                return null;
            }

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Animator animator = null;
            var mapperType = mapperComponent.GetType();
            var animatorField = mapperType.GetField("a2", flags);
            if (animatorField != null)
            {
                animator = animatorField.GetValue(mapperComponent) as Animator;
            }

            if (animator == null)
            {
                var animatorProperty = mapperType.GetProperty("a2", flags);
                if (animatorProperty != null)
                {
                    animator = animatorProperty.GetValue(mapperComponent, null) as Animator;
                }
            }

            if (animator == null)
            {
                animator = mapperComponent.GetComponent<Animator>();
            }

            return animator != null ? animator.GetBoneTransform(HumanBodyBones.LeftHand) : null;
        }
    }

    public class Firework : MonoBehaviour
    {
        float timer = 0;

        void Update()
        {
            timer += Time.deltaTime;
            if (timer > 6)
            {
                Destroy(gameObject);
            }
            else if (timer > 2)
            {
                var flares = gameObject.transform.Find("flares");
                if (flares != null)
                {
                    flares.gameObject.SetActive(false);
                }

                var joy = gameObject.transform.Find("joy");
                if (joy != null)
                {
                    joy.gameObject.SetActive(true);
                }
            }
            else
            {
                gameObject.transform.position += new Vector3(0, 70 * Time.deltaTime, 0);
            }
        }
    }
}
