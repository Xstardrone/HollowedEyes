using UnityEngine;

public class MenuUIController : MonoBehaviour
{
    public GameObject creditsPanel;
    public GameObject mainMenu;

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
}
