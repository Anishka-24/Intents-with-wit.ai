using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Utilities;

public class TTSSpeakerDropdownController : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Dropdown to select voice preset")]
    [SerializeField] private TMP_Dropdown voicePresetDropdown;

    private TTSSpeaker speaker;

    void Start()
    {
        speaker = FindObjectOfType<TTSSpeaker>();
        if (speaker == null)
        {
            Debug.LogError("TTSSpeaker not found in scene.");
            return;
        }

        InitializeDropdown();
    }

    void InitializeDropdown()
    {
        var presets = Meta.WitAi.TTS.TTSService.Instance.GetAllPresetVoiceSettings()
            .Where(p => !string.IsNullOrEmpty(p.SettingsId))
            .Select(p => p.SettingsId)
            .Distinct()
            .ToList();

        if (presets.Count == 0)
        {
            Debug.LogWarning("No voice presets found from TTSService.");
            return;
        }

        voicePresetDropdown.ClearOptions();
        voicePresetDropdown.AddOptions(presets);

        int currentIndex = presets.IndexOf(speaker.presetVoiceID);
        voicePresetDropdown.value = currentIndex >= 0 ? currentIndex : 0;
        voicePresetDropdown.RefreshShownValue();

        voicePresetDropdown.onValueChanged.AddListener(OnPresetChanged);
    }

    void OnPresetChanged(int index)
    {
        string selectedPreset = voicePresetDropdown.options[index].text;
        speaker.presetVoiceID = selectedPreset;
        Debug.Log($"Voice preset updated to: {selectedPreset}");
    }
}