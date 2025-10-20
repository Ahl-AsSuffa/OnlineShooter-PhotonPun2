using UnityEngine;
using UnityEngine.UI;

public class ChangePlayerSettings : MonoBehaviour
{
    public Slider GlobalAudioSlider, MusicSlider, MouseSensitivity, AimMouseSensitivity, SniperAimMouseSensitivity;
    public PlayerStats PlayerStats;
    private void Start()
    {
        GetSettings();
    }
    public void GetSettings()
    {
        GlobalAudioSlider.value = PlayerStats.AudioVolume;
        MusicSlider.value = PlayerStats.MusicVolume;
        MouseSensitivity.value = PlayerStats.MouseSensitivity;
        AimMouseSensitivity.value = PlayerStats.AimMouseSentitivity;
        SniperAimMouseSensitivity.value = PlayerStats.SniperAimMouseSensitivity;
    }
    public void SaveChanges()
    {
        PlayerStats.AudioVolume = GlobalAudioSlider.value;
        PlayerStats.MusicVolume = MusicSlider.value;
        PlayerStats.MouseSensitivity = MouseSensitivity.value;
        PlayerStats.AimMouseSentitivity = AimMouseSensitivity.value;
        PlayerStats.SniperAimMouseSensitivity = SniperAimMouseSensitivity.value;
        PlayerStats.UpdateVolumes();
        PlayerStats.SavePlayerSettings();
    }
}
