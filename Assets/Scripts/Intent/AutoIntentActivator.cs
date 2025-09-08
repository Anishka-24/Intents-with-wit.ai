using Oculus.Voice;
using UnityEngine;

public class AutoIntentActivator : MonoBehaviour
{
    [SerializeField] private AppVoiceExperience appVoice;

    private void Start()
    {
        if (appVoice != null)
        {
            Debug.Log("Auto-activating Wit.ai listening...");
            appVoice.Activate();
        }
    }
}
