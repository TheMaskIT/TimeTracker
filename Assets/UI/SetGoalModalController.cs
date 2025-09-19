using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Numpad "Set Goal" modal shown over the main screen (Template instance A2).
/// Call Show(mode, hours, minutes) / Hide().
/// Subscribe to Confirmed(hours, minutes) and Canceled().
/// </summary>
public class SetGoalModalController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    public event Action<int, int> Confirmed;
    public event Action Canceled;

    // Scope root to avoid ID clashes with main screen
    VisualElement modal, card, scrim;
    Label modeLabel, hVal, mVal;
    Button hTab, mTab, confirmBtn, cancelBtn, clearBtn, backBtn;

    enum Field { Hours, Minutes }
    Field active = Field.Hours;

    string hoursBuf = "00";
    string minutesBuf = "00";

    void OnEnable()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Find the instance and then query INSIDE it
        modal = root.Q<VisualElement>("settingMenu");
        if (modal == null) return; // instance not added yet

        scrim = modal.Q<VisualElement>("goalScrim");
        card = modal.Q<VisualElement>("goalCard");
        modeLabel = modal.Q<Label>("goalModeLabel");

        hTab = modal.Q<Button>("goalHoursTab");
        mTab = modal.Q<Button>("goalMinutesTab");
        hVal = modal.Q<Label>("goalHoursValue");
        mVal = modal.Q<Label>("goalMinutesValue");

        confirmBtn = modal.Q<Button>("goalConfirmButton");
        cancelBtn = modal.Q<Button>("goalCancelButton");
        clearBtn = modal.Q<Button>("goalClear");
        backBtn = modal.Q<Button>("goalBackspace");

        // digits
        for (int n = 0; n <= 9; n++)
        {
            var b = modal.Q<Button>("goalK" + n);
            if (b != null) { int d = n; b.clicked += () => PushDigit(d); }
        }

        if (hTab != null) hTab.clicked += () => SetActive(Field.Hours);
        if (mTab != null) mTab.clicked += () => SetActive(Field.Minutes);

        if (clearBtn != null) clearBtn.clicked += ClearActive;
        if (backBtn != null) backBtn.clicked += Backspace;

        if (confirmBtn != null) confirmBtn.clicked += () => { Confirmed?.Invoke(Parse(hoursBuf), Parse(minutesBuf)); Hide(); };
        if (cancelBtn != null) cancelBtn.clicked += () => { Canceled?.Invoke(); Hide(); };

        // Optional: tap outside to cancel
        if (scrim != null) scrim.RegisterCallback<ClickEvent>(_ => { Canceled?.Invoke(); Hide(); });

        Hide(); // start hidden
    }

    /* ---------- Public API ---------- */

    public void Show(string mode, int hours, int minutes)
    {
        
        if (modal == null)
        {
            return;
        };
        modeLabel.text = mode;
        hoursBuf = ClampTwoDigits(hours, 0, 99);
        minutesBuf = ClampTwoDigits(minutes, 0, 59);
        UpdateDisplay();
        SetActive(Field.Hours);

        modal.style.display = DisplayStyle.Flex;
        modal.style.visibility = Visibility.Visible;
        modal.pickingMode = PickingMode.Position;
    }

    public void Hide()
    {
        if (modal != null)
        {
            modal.style.display = DisplayStyle.None;
            modal.style.visibility = Visibility.Hidden;
            modal.pickingMode = PickingMode.Ignore;
        }
    }

    /* ---------- Internals ---------- */

    void SetActive(Field f)
    {
        active = f;
        hTab.RemoveFromClassList("selected");
        mTab.RemoveFromClassList("selected");
        (active == Field.Hours ? hTab : mTab).AddToClassList("selected");
    }

    void UpdateDisplay()
    {
        if (hVal != null) hVal.text = hoursBuf;
        if (mVal != null) mVal.text = minutesBuf;
    }

    void PushDigit(int d)
    {
        if (active == Field.Hours)
        {
            hoursBuf = ShiftAppend(hoursBuf, d);
            hoursBuf = ClampTwoDigits(Parse(hoursBuf), 0, 99);
        }
        else
        {
            minutesBuf = ShiftAppend(minutesBuf, d);
            minutesBuf = ClampTwoDigits(Parse(minutesBuf), 0, 59);
        }
        UpdateDisplay();
    }

    void Backspace()
    {
        if (active == Field.Hours) hoursBuf = BackspaceBuf(hoursBuf);
        else minutesBuf = BackspaceBuf(minutesBuf);
        UpdateDisplay();
    }

    void ClearActive()
    {
        if (active == Field.Hours) hoursBuf = "00";
        else minutesBuf = "00";
        UpdateDisplay();
    }

    /* ---------- Helpers ---------- */

    static string ShiftAppend(string buf, int d)
    {
        string s = string.IsNullOrEmpty(buf) ? "00" : buf;
        if (s.Length < 2) s = s.PadLeft(2, '0');
        return s.Substring(1) + Mathf.Clamp(d, 0, 9);
    }
    static string BackspaceBuf(string buf)
    {
        string s = string.IsNullOrEmpty(buf) ? "00" : buf;
        if (s.Length < 2) s = s.PadLeft(2, '0');
        return "0" + s.Substring(0, 1);
    }
    static string ClampTwoDigits(int val, int min, int max)
    {
        val = Mathf.Clamp(val, min, max);
        return val.ToString("00");
    }
    static int Parse(string s) => int.TryParse(s, out var v) ? v : 0;
}
