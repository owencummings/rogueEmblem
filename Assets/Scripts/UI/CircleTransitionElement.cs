using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace UILibrary{
    public class CircleTransitionElement : VisualElement
    {
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            // The progress property is exposed to UXML.
            UxmlFloatAttributeDescription m_ProgressAttribute = new UxmlFloatAttributeDescription()
            {
                name = "progress"
            };

            // Use the Init method to assign the value of the progress UXML attribute to the C# progress property.
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                (ve as CircleTransitionElement).progress = m_ProgressAttribute.GetValueFromBag(bag, cc);
            }
        }

        public new class UxmlFactory : UxmlFactory<CircleTransitionElement, UxmlTraits>{}

        // These are USS class names for the control overall and the label.
        public static readonly string ussClassName = "circle-transition-element";
        public static readonly string ussLabelClassName = "circle-transition-element__label";

        // This is the label that displays the percentage.
        Label m_Label;

        // This is the number that the Label displays as a percentage.
        float m_Progress;

        // A value between 0 and 100

        public float progress
        {
            // The progress property is exposed in C#.
            get => m_Progress;
            set
            {
                // Whenever the progress property changes, MarkDirtyRepaint() is named. This causes a call to the
                // generateVisualContents callback.
                m_Progress = value;
                m_Label.text = Mathf.Clamp(Mathf.Round(value), 0, 100) + "%";
                MarkDirtyRepaint();
            }
        }

        public CircleTransitionElement()
        {
            // Create a Label, add a USS class name, and add it to this visual tree.
            m_Label = new Label();
            m_Label.AddToClassList(ussLabelClassName);
            Add(m_Label);

            // Add the USS class name for the overall control.
            AddToClassList(ussClassName);

            // Register a callback after custom style resolution.
            RegisterCallback<CustomStyleResolvedEvent>(evt => CustomStylesResolved(evt));

            // Register a callback to generate the visual content of the control.
            generateVisualContent += context => GenerateVisualContent(context);

            progress = 0.0f;
        }
         static void CustomStylesResolved(CustomStyleResolvedEvent evt)
        {
            CircleTransitionElement element = (CircleTransitionElement)evt.currentTarget;
            element.UpdateCustomStyles();
        }

        // After the custom colors are resolved, this method uses them to color the meshes and (if necessary) repaint
        // the control.
        void UpdateCustomStyles()
        {
            bool repaint = false;
            /*
            if (customStyle.TryGetValue(s_ProgressColor, out m_ProgressColor))
                repaint = true;

            if (customStyle.TryGetValue(s_TrackColor, out m_TrackColor))
                repaint = true;
            */

            if (repaint)
                MarkDirtyRepaint();
        }

        void GenerateVisualContent(MeshGenerationContext context)
        {
            float width = contentRect.width;
            float height = contentRect.height;
            float finalCircleRadius = Mathf.Sqrt(width * width + height * height)/2;

            var painter = context.painter2D;
            painter.lineCap = LineCap.Butt;
            painter.lineWidth = 10.0f;
            painter.strokeColor = Color.black;

            painter.BeginPath();
            painter.MoveTo(new Vector2(-width, -height));
            painter.LineTo(new Vector2(-width, 2*height));
            painter.LineTo(new Vector2(2*width, 2*height));
            painter.LineTo(new Vector2(2*width, -height));
            painter.LineTo(new Vector2(-width, -height));

            float arcLength = progress/100 * finalCircleRadius;
            painter.MoveTo(new Vector2(width/2 + arcLength, height/2));
            painter.Arc(new Vector2(width/2, height/2), arcLength, 0f, 360f);
            painter.ClosePath();

            painter.Fill(FillRule.OddEven);
        }
    }
}

