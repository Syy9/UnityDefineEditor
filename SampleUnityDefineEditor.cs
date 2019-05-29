using System.Collections;
using System.Collections.Generic;
using Syy.Tools.DefineEditor;
using UnityEditor;
using UnityEngine;

namespace Syy.Tools.Samples
{
    public static class SampleUnityDefineEditor
    {
        [MenuItem("Window/UnityDefineEditor")]
        public static void Open()
        {
            var builder = UnityDefineEditor.Open();

            var DEBUG_MODE = new Define("DEBUG_MODE", "Enable Debug mode");
            var ENABLE_LOG = new Define("ENABLE_LOG", "Enable log");

            var develop = builder.CreateDefineGroup("Develop", "Develop Mode");
            develop.RegisterDefine(DEBUG_MODE);
            develop.RegisterDefine(ENABLE_LOG);

            var logOnly = builder.CreateDefineGroup("Log Only", "Log Only Mode");
            logOnly.RegisterDefine(ENABLE_LOG);

            builder.CreateDefineGroup("Release", "Game Release Mode. No Define.");
        }
    }
}
