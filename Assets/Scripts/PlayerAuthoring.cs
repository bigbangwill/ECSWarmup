using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.Collections;

public struct PlayerTag : IComponentData { }

public struct InitializeCharacterFlag : IComponentData, IEnableableComponent { }

public struct  PlayerFixedStats : IComponentData
{
    public int maxHp;
}

public class PlayerAuthoring : MonoBehaviour
{
    public int playerMaxHp;
    public class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerFixedStats { maxHp = 100 });
            AddComponent(entity, new MovementSpeed { Value = 3 });
            AddComponent<MoveDirectionData>(entity);
            AddComponent<PlayerTag>(entity);
            AddComponent<InitializeCharacterFlag>(entity);
        }
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct CharacterInitializeSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (mass, flag) in SystemAPI.Query<RefRW<PhysicsMass>, EnabledRefRW<InitializeCharacterFlag>>())
        {
            mass.ValueRW.InverseInertia = float3.zero;
            flag.ValueRW = false;
        }
    }
}

public partial class PlayerInputSystem : SystemBase
{
    private PlayerInput input;

    protected override void OnCreate()
    {
        input = new();
        input.Enable();
    }

    protected override void OnUpdate()
    {
        var currentInput = (float2)input.Player.Move.ReadValue<Vector2>();

        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var commandBuffer = ecbSystem.CreateCommandBuffer(CustomBootstrap.World1.Unmanaged);

        foreach (var moveDirection in SystemAPI.Query<RefRW<MoveDirectionData>>().WithAll<PlayerTag>())
        {
            moveDirection.ValueRW.Value = new float3(currentInput, 0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            foreach (var (mat, entity) in SystemAPI.Query<MaterialMeshInfo>().WithEntityAccess())
            {
                //ecb.AddComponent<DisableRendering>(entity);
                commandBuffer.AddComponent<DisableRendering>(entity);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            foreach (var (mat, entity) in SystemAPI.Query<MaterialMeshInfo>().WithEntityAccess().WithAll<DisableRendering>())
            {
                //ecb.RemoveComponent<DisableRendering>(entity);
                commandBuffer.RemoveComponent<DisableRendering>(entity);
            }
        }

        //ecb.Playback(EntityManager);



    }
}