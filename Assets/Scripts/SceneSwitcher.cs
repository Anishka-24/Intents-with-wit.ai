using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Button References")]
    public Button TTSbutton;
    public Button STTbutton;

    void Start()
    {
        // Assign listeners to buttons
        if (TTSbutton != null)
            TTSbutton.onClick.AddListener(() => LoadSceneByIndex(0));

        if (STTbutton != null)
            STTbutton.onClick.AddListener(() => LoadSceneByIndex(1));
    }

    void LoadSceneByIndex(int sceneIndex)
    {
        // Optional: Add transition effects or checks here
        SceneManager.LoadScene(sceneIndex);
    }
}