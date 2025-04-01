using System.Data.Common;
using Unity.Entities;
using Unity.Rendering;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.Rendering;

public class SubSceneLoader : MonoBehaviour
{
    public SubScene SubSceneForWorld1;
    public SubScene SubSceneForWorld2;

    private void Start()
    {
        var loadParameters = new SceneSystem.LoadParameters
        {
            Flags = SceneLoadFlags.NewInstance
        };

        SceneSystem.LoadSceneAsync(CustomBootstrap.World1.Unmanaged, SubSceneForWorld1.SceneGUID, loadParameters);
        SceneSystem.LoadSceneAsync(CustomBootstrap.World2.Unmanaged, SubSceneForWorld2.SceneGUID, loadParameters);
    }
}

public class CustomBootstrap : ICustomBootstrap
{
    public static World World1;
    public static World World2;
    public bool Initialize(string defaultWorldName)
    {
        // Create two worlds
        World1 = new World("World1");
        World2 = new World("World2");

        // Retrieve the list of default systems.
        var systemTypes = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);

        // Add systems to each world.
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World1, systemTypes);
        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World2, systemTypes);

        // Optionally, add the worlds to the player loop so they update automatically.
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(World1);
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(World2);

        // Optionally set one as the default injection world.
        World.DefaultGameObjectInjectionWorld = World1;
        return true;
    }
}