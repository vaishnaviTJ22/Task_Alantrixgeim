using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelsPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void OnPlayClicked()
    {
        ShowLevels();
    }

    public void OnBackClicked()
    {
        ShowMainMenu();
    }

    public void OnQuitClicked()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    private void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelsPanel != null) levelsPanel.SetActive(false);
    }

    private void ShowLevels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelsPanel != null) levelsPanel.SetActive(true);
    }
}
