using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct PlayerTag : IComponentData { }

[Serializable]
public struct Movement : IComponentData
{
	public float2 MoveAmount;
	public float SpeedMultiplier;
}

[Serializable]
public struct Attack : IComponentData
{
	public bool IsAttacking;
}

[Serializable]
public struct HumanPlayerInput : IComponentData
{
	public float2 MovementInput;
	public bool Attack;
}

