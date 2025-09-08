using UnityEngine;
using TMPro;
using Oculus.Voice;          // For AppVoiceExperience
using Meta.WitAi.Json;       // For WitResponseNode

public class VoiceUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AppVoiceExperience appVoice;  // Drag your App Voice Experience here
    [SerializeField] private TMP_Text statusText;          // Drag your StatusText TMP object here

    private void OnEnable()
    {
        if (appVoice == null)
        {
            Debug.LogError("AppVoiceExperience not assigned!");
            return;
        }

        // Hook into all key events
        appVoice.VoiceEvents.OnStartListening.AddListener(OnStartListening);
        appVoice.VoiceEvents.OnStoppedListening.AddListener(OnStoppedListening);
        appVoice.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoice.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);
        appVoice.VoiceEvents.OnResponse.AddListener(OnResponse);
    }

    private void OnDisable()
    {
        if (appVoice == null) return;

        // Unsubscribe when disabled
        appVoice.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
        appVoice.VoiceEvents.OnStoppedListening.RemoveListener(OnStoppedListening);
        appVoice.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
        appVoice.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);
        appVoice.VoiceEvents.OnResponse.RemoveListener(OnResponse);
    }

    // ---------------- Event Handlers ----------------

    private void OnStartListening()
    {
        SetText("🎤 Listening...");
    }

    private void OnStoppedListening()
    {
        SetText("⏳ Processing...");
    }

    private void OnPartialTranscription(string text)
    {
        SetText("Partial: " + text);
    }

    private void OnFullTranscription(string text)
    {
        SetText("Heard: " + text);
    }

    private void OnResponse(WitResponseNode response)
    {
        string intent = response?["intents"]?[0]?["name"]?.Value;
        if (!string.IsNullOrEmpty(intent))
            SetText("✅ Intent: " + intent);
        else
            SetText("⚠ No intent detected. Text: " + response?["text"]?.Value);
    }

    // ---------------- Helper ----------------
    private void SetText(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
        Debug.Log(msg);
    }
}
