using System.IO;
using System;
using UnityEditor;
using UnityEngine;

namespace OVAWORKT.MonoScriptTools {
    [InitializeOnLoad]
    public static class MonoScriptTemplateManager {
        #region Defaults

        private const string MONOBEHAVIOUR = @"{
    ""FileName"": ""MonoBehaviour"",
    ""MatchRegex"": """",
    ""NameRegex"": """",
    ""IconPath"": ""cs Script Icon"",
    ""Template"": ""using UnityEngine;\n\nnamespace #NAMESPACE# {\n    public sealed class #CLASS# : MonoBehaviour {\n        #CURSOR#\n    }\n}""
}";

        private const string SCRIPTABLE_OBJECT = @"{
    ""FileName"": ""ScriptableObject"",
    ""MatchRegex"": ""^*[a-zA-Z](SO)$"",
    ""NameRegex"": ""SO"",
    ""IconPath"": ""d_ScriptableObject Icon"",
    ""Template"": ""using UnityEngine;\n\nnamespace #NAMESPACE# {\n    [Icon(\""#ICON#\"")]\n    [CreateAssetMenu(fileName = \""#CLASSNAME#\"", menuName = \""#CLASSNAME#\"")]\n    public sealed class #CLASS# : ScriptableObject {\n        #CURSOR#\n    }\n}""
}";

        #endregion

        public static readonly string TemplatePath;
        public static readonly (string matchRegex, MonoScriptTemplate template)[] Templates;

        static MonoScriptTemplateManager() {
            TemplatePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            TemplatePath = Path.Combine(TemplatePath, "Unity", "ovaworkt-templates");
            if (!Directory.Exists(TemplatePath)) {
                Directory.CreateDirectory(TemplatePath);
                if (!File.Exists(Path.Combine(TemplatePath, "MonoBehaviour.json"))) File.WriteAllText(Path.Combine(TemplatePath, "MonoBehaviour.json"), MONOBEHAVIOUR);
                if (!File.Exists(Path.Combine(TemplatePath, "ScriptableObject.json"))) File.WriteAllText(Path.Combine(TemplatePath, "ScriptableObject.json"), SCRIPTABLE_OBJECT);
            }

            string[] filePaths = Directory.GetFiles(TemplatePath, "*.json");
            Templates = new (string, MonoScriptTemplate)[filePaths.Length];
            for (int i = 0; i < filePaths.Length; i++) {
                MonoScriptTemplate template = Load(filePaths[i]);
                Templates[i] = (template.MatchRegex ?? string.Empty, template);
            }
        }

        public static void Save(MonoScriptTemplate template) {
            string templateJson = JsonUtility.ToJson(template);
            File.WriteAllText(Path.Combine(TemplatePath, template.FileName + ".json"), templateJson);
        }

        public static MonoScriptTemplate Load(string filePath) {
            string templateJson = File.ReadAllText(filePath);
            return JsonUtility.FromJson<MonoScriptTemplate>(templateJson);
        }
    }
}