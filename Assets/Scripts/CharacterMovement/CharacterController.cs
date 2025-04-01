using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Burst;

public struct MoveDirectionData : IComponentData
{
    public float3 Value;
}

public struct MovementSpeed : IComponentData
{
    public float Value;
}

[BurstCompile]
public partial struct CharacterMovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (velocity, moveDirection, moveSpeed) in SystemAPI.Query<RefRW<PhysicsVelocity>, MoveDirectionData, MovementSpeed>())
        {
            velocity.ValueRW.Linear = moveDirection.Value * moveSpeed.Value;
        }
    }
}