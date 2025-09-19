using System;
using System.ComponentModel;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    [Header("Goal (HH:MM:SS)")]
    public int goalHours = 0;
    public int goalMinutes = 1;

    [SerializeField]
    private float elapsed; // seconds
    [SerializeField]
    private float goal;    // seconds
    [SerializeField]
    private bool running;

    public MainScreenController screenControler;
    public SetGoalModalController settingsMenu;

    public string state = "Running";

    private void Start()
    {
        //load data
        int day = DateTime.Now.Day;

        elapsed = PlayerPrefs.GetFloat("progress",0);
        goalHours = PlayerPrefs.GetInt("goalHours", 8);
        goalMinutes = PlayerPrefs.GetInt("goalMinutes", 0);
        int wasDay = PlayerPrefs.GetInt("WasDate", 0);

        //setup
        SetGoal(goalHours, goalMinutes);

        screenControler.TimeAdjusted += AdjustTime;
        screenControler.ExitRequested += Exit;
        screenControler.ResetRequested += ResetProgress;
        screenControler.ActionButtonCliked += StateChange;

        settingsMenu.Confirmed += SetGoal;

        //check if new days
        if(wasDay != day)
        {
            elapsed = 0;
            PlayerPrefs.SetInt("WasDate", day);
        }
    }

    private void StateChange()
    {
        if(state == "Running")
        {
            state = "Stopped";
            running = false;  
        }
        else
        {
            state = "Running";
            running = true;
        }

        screenControler.SetState(state);
    }

    private void OnDestroy()
    {
        screenControler.TimeAdjusted -= AdjustTime;
        screenControler.ExitRequested -= Exit;
        screenControler.ResetRequested -= ResetProgress;

        
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetFloat("progress", elapsed);
        PlayerPrefs.SetInt("WasDate", DateTime.Now.Day);
    }

    void Update()
    {
        if (!running) return; 
        elapsed += Time.deltaTime;

        screenControler.updateTimeDisplay($"{FormatHMS(elapsed)}/{goalHours:00}:{goalMinutes:00}", GetProgress()); 

    }

    public void StartTimer() { running = true; }
    public void PauseTimer() { running = false; }


    public void SetGoal(int hours, int minutes)
    {
        goalHours = hours;
        goalMinutes = minutes;
        goal = HmsToSeconds(hours, minutes,0);
        //save
        PlayerPrefs.SetInt("goalHours", goalHours);
        PlayerPrefs.SetInt("goalMinutes", goalMinutes);

        screenControler.updateTimeDisplay($"{FormatHMS(elapsed)}/{goalHours:00}:{goalMinutes:00}", GetProgress());

    }

    public float GetProgress() => (goal > 0) ? (elapsed / goal) : 1;

    static float HmsToSeconds(int h, int m, int s) => (h * 3600.0f) + (m * 60.0f) + s;

    static string FormatHMS(float totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0;
        var ts = System.TimeSpan.FromSeconds(totalSeconds);
        // Hours can exceed 24, so use TotalHours for HH field
        int hours = (int)ts.TotalHours;
        return $"{hours:00}:{ts.Minutes:00}";
    }

    private void AdjustTime(int time) //attached to ui events will adjust timer
    {
        elapsed += (time * 60);
        if(elapsed < 0)
        {
            elapsed = 0;
        }
    }

    private void Exit()
    {
        print("test");
        Application.Quit();
    }

    private void ResetProgress()
    {
        elapsed = 0;
        screenControler.updateTimeDisplay($"{FormatHMS(elapsed)}/{goalHours:00}:{goalMinutes:00}", GetProgress());
    }

}
