using UnityEngine;

public class HandlePauseIcon : MonoBehaviour
{
    [SerializeField] GameObject pauseIcon;
    [SerializeField] GameObject playIcon;

    private bool oldTimeTick;

    // Update is called once per frame
    void Update()
    {
        if (TimeTickSystem.DoTimeTick != oldTimeTick)
        {
            if (TimeTickSystem.DoTimeTick)
            {
                pauseIcon.SetActive(false);
                playIcon.SetActive(true);
            }
            else
            {
                pauseIcon.SetActive(true);
                playIcon.SetActive(false);
            }
        }
        oldTimeTick = TimeTickSystem.DoTimeTick;
    }
}
