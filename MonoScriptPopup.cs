using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using UnityEditor.ShortcutManagement;
using UnityEditor;
using UnityEngine;

namespace OVAWORKT.MonoScriptTools {
    public sealed class MonoScriptPopup : PopupWindowContent {
        private static class Styles {
            public static readonly GUIStyle Title, ScrollMargins, ListMargins, FooterMargins, MiniButtonStyle, FolderButtonStyle;
            public static readonly Texture2D AddIcon, RemoveIcon, ScriptIcon, ErrorIcon, FolderIcon;
            static Styles() {
                Title = new GUIStyle(EditorStyles.boldLabel) {
                    contentOffset = new Vector2(0, 0),
                };

                ScrollMargins = new GUIStyle() {
                    margin = new RectOffset(0, 2, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0),
                };

                ListMargins = new GUIStyle() {
                    margin = new RectOffset(1, 0, 0, 1),
                    padding = new RectOffset(2, 2, 2, 2),
                };

                FooterMargins = new GUIStyle() {
                    margin = new RectOffset(2, 2, 3, 3),
                    padding = new RectOffset(0, 0, 0, 0),
                };

                MiniButtonStyle = new GUIStyle(EditorStyles.iconButton) {
                    margin = new RectOffset(0, 1, 0, 0),
                    padding = new RectOffset(1, 1, 1, 1),
                    fixedWidth = EditorGUIUtility.singleLineHeight,
                    fixedHeight = EditorGUIUtility.singleLineHeight,
                };

                FolderButtonStyle = new GUIStyle(MiniButtonStyle) {
                    contentOffset = new Vector2(-1, 0)
                };

                AddIcon = EditorGUIUtility.IconContent("d_Toolbar Plus").image as Texture2D;
                RemoveIcon = EditorGUIUtility.IconContent("d_Toolbar Minus").image as Texture2D;
                ScriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
                ErrorIcon = EditorGUIUtility.IconContent("console.erroricon").image as Texture2D;
                FolderIcon = EditorGUIUtility.IconContent("FolderOpened Icon").image as Texture2D;
            }
        }

        private sealed class MonoScriptCreator {
            public string Name;
            public string IconPath, FallbackIconPath;
            public MonoScriptTemplate Template;

            public MonoScriptCreator(string name = "") {
                Name = name;
                IconPath = FallbackIconPath = string.Empty;
            }

            public void SetTemplate(MonoScriptTemplate template) {
                Template = template;
                FallbackIconPath = template != null ? template.IconPath : string.Empty;
            }

            public string GetIconPath() => IconPath == string.Empty ? FallbackIconPath : IconPath;
        }

        private int _index, _currentIndex, _iconIndex;
        private string _namespace, _filePath;
        private bool _hasOpened;
        private Vector2 _scrollPosition;
        private GUIContent[] _scriptContents;
        private List<MonoScriptCreator> _scripts;

        private const string SCRIPT_EXTENSION = ".cs", ILLEGAL_CHARACTERS = "/?<>\\:*|.\" \t";

        private static IconPopup _iconPopup;
        private static MonoScriptPopup _instance;

        private void IconPopupClosed() {
            _iconPopup.OnAssignIcon -= AssignIcon;
            _iconPopup.OnPopupClosed -= IconPopupClosed;
        }

        private int FindIndex(out string focusedControl) {
            focusedControl = GUI.GetNameOfFocusedControl();
            string[] controlParts = focusedControl.Split('-', System.StringSplitOptions.RemoveEmptyEntries);
            if (controlParts.Length < 2 || !int.TryParse(focusedControl.Split('-')[1], out int index)) return -1;
            if (index < 0 || index >= _scripts.Count) return -1;
            return index;
        }

        private void AssignIcon(string iconPath) {
            if (_iconIndex < 0) return;
            _scripts[_iconIndex].IconPath = iconPath;
            editorWindow.Repaint();
        }

        private bool CanCreateScripts() {
            for (int i = 0; i < _scripts.Count; i++) {
                for (int j = 0; j < _scripts[i].Name.Length; j++) {
                    if (ILLEGAL_CHARACTERS.Contains(_scripts[i].Name[j])) return false;
                }
            }
            return !(_scripts.Count <= 0 || _scripts.Any(script => script.Name == string.Empty));
        }

