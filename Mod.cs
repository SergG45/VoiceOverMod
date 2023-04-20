using MelonLoader;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace VoiceOverMod
{
    public class Mod : MelonMod
    {
        public static MelonPreferences_Category prefCategory;
        public static MelonPreferences_Entry<float> volume;
        public static bool MelonInitialize = false;
        public static bool SceneInitialize = false;
        public static string modPath;
        public static List<LinesClass> lines;
        public static AudioClip currentClip = null;

        public override void OnInitializeMelon()
        {           
            XDocument doc = new XDocument();
            try { doc = XDocument.Load(@"Mods\Story.xml"); }
            catch (Exception ex)
            {
                Melon<Mod>.Logger.Msg("[ERROR] Can't load Story.xml in Mods folder! Exception text:");
                Melon<Mod>.Logger.Msg(ex.Message);
                return; 
            }
            try
            {
                var query = from data in doc.Descendants("Line")
                            select new LinesClass
                            {
                                Index = (string)data.Attribute("Index"),
                                Character = (string)data.Attribute("Character"),
                                Text = (string)data.Attribute("Text"),
                            };
                lines = query.ToList();

                modPath = Application.dataPath.Substring(0, Application.dataPath.Length - 38)
                    + @"/Mods/Audio";

                prefCategory = MelonPreferences.CreateCategory("VoiceOverMod");
                volume = prefCategory.CreateEntry<float>("Volume", 1.5f);
            }
            catch (Exception ex)
            {
                Melon<Mod>.Logger.Msg("[ERROR] Exception while initializing:");
                Melon<Mod>.Logger.Msg(ex.Message);
                return;
            }
            MelonInitialize = true;
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            GameObject gameobj = new GameObject("VOMobject");
            gameobj.AddComponent<AudioSource>();

            GameObject guiobj = new GameObject("VOMgui");
            ModGUI gui = guiobj.AddComponent<ModGUI>();
            gui.enabled = false;
            SceneInitialize = true;
        }

        [HarmonyPatch(typeof(DialogView), "UpdateDialog", new Type[] { typeof(string), typeof(string) })]
        private static class Patch
        {
            private static void Prefix() { }
            private static void Postfix()
            {
                if (MelonInitialize && volume.Value > 0)
                {
                    DialogView.DialogState DialogState = DialogView.Instance.GetState();
                    if (DialogState.currentSpeaker != "")
                    PlayClip(DialogState.currentSpeaker, DialogState.currentDialog);
                }                
            }
        }

        [HarmonyPatch(typeof(SettingsScreen), "OnEnable")]
        private static class GUIopen
        {
            private static void Prefix() { }
            private static void Postfix()
            {
                if (SceneInitialize)
                {
                    GameObject guiobj = GameObject.Find("VOMgui");
                    ModGUI gui = guiobj.GetComponent<ModGUI>();
                    gui.enabled = true;

                    // Disabling background close button
                    GameObject SettingScreen = GameObject.Find("SettingScreen");
                    GameObject OverlayScreenBase = SettingScreen.transform.GetChild(0).gameObject;
                    GameObject Darken = OverlayScreenBase.transform.GetChild(0).gameObject;
                    Button Button = Darken.GetComponent<Button>();
                    Button.enabled = false;
                }
            }
        }

        [HarmonyPatch(typeof(SettingsScreen), "OnDisable")]
        private static class GUIclose
        {
            private static void Prefix() { }
            private static void Postfix()
            {
                if (SceneInitialize)
                {
                    GameObject guiobj = GameObject.Find("VOMgui");
                    ModGUI gui = guiobj.GetComponent<ModGUI>();
                    gui.enabled = false;
                    MelonPreferences.Save();
                }
            }
        }

        public static void PlayClip(string speaker, string dialog)
        {
            string text = Regex.Replace(dialog, "<.+?>", "");
            string name = StoryManager.Instance.GetPlayerName();

            text = text.Replace("\\t", "");
            text = text.Replace("\\", "");
            text = text.Replace("\"", "");
            text = text.Replace("^", "");
            text = text.Replace("…", "...");
            if (text.EndsWith(" ,"))
                text = text.Substring(0, text.Length - 2);
            if (text.EndsWith(","))
                text = text.Substring(0, text.Length - 1);

            LinesClass line = null;

            if (!text.Contains(name))
                line = lines.FirstOrDefault(l => (l.Character == speaker) && (l.Text == text.Trim()));
            else
                line = lines.FindAll(l => (l.Text.Contains("PLAYER_NAME")) || (l.Text.Contains(name)))
                    .OrderBy(l => LevenshteinDistance.Compute(text.Trim(), l.Text)).First();

            if (line != null)
            {
                AudioClip clip = GetAudioClip(line.Index);
                if (clip == null) return;
                GameObject gameobj = GameObject.Find("VOMobject");
                if (gameobj = null) 
                {
                    GameObject newgameobj = new GameObject("VOMobject");
                    AudioSource newaudiosource = gameobj.AddComponent<AudioSource>();
                    while (!newaudiosource.isActiveAndEnabled) { }
                }
                if (currentClip != null) UnityEngine.Object.Destroy(currentClip);
                AudioSource audiosource = GameObject.Find("VOMobject").GetComponent<AudioSource>();
                audiosource.Stop();
                audiosource.clip = clip;
                audiosource.volume = volume.Value;
                audiosource?.Play();
                currentClip = clip;
            }
            else
            {
                Melon<Mod>.Logger.Msg("[ERROR] Can't find line of dialogue in Story.xml!");
                Melon<Mod>.Logger.Msg("Speaker: " + speaker);
                Melon<Mod>.Logger.Msg("Text: " + text);
            }
        }

        public static AudioClip GetAudioClip(string Index)
        {
            DirectoryInfo d = new DirectoryInfo(modPath);
            FileInfo File = d.GetFiles().Where(p => p.Name.StartsWith(Index)).FirstOrDefault();
            if (File == null) return null; 

            AudioType audiotype = GetAudioType(File.Name);
            if (audiotype == AudioType.UNKNOWN) 
            {
                Melon<Mod>.Logger.Msg("[ERROR] Unknown file format for [" + Index + "] line.");
                return null; 
            }

            UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(
                "file://" + "/Mods/Audio/" + File.Name, audiotype);
            webRequest.SendWebRequest();

            while (!webRequest.isDone) { };

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Melon<Mod>.Logger.Msg("[ERROR] Unable to get audio filefor [" + Index + "] line. Error text of web request:");
                Melon<Mod>.Logger.Msg(webRequest.error);
                return null;
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);
                clip.name = File.Name;
                return clip;
            }
        }

        public static AudioType GetAudioType(string fileName)
        {
            if (fileName.EndsWith("mp3"))
            {
                return AudioType.MPEG;
            }
            if (fileName.EndsWith("wav"))
            {
                return AudioType.WAV;
            }
            if (fileName.EndsWith("aiff"))
            {
                return AudioType.AIFF;
            }
            return AudioType.UNKNOWN;
        }
        public class ModGUI : MonoBehaviour
        {
            void OnGUI()
            {
                GUILayout.BeginArea(new Rect(0, 0, 100, 100));
                GUILayout.BeginVertical();
                GUILayout.Box("Voice Volume");
                volume.Value = GUILayout.HorizontalSlider(volume.Value, 0.0f, 2.0f);
                GUILayout.EndVertical();
                GUILayout.EndArea();

                AudioSource audiosource = GameObject.Find("VOMobject").GetComponent<AudioSource>();
                audiosource.volume = volume.Value;
            }
        }

        public static class LevenshteinDistance
        {
            /// Used from paparazzo answer on https://stackoverflow.com/a/13793600
            /// <summary>
            /// Compute the distance between two strings.
            /// </summary>
            public static int Compute(string s, string t)
            {
                int n = s.Length;
                int m = t.Length;
                int[,] d = new int[n + 1, m + 1];

                // Step 1
                if (n == 0)
                {
                    return m;
                }

                if (m == 0)
                {
                    return n;
                }

                // Step 2
                for (int i = 0; i <= n; d[i, 0] = i++)
                {
                }

                for (int j = 0; j <= m; d[0, j] = j++)
                {
                }

                // Step 3
                for (int i = 1; i <= n; i++)
                {
                    //Step 4
                    for (int j = 1; j <= m; j++)
                    {
                        // Step 5
                        int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                        // Step 6
                        d[i, j] = Math.Min(
                            Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                            d[i - 1, j - 1] + cost);
                    }
                }
                // Step 7
                return d[n, m];
            }
        }
    }

    public class LinesClass
    {
        public string Index { get; set; }
        public string Character { get; set; }
        public string Text { get; set; }
    }
}
