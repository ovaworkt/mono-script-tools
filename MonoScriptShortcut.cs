using System.Text.RegularExpressions;
using System.IO;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.ShortcutManagement;
using UnityEditor;
using UnityEngine;

namespace OVAWORKT.MonoScriptTools {
    public static class MonoScriptShortcut {
        private static Texture2D _scriptIcon;

        private sealed class DoCreateMonoScript : EndNameEditAction {
            public readonly Texture2D Icon;

            public DoCreateMonoScript() => Icon = _scriptIcon;
            public DoCreateMonoScript(Texture2D icon) => Icon = icon;

            public override void Action(int instanceId, string filePath, string resourceFile) {
                MonoScriptTemplate monoScriptTemplate = MonoScriptTemplateManager.Templates[0].template;
                string scriptName = Path.GetFileNameWithoutExtension(filePath);
                for (int j = MonoScriptTemplateManager.Templates.Length - 1; j >= 0; j--) {
                    (string matchRegex, MonoScriptTemplate template) template = MonoScriptTemplateManager.Templates[j];
                    Regex regex = new Regex(template.matchRegex);
                    if (regex.IsMatch(scriptName)) {
                        monoScriptTemplate = template.template;
                        scriptName = Regex.Replace(scriptName, template.template.NameRegex, string.Empty);
                        break;
                    }
                }

                string code = MonoScriptWriter.Write(scriptName, MonoScriptWriter.GetNamespace(), monoScriptTemplate, string.Empty, out int lineNumber);
                filePath = Path.Combine(ProjectUtility.GetActiveFolderPath(), scriptName + SCRIPT_EXTENSION);

                File.WriteAllText(filePath, code);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                MonoScript monoScript = AssetDatabase.LoadAssetAtPath(filePath, typeof(MonoScript)) as MonoScript;
                AssetDatabase.OpenAsset(monoScript, lineNumber);
            }
        }

        private const string SCRIPT_EXTENSION = ".cs";

        [Shortcut("Create Script", KeyCode.C, ShortcutModifiers.Alt, displayName = "Create Script")]
        private static void CreateMonoScript() {
            if (_scriptIcon == null) _scriptIcon = EditorGUIUtility.IconContent("d_cs Script Icon").image as Texture2D;
            string fileName = "NewBehaviourScript", filePath = ProjectUtility.GetActiveFolderPath() + Path.DirectorySeparatorChar + fileName + SCRIPT_EXTENSION;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateMonoScript>(), filePath, _scriptIcon, null);
        }
    }
}