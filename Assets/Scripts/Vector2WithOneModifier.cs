using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
using UnityEditor;

namespace UnityEngine.InputSystem.Composites
{
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
    public class Vector2WithOneModifier : InputBindingComposite<Vector2>
    {
        [InputControl(layout = "Modifier")] public int modifier;
        [InputControl(layout = "Vector2")] public int vector2;
		
        public override Vector2 ReadValue(ref InputBindingCompositeContext context)
        {
            if (context.ReadValueAsButton(modifier))
			{
				Debug.Log("Modifier pressed");
                return context.ReadValue<Vector2, Vector2MagnitudeComparer>(vector2);
			}

            return default;
        }

        public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        {
            var value = ReadValue(ref context);
            return value.magnitude;
        }
		
	#if UNITY_EDITOR
		static Vector2WithOneModifier()
		{
			Register();
		}
	#endif
	
		[RuntimeInitializeOnLoadMethod]
		private static void Register()
		{
			InputSystem.RegisterBindingComposite<Vector2WithOneModifier>();
		}
    }
}