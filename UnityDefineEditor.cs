using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Syy.Tools.DefineEditor
{
    public class UnityDefineEditor : EditorWindow
    {
        public static DefineBuilder Open()
        {
            var window = GetWindow<UnityDefineEditor>("UnityDefineEditor");
            window._builder.Clear();
            return window._builder;
        }

        [SerializeField]
        DefineBuilder _builder = new DefineBuilder();
        Vector2 _scroll;
        bool _waitCompile;

        void OnGUI()
        {
            EditorGUILayout.Space();
            var currentDefineStr = GetCurrentDefine();
            if (string.IsNullOrEmpty(currentDefineStr))
            {
                currentDefineStr = "Nothing";
            }
            EditorGUILayout.LabelField("Current Define : " + currentDefineStr);

            EditorGUILayout.Space();
            using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll))
            {
                _scroll = scroll.scrollPosition;

                foreach (var group in _builder.Data.OrderBy(value => value.Name))
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var isCurrentDefine = currentDefineStr == group.DefineStr;
                            if (isCurrentDefine)
                            {
                                EditorGUILayout.LabelField("■ " + group.Name + " (Current)");
                            }
                            else
                            {
                                EditorGUILayout.LabelField("■ " + group.Name);
                            }
                            if (GUILayout.Button("Change Define"))
                            {
                                _waitCompile = ChangeDefine(group.Defines);
                                if (_waitCompile)
                                {
                                    ShowNotification(new GUIContent("Compiling"));
                                }
                            }
                        }

                        EditorGUILayout.LabelField("Description : " + group.Description);
                        for (int i = 0; i < group.Defines.Count; i++)
                        {
                            EditorGUILayout.LabelField($"Define{i + 1} : " + group.Defines[i].Name);
                        }

                        EditorGUILayout.Space();
                    }
                }

                if (GUILayout.Button("Clear Define"))
                {
                    _waitCompile = ChangeDefine(new DefineBuilder.Define[0]);
                    if (_waitCompile)
                    {
                        ShowNotification(new GUIContent("Compiling"));
                    }
                }
            }

            if (_waitCompile)
            {
                if (!EditorApplication.isCompiling)
                {
                    _waitCompile = false;
                    RemoveNotification();
                    AssetDatabase.SaveAssets();
                    EditorUtility.DisplayDialog("Complete", "Complete Change Define!", "Close");
                }
            }
        }

        bool ChangeDefine(IEnumerable<DefineBuilder.Define> defines)
        {
            var defineStr = string.Join(";", defines.Select(value => value.Name));
            var message = string.IsNullOrEmpty(defineStr) ? "No Define" : defineStr;
            var currentDefine = GetCurrentDefine();
            if (defineStr == currentDefine)
            {
                EditorUtility.DisplayDialog($"It is current define", "Define : " + message, "OK");
                return false;
            }

            if (EditorUtility.DisplayDialog("Change Define?", "Define : " + message, "OK", "Cancel"))
            {
                var target = EditorUserBuildSettings.activeBuildTarget;
                var group = BuildPipeline.GetBuildTargetGroup(target);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defineStr);
                return true;
            }

            return false;
        }

        string GetCurrentDefine()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        }

        [Serializable]
        public class DefineBuilder : IDefineBuilder
        {
            public List<DefineGroup> Data = new List<DefineGroup>();

            public IDefineGroup CreateDefineGroup(string name, string description)
            {
                var group = new DefineGroup() { Name = name, Description = description };
                Data.Add(group);
                return group;
            }

            public void Clear()
            {
                Data.Clear();
            }

            [Serializable]
            public class DefineGroup : IDefineGroup
            {
                public string Name;
                public string Description;

                public List<Define> Defines = new List<Define>();
                public string DefineStr { get { return string.Join(";", Defines.Select(value => value.Name)); } }

                public void RegisterDefine(string name)
                {
                    Defines.Add(new Define() { Name = name } );
                }
            }

            [Serializable]
            public class Define
            {
                public string Name;
            }
        }
    }

    public interface IDefineBuilder
    {
        IDefineGroup CreateDefineGroup(string name, string comment);
    }

    public interface IDefineGroup
    {
        void RegisterDefine(string name);
    }

}
