using Oculus.Voice;
using Meta.WitAi.Json;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class WitActivator : MonoBehaviour
{
    [Header("Voice Setup")]
    [SerializeField] private AppVoiceExperience appVoice;
    [SerializeField] private TextMeshProUGUI chatBoxText;

    [Header("Player & Teleport Points (Optional)")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Transform spControlRoom;
    [SerializeField] private Transform spHeliDeck;
    [SerializeField] private Transform spLivingQuarters;

    [Header("Settings")]
    [SerializeField] private float listenDelay = 3f;

    private string lastReply = "";  // For Repeat_Last_Answer intent

    private void OnEnable()
    {
        if (appVoice != null)
        {
            appVoice.VoiceEvents.OnResponse.AddListener(OnWitResponse);
            appVoice.VoiceEvents.OnError.AddListener(OnWitError);
        }
    }

    private void OnDisable()
    {
        if (appVoice != null)
        {
            appVoice.VoiceEvents.OnResponse.RemoveListener(OnWitResponse);
            appVoice.VoiceEvents.OnError.RemoveListener(OnWitError);
        }
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
            ShowInUI("🎤 Listening...");
        }
    }

    private void OnWitResponse(WitResponseNode response)
    {
        string intent = response["intents"][0]?["name"].Value;
        string spokenText = response["text"]?.Value?.ToLower() ?? "";
        Debug.Log($"🎯 Intent Detected: {intent}");
        Debug.Log($"🗣️ Spoken Text: {spokenText}");

        if (HandleActionIntents(intent, spokenText))
        {
            Invoke(nameof(StartListening), listenDelay);
            return;
        }

        string reply = GetReplyForIntent(intent);
        ShowInUI(reply);
        lastReply = reply;

        Invoke(nameof(StartListening), listenDelay);
    }

    private void OnWitError(string error, string message)
    {
        ShowInUI($"❌ Error: {error}");
        Invoke(nameof(StartListening), listenDelay);
    }

    private bool HandleActionIntents(string intent, string spokenText)
    {
        switch (intent)
        {
            // --- Teleports ---
            case "Navigate_ControlRoom": Teleport(spControlRoom); return true;
            case "Navigate_HeliDeck": Teleport(spHeliDeck); return true;
            case "Navigate_LivingQuarters": Teleport(spLivingQuarters); return true;

            // --- Scene Loading ---
            case "Load_Scene":
                LoadSceneByVoice(spokenText);
                return true;

            case "Restart_Scene":
                ShowInUI("🔁 Restarting Scene...");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return true;

            // --- Simulation Control ---
            case "Pause_Simulation":
                Time.timeScale = 0f;
                ShowInUI("⏸ Simulation Paused.");
                return true;

            case "Resume_Simulation":
                Time.timeScale = 1f;
                ShowInUI("▶ Simulation Resumed.");
                return true;

            case "Exit_App":
                ShowInUI("👋 Exiting Application...");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                return true;

            // --- Interactive Features ---
            case "Highlight_Module":
                ShowInUI("✨ Highlighting Module (placeholder)");
                return true;

            case "Toggle_Annotations":
                ShowInUI("📝 Toggling annotations (placeholder)");
                return true;

            case "Play_Simulation":
                ShowInUI("▶ Playing simulation (placeholder)");
                return true;

            case "Stop_Simulation":
                ShowInUI("⏹ Stopping simulation (placeholder)");
                return true;

            case "Repeat_Last_Answer":
                if (!string.IsNullOrEmpty(lastReply))
                    ShowInUI("🔁 " + lastReply);
                else
                    ShowInUI("No previous answer to repeat.");
                return true;

            default: return false;
        }
    }

    private void LoadSceneByVoice(string spokenText)
    {
        string sceneToLoad = "FireDrill"; // default fallback

        if (spokenText.Contains("lobby")) sceneToLoad = "Lobby";
        else if (spokenText.Contains("menu")) sceneToLoad = "MainMenu";
        else if (spokenText.Contains("training") || spokenText.Contains("firedrill")) sceneToLoad = "FireDrill";

        if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            ShowInUI($"📦 Loading {sceneToLoad}...");
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
        else
        {
            ShowInUI($"❌ Scene '{sceneToLoad}' not found in Build Settings!");
            Debug.LogError($"Scene '{sceneToLoad}' not found in Build Settings!");
        }
    }

    private void Teleport(Transform target)
    {
        if (playerRoot == null || target == null)
        {
            ShowInUI("⚠️ Teleport failed: PlayerRoot or target not set.");
            return;
        }

        playerRoot.SetPositionAndRotation(target.position, target.rotation);
        ShowInUI($"🗺️ Teleported to {target.name}");
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

    private void ShowInUI(string message)
    {
        Debug.Log($"🤖 {message}");
        if (chatBoxText != null)
        {
            chatBoxText.text = message;
        }
    }
}
