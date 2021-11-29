using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{

    public GameObject MainMenu;
    public GameObject DanceSelectionMenu;

    // Start is called before the first frame update
    void Start()
    {
        navigateToMainMenu();
    }

    public void NavigateToDanceMenu()
    {
        MainMenu?.SetActive(false);
        DanceSelectionMenu?.SetActive(true);
    }

    public void navigateToMainMenu()
    {
        MainMenu?.SetActive(true);
        DanceSelectionMenu?.SetActive(false);
    }
}
