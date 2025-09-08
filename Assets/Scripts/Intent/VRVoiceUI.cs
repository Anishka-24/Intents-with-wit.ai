using UnityEngine;
using TMPro;
using Oculus.Voice;
using Meta.WitAi.Json;

public class VRVoiceUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AppVoiceExperience appVoice;  // Drag your AppVoiceExperience here
    [SerializeField] private TMP_Text statusText;          // Shows listening/processing status
    [SerializeField] private TMP_Text chatBoxText;         // Shows final AI reply

    [Header("Settings")]
    [SerializeField] private float listenDelay = 3f;       // Wait before listening again

    private void OnEnable()
    {
        if (appVoice == null)
        {
            Debug.LogError("AppVoiceExperience not assigned!");
            return;
        }

        // Subscribe to voice events
        appVoice.VoiceEvents.OnStartListening.AddListener(OnStartListening);
        appVoice.VoiceEvents.OnStoppedListening.AddListener(OnStoppedListening);
        appVoice.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoice.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);
        appVoice.VoiceEvents.OnResponse.AddListener(OnWitResponse);
        appVoice.VoiceEvents.OnError.AddListener(OnWitError);
    }

    private void OnDisable()
    {
        if (appVoice == null) return;

        // Unsubscribe from events
        appVoice.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
        appVoice.VoiceEvents.OnStoppedListening.RemoveListener(OnStoppedListening);
        appVoice.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
        appVoice.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);
        appVoice.VoiceEvents.OnResponse.RemoveListener(OnWitResponse);
        appVoice.VoiceEvents.OnError.RemoveListener(OnWitError);
    }

    private void Start()
    {
        StartListening();
    }

    private void StartListening()
    {
        if (appVoice != null && !appVoice.Active)
        {
            appVoice.Activate();
            SetStatusText("🎤 Listening...");
        }
    }

    // ---------------- Event Handlers ----------------

    private void OnStartListening()
    {
        SetStatusText("🎤 Listening...");
    }

    private void OnStoppedListening()
    {
        SetStatusText("⏳ Processing...");
    }

    private void OnPartialTranscription(string text)
    {
        SetStatusText("Partial: " + text);
    }

    private void OnFullTranscription(string text)
    {
        SetStatusText("Heard: " + text);
    }

    private void OnWitResponse(WitResponseNode response)
    {
        string intent = response?["intents"]?[0]?["name"]?.Value;

        string reply = GetReplyForIntent(intent);

        ShowReply(reply);  // Show final reply in chatBoxText

        // Wait a few seconds, then listen again
        Invoke(nameof(StartListening), listenDelay);
    }

    private void OnWitError(string error, string message)
    {
        ShowReply($"❌ Error: {error}");
        Invoke(nameof(StartListening), listenDelay);
    }

    // ---------------- Helpers ----------------

    private void SetStatusText(string msg)
    {
        if (statusText != null)
            statusText.text = msg;

        Debug.Log("[Status] " + msg);
    }

    private void ShowReply(string msg)
    {
        if (chatBoxText != null)
            chatBoxText.text = msg;

        Debug.Log("[Reply] " + msg);
    }

    private string GetReplyForIntent(string intent)
    {
        switch (intent)
        {
            case "greet_user": return "Hello! How can I help you today?";
            case "Get_Regulations": return "Fire safety regulations require clear exits, alarms tested, and extinguishers ready.";
            case "Get_Timeline": return "Timeline: Step 1 - Report issue, Step 2 - Investigation, Step 3 - Resolution.";
            case "Get_Cause": return "The incident was caused by a pressure valve failure.";
            case "Get_Incident_Overview": return "It was a small fire, quickly contained, no casualties.";
            case "Get_Human_Factors": return "Human error contributed — inspection was delayed by 3 days.";
            case "Get_Safety_Systems": return "Sprinklers, alarms, and fire doors were active.";
            case "Get_Emergency_Response": return "Emergency team arrived in 3 minutes and evacuated everyone.";
            case "Get_Rescue_Info": return "Five people were rescued by the on-site safety team.";
            case "Get_Safety_Case_Info": return "Safety case says plant can keep running with stricter inspections.";
            case "Get_Aftermath": return "Weekly safety drills were scheduled after the incident.";
            case "Get_Findings": return "Report found missing maintenance checks as root cause.";
            case "Get_Lessons_Learned": return "Lesson: Never delay inspections and always check safety valves.";
            case "Get_Glossary_Term": return "A vessel is a container designed to hold fluids or gases under pressure.";
            default: return "Sorry, I didn't understand that. Can you rephrase?";
        }
    }
}
