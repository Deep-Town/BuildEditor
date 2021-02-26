using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System;
using static DeepTown.GlobalData;
using DeepTown.Utils;
using DeepTown.Editor;

namespace DeepTown
{
    public class PlatformManager
    {
        public enum Platform { Windows, Android, Quest, IOS, Undefined };
        
        /// <summary>
        /// Get Current Platform from Build Data
        /// </summary>
        /// <returns></returns>
        public static Platform GetPlatform()
        {
            string s = File.ReadAllText(FileUtils.GetStreammingAssetsPath("build-data.json"));
            var platformData = JsonUtility.FromJson<BuildData>(s);
            string currentPlatform = platformData.platform;
            if (currentPlatform == "Windows")
            {
                activePlatform = Platform.Windows;
                return Platform.Windows;
            }
            else if (currentPlatform == "Android")
            {
                activePlatform = Platform.Android;
                return Platform.Android;
            }
            else if (currentPlatform == "Quest")
            {
                activePlatform = Platform.Quest;
                return Platform.Quest;
            }
            else if (currentPlatform == "IOS")
            {
                activePlatform = Platform.IOS;
                return Platform.IOS;
            }
            else
            {
                activePlatform = Platform.Undefined;
                return Platform.Undefined;
            }
        }

        /// <summary>
        /// Get Platform from Json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Platform GetPlatform(string json)
        {
            var platformData = JsonUtility.FromJson<BuildData>(json);
            string currentPlatform = platformData.platform;
            if (currentPlatform == "Windows")
            {
                activePlatform = Platform.Windows;
                return Platform.Windows;
            }
            else if (currentPlatform == "Android")
            {
                activePlatform = Platform.Android;
                return Platform.Android;
            }
            else if (currentPlatform == "Quest")
            {
                activePlatform = Platform.Quest;
                return Platform.Quest;
            }
            else if (currentPlatform == "IOS")
            {
                activePlatform = Platform.IOS;
                return Platform.IOS;
            }
            else
            {
                activePlatform = Platform.Undefined;
                return Platform.Undefined;
            }
        }

        /// <summary>
        /// Save file to build-data.json
        /// </summary>
        /// <param name="activePlatform"></param>
        public static void SavePlatform(Platform activePlatform)
        {
            if (File.Exists(Application.streamingAssetsPath + "/build-data.json"))
            {
                File.WriteAllText(Application.streamingAssetsPath + "/build-data.json", string.Empty);
            }
            BuildData data = new BuildData(activePlatform.ToString());
            File.WriteAllText(Application.streamingAssetsPath + "/build-data.json", JsonUtility.ToJson(data, true));
        }
    }
}