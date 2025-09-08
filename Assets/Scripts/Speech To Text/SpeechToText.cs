using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class SpeechToText : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI resultText;

    [Header("Recording Settings")]
    public int recordingDuration = 5;
    public int samplingRate = 16000;
    public string wavFilename = "recordedSpeech";

    [Header("Wit.ai Settings")]
    public string witToken = "Bearer YOUR_WIT_AI_SERVER_TOKEN";
    private string witApiUrl = "https://api.wit.ai/speech?v=20210928";

    private AudioClip recordedClip;
    private WavSaver wavSaver;

    void Awake()
    {
        wavSaver = GetComponent<WavSaver>();
        if (wavSaver == null)
        {
            Debug.LogError("WavSaver component missing!");
        }
    }

    public void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone detected!");
            return;
        }

        recordedClip = Microphone.Start(null, false, recordingDuration, samplingRate);
        Debug.Log("Recording started...");
    }

    public void StopRecording()
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
            Debug.Log("Recording stopped.");

            // Save WAV using WavSaver
            wavSaver.SaveWav(wavFilename, recordedClip);

            // Convert to byte[] and send to Wit.ai
            byte[] audioBytes = WavSaverStatic.FromAudioClip(recordedClip); // See below
            StartCoroutine(SendToWitAI(audioBytes));
        }
    }

    IEnumerator SendToWitAI(byte[] data)
    {
        UnityWebRequest www = new UnityWebRequest(witApiUrl, "POST");
        www.uploadHandler = new UploadHandlerRaw(data);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", witToken);
        www.SetRequestHeader("Content-Type", "audio/wav");

        Debug.Log("Sending to Wit.ai...");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;
            string recognizedText = ExtractText(json);
            resultText.text = recognizedText;
        }
        else
        {
            Debug.LogError("Wit.ai error: " + www.error);
            resultText.text = "Error: " + www.error;
        }
    }

    private string ExtractText(string json)
    {
        int index = json.IndexOf("\"text\":\"");
        if (index >= 0)
        {
            int start = index + 8;
            int end = json.IndexOf("\"", start);
            return json.Substring(start, end - start);
        }
        return "No text recognized.";
    }
}