using UnityEngine;
using UnityEngine.UI;

public class ToggleAudio : MonoBehaviour
{
    private static bool isInitialized = false;
    private Toggle soundToggle;

    private void Awake()
    {
        soundToggle = GetComponent<Toggle>();
        if (soundToggle != null)
        {
            // Only set initial state if not already initialized
            if (!isInitialized)
            {
                bool isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
                AudioListener.volume = isSoundOn ? 1f : 0f;
                isInitialized = true;
            }

            // Always sync the toggle state with the current AudioListener volume
            soundToggle.isOn = AudioListener.volume > 0f;
            soundToggle.onValueChanged.AddListener(Mute);
        }
        else
        {
            Debug.LogError("Toggle component not found on GameObject!");
        }
    }

    private void OnEnable()
    {
        // Sync toggle state whenever the component is enabled
        if (soundToggle != null)
        {
            soundToggle.isOn = AudioListener.volume > 0f;
        }
    }

    private void Mute(bool isOn)
    {
        AudioListener.volume = isOn ? 1f : 0f;
        PlayerPrefs.SetInt("SoundOn", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (soundToggle != null)
        {
            soundToggle.onValueChanged.RemoveListener(Mute);
        }
    }
}