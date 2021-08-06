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

namespace LostAPI
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class LostAPIMod : BaseUnityPlugin
    {
        public const string pluginGuid = "whereami.lostAPI.mod";
        public const string pluginName = "Lost API";
        public const string pluginVersion = "0.1";

        public void Awake()
        {
            Logger.LogInfo("LostAPI initializing");
            var harmony = new Harmony("testPatch");
            harmony.PatchAll();
        }
    }

    
}
