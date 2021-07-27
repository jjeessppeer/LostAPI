using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;

using BepInEx;
using HarmonyLib;
using UnityEngine;

using Muse;
//using Lost;

namespace Lost
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class LostAPIMod : BaseUnityPlugin
    {
        public const string pluginGuid = "whereami.lostAPI.mod";
        public const string pluginName = "Lost API";
        public const string pluginVersion = "1.2.3.4";

        public void Awake()
        {
            Logger.LogInfo("hello there\n\n\n\n___");
            Logger.LogInfo(FileLog.logPath);

            FileLog.Log("__START LOG__");
            var harmony = new Harmony("testPatch");
            harmony.PatchAll();
        }
    }

    
}
