using UnityEngine;
using TMPro;

public class TextVisibilityApplier : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;

    void Awake()
    {
        bool hide = PlayerPrefs.GetInt("hideText", 0) == 1;
        foreach (var tmp in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            tmp.enabled = !hide;

        if (languageDropdown != null)
            languageDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt("languageIndex", 0));
    }
}
