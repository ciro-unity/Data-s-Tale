using System;
using Unity.Entities;
using Unity.Mathematics;


//----------------------------------  PLAYER-SPECIFIC ----------------------------------------

public struct PlayerTag : IComponentData { }

public struct Score : IComponentData
{
	public int Value;
}

public struct UpdateScoreUI : IComponentData { }

//----------------------------------  ALL CHARACTERS ----------------------------------------

public struct EnemyTag : IComponentData { }

public struct Wanderer : IComponentData
{
	public Unity.Mathematics.Random RandomSeed;
	public float3 InitialPosition;
}

public struct MovementInput : IComponentData
{
	public float3 MoveAmount;
}

public struct AttackInput : IComponentData
{
	public bool Attack;
	public float AttackLength;
	public int AttackStrength;
}

public struct Speed : IComponentData
{
	public float Value;
}

public struct AttackRange : IComponentData
{
	public float Range;
}

public struct AlertRange : IComponentData
{
	public float Range;
}

public struct AnimationState : ISystemStateComponentData
{
	public float Speed;
	public bool IsWalking;
	public bool TriggerAttack;
	public bool TriggerTakeDamage;
	public bool TriggerIsDead;
}

public struct DealBlow : IComponentData
{
	public float When;
	public int DamageAmount;
}

public struct Droppable : IComponentData
{
	public Entity Drop;
}


//----------------------------------  ATTACKING/DAMAGING ----------------------------------------

public struct Target : IComponentData
{
	public Entity Entity;
}

public struct Damage : IBufferElementData
{
	public int Amount;
}

public struct Health : IComponentData
{
	public int Current;
	public int FullHealth;
}

public struct IsDead : IComponentData { }

//----------------------------------  ITEMS ----------------------------------------

public struct Collectable : IComponentData
{
	public int Value;
}


//----------------------------------  GENERAL USE ----------------------------------------

public struct Busy : IComponentData
{
	public float Until; //time value
}