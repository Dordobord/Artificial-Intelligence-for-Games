using System.Collections;
using TMPro;
using UnityEngine;

public class UIGame : MonoBehaviour
{
    public static UIGame main;

    [SerializeField]private TextMeshProUGUI announcementTxt;

    void Awake()
    {
        main = this;
        announcementTxt.gameObject.SetActive(false);
    }

    public void Show(string msg)
    {
        StopAllCoroutines();
        StartCoroutine(AnnouncementRoutine(msg));
    }

    private IEnumerator AnnouncementRoutine(string msg)
    {
        announcementTxt.gameObject.SetActive(true);
        announcementTxt.text = msg;

        yield return new WaitForSeconds(2f);
        announcementTxt.gameObject.SetActive(false);
    }

    public void GameOver()
    {
        announcementTxt.gameObject.SetActive(true);
        announcementTxt.text = "GAME OVER";

        if (AudioManager.main != null)
        {
            AudioManager.main.PlayGameOver();
        }
    }

    public void Victory()
    {
        announcementTxt.gameObject.SetActive(true);
        announcementTxt.text = "YOU WIN!";

        if (AudioManager.main != null)
        {
            AudioManager.main.PlayVictory();
        }
    }
}
