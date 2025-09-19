using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.UI  // <-- change to your project's namespace
{
    /// <summary>
    /// A fully stylable numeric stepper (no default input fields used).
    /// UXML tag: <app:StepperField />
    /// USS classes you can target:
    ///  - .stepper, .stepper__title, .stepper__frame, .stepper__btn, .stepper__btn--inc, .stepper__btn--dec, .stepper__value
    /// </summary>
    public class StepperField : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<StepperField, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Label = new() { name = "label", defaultValue = "Label" };
            readonly UxmlIntAttributeDescription m_Value = new() { name = "value", defaultValue = 0 };
            readonly UxmlIntAttributeDescription m_Min = new() { name = "min", defaultValue = 0 };
            readonly UxmlIntAttributeDescription m_Max = new() { name = "max", defaultValue = 59 };
            readonly UxmlIntAttributeDescription m_Step = new() { name = "step", defaultValue = 1 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var f = (StepperField)ve;
                f.Label = m_Label.GetValueFromBag(bag, cc);
                f.Min = m_Min.GetValueFromBag(bag, cc);
                f.Max = m_Max.GetValueFromBag(bag, cc);
                f.Step = Mathf.Max(1, m_Step.GetValueFromBag(bag, cc));
                f.Value = Mathf.Clamp(m_Value.GetValueFromBag(bag, cc), f.Min, f.Max);
            }
        }

        public const string UssClass = "stepper";
        public const string TitleUssClass = "stepper__title";
        public const string FrameUssClass = "stepper__frame";
        public const string BtnUssClass = "stepper__btn";
        public const string BtnIncUssClass = "stepper__btn--inc";
        public const string BtnDecUssClass = "stepper__btn--dec";
        public const string ValueUssClass = "stepper__value";

        private readonly Label _title;
        private readonly Label _valueLabel;
        private readonly Button _incBtn;
        public readonly Button _decBtn;

        public event Action<int> ValueChanged;

        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                var clamped = Mathf.Clamp(value, Min, Max);
                if (clamped == _value) { UpdateVisual(); return; }
                _value = clamped;
                UpdateVisual();
                ValueChanged?.Invoke(_value);
            }
        }

        public int Min { get; set; } = 0;
        public int Max { get; set; } = 59;
        public int Step { get; set; } = 1;

        public string Label
        {
            get => _title.text;
            set => _title.text = value;
        }

        public StepperField()
        {
            AddToClassList(UssClass);
            style.flexDirection = FlexDirection.Column;

            _title = new Label("Label");
            _title.AddToClassList(TitleUssClass);
            hierarchy.Add(_title);

            var frame = new VisualElement();
            frame.AddToClassList(FrameUssClass);
            frame.style.flexDirection = FlexDirection.Row;
            frame.style.alignItems = Align.Center;
            frame.style.justifyContent = Justify.Center;
            hierarchy.Add(frame);

            _decBtn = new Button(() => Value -= Step) { text = "–" };
            _decBtn.AddToClassList(BtnUssClass);
            _decBtn.AddToClassList(BtnDecUssClass);

            _valueLabel = new Label("00");
            _valueLabel.AddToClassList(ValueUssClass);

            _incBtn = new Button(() => Value += Step) { text = "+" };
            _incBtn.AddToClassList(BtnUssClass);
            _incBtn.AddToClassList(BtnIncUssClass);

            frame.Add(_decBtn);
            frame.Add(_valueLabel);
            frame.Add(_incBtn);

            // wheel / keyboard support
            RegisterCallback<WheelEvent>(e =>
            {
                if (e.delta.y < 0) Value += Step;
                else if (e.delta.y > 0) Value -= Step;
            });

            RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.UpArrow) { Value += Step; e.StopPropagation(); }
                else if (e.keyCode == KeyCode.DownArrow) { Value -= Step; e.StopPropagation(); }
                else if (e.keyCode >= KeyCode.Keypad0 && e.keyCode <= KeyCode.Keypad9 ||
                         e.keyCode >= KeyCode.Alpha0 && e.keyCode <= KeyCode.Alpha9)
                {
                    // Optional: quick type-to-set behavior (00..99).
                    int digit = (int)e.character - '0';
                    Value = Mathf.Clamp(((_value % 10) * 10) + digit, Min, Max);
                    e.StopPropagation();
                }
            });

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            _valueLabel.text = _value.ToString("00");
        }
    }
}