        [MenuItem("Assets/Create/Multiple C# Scripts", priority = 80)]
        private static void Open() {
            if (_instance == null) _instance = new MonoScriptPopup() { _scripts = new List<MonoScriptCreator>() { new MonoScriptCreator() } };
            _instance._scriptContents = new GUIContent[] { new GUIContent(Styles.ErrorIcon, tooltip: "Invalid Template!") };
            PopupWindow.Show(Rect.zero, _instance);
        }

        [Shortcut(id: "CreateMultipleScripts", defaultKeyCode: KeyCode.C, defaultShortcutModifiers: ShortcutModifiers.Shift & ShortcutModifiers.Alt, displayName = "Create Multiple C# Scripts")]
        private static void KeyboardShortcut() => EditorApplication.update += OpenNextUpdate;

        private static void OpenNextUpdate() {
            EditorApplication.update -= OpenNextUpdate;
            Open();
        }

        private void CreateAll() {
            MonoScript[] monoScripts = new MonoScript[_scripts.Count];
            for (int i = 0; i < _scripts.Count; i++) {
                string filePath = _filePath + '/' + _scripts[i].Name + SCRIPT_EXTENSION, fileContent = MonoScriptWriter.Write(_scripts[i].Name, _namespace, _scripts[i].Template, _scripts[i].GetIconPath(), out int cursor);
                File.WriteAllText(filePath, fileContent);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            for (int i = 0; i < _scripts.Count; i++) {
                string filePath = _filePath + '/' + _scripts[i].Name + SCRIPT_EXTENSION;
                monoScripts[i] = AssetDatabase.LoadAssetAtPath(filePath, typeof(MonoScript)) as MonoScript;
            }

            AssetDatabase.OpenAsset(monoScripts);
            editorWindow.Close();
            _instance = null;
        }

        public override void OnOpen() {
            base.OnOpen();
            string activeFolderPath = ProjectUtility.GetActiveFolderPath();
            if (_filePath != activeFolderPath) {
                _filePath = activeFolderPath;
                _namespace = MonoScriptWriter.GetNamespace();
            }

            if (_scripts.Count <= 0) _scripts.Add(new MonoScriptCreator());
            if (_scripts.Count <= 1) {
                _hasOpened = false;
                _index = 0;
            }
        }

        public override void OnClose() {
            _hasOpened = false;
            _index = FindIndex(out _);
        }

        public override void OnGUI(Rect rect) {
            void HorizontalSeparator() {
                Rect line = EditorGUILayout.GetControlRect(false, 1);
                line.x = rect.x;
                line.width = rect.width;
                EditorGUI.DrawRect(line, new Color(.4f, .4f, .4f, 1));
            }

            EditorGUILayout.BeginHorizontal(Styles.FooterMargins);
            GUILayout.Label("Create Scripts", Styles.Title);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(Styles.FolderIcon, Styles.FolderButtonStyle)) EditorUtility.OpenWithDefaultApp(MonoScriptTemplateManager.TemplatePath);
            if (GUILayout.Button(Styles.AddIcon, Styles.MiniButtonStyle)) {
                _scripts.Add(new MonoScriptCreator());
                _scriptContents = new GUIContent[_scripts.Count];
                for (int j = 0; j < _scriptContents.Length; j++) _scriptContents[j] = new GUIContent();
            }

            EditorGUILayout.EndHorizontal();
            HorizontalSeparator();

            if (_scripts.Count < 7) GUI.enabled = false;
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, Styles.ScrollMargins);
            GUI.enabled = true;
            for (int i = 0; i < _scripts.Count; i++) {
                _currentIndex = i;
                EditorGUILayout.BeginHorizontal(Styles.ListMargins);

                bool emptyScriptName = _scripts[i].Name == string.Empty;
                EditorGUI.BeginDisabledGroup(emptyScriptName);
                if (emptyScriptName) _iconIndex = -1;

                if (_scriptContents.Length != _scripts.Count) {
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    editorWindow.Close();
                    _instance = null;
                    return;
                }

                string iconPath = _scripts[i].GetIconPath();
                if (iconPath != string.Empty) {
                    if (iconPath.Contains('/')) {
                        string[] parts = iconPath.Split('/');
                        if (parts[0] == "Resources") _scriptContents[i].image = Resources.Load(iconPath, typeof(Texture2D)) as Texture2D;
                        else _scriptContents[i].image = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D)) as Texture2D;
                    } else _scriptContents[i].image = EditorGUIUtility.IconContent(_scripts[i].GetIconPath()).image;
                } else _scriptContents[i].image = Styles.ErrorIcon;

                if (_scripts[i].Template != null) _scriptContents[i].tooltip = _scripts[i].Template.FileName;
                else _scriptContents[i].tooltip = "Invalid Template!";

                if (GUILayout.Button(_scriptContents[i], Styles.MiniButtonStyle)) {
                    _iconIndex = i;
                    _iconPopup = new IconPopup(_scripts[i].IconPath);
                    _iconPopup.OnAssignIcon += AssignIcon;
                    _iconPopup.OnPopupClosed += IconPopupClosed;

                    Rect popupRect = GUILayoutUtility.GetLastRect();
                    popupRect.position += Vector2.one * EditorGUIUtility.singleLineHeight;
                    PopupWindow.Show(popupRect, _iconPopup);
                }

                EditorGUI.EndDisabledGroup();
                string controlName = $"script-{i}";
                GUI.SetNextControlName(controlName);

                EditorGUI.BeginChangeCheck();
                _scripts[i].Name = EditorGUILayout.TextField(_scripts[i].Name);
                if (EditorGUI.EndChangeCheck()) {
                    bool illegalCharacterFound = false;
                    for (int j = 0; j < _scripts[i].Name.Length; j++) {
                        if (ILLEGAL_CHARACTERS.Contains(_scripts[i].Name[j])) {
                            illegalCharacterFound = true;
                            EditorUtility.DisplayDialog("Invalid Script Name", $"A file name can\'t contain any of the following characters: \'{ILLEGAL_CHARACTERS}\'", "OK", string.Empty);
                            break;
                        }
                    }

                    if (!illegalCharacterFound) {
                        for (int j = MonoScriptTemplateManager.Templates.Length - 1; j >= 0; j--) {
                            if (_scripts[i].Name == string.Empty) {
                                _scripts[i].SetTemplate(null);
                                break;
                            }

                            (string matchRegex, MonoScriptTemplate template) template = MonoScriptTemplateManager.Templates[j];
                            Regex regex = new Regex(template.matchRegex);
                            if (regex.IsMatch(_scripts[i].Name)) {
                                _scripts[i].SetTemplate(template.template);
                                break;
                            }
                        }
                    }
                }

                if (GUILayout.Button(Styles.RemoveIcon, Styles.MiniButtonStyle)) {
                    _scripts.RemoveAt(i);
                    _scriptContents = new GUIContent[_scripts.Count];
                    for (int j = 0; j < _scriptContents.Length; j++) _scriptContents[j] = new GUIContent();
                }

                GUILayout.Space(1);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            HorizontalSeparator();

            EditorGUILayout.BeginVertical(Styles.FooterMargins);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Namespace", GUILayout.ExpandWidth(false));

            _namespace = EditorGUILayout.TextField(_namespace, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Cancel", GUILayout.Height(EditorGUIUtility.singleLineHeight))) {
                editorWindow.Close();
                _instance = null;
            }

            EditorGUI.BeginDisabledGroup(!CanCreateScripts());
            if (GUILayout.Button("Create", GUILayout.Height(EditorGUIUtility.singleLineHeight))) CreateAll();

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            if (!_hasOpened) {
                _hasOpened = true;
                GUI.FocusControl($"script-{_index}");
            }

            if (Event.current.type == EventType.KeyDown) {
                if (Event.current.keyCode == KeyCode.Tab) {
                    if (Event.current.modifiers.HasFlag(EventModifiers.Shift)) return;
                    if ((_index = FindIndex(out string focusedControl)) < 0) return;
                    if (focusedControl == $"script-{_scripts.Count - 1}") {
                        _scripts.Add(new MonoScriptCreator());
                        _scriptContents = new GUIContent[_scripts.Count];
                        for (int j = 0; j < _scriptContents.Length; j++) _scriptContents[j] = new GUIContent();
                    }
                } else if (Event.current.keyCode == KeyCode.Return) {
                    if (CanCreateScripts()) {
                        GUIUtility.keyboardControl = 0;
                        CreateAll();
                    } else {
                        editorWindow.Close();
                        if (_scripts.All(script => script.Name == string.Empty)) _instance = null;
                    }
                } else if (Event.current.keyCode == KeyCode.Escape) {
                    editorWindow.Close();
                    if (_scripts.All(script => script.Name == string.Empty)) _instance = null;
                }
            }
        }

        public override Vector2 GetWindowSize() => new Vector2(500, 200);
    }
}