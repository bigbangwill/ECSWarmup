using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public struct EnemyTag : IComponentData { }

public class EnemyAuthoring : MonoBehaviour
{
    public class EnemyBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<EnemyTag>(entity);
            AddComponent<MoveDirectionData>(entity);
            AddComponent(entity, new MovementSpeed { Value = 1 });
            AddComponent<InitializeCharacterFlag>(entity);
        }
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EnemyMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
    }

    public void OnUpdate(ref SystemState state)        
    {
        Entity targetEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        float3 targetPos = SystemAPI.GetComponent<LocalTransform>(targetEntity).Position;

        foreach (var (moveDirection, localPos) in SystemAPI.Query<RefRW<MoveDirectionData>, RefRO<LocalTransform>>().WithAll<EnemyTag>())
        {
            float2 direction = targetPos.xy - localPos.ValueRO.Position.xy;
            float2 directionMag = math.normalizesafe(direction);
            moveDirection.ValueRW.Value = new float3(directionMag, 0);
        }
    }
}