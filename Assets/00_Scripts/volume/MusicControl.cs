using UnityEngine;
using UnityEngine.UI;

public class MusicControl : MonoBehaviour
{
    public Image icon;
    public Sprite highVolumeSprite;
    public Sprite muteSprite;

    public void UpdateIcon(float volume)
    {
        if (icon == null) return;
        icon.sprite = volume == 0 ? muteSprite : highVolumeSprite;
    }
}
