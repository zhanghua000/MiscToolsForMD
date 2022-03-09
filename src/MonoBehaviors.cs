﻿using Assets.Scripts.Database;
using Assets.Scripts.GameCore.HostComponent;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiscToolsForMD
{
    public class Indicator : MonoBehaviour
    {
        private readonly Dictionary<string, string> keyDisplayNames = Defines.keyDisplayNames;
        private Rect windowRect = new(MiscToolsForMDMod.config.x, MiscToolsForMDMod.config.y, MiscToolsForMDMod.config.width, MiscToolsForMDMod.config.height);
        private string accuracyText, lyricContent;
        private List<string> workingKeys;
        private Dictionary<string, uint> counters;
        private Rect lyricWindowRect = new(MiscToolsForMDMod.config.lyric_x, MiscToolsForMDMod.config.lyric_y, MiscToolsForMDMod.config.lyric_width, MiscToolsForMDMod.config.lyric_height);

        private readonly GUIStyle accuracyStyle = new()
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 48
        };

        private readonly GUIStyle keyStyle = new()
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24
        };

        public int actualWeight = 0;
        public int targetWeight = 0;
        private List<Lyric> lyrics;

        public Indicator(IntPtr intPtr) : base(intPtr)
        {
        }

        public void OnGUI()
        {
            if (MiscToolsForMDMod.config.ap_indicator || MiscToolsForMDMod.config.key_indicator)
            {
                windowRect = GUILayout.Window(0, windowRect, (GUI.WindowFunction)IndicatorWindow, "MiscToolsUI", null);
            }
            if (MiscToolsForMDMod.config.lyric)
            {
                lyricWindowRect = GUILayout.Window(1, lyricWindowRect, (GUI.WindowFunction)LyricWindow, "Lyric", null);
            }
        }

        public void Start()
        {
            bool needUpdateConfig = false;
            if (MiscToolsForMDMod.config.ap_indicator || MiscToolsForMDMod.config.key_indicator)
            {
                if (MiscToolsForMDMod.config.x == -1)
                {
                    MiscToolsForMDMod.config.x = (Screen.width - MiscToolsForMDMod.config.width) / 2;
                    needUpdateConfig = true;
                }
                if (MiscToolsForMDMod.config.y == -1)
                {
                    MiscToolsForMDMod.config.y = 20;
                    needUpdateConfig |= true;
                }
                accuracyText = "Accuracy: " + 1.ToString("P");
                accuracyStyle.normal.textColor = Defines.apColor;
                workingKeys = MiscToolsForMDMod.GetControlKeys();
                counters = new Dictionary<string, uint>();
                if (workingKeys.Count >= 3 && workingKeys.Count <= 9)
                {
                    foreach (string key in workingKeys)
                    {
                        counters.Add(key, 0);
                    }
                }
                else
                {
                    MiscToolsForMDMod.instance.LoggerInstance.Error("Unexcepted Keys List.");
                }
                windowRect = new Rect(MiscToolsForMDMod.config.x, MiscToolsForMDMod.config.y, MiscToolsForMDMod.config.width, MiscToolsForMDMod.config.height);
                actualWeight = 0;
                targetWeight = 0;
            }
            if (MiscToolsForMDMod.config.lyric)
            {
                if (MiscToolsForMDMod.config.lyric_x == -1)
                {
                    MiscToolsForMDMod.config.lyric_x = (Screen.width - MiscToolsForMDMod.config.lyric_width) / 2;
                    needUpdateConfig = true;
                }
                if (MiscToolsForMDMod.config.lyric_y == -1)
                {
                    MiscToolsForMDMod.config.lyric_y = Screen.height - MiscToolsForMDMod.config.lyric_height - 100;
                    needUpdateConfig = true;
                }
                // See SetSelectedMusicNameTxt
                string musicName, musicAuthor;
                if (DataHelper.selectedAlbumUid != "collection" || DataHelper.selectedMusicIndex < 0)
                {
                    musicName = Singleton<ConfigManager>.instance.GetConfigStringValue(DataHelper.selectedAlbumName, "uid", "name", DataHelper.selectedMusicUidFromInfoList);
                    musicAuthor = Singleton<ConfigManager>.instance.GetConfigStringValue(DataHelper.selectedAlbumName, "uid", "author", DataHelper.selectedMusicUidFromInfoList);
                }
                else if (DataHelper.collections.Count == 0 || DataHelper.collections.Count < DataHelper.selectedMusicIndex)
                {
                    musicName = "?????";
                    musicAuthor = "???";
                }
                else
                {
                    musicName = Singleton<ConfigManager>.instance.GetConfigStringValue(DataHelper.selectedAlbumName, "uid", "name", DataHelper.collections[DataHelper.selectedMusicIndex]);
                    musicAuthor = Singleton<ConfigManager>.instance.GetConfigStringValue(DataHelper.selectedAlbumName, "uid", "author", DataHelper.collections[DataHelper.selectedMusicIndex]);
                }

                MiscToolsForMDMod.instance.Log("Song name: " + musicName + "; author: " + musicAuthor);
                bool successGetLyric = false;
                foreach (ILyricSource source in MiscToolsForMDMod.instance.lyricSources)
                {
                    try
                    {
                        lyrics = source.GetLyrics(musicName, musicAuthor);
                        successGetLyric = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        MiscToolsForMDMod.instance.LoggerInstance.Error(ex.ToString(), "Failed to get lyric through source " + source.Name);
                    }
                }
                if (!successGetLyric || lyrics.Count == 0)
                {
                    MiscToolsForMDMod.instance.LoggerInstance.Error("No available lyric.");
                }
                lyricWindowRect = new Rect(MiscToolsForMDMod.config.lyric_x, MiscToolsForMDMod.config.lyric_y, MiscToolsForMDMod.config.lyric_width, MiscToolsForMDMod.config.lyric_height);
                lyricContent = "";
            }
            if (needUpdateConfig)
            {
                MiscToolsForMDMod.instance.SaveConfig();
            }
        }

        public void Update()
        {
            if (MiscToolsForMDMod.config.key_indicator)
            {
                foreach (string key in workingKeys)
                {
                    if (Input.GetKeyDown(GetKeyCodeByName(key)))
                    {
                        AddKeyCount(key);
                    }
                }
            }
            if (MiscToolsForMDMod.config.lyric)
            {
                float time = Singleton<FormulaBase.StageBattleComponent>.instance.timeFromMusicStart;
                lyricContent = Lyric.GetLyricByTime(lyrics, time).content;
            }
        }

        public void OnDestroy()
        {
            MiscToolsForMDMod.indicator = null;
        }

        public void IndicatorWindow(int windowID)
        {
            GUILayout.BeginVertical(null);
            if (MiscToolsForMDMod.config.ap_indicator)
            {
                GUILayout.Label(accuracyText, accuracyStyle, null);
            }
            if (MiscToolsForMDMod.config.key_indicator)
            {
                GUILayout.BeginHorizontal(null);
                foreach (string key in workingKeys)
                {
                    if (key != null)
                    {
                        string keyDisplayName;
                        if (keyDisplayNames.ContainsKey(key))
                        {
                            keyDisplayName = keyDisplayNames[key];
                        }
                        else
                        {
                            keyDisplayName = key;
                        }
                        GUILayout.Label(keyDisplayName + "\n\n" + counters[key], keyStyle, null);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        public void LyricWindow(int windowId)
        {
            GUIStyle lyricStyle = new()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 48
            };
            GUILayout.BeginVertical(null);
            GUILayout.Label(lyricContent, lyricStyle, null);
            GUILayout.EndVertical();
        }

        public void UpdateAccuracy()
        {
            if (targetWeight > 0)
            {
                float unit = 0.0001f;
                float trueAcc = actualWeight * 1.0f / targetWeight;
                float acc = Mathf.RoundToInt(trueAcc / unit) * unit;
                // See Assets.Scripts.GameCore.HostComponent.TaskStageTarget.GetAccuracy
                if (trueAcc < acc && (acc == 0.6f || acc == 0.7f || acc == 0.8f || acc == 0.9f || acc == 1.0f))
                {
                    acc -= unit;
                }
                accuracyText = "Accuracy: " + acc.ToString("P");
                if (acc < 1f)
                {
                    if (Singleton<TaskStageTarget>.instance.GetMiss() > 0)
                    {
                        accuracyStyle.normal.textColor = Defines.missColor;
                    }
                    else
                    {
                        accuracyStyle.normal.textColor = Defines.greatColor;
                    }
                }
            }
        }

        private void AddKeyCount(string actKey, uint num = 1)
        {
            foreach (string workingKey in workingKeys)
            {
                if (workingKey == actKey)
                {
                    counters[workingKey] += num;
                }
            }
        }

        private KeyCode GetKeyCodeByName(string name)
        {
            foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {
                if (code.ToString() == name)
                {
                    return code;
                }
            }
            return KeyCode.None;
        }
    }
}
