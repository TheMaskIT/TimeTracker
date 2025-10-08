using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Numpad "Set Goal" modal shown over the main screen (Template instance A2).
/// Call Show(mode, hours, minutes) / Hide().
/// Subscribe to Confirmed(hours, minutes) and Canceled().
/// </summary>
public class ResetConferm : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    public event Action Confirmed;
    public event Action Canceled;

    public TimerController controller;


    Button confirmBtn, cancelBtn;
    VisualElement modal;


    void OnEnable()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Find the instance and then query INSIDE it
        modal = root.Q<VisualElement>("resetMenu");
        if (modal == null) return; // instance not added yet

        confirmBtn = modal.Q<Button>("ConfirmButton");
        cancelBtn = modal.Q<Button>("CancelButton");


        if (confirmBtn != null) confirmBtn.clicked += () => { Confirmed?.Invoke(); Hide(); };
        if (cancelBtn != null) cancelBtn.clicked += () => {Hide(); };


        Hide(); // start hidden
    }

    /* ---------- Public API ---------- */

    public void Start()
    {
        Confirmed += ResetSelected;
    }
    public void Show()
    {
        print("ping");
        if (modal == null)
        {
            return;
        };

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

    public void ResetSelected()
    {
        controller.ResetProgress();
    }

}
