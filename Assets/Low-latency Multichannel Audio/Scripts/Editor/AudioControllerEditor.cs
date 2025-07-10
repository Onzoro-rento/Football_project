using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using ASIO.NET;

namespace LowLatencyMultichannelAudio
{
    [CustomEditor(typeof(AudioManager))]
    public class AudioControllerEditor : Editor
    {
        private const int ButtonHeight = 28;

        private static string[] DeviceNames = new string[0];
        private int DeviceIndex;
        private bool ShouldRefresh;

        private AudioManager TargetScript { get { return target as AudioManager; } }
        private string[] Extensions { get { return TargetScript.Extensions; } }
        private bool IsPlaying { get { return EditorApplication.isPlayingOrWillChangePlaymode; } }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ShowDeviceSettings();
            ShowSoundSettings();
            ShowOtherSettings();

            CheckDuplication();
        }

        void OnEnable()
        {
            ShouldRefresh = true;
        }

        private void ShowDeviceSettings()
        {
            EditorGUILayout.LabelField("Device Settings", EditorStyles.boldLabel);

            UpdateDeviceList();

            if (DeviceNames.Length == 0)
                ShowInstallButton();
            else
                ShowDeviceList();

            ShowSettingButton();

            EditorGUILayout.Space();
        }

        private void UpdateDeviceList()
        {
            var names = Asio.GetDeviceNames();
            if (DeviceNames.Length != names.Length)
            {
                DeviceNames = names;
                Debug.Log("Audio device list is updated");
            }
        }

        private void ShowDeviceList()
        {
            if (IsPlaying)
            {
                EditorGUILayout.LabelField("Device Name", TargetScript.DeviceName);
                ShowDeviceInfo(false);
                return;
            }

            DeviceIndex = DeviceNames.ToList().FindIndex(name => name == TargetScript.DeviceName);
            var selectedIndex = EditorGUILayout.Popup("Device Name", DeviceIndex, DeviceNames);
            var updated = selectedIndex != DeviceIndex;

            DeviceIndex = selectedIndex;
            TargetScript.DeviceName = DeviceIndex < 0 ? null : DeviceNames[DeviceIndex];

            ShowDeviceInfo(ShouldRefresh || updated);
        }

        private void ShowInstallButton()
        {
            if (IsPlaying)
                return;

            if (!GUILayout.Button("Install Audio driver"))
                return;

            TargetScript.DeviceName = null;
            System.Diagnostics.Process.Start("http://www.asio4all.org");
        }

        private void ShowDeviceInfo(bool updated)
        {
            ShouldRefresh = false;

            if (DeviceIndex < 0 || DeviceNames.Length == 0)
                return;

            try
            {
                if (updated)
                    TargetScript.ChannelCount = (int)Asio.GetOutputChannelCount(DeviceNames[DeviceIndex]);
                if (TargetScript.ChannelCount == 0)
                    throw new Exception();

                EditorGUILayout.LabelField("Output Channels", TargetScript.ChannelCount.ToString());
            }
            catch
            {
                TargetScript.ChannelCount = 0;
                EditorGUILayout.HelpBox("Device is not connected or something is wrong.", MessageType.Error);
            }
        }

        private void ShowSettingButton()
        {
            if (IsPlaying)
            {
                EditorGUILayout.HelpBox("Do NOT change audio setting during app playing.", MessageType.Warning);
                return;
            }

            if (DeviceNames.Length == 0)
                return;

            if (TargetScript.DeviceName == null)
            {
                EditorGUILayout.HelpBox("Audio device is not selected yet.", MessageType.Error);
                return;
            }

            if (!GUILayout.Button("Open control panel", GUILayout.Height(ButtonHeight)))
                return;

            Asio.ShowControlPanel(DeviceNames[DeviceIndex]);
        }

        private void ShowSoundSettings()
        {
            EditorGUILayout.LabelField("Sound Settings", EditorStyles.boldLabel);

            CheckFilesExist();
            LoadSoundList();

            EditorGUILayout.Space();
        }

        private void CheckFilesExist()
        {
            TargetScript.Sound.Files = TargetScript.Sound.Files.ToList()
                .Where(p => File.Exists(p)).ToArray();
        }

        private void LoadSoundList()
        {
            if (IsPlaying)
                return;

            var res = GUILayout.Button("Add audio files from directory...", GUILayout.Height(ButtonHeight));
            EditorGUILayout.HelpBox("" +
                "Recursively searches for audio files in the selected directory" +
                " (only in StreamingAssets and Resources) and adds them to the list.", MessageType.Info);

            if (!res)
                return;

            var dataPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"));
            var searchPath = EditorUtility.OpenFolderPanel("Select Directory", dataPath, "");
            if (string.IsNullOrEmpty(searchPath))
                return;

            if (!searchPath.Contains(Application.dataPath))
            {
                Debug.LogError("Not in assets path: " + searchPath);
                return;
            }
            searchPath = searchPath.Replace(Application.dataPath, "Assets");

            var guids = AssetDatabase.FindAssets("", new string[] { searchPath });
            var filePathes = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(p => IsSupportedAudioFile(p))
                .Distinct();

            TargetScript.Sound.SearchDirectory = searchPath;
            TargetScript.Sound.Files = filePathes.ToArray();
        }

        private bool IsSupportedAudioFile(string path)
        {
            if (path.Split(new[] { '/', '\\' }).Contains(AudioManager.TargetDir.Resources.ToString()))
                return AssetDatabase.LoadAssetAtPath<AudioClip>(path) != null;
            else if (Path.GetFullPath(path).IndexOf(Path.GetFullPath(Application.streamingAssetsPath)) == 0)
                return Extensions.Any(ext => path.EndsWith(ext));

            return false;
        }

        private void ShowOtherSettings()
        {
            EditorGUILayout.LabelField("Other Settings", EditorStyles.boldLabel);

            ShowSoundList();
            ShowRefreshButton();

            EditorGUILayout.Space();
        }

        private void ShowSoundList()
        {
            if (SoundListWindow.IsOpen)
                return;

            if (GUILayout.Button("Open sound list window", GUILayout.Height(ButtonHeight)))
                SoundListWindow.ShowWindow();
        }

        private void ShowRefreshButton()
        {
            if (IsPlaying)
                return;

            if (GUILayout.Button("Refresh", GUILayout.Height(ButtonHeight)))
                ShouldRefresh = true;
        }

        private void CheckDuplication()
        {
            var managers = FindObjectsOfType<AudioManager>();
            if (managers.Length != 1)
                EditorGUILayout.HelpBox("The number of AudioManager components should be one.", MessageType.Error);
        }
    }
}
