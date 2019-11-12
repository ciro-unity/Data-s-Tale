using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
using UnityEditor;

namespace UnityEngine.InputSystem.Composites
{
    public class Vector2WithOneModifier : InputBindingComposite<Vector2>
    {
        [InputControl(layout = "Modifier")] public int modifier;
        [InputControl(layout = "Vector2")] public int vector2;
		
        public override Vector2 ReadValue(ref InputBindingCompositeContext context)
        {
            if (context.ReadValueAsButton(modifier))
			{
                return context.ReadValue<Vector2, Vector2MagnitudeComparer>(vector2);
			}

            return default;
        }

        public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        {
            var value = ReadValue(ref context);
            return value.magnitude;
        }
    }
}