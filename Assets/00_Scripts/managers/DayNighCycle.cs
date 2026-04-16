using UnityEngine;
public class DayNighCycle : MonoBehaviour
{
    [SerializeField]
    private Game game;

    void Start()
    {
        transform.rotation = Quaternion.Euler(-90, 0, 0);
        game.TimeChanged += Game_TimeChanged;
    }

    private void Game_TimeChanged(object sender, TimeChangedEventArgs e)
    {
        float dayProgress = (float)(e.NewTime / 86400.0);
        float rotation = 360 * dayProgress;
        transform.rotation = Quaternion.Euler(rotation - 90, 0f, 0f);
    }
}
