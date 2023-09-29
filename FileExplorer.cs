using UnityEngine;

namespace FileExplorer
{
    public class FileExplorer
    {
        public delegate void OnSuccess(string path, string file);
        public delegate void OnCancel();
        public static void OpenFileSelect(OnSuccess onSuccess, OnCancel onCancel, string filter = "")
        {
            GameObject gameObject = new GameObject("File Explorer");
            Explorer explorer = gameObject.AddComponent<Explorer>();
            explorer.Open(onSuccess, onCancel, FileExplorerType.File, filter);
        }
        public static void OpenDirectorySelect(OnSuccess onSuccess, OnCancel onCancel)
        {
            GameObject gameObject = new GameObject("File Explorer");
            Explorer explorer = gameObject.AddComponent<Explorer>();
            explorer.Open(onSuccess, onCancel, FileExplorerType.Directory);
        }
        public static void OpenFileSave(OnSuccess onSuccess, OnCancel onCancel)
        {
            GameObject gameObject = new GameObject("File Explorer");
            Explorer explorer = gameObject.AddComponent<Explorer>();
            explorer.Open(onSuccess, onCancel, FileExplorerType.SaveFile);
        }
    }
}