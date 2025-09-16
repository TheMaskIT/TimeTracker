// DonutRing.cs — Unity 6 / UI Toolkit (degrees, full 360 track)
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class DonutRing : VisualElement
{
    [UxmlAttribute("value")] public float value { get => _value; set { _value = Mathf.Clamp01(value); MarkDirtyRepaint(); } }
    [UxmlAttribute("inner-empty-percent")] public float innerEmptyPercent { get => _innerEmptyPercent; set { _innerEmptyPercent = Mathf.Clamp(value, 0f, 0.95f); MarkDirtyRepaint(); } }
    [UxmlAttribute("start-angle")] public float startAngle { get => _startAngle; set { _startAngle = value; MarkDirtyRepaint(); } }
    [UxmlAttribute("clockwise")] public bool clockwise { get => _clockwise; set { _clockwise = value; MarkDirtyRepaint(); } }
    [UxmlAttribute("rounded-caps")] public bool roundedCaps { get => _roundedCaps; set { _roundedCaps = value; MarkDirtyRepaint(); } }
    [UxmlAttribute("progress-color")] public Color progressColor { get => _progressColor; set { _progressColor = value; MarkDirtyRepaint(); } }
    [UxmlAttribute("track-color")] public Color trackColor { get => _trackColor; set { _trackColor = value; MarkDirtyRepaint(); } }

    float _value = 0;
    float _innerEmptyPercent = 0.6f;
    float _startAngle = -90; // degrees (12 o'clock)
    bool _clockwise = true;
    bool _roundedCaps = true;
    Color _progressColor = new(0.24f, 0.64f, 1f, 1f);
    Color _trackColor = new(1f, 1f, 1f, 0.12f);

    public DonutRing()
    {
        pickingMode = PickingMode.Ignore;
        generateVisualContent += OnGenerateVisualContent;
        RegisterCallback<GeometryChangedEvent>(_ => MarkDirtyRepaint());
    }

    private void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        var p = mgc.painter2D;
        var rect = contentRect;
        float w = rect.width, h = rect.height;
        if (w <= 0f || h <= 0f) return;

        float outerR = Mathf.Min(w, h) * 0.5f;
        float innerR = Mathf.Clamp01(_innerEmptyPercent) * outerR;

        float thickness = Mathf.Max(1f, outerR - innerR);
        float radius = innerR + thickness * 0.5f;
        Vector2 center = rect.center;

        // --- TRACK: draw a true 360° by chaining two arcs (degrees) ---
        if (_trackColor.a > 0f && thickness > 0f)
        {
            p.BeginPath();
            p.strokeColor = _trackColor;
            p.lineWidth = thickness;
            p.lineCap = LineCap.Butt;
            p.Arc(center, radius, 0f, 180f);
            p.Arc(center, radius, 180f, 360f);
            p.Stroke();
        }

        // --- PROGRESS: partial arc in degrees ---
        float v = Mathf.Clamp01(_value);
        if (_progressColor.a > 0f && v > 0f && thickness > 0f)
        {
            float sweepDeg = v * 360f;
            float startDeg = _startAngle;

            float a0, a1;
            if (_clockwise)
            {
                a0 = startDeg - sweepDeg; // earlier angle first
                a1 = startDeg;
            }
            else
            {
                a0 = startDeg;
                a1 = startDeg + sweepDeg;
            }

            p.BeginPath();
            p.strokeColor = _progressColor;
            p.lineWidth = thickness;
            p.lineCap = _roundedCaps ? LineCap.Round : LineCap.Butt;

            if (v >= 0.9999f)
            {
                // Full circle: stitch two arcs to avoid seam/gap
                p.Arc(center, radius, 0f, 180f);
                p.Arc(center, radius, 180f, 360f);
            }
            else
            {
                p.Arc(center, radius, a0, a1); // Arc draws CCW from a0 to a1
            }

            p.Stroke();
        }
    }
}
