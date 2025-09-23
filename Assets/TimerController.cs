using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TimerController : MonoBehaviour
{
    private float goal;    // seconds
    [SerializeField]
    private bool running;

    public MainScreenController screenControler;
    public SetGoalModalController settingsMenu;
    public SaveData save;


    public string state = "Running";

    public mode ActiveMode;


    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        //load data
        int day = DateTime.Now.DayOfYear;
        int weekStart = DateTime.Now.AddDays(-(DateTime.Now.DayOfYear - (((int)DateTime.Now.DayOfWeek) - 1))).DayOfYear; // gets the day of year of the monday of the week 
        int month = DateTime.Now.Month;

        save = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("saveData",""));
        if(save.modes.Length == 0)
        {
            save.modes = new mode[3];
            save.modes[0] = new mode();
            save.modes[0].name = "Day";
            save.modes[1] = new mode();
            save.modes[1].name = "Week";
            save.modes[2] = new mode();
            save.modes[2].name = "Month";
        }

        ActiveMode = save.modes[0];
        //setup
        SetGoal(ActiveMode.goalHours, ActiveMode.goalMinutes);

        screenControler.TimeAdjusted += AdjustTime;
        screenControler.ExitRequested += Exit;
        screenControler.ResetRequested += ResetProgress;
        screenControler.ActionButtonCliked += StateChange;

        screenControler.TabSelected += SetActiveTab;

        settingsMenu.Confirmed += SetGoal;

        if(PlayerPrefs.GetInt("day",0) != day)
        {
            save.modes[0].elapsed = 0;
        }

        if (PlayerPrefs.GetInt("week", 0) != weekStart)
        {
            save.modes[1].elapsed = 0;
        }

        if (PlayerPrefs.GetInt("month", 0) != month)
        {
            save.modes[2].elapsed = 0;
        }

        PlayerPrefs.SetInt("day", day);
        PlayerPrefs.SetInt("week", weekStart);
        PlayerPrefs.SetInt("month", month);
    }

    private void SetActiveTab(string obj)
    {
        try
        {
            ActiveMode = save.modes.First(x => x.name == obj);
            goal = HmsToSeconds(ActiveMode.goalHours, ActiveMode.goalMinutes, 0);
        }
        catch
        {
            //issue with save file wipe and make a new one
            save.modes = new mode[3];


            //NOTE: clean this up later make it not as hard coded
            save.modes[0] = new mode();
            save.modes[0].name = "Day";
            save.modes[1] = new mode();
            save.modes[1].name = "Week";
            save.modes[2] = new mode();
            save.modes[2].name = "Month";

            ActiveMode = save.modes.First(x => x.name == obj);
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
        PlayerPrefs.SetString("saveData",JsonUtility.ToJson(save));
    }

    void Update()
    {

        //make sure the fps is limiterd
        if (Application.targetFrameRate != 30)
            Application.targetFrameRate = 30;

        if (!running) return;
        for (int i = 0; i < save.modes.Length; i++ )
        {
            save.modes[i].elapsed += Time.deltaTime;
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



        if (ActiveMode.name == "Month")
        {
            save.modes[2].elapsed += (time * 60);
            if (save.modes[2].elapsed < 0)
            {
                save.modes[2].elapsed = 0;
            }
        }

        if (ActiveMode.name == "Week")
        {
            save.modes[2].elapsed += (time * 60);
            if (save.modes[2].elapsed < 0)
            {
                save.modes[2].elapsed = 0;
            }

            save.modes[1].elapsed += (time * 60);
            if (save.modes[1].elapsed < 0)
            {
                save.modes[1].elapsed = 0;
            }
        }

        // realy need to clean up how i do thease
        if (ActiveMode.name == "Day")
        {
            save.modes[2].elapsed += (time * 60);
            if (save.modes[2].elapsed < 0)
            {
                save.modes[2].elapsed = 0;
            }

            save.modes[1].elapsed += (time * 60);
            if (save.modes[1].elapsed < 0)
            {
                save.modes[1].elapsed = 0;
            }

            save.modes[0].elapsed += (time * 60);
            if (save.modes[0].elapsed < 0)
            {
                save.modes[0].elapsed = 0;
            }
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

        public mode()
        {
            name = "";
            goalHours = 0;
            goalMinutes = 0;
            elapsed = 0;
        }
    }

    [Serializable]
    public struct SaveData
    {
        public mode[] modes;
    }
}
