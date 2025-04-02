using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Burst;
using Unity.Transforms;

public struct MoveDirectionData : IComponentData
{
    public float3 Value;
}

public struct MovementSpeed : IComponentData
{
    public float Value;
}

public struct MovableObjects : IComponentData { }


[UpdateInGroup(typeof(SimulationSystemGroup))]
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


[UpdateAfter(typeof(CharacterMovementSystem))]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct ObjectOffsetSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        LocalTransform playerPos = SystemAPI.GetComponent<LocalTransform>(playerEntity);
        float3 playerStartingPos = SystemAPI.GetComponent<PlayerFixedStats>(playerEntity).startingPos;
        float3 movedDistance = playerPos.Position - playerStartingPos;

        foreach (var transform in SystemAPI.Query<RefRW<LocalTransform>>())
        {
            transform.ValueRW.Position -= movedDistance;
        }
    }

}