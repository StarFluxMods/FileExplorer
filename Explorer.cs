using System;
using System.Collections.Generic;
using System.IO;
using KitchenLib.Utils;
using UnityEngine;

namespace FileExplorer
{
    public enum FileExplorerType
    {
        File,
        Directory,
        SaveFile
    }
    public class Explorer : MonoBehaviour
    {
        private Rect windowRect = new Rect((Screen.width / 2) - (_windowWidth / 2), (Screen.height / 2) - (_windowHeight / 2), _windowWidth, _windowHeight);

        private GUIStyle _style = null;
        private GUIStyle _favouritesButtonStyle = null;
        private GUIStyle _contentButtonStyle = null;
        private GUIStyle _buttonIcon = null;
        private static float _windowWidth = (Screen.width / 3) * 2;
        private static float _windowHeight = (Screen.height / 3) * 2;
        private static float NavBarHeight = _windowHeight / 6;
        private static float ContentHight = _windowHeight - (NavBarHeight * 2);
        
        private static float ContentLeftWidth = _windowWidth / 6;
        private static float ContentRightWidth = _windowWidth - ContentLeftWidth;
        
        private string lastDirectory = "";
        private string currentDirectory = "";
        private DirectoryInfo currentDirectoryInfo;
        private string defaultDirectory = Application.persistentDataPath;
        private string selectedFile = "";
        
        private Vector2 favouritesScrollPosition;
        private Vector2 contentScrollPosition;

        private FileExplorer.OnSuccess _onSuccess;
        private FileExplorer.OnCancel _onCancel;
        private FileExplorerType _type = FileExplorerType.File;
        private string _fileFilter = "";
        private bool _isOpen = false;
        
        
        private List<(string, string)> favorites = new List<(string, string)>
        {
            ("Persistent", Application.persistentDataPath ),
            ("Desktop", System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
            ("Documents", System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
            ("Music", System.Environment.GetFolderPath(Environment.SpecialFolder.MyMusic))
        };

        public void Open(FileExplorer.OnSuccess onSuccess, FileExplorer.OnCancel onCancel, FileExplorerType type, string filter = "")
        {
            _onSuccess = onSuccess;
            _onCancel = onCancel;
            _fileFilter = filter;
            _type = type;
            _isOpen = true;
        }

        public void OnGUI()
        {
            if (!_isOpen)
                return;
            
            if (_windowWidth > (_windowHeight * 1.778f))
            {
                _windowWidth = _windowHeight * 1.778f;
            }
            
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.window);
                var backgroundTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false); 
                backgroundTexture.SetPixel(0, 0, new Color(0.17f, 0.17f, 0.17f, 1));
                backgroundTexture.Apply();
                _style.normal.background = backgroundTexture;
            }

            if (_favouritesButtonStyle == null)
            {
                _favouritesButtonStyle = new GUIStyle(GUI.skin.button);
                _favouritesButtonStyle.normal.background = null;
                _favouritesButtonStyle.hover.background = TextureUtils.GetTexture(new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.1f), 1, 1);
                _favouritesButtonStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (_contentButtonStyle == null)
            {
                _contentButtonStyle = new GUIStyle(GUI.skin.button);
                _contentButtonStyle.normal.background = null;
                _contentButtonStyle.hover.background = TextureUtils.GetTexture(new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.1f), 1, 1);
                _contentButtonStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (_buttonIcon == null)
            {
                _buttonIcon = new GUIStyle(GUI.skin.button);
                _buttonIcon.normal.background = null;
                _buttonIcon.hover.background = null;
            }
            
            windowRect = GUILayout.Window(VariousUtils.GetID("fileexplorer"), windowRect, ExplorerWindow, "", _style, GUILayout.Width(_windowWidth), GUILayout.Height(_windowHeight));
        }
        public void ExplorerWindow(int id)
        {
            GUILayout.Space(0);
            
            GUILayout.BeginArea(new Rect(0, 0, _windowWidth, NavBarHeight), TextureUtils.GetTexture(new Color(0.05f, 0.05f, 0.05f), (int)_windowWidth, (int)NavBarHeight)); // Header
            Header((int)_windowWidth, (int)NavBarHeight);
            GUILayout.EndArea();
            
            GUILayout.BeginArea(new Rect(0, NavBarHeight, _windowWidth, ContentHight)); // Content
            
            GUILayout.BeginArea(new Rect(0, 0, ContentLeftWidth, ContentHight), TextureUtils.GetTexture(new Color(0.1f, 0.1f, 0.1f), (int)ContentLeftWidth, (int)ContentHight)); // Content Left
            ContentLeft((int)ContentLeftWidth, (int)ContentHight);
            GUILayout.EndArea();
            
            GUILayout.BeginArea(new Rect(ContentLeftWidth, 0, ContentRightWidth, ContentHight), TextureUtils.GetTexture(new Color(0.15f, 0.15f, 0.15f), (int)ContentRightWidth, (int)ContentHight)); // Content Right
            ContentRight((int)ContentRightWidth, (int)ContentHight);
            GUILayout.EndArea();
            
            GUILayout.EndArea();
            
            GUILayout.BeginArea(new Rect(0, NavBarHeight + ContentHight, _windowWidth, NavBarHeight), TextureUtils.GetTexture(new Color(0.05f, 0.05f, 0.05f), (int)_windowWidth, (int)NavBarHeight)); // Footer
            Footer((int)_windowWidth, (int)NavBarHeight);
            GUILayout.EndArea();
            
            GUI.DragWindow();
        }

        public void Header(int width, int height)
        {
            GUILayout.BeginArea(new Rect(0, height / 3, width, height / 3));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Mod.Bundle.LoadAsset<Texture2D>("back-icon"), _buttonIcon, GUILayout.Width(height / 3), GUILayout.Height(height / 3)))
            {
                ChangeDirectory(lastDirectory);
            }
            currentDirectory = GUILayout.TextField(currentDirectory, GUILayout.Width(width - (height / 3)), GUILayout.Height(height / 3));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        public void ContentLeft(int width, int height)
        {
            favouritesScrollPosition = GUILayout.BeginScrollView(favouritesScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            int count = 0;
            
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(Mod.Bundle.LoadAsset<Texture2D>("drive-icon"), _buttonIcon, GUILayout.Width(height / 12), GUILayout.Height(height / 12)))
                {
                    ChangeDirectory(drive.Name);
                }
                if (GUILayout.Button(drive.Name, _favouritesButtonStyle, GUILayout.Width(width - (height / 12)), GUILayout.Height(height / 12)))
                {
                    ChangeDirectory(drive.Name);
                }
                GUILayout.EndHorizontal();
                count++;
            }
            
            foreach ((string, string) favorite in favorites)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(Mod.Bundle.LoadAsset<Texture2D>("folder-icon"), _buttonIcon, GUILayout.Width(height / 12), GUILayout.Height(height / 12)))
                {
                    ChangeDirectory(favorite.Item2);
                }
                if (GUILayout.Button(favorite.Item1, _favouritesButtonStyle, GUILayout.Width(width - (height / 12)), GUILayout.Height(height / 12)))
                {
                    ChangeDirectory(favorite.Item2);
                }
                GUILayout.EndHorizontal();
                count++;
            }
            
            GUILayout.EndScrollView();
        }
        
        public void ContentRight(int width, int height)
        {
            UpdateDirectory();
            contentScrollPosition = GUILayout.BeginScrollView(contentScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
            int count = 0;
            foreach (var directory in currentDirectoryInfo.GetDirectories())
            {
                if ((directory.Attributes & FileAttributes.Hidden) != 0)
                    continue;
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(Mod.Bundle.LoadAsset<Texture2D>("folder-icon"), _buttonIcon, GUILayout.Width(height / 15), GUILayout.Height(height / 15)))
                {
                    ChangeDirectory(directory.FullName);
                }if (GUILayout.Button(directory.Name, _contentButtonStyle, GUILayout.Width(width - (height / 15)), GUILayout.Height(height / 15)))
                {
                    ChangeDirectory(directory.FullName);
                }
                GUILayout.EndHorizontal();
                count++;
            }

            foreach (var file in currentDirectoryInfo.GetFiles(_fileFilter))
            {
                if ((file.Attributes & FileAttributes.Hidden) != 0)
                    continue;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(Mod.Bundle.LoadAsset<Texture2D>("file-icon"), _buttonIcon, GUILayout.Width(height / 15), GUILayout.Height(height / 15)))
                {
                    selectedFile = file.FullName;
                }
                if (GUILayout.Button(file.Name, _contentButtonStyle, GUILayout.Width(width - (height / 15)), GUILayout.Height(height / 15)))
                {
                    selectedFile = file.FullName;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        public void Footer(int width, int height)
        {
            GUILayout.BeginArea(new Rect(0, (height / 4), width, height / 2));
            selectedFile = GUILayout.TextField(selectedFile);
            GUILayout.EndArea();
            
            GUILayout.BeginArea(new Rect(0, (height / 2), width, height / 2));
            GUILayout.BeginHorizontal();
            GUILayout.Label(_fileFilter, GUILayout.Width((width / 8) * 6));
            if (_type == FileExplorerType.Directory || _type == FileExplorerType.File)
            {
                if (GUILayout.Button("Open", GUILayout.Width(width / 8)))
                {
                    _onSuccess?.Invoke(currentDirectory, selectedFile);
                    _isOpen = false;
                }
            }
            else if (_type == FileExplorerType.SaveFile)
            {
                if (GUILayout.Button("Save", GUILayout.Width(width / 8)))
                {
                    _onSuccess?.Invoke(currentDirectory, selectedFile);
                    _isOpen = false;
                }
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(width / 8)))
            {
                _onCancel?.Invoke();
                _isOpen = false;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void ChangeDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                selectedFile = "";
                lastDirectory = currentDirectory;
                currentDirectory = directory;
            }
        }
        
        private void UpdateDirectory()
        {
            if (currentDirectoryInfo == null)
            {
                currentDirectoryInfo = new DirectoryInfo (defaultDirectory);
                currentDirectory = currentDirectoryInfo.FullName;
            }
            else if (!string.IsNullOrEmpty(currentDirectory) && currentDirectoryInfo.FullName != currentDirectory && Directory.Exists(currentDirectory))
            {
                currentDirectoryInfo = new DirectoryInfo (currentDirectory);
                currentDirectory = currentDirectoryInfo.FullName;
            }
        }
    }
}