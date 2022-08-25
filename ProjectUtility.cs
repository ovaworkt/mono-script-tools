using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace OVAWORKT.MonoScriptTools {
    public static class ProjectUtility {
        public static string GetActiveFolderPath() {
            MethodInfo getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            string folderPath = (string)getActiveFolderPath.Invoke(null, null);
            return folderPath;
        }
    }
}