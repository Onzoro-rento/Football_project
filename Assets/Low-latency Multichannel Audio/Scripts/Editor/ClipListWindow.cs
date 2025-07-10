using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LowLatencyMultichannelAudio
{
    public class SoundListWindow : EditorWindow
    {
        private const string Title = "Sound List";
        Vector2 scrollPosition = Vector2.zero;

        public static bool IsOpen { get; private set; }

        [MenuItem("Window/Low-Latency Multichannel Audio/" + Title)]
        public static void ShowWindow()
        {
            GetWindow<SoundListWindow>(Title);
        }
        
        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            var manager = SearchAudioManager();
            if (manager == null)
            {
                EditorGUILayout.EndScrollView();
                return;
            }

            ShowDeviceName(manager);
            ShowOuputChannels(manager);
            ShowAudioFiles(manager);            

            EditorGUILayout.EndScrollView();
        }
        
        private AudioManager SearchAudioManager()
        {
            var managers = FindObjectsOfType<AudioManager>();
            if (managers.Length != 1)
            {
                if (managers.Length == 0)
                    EditorGUILayout.HelpBox("There is no AudioManager components.", MessageType.Warning);
                else
                    EditorGUILayout.HelpBox("The number of AudioManager components should be one.", MessageType.Error);

                return null;
            }

            return managers[0];
        }

        private void ShowDeviceName(AudioManager manager)
        {
            EditorGUILayout.LabelField("Device Name", EditorStyles.boldLabel);
            if (manager.DeviceName == null)
                EditorGUILayout.HelpBox("Audio device is not selected yet.", MessageType.Error);
            else
                EditorGUILayout.LabelField(manager.DeviceName);
            EditorGUILayout.Space();
        }

        private void ShowOuputChannels(AudioManager manager)
        {
            EditorGUILayout.LabelField("Output Channels", EditorStyles.boldLabel);

            var channels = manager.ChannelCount;
            if (0 < channels)
                EditorGUILayout.LabelField(channels.ToString());
            else
                EditorGUILayout.HelpBox("Device is not connected or something is wrong.", MessageType.Error);

            EditorGUILayout.Space();
        }

        private void ShowAudioFiles(AudioManager manager)
        {
            CheckFilesExist(manager);

            EditorGUILayout.LabelField("Audio Files", EditorStyles.boldLabel);
            var names = manager.Sound.Files.Where(c => c != null).ToArray();
            for (var i = 0; i < names.Length; ++i)
                EditorGUILayout.LabelField((i + 1).ToString() + ":", names[i]);
            EditorGUILayout.Space();
        }

        private void CheckFilesExist(AudioManager manager)
        {
            manager.Sound.Files = manager.Sound.Files.ToList()
                .Where(p => File.Exists(p)).ToArray();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnEnable()
        {
            IsOpen = true;
        }

        void OnDisable()
        {
            IsOpen = false;
        }
    }
}