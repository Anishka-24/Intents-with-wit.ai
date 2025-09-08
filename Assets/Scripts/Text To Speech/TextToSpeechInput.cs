using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Meta.WitAi.TTS.Utilities; // From Meta Voice SDK

public class TextToSpeechInput : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button submitButton;
    public TTSSpeaker speaker; // Drag the TTSSpeaker component here

    void Start()
    {
        submitButton.onClick.AddListener(OnSubmit);
    }

    void OnSubmit()
    {
        string textToSpeak = inputField.text;

        if (!string.IsNullOrEmpty(textToSpeak) && speaker != null)
        {
            speaker.Speak(textToSpeak);
        }
    }
}