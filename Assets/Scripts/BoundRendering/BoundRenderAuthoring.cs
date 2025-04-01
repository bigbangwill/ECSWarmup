using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditorInternal;
using UnityEngine;

public struct RenderBoundingBoxData : IComponentData
{
    public float3 boundMin;
    public float3 boundMax;
}

public class BoundRenderAuthoring : MonoBehaviour
{
    public Transform bottomLeft;
    public Transform topRight;
    public class BoundRender : Baker<BoundRenderAuthoring>
    {
        public override void Bake(BoundRenderAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            float3 min = authoring.bottomLeft.position;
            float3 max = authoring.topRight.position;
            AddComponent(entity, new RenderBoundingBoxData 
            {
                boundMin = new float3(min.x,min.y,-10),
                boundMax = new float3(max.x, max.y, 10),
            });
        }
    }
}

public partial struct BoundRenderSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RenderBoundingBoxData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Entity renderBoxEntity = SystemAPI.GetSingletonEntity<RenderBoundingBoxData>();        
        RenderBoundingBoxData renderBox = SystemAPI.GetComponent<RenderBoundingBoxData>(renderBoxEntity);
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecbRef = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        ComponentLookup<DisableRendering> disableRenderingLookup = SystemAPI.GetComponentLookup<DisableRendering>(true);
        BoundRenderJob job = new() 
        { 
            boundMax = renderBox.boundMax, 
            boundMin = renderBox.boundMin, 
            ecb = ecbRef , 
            disableRenderingLookup = disableRenderingLookup
        };
        job.ScheduleParallel();

    }
}

public partial struct BoundRenderJob : IJobEntity
{
    public float3 boundMin;
    public float3 boundMax;
    public EntityCommandBuffer.ParallelWriter ecb;
    [ReadOnly] public ComponentLookup<DisableRendering> disableRenderingLookup;

    public void Execute(Entity entity, [EntityIndexInChunk] int entityId, in LocalTransform localTransform)
    {
        bool insideBounds = math.all(localTransform.Position >= boundMin) &&
                math.all(localTransform.Position <= boundMax);
        bool hasDisable = disableRenderingLookup.HasComponent(entity);
        if (!insideBounds && !hasDisable)
        {            
            ecb.AddComponent<DisableRendering>(entityId, entity);
        }
        else if (insideBounds && hasDisable)
        {
            ecb.RemoveComponent<DisableRendering>(entityId, entity);
        }
    }
}