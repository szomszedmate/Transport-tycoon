using UnityEngine;
using UnityEngine.UI;

public class SFXControl : MonoBehaviour
{
    public Image icon;
    public Sprite highVolumeSprite;
    public Sprite lowVolumeSprite;
    public Sprite muteSprite;

    public void UpdateIcon(float volume)
    {
        if (icon == null) return;
        if (volume == 0)
            icon.sprite = muteSprite;
        else if (volume < 0.5f)
            icon.sprite = lowVolumeSprite;
        else
            icon.sprite = highVolumeSprite;
    }
}
