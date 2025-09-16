using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

/// <summary>
/// Main screen controller (top/bottom adjustment layout).
/// Events:
///  - TabSelected(string): "Day" | "Week" | "Month"
///  - TimeAdjusted(int): minutes (negative for minus buttons)
///  - SettingsOpened()
///  - ExitRequested()
/// </summary>
public class MainScreenController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    // Events to expose UI interactions
    public event Action<string> TabSelected;
    public event Action<int> TimeAdjusted;
    public event Action SettingsOpened;
    public event Action ExitRequested;

    // UI refs
    private Button dayTabButton, weekTabButton, monthTabButton;
    private Label modeLabel, timeLabel;
    private Button settingsButton, exitButton;

    // Minus (top)
    private Button sub1MinButton, sub15MinButton, sub30MinButton, sub1HrButton;
    // Plus (bottom)
    private Button add1MinButton, add15MinButton, add30MinButton, add1HrButton;

    private readonly List<Button> _tabs = new();

    private DonutRing ring;

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Tabs
        dayTabButton = root.Q<Button>("dayTabButton");
        weekTabButton = root.Q<Button>("weekTabButton");
        monthTabButton = root.Q<Button>("monthTabButton");
        _tabs.Clear(); _tabs.AddRange(new[] { dayTabButton, weekTabButton, monthTabButton });

        // Labels / misc
        modeLabel = root.Q<Label>("modeLabel");
        timeLabel = root.Q<Label>("timeLabel");

        settingsButton = root.Q<Button>("settingsButton");
        exitButton = root.Q<Button>("exitButton");

        // Adjust groups
        sub1MinButton = root.Q<Button>("sub1MinButton");
        sub15MinButton = root.Q<Button>("sub15MinButton");
        sub30MinButton = root.Q<Button>("sub30MinButton");
        sub1HrButton = root.Q<Button>("sub1HrButton");

        add1MinButton = root.Q<Button>("add1MinButton");
        add15MinButton = root.Q<Button>("add15MinButton");
        add30MinButton = root.Q<Button>("add30MinButton");
        add1HrButton = root.Q<Button>("add1HrButton");

        //ring
        ring = root.Q<DonutRing>("ring");

        // Hook events (use named methods so we can cleanly unsubscribe)
        if (dayTabButton != null) dayTabButton.clicked += OnDayTabClicked;
        if (weekTabButton != null) weekTabButton.clicked += OnWeekTabClicked;
        if (monthTabButton != null) monthTabButton.clicked += OnMonthTabClicked;

        if (settingsButton != null) settingsButton.clicked += OnSettingsClicked;
        if (exitButton != null) exitButton.clicked += OnExitClicked;

        if (sub1MinButton != null) sub1MinButton.clicked += OnSub1Min;
        if (sub15MinButton != null) sub15MinButton.clicked += OnSub15Min;
        if (sub30MinButton != null) sub30MinButton.clicked += OnSub30Min;
        if (sub1HrButton != null) sub1HrButton.clicked += OnSub1Hr;

        if (add1MinButton != null) add1MinButton.clicked += OnAdd1Min;
        if (add15MinButton != null) add15MinButton.clicked += OnAdd15Min;
        if (add30MinButton != null) add30MinButton.clicked += OnAdd30Min;
        if (add1HrButton != null) add1HrButton.clicked += OnAdd1Hr;

        // Default selection
        SetActiveTab(dayTabButton, "Day");
    }

    private void OnDisable()
    {
        if (dayTabButton != null) dayTabButton.clicked -= OnDayTabClicked;
        if (weekTabButton != null) weekTabButton.clicked -= OnWeekTabClicked;
        if (monthTabButton != null) monthTabButton.clicked -= OnMonthTabClicked;

        if (settingsButton != null) settingsButton.clicked -= OnSettingsClicked;
        if (exitButton != null) exitButton.clicked -= OnExitClicked;

        if (sub1MinButton != null) sub1MinButton.clicked -= OnSub1Min;
        if (sub15MinButton != null) sub15MinButton.clicked -= OnSub15Min;
        if (sub30MinButton != null) sub30MinButton.clicked -= OnSub30Min;
        if (sub1HrButton != null) sub1HrButton.clicked -= OnSub1Hr;

        if (add1MinButton != null) add1MinButton.clicked -= OnAdd1Min;
        if (add15MinButton != null) add15MinButton.clicked -= OnAdd15Min;
        if (add30MinButton != null) add30MinButton.clicked -= OnAdd30Min;
        if (add1HrButton != null) add1HrButton.clicked -= OnAdd1Hr;
    }

    /* ------------ Public helpers ------------ */

    /// <summary>Update the time text, e.g., "01:15 / 02:00".</summary>
    public void SetTimeText(string display)
    {
        if (timeLabel != null) timeLabel.text = display;
    }

    /// <summary>Programmatically set active mode.</summary>
    public void SetActiveMode(string mode)
    {
        switch (mode)
        {
            case "Week": SetActiveTab(weekTabButton, "Week"); break;
            case "Month": SetActiveTab(monthTabButton, "Month"); break;
            default: SetActiveTab(dayTabButton, "Day"); break;
        }
    }

    /* ------------ Internal ------------ */

    private void OnDayTabClicked() => OnTabClicked(dayTabButton, "Day");
    private void OnWeekTabClicked() => OnTabClicked(weekTabButton, "Week");
    private void OnMonthTabClicked() => OnTabClicked(monthTabButton, "Month");

    private void OnSettingsClicked() => SettingsOpened?.Invoke();
    private void OnExitClicked() => ExitRequested?.Invoke();

    private void OnSub1Min() => TimeAdjusted?.Invoke(-1);
    private void OnSub15Min() => TimeAdjusted?.Invoke(-15);
    private void OnSub30Min() => TimeAdjusted?.Invoke(-30);
    private void OnSub1Hr() => TimeAdjusted?.Invoke(-60);

    private void OnAdd1Min() => TimeAdjusted?.Invoke(+1);
    private void OnAdd15Min() => TimeAdjusted?.Invoke(+15);
    private void OnAdd30Min() => TimeAdjusted?.Invoke(+30);
    private void OnAdd1Hr() => TimeAdjusted?.Invoke(+60);

    private void OnTabClicked(Button button, string label)
    {
        SetActiveTab(button, label);
        TabSelected?.Invoke(label);
    }

    private void SetActiveTab(Button active, string label)
    {
        foreach (var t in _tabs)
        {
            if (t == null) continue;
            t.RemoveFromClassList("selected");
        }
        if (active != null) active.AddToClassList("selected");
        if (modeLabel != null) modeLabel.text = label;
    }

    public void updateTimeDisplay(string text, float progress)
    {
        timeLabel.text = text;
        ring.value = progress;
    }
}
