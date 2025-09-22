using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    private float goal;    // seconds
    [SerializeField]
    private bool running;

    public MainScreenController screenControler;
    public SetGoalModalController settingsMenu;
    public mode[] modes;

    public string state = "Running";

    public mode ActiveMode;

    private void Start()
    {
        //load data
        int day = DateTime.Now.Day;


        ActiveMode = modes[0];
        //setup
        SetGoal(ActiveMode.goalHours, ActiveMode.goalMinutes);

        screenControler.TimeAdjusted += AdjustTime;
        screenControler.ExitRequested += Exit;
        screenControler.ResetRequested += ResetProgress;
        screenControler.ActionButtonCliked += StateChange;

        screenControler.TabSelected += SetActiveTab;

        settingsMenu.Confirmed += SetGoal;
    }

    private void SetActiveTab(string obj)
    {
        ActiveMode = modes.First(x => x.name == obj);
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
       
    }

    void Update()
    {
        if (!running) return;
        for (int i = 0; i < modes.Length; i++ )
        {
            modes[i].elapsed += Time.deltaTime;
        }

        screenControler.updateTimeDisplay($"{FormatHMS(ActiveMode.elapsed)}/{ActiveMode.goalHours:00}:{ActiveMode.goalMinutes:00}", GetProgress()); 

    }

    public void StartTimer() { running = true; }
    public void PauseTimer() { running = false; }


    public void SetGoal(int hours, int minutes)
    {
        ActiveMode.goalHours = hours;
        ActiveMode.goalMinutes = minutes;
        goal = HmsToSeconds(hours, minutes,0);
        //save

        screenControler.updateTimeDisplay($"{FormatHMS(ActiveMode.elapsed)}/{ActiveMode.goalHours:00}:{ActiveMode.goalMinutes:00}", GetProgress());

    }

    public float GetProgress() => (goal > 0) ? (ActiveMode.elapsed / goal) : 1;

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
        ActiveMode.elapsed += (time * 60);
        if(ActiveMode.elapsed < 0)
        {
            ActiveMode.elapsed = 0;
        }
    }

    private void Exit()
    {
        print("test");
        Application.Quit();
    }

    private void ResetProgress()
    {
        ActiveMode.elapsed = 0;
        screenControler.updateTimeDisplay($"{FormatHMS(ActiveMode.elapsed)}/{ActiveMode.goalHours:00}:{ActiveMode.goalMinutes:00}", GetProgress());
    }

    [Serializable]
    public class mode
    {
        public string name;
        public int goalHours;
        public int goalMinutes;
        public float elapsed;
    }
}
