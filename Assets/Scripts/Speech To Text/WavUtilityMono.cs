using UnityEngine;
using System.IO;

public class WavUtilityMono : MonoBehaviour
{
    public byte[] ConvertClipToWav(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();
        int sampleCount = clip.samples * clip.channels;
        float[] samples = new float[sampleCount];
        clip.GetData(samples, 0);

        byte[] wavBytes = ConvertToWav(samples, clip.channels, clip.frequency);
        stream.Write(wavBytes, 0, wavBytes.Length);
        return stream.ToArray();
    }

    private byte[] ConvertToWav(float[] samples, int channels, int sampleRate)
    {
        MemoryStream stream = new MemoryStream();

        int byteRate = sampleRate * channels * 2;
        int dataSize = samples.Length * 2;

        // RIFF header
        stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
        stream.Write(System.BitConverter.GetBytes(36 + dataSize), 0, 4);
        stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, 4);

        // fmt chunk
        stream.Write(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, 4);
        stream.Write(System.BitConverter.GetBytes(16), 0, 4); // Subchunk1Size
        stream.Write(System.BitConverter.GetBytes((short)1), 0, 2); // AudioFormat
        stream.Write(System.BitConverter.GetBytes((short)channels), 0, 2);
        stream.Write(System.BitConverter.GetBytes(sampleRate), 0, 4);
        stream.Write(System.BitConverter.GetBytes(byteRate), 0, 4);
        stream.Write(System.BitConverter.GetBytes((short)(channels * 2)), 0, 2); // BlockAlign
        stream.Write(System.BitConverter.GetBytes((short)16), 0, 2); // BitsPerSample

        // data chunk
        stream.Write(System.Text.Encoding.ASCII.GetBytes("data"), 0, 4);
        stream.Write(System.BitConverter.GetBytes(dataSize), 0, 4);

        // Write samples
        foreach (var sample in samples)
        {
            short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
            stream.Write(System.BitConverter.GetBytes(intSample), 0, 2);
        }

        return stream.ToArray();
    }
}