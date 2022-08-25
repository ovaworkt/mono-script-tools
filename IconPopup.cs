using UnityEditor;
using UnityEngine;

namespace OVAWORKT.MonoScriptTools {
    public sealed class IconPopup : PopupWindowContent {
        private static class Styles {
            public static readonly GUIContent NONE_BUTTON;
            public static readonly GUIStyle LABEL_ICON_STYLE, ICON_BUTTON_STYLE, NONE_BUTTON_STYLE, ICON_TEXT_BUTTON_STYLE;

            static Styles() {
                NONE_BUTTON = new GUIContent() {
                    text = " None",
                    image = EditorGUIUtility.IconContent("sv_icon_none").image as Texture2D,
                };

                LABEL_ICON_STYLE = new GUIStyle(EditorStyles.iconButton) {
                    fontSize = 0,
                    fixedWidth = 0,
                    fixedHeight = 18,
                    imagePosition = ImagePosition.ImageOnly,
                    padding = new RectOffset(6, 6, 4, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                };

                ICON_BUTTON_STYLE = new GUIStyle(EditorStyles.iconButton) {
                    fontSize = 0,
                    fixedWidth = 0,
                    fixedHeight = 0,
                    imagePosition = ImagePosition.ImageOnly,
                    padding = new RectOffset(4, 4, 4, 4),
                    margin = new RectOffset(0, 0, 0, 0),
                };

                ICON_TEXT_BUTTON_STYLE = new GUIStyle(EditorStyles.iconButton) {
                    fixedWidth = 0,
                    fixedHeight = 0,
                    stretchWidth = true,
                    stretchHeight = true,
                    imagePosition = ImagePosition.ImageLeft,
                    padding = new RectOffset(6, 6, 4, 4),
                };
            }
        }


        private int _selectedIndex, _objectPickerControlID;
        private static string _iconPath;
        private static string[] _iconPaths, _labelPaths, _dotPaths, _diamondPaths;
        private static Texture2D[] _icons, _labels, _dots, _diamonds;

        private const int PADDING = 8, DOUBLE_PADDING = PADDING * 2;
        private const string OBJECT_SELECTOR_UPDATED = "ObjectSelectorUpdated", OBJECT_SELECTOR_CLOSED = "ObjectSelectorClosed";
        private static readonly Vector2 WINDOW_SIZE = new Vector2(160, 164);

        public delegate void IconPopupDelegate();
        public delegate void AssignIconDelegate(string iconPath);

        public event IconPopupDelegate OnPopupClosed;
        public event AssignIconDelegate OnAssignIcon;

        public IconPopup() { }

        public IconPopup(string iconPath) {
            if (_icons == null) {
                _icons = new Texture2D[24];
                _iconPaths = new string[24];
                for (int i = 0; i < 8; i++) _icons[i] = EditorGUIUtility.IconContent(_iconPaths[i] = $"sv_label_{i}").image as Texture2D;
                for (int i = 0; i < 16; i++) _icons[8 + i] = EditorGUIUtility.IconContent(_iconPaths[8 + i] = $"sv_icon_dot{i}_pix16_gizmo").image as Texture2D;
            }

            if (_labels == null) {
                _labels = new Texture2D[8];
                _labelPaths = new string[8];
                for (int i = 0; i < 8; i++) _labels[i] = EditorGUIUtility.IconContent(_labelPaths[i] = $"sv_icon_name{i}").image as Texture2D;
            }

            if (_dots == null) {
                _dots = new Texture2D[8];
                _dotPaths = new string[8];
                for (int i = 0; i < 8; i++) _dots[i] = EditorGUIUtility.IconContent(_dotPaths[i] = $"sv_icon_dot{i}_sml").image as Texture2D;
            }

            if (_diamonds == null) {
                _diamonds = new Texture2D[8];
                _diamondPaths = new string[8];
                for (int i = 0; i < 8; i++) _diamonds[i] = EditorGUIUtility.IconContent(_diamondPaths[i] = $"sv_icon_dot{8 + i}_sml").image as Texture2D;
            }

            _iconPath = iconPath;
            FindSelectedIndex();
        }

        private void AssignIcon(int selectedIndex, bool closeWindow = false) {
            _selectedIndex = selectedIndex;
            _iconPath = _iconPaths[_selectedIndex];
            OnAssignIcon?.Invoke(_iconPath);

            if (closeWindow) editorWindow.Close();
        }

        private void AssignIcon(string iconPath, int selectedIndex, bool closeWindow = false) {
            _iconPath = iconPath;
            _selectedIndex = selectedIndex;
            OnAssignIcon?.Invoke(_iconPath);

            if (closeWindow) editorWindow.Close();
        }

        private void FindSelectedIndex() {
            _selectedIndex = -1;
            for (int i = 0; i < _iconPaths.Length; i++)
                if (_iconPath == _iconPaths[i])
                    _selectedIndex = i;
        }

        private static void HorizontalLine(Vector2 position, float width) {
            Rect separatorRect = new Rect(position, new Vector2(width, 1));
            EditorGUI.DrawRect(separatorRect, new Color(.4f, .4f, .4f, 1));
        }

        public override void OnClose() => OnPopupClosed?.Invoke();

        public override void OnGUI(Rect position) {
            Rect originalPosition = position;

            position.height = 21;
            EditorGUI.BeginDisabledGroup(_iconPath == string.Empty);
            if (GUI.Button(position, Styles.NONE_BUTTON, Styles.ICON_TEXT_BUTTON_STYLE)) AssignIcon(string.Empty, -1);
            EditorGUI.EndDisabledGroup();

            position.y += position.height;
            HorizontalLine(new Vector2(originalPosition.x, position.y), originalPosition.width);
            position.y++;

            position = originalPosition;
            position.y += 22;
            position.height = originalPosition.height - 22;

            position.width -= DOUBLE_PADDING;
            position.height -= DOUBLE_PADDING;
            position.x += PADDING;
            position.y += PADDING;

            EditorGUI.BeginChangeCheck();
            int labelIndex = GUI.SelectionGrid(position, _selectedIndex, _labels, 4, Styles.LABEL_ICON_STYLE);
            if (EditorGUI.EndChangeCheck()) AssignIcon(labelIndex);

            position.y += 44;
            HorizontalLine(new Vector2(originalPosition.x, position.y), originalPosition.width);

            position.y += 9;
            position.height = 18;
            EditorGUI.BeginChangeCheck();
            int dotIndex = 8 + GUI.SelectionGrid(position, _selectedIndex - 8, _dots, 8, Styles.ICON_BUTTON_STYLE);
            if (EditorGUI.EndChangeCheck()) AssignIcon(dotIndex);

            position.y += position.height;
            EditorGUI.BeginChangeCheck();
            int diamondIndex = 16 + GUI.SelectionGrid(position, _selectedIndex - 16, _diamonds, 8, Styles.ICON_BUTTON_STYLE);
            if (EditorGUI.EndChangeCheck()) AssignIcon(diamondIndex);

            position.y += position.height + 8;
            HorizontalLine(new Vector2(originalPosition.x, position.y), originalPosition.width);
            position.y += 9;
            position.height = 19;

            if (GUI.Button(position, "Other...")) {
                _objectPickerControlID = GUIUtility.GetControlID(FocusType.Passive);
                EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, string.Empty, _objectPickerControlID);
            }

            string commandName = Event.current.commandName;
            if (commandName == OBJECT_SELECTOR_UPDATED && EditorGUIUtility.GetObjectPickerControlID() == _objectPickerControlID) AssignIcon(AssetDatabase.GetAssetPath(EditorGUIUtility.GetObjectPickerObject()), -1);
            else if (commandName == OBJECT_SELECTOR_CLOSED) _objectPickerControlID = -1;
        }

        public override Vector2 GetWindowSize() => WINDOW_SIZE;
    }
}