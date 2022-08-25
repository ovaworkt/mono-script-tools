using System.Text;
using UnityEditorInternal;
using UnityEditor;

namespace OVAWORKT.MonoScriptTools {
    public static class MonoScriptWriter {
        public static string GetNamespace() {
            string activeFolderPath = ProjectUtility.GetActiveFolderPath();
            string[] guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", new[] { activeFolderPath });
            if (guids.Length == 1) {
                string assemblyPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                AssemblyDefinitionAsset assembly = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assemblyPath);
                return assembly.name;
            } else {
                StringBuilder @namespace = new StringBuilder(EditorSettings.projectGenerationRootNamespace);
                string[] additionalNamespace = activeFolderPath.Replace("\\", " ").Replace("/", " ").Replace("Assets", string.Empty).Split(new[] { '-', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < additionalNamespace.Length; i++) {
                    if (@namespace.Length > 0) @namespace.Append('.');
                    @namespace.Append(additionalNamespace[i][..1].ToUpper());
                    @namespace.Append(additionalNamespace[i][1..].ToLower());
                }

                return @namespace.ToString();
            }
        }

        public static string Write(string @class, string @namespace, MonoScriptTemplate monoScriptTemplate, string iconPath, out int lineNumber) {
            StringBuilder templateBuilder = new StringBuilder(), keywordBuilder = new StringBuilder();
            bool readingKeyword = false;
            string template = monoScriptTemplate.Template;
            lineNumber = 0;

            for (int i = 0, line = 0; i < template.Length; i++) {
                if (template[i] == '#') {
                    if (readingKeyword) {
                        switch (keywordBuilder.ToString().ToUpper()) {
                            case "CLASS":
                                templateBuilder.Append(@class);
                                break;

                            case "NAMESPACE":
                                templateBuilder.Append(@namespace);
                                break;

                            case "CURSOR":
                                lineNumber = line;
                                break;

                            case "ICON":
                                templateBuilder.Append(iconPath);
                                break;

                            default: break;
                        }

                        keywordBuilder.Clear();
                        readingKeyword = false;
                    } else readingKeyword = true;

                    continue;
                } else if (readingKeyword) {
                    keywordBuilder.Append(template[i]);
                } else {
                    if (template[i] == '\n') line++;
                    templateBuilder.Append(template[i]);
                }
            }

            return templateBuilder.ToString();
        }
    }
}