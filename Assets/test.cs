using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] private MainScreenController ui;

    void OnEnable()
    {
        ui.TabSelected += OnTabSelected;
        ui.TimeAdjusted += OnTimeAdjusted;
        ui.SettingsOpened += OnSettingsOpened;
        ui.ExitRequested += OnExitRequested;
    }

    void OnDisable()
    {
        ui.TabSelected -= OnTabSelected;
        ui.TimeAdjusted -= OnTimeAdjusted;
        ui.SettingsOpened -= OnSettingsOpened;
        ui.ExitRequested -= OnExitRequested;
    }

    void OnTabSelected(string tab) { Debug.Log($"Switched to {tab}"); }
    void OnTimeAdjusted(int minutes) { Debug.Log($"Adjust by {minutes} min"); }
    void OnSettingsOpened() { /* open your settings panel */ }
    void OnExitRequested() { Application.Quit(); /* or your own flow */ }
}
