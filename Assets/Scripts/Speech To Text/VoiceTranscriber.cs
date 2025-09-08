using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using SimpleJSON;

public class VoiceTranscriber : MonoBehaviour
{
    [Header("Wit.ai Settings")]
    public string witToken = "YOUR_WIT_AI_TOKEN"; // Replace with your actual token
    public WavUtilityMono wavUtility;

    [Header("UI Elements")]
    public TextMeshProUGUI transcriptText;

    private AudioClip recordedClip;
    private bool isRecording = false;

    public void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            transcriptText.text = " No microphone detected!";
            return;
        }

        recordedClip = Microphone.Start(null, false, 10, 16000);
        isRecording = true;
        transcriptText.text = " Listening...";
    }

    public void StopRecording()
    {
        if (!isRecording) return;

        Microphone.End(null);
        isRecording = false;

        float length = recordedClip.length;
        Debug.Log("Recorded clip length: " + length);

        transcriptText.text = " Transcribing...";
        StartCoroutine(SendToWit(recordedClip));
    }

    IEnumerator SendToWit(AudioClip clip)
    {
        byte[] wavData = wavUtility.ConvertClipToWav(clip);
        Debug.Log("WAV data size: " + wavData.Length);

        UnityWebRequest request = new UnityWebRequest("https://api.wit.ai/speech?v=20210928", "POST");
        request.uploadHandler = new UploadHandlerRaw(wavData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + witToken);
        request.SetRequestHeader("Content-Type", "audio/wav");

        yield return request.SendWebRequest();

        string jsonResponse = request.downloadHandler.text;
        Debug.Log("Wit.ai response: " + jsonResponse);

        if (request.result == UnityWebRequest.Result.Success)
        {
            string transcript = ExtractFinalText(jsonResponse);
            transcriptText.text = " " + transcript;
        }
        else
        {
            transcriptText.text = " Error: " + request.error;
        }
    }

    string ExtractFinalText(string json)
    {
        string finalText = "No transcription found.";

        // Split using a pattern that separates JSON objects
        string[] chunks = json.Split(new[] { "\n{" }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = chunks.Length - 1; i >= 0; i--)
        {
            string chunk = chunks[i].Trim();

            // Ensure it starts with '{'
            if (!chunk.StartsWith("{")) chunk = "{" + chunk;

            try
            {
                var parsed = SimpleJSON.JSON.Parse(chunk);
                if (parsed != null && parsed.HasKey("text"))
                {
                    finalText = parsed["text"];
                    break;
                }
            }
            catch
            {
                // Skip malformed chunks
                continue;
            }
        }

        return finalText;
    }
}