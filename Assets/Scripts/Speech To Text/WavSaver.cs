using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class WavSaver : MonoBehaviour
{
    [Header("Save Settings")]
    [Tooltip("Filename to save the WAV file as (without extension)")]
    public string filename = "output";

    [Tooltip("AudioClip to save")]
    public AudioClip clipToSave;

    [Tooltip("Trim silence below this threshold")]
    [Range(0f, 1f)]
    public float silenceThreshold = 0.01f;

    [Tooltip("Automatically save on Start")]
    public bool saveOnStart = false;

    private const int HEADER_SIZE = 44;

    void Start()
    {
        if (saveOnStart && clipToSave != null)
        {
            SaveWav(filename, clipToSave);
        }
    }

    public void SaveWav(string fileName, AudioClip clip)
    {
        if (!fileName.ToLower().EndsWith(".wav"))
        {
            fileName += ".wav";
        }

        string filepath = Path.Combine(Application.persistentDataPath, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        using (var fileStream = CreateEmpty(filepath))
        {
            ConvertAndWrite(fileStream, clip);
            WriteHeader(fileStream, clip);
        }

        Debug.Log($"WAV saved to: {filepath}");
    }

    public AudioClip TrimSilence(AudioClip clip)
    {
        var samples = new float[clip.samples];
        clip.GetData(samples, 0);
        return TrimSilence(new List<float>(samples), silenceThreshold, clip.channels, clip.frequency);
    }

    public AudioClip TrimSilence(List<float> samples, float min, int channels, int hz, bool _3D = false, bool stream = false)
    {
        int i;
        for (i = 0; i < samples.Count; i++)
        {
            if (Mathf.Abs(samples[i]) > min) break;
        }
        samples.RemoveRange(0, i);

        for (i = samples.Count - 1; i > 0; i--)
        {
            if (Mathf.Abs(samples[i]) > min) break;
        }
        samples.RemoveRange(i, samples.Count - i);

        var clip = AudioClip.Create("TrimmedClip", samples.Count, channels, hz, _3D, stream);
        clip.SetData(samples.ToArray(), 0);
        return clip;
    }

    private FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();
        for (int i = 0; i < HEADER_SIZE; i++)
        {
            fileStream.WriteByte(emptyByte);
        }
        return fileStream;
    }

    private void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        var samples = new float[clip.samples];
        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        Byte[] bytesData = new Byte[samples.Length * 2];
        int rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    private void WriteHeader(FileStream fileStream, AudioClip clip)
    {
        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);

        fileStream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
        fileStream.Write(BitConverter.GetBytes(fileStream.Length - 8), 0, 4);
        fileStream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
        fileStream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
        fileStream.Write(BitConverter.GetBytes(16), 0, 4);
        fileStream.Write(BitConverter.GetBytes((ushort)1), 0, 2);
        fileStream.Write(BitConverter.GetBytes(channels), 0, 2);
        fileStream.Write(BitConverter.GetBytes(hz), 0, 4);
        fileStream.Write(BitConverter.GetBytes(hz * channels * 2), 0, 4);
        fileStream.Write(BitConverter.GetBytes((ushort)(channels * 2)), 0, 2);
        fileStream.Write(BitConverter.GetBytes((ushort)16), 0, 2);
        fileStream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
        fileStream.Write(BitConverter.GetBytes(samples * channels * 2), 0, 4);
    }
}