using UnityEngine;

public class MenuUIController : MonoBehaviour
{
    public GameObject creditsPanel;
    public GameObject mainMenu;

    public GameObject levelsMenu;

    public void ShowCredits()
    {
        mainMenu.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        creditsPanel.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void ShowLevels()
    {
        mainMenu.SetActive(false);
        levelsMenu.SetActive(true);
    }
}
