using UnityEngine;

namespace OVAWORKT.MonoScriptTools {
    [System.Serializable]
    public sealed class MonoScriptTemplate {
        public string FileName, MatchRegex, NameRegex, IconPath;
        [TextArea(5, 10)] public string Template;
    }
}