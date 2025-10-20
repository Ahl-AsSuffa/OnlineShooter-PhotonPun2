using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnsTexture : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite DefaultTexture, CustomTexture;
    public Image CurrentTexture;
    public AudioClip SoundClip;
    public AudioSource AudioSource;
    public void OnPointerEnter(PointerEventData eventData)
    {
        CurrentTexture.sprite = CustomTexture;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CurrentTexture.sprite = DefaultTexture;
    }
    public void OnDisable()
    {
        CurrentTexture.sprite = DefaultTexture;
    }
    public void PlaySound()
    {
        if(AudioSource != null)
            AudioSource.PlayOneShot(SoundClip);
    }
}
