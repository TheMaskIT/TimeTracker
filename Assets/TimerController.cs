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

    private void Start()
    {
        SetGoal(goalHours, goalMinutes);
    }

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;

        screenControler.updateTimeDisplay($"{FormatHMS(elapsed)}/{goalHours}:{goalMinutes}", GetProgress());

    }

    public void StartTimer() { running = true; }
    public void PauseTimer() { running = false; }


    public void SetGoal(int hours, int minutes)
    {
        goal = HmsToSeconds(hours, minutes,0);
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
}
