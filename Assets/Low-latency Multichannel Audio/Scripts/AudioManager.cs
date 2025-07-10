using System;
using System.IO;
using System.Linq;
using UnityEngine;
using ASIO.NET;

namespace LowLatencyMultichannelAudio
{
    public class AudioManager : MonoBehaviour
    {
        public enum TargetDir
        {
            StreamingAssets,
            Resources
        }
        private string[] TargetDirs { get { return Enum.GetNames(typeof(TargetDir)); } }

        public readonly string[] Extensions = new[] { ".wav", ".wma", ".mp3", ".aif", ".flac", ".aac" };
        private const int SampleRate = 48000;

        [ReadOnly(ReadOnlyType.NeverShow)]
        public string DeviceName;

        [ReadOnly(ReadOnlyType.NeverShow)]
        public int ChannelCount;

        [ReadOnly]
        public AudioList Sound;

        public static Asio Asio { get; private set; }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitAsio();
            LoadSoundFiles();
        }

        private void InitAsio()
        {
            if (string.IsNullOrEmpty(DeviceName))
                Debug.LogError("Audio device is not selected");

            try
            {
                Asio = Asio.CreatePlayer(DeviceName, SampleRate);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to init audio: " + e.Message);
            }
        }

        private void LoadSoundFiles()
        {
            foreach (var file in Sound.Files)
            {
                try
                {
                    var dir = TargetDirs.ToList().Find(d => file.Contains(d));
                    var type = (TargetDir)Enum.Parse(typeof(TargetDir), dir);
                    switch (type)
                    {
                        case TargetDir.Resources:
                            var audio = Resources.Load(Path.GetFileNameWithoutExtension(file)) as AudioClip;
                            if (audio == null)
                                throw new Exception("File is not audio clip");

                            var channels = audio.channels;
                            var sampleRate = audio.frequency;
                            var data = new float[audio.samples * channels];
                            audio.GetData(data, 0);
                            Resources.UnloadAsset(audio);

                            Asio.Load(data, (uint)sampleRate, (uint)channels);
                            break;

                        case TargetDir.StreamingAssets:
                            var path = Path.Combine(Application.dataPath + "/..", file);
                            Asio.Load(path);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message + " (" + file + ")");
                }
            }
        }

        void OnApplicationQuit()
        {
            if (Asio != null)
                Asio.Dispose();
        }

        [Serializable]
        public class AudioList
        {
            public string SearchDirectory;
            public string[] Files = new string[0];
        }
    }
}
