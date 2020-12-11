using ModestTree;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

[CreateAssetMenu(fileName = "DefaultSceneInstaller", menuName = "Installers/DefaultSceneInstaller")]
public class DefaultSceneInstaller : ScriptableObjectInstaller<DefaultSceneInstaller>
{
    [SerializeField] GameObject playerPrefab, progressionPrefab, readableItemPrefab, cameraPannerPrefab, cameraPrefab, itemPreviewPrefab, playerInventoryOpenerUIPrefab;
    [SerializeField] GameObject pauseMenuPrefab, toolTipPrefab, eventSystemPrefab, inWorldCanvasPrefab;
    [SerializeField] GameObject debugModePrefab, inventoryManagerPrefab, saveHandlerPrefab, gameStatePrefab;
    
    public override void InstallBindings()
    {
        Container.Bind(typeof(PlayerStateMachine), typeof(PlayerInteractionHandler), typeof(OverworldEffectHandler)).FromComponentInNewPrefab(playerPrefab).AsSingle().NonLazy();
        Container.Bind(typeof(ProgressionHandler), typeof(SacrificeActions)).FromComponentInNewPrefab(progressionPrefab).AsSingle().NonLazy();
        Container.Bind<ReadableItemHandler>().FromComponentInNewPrefab(readableItemPrefab).AsSingle().NonLazy();
        Container.Bind<CameraPanner>().FromComponentInNewPrefab(cameraPannerPrefab).AsSingle().NonLazy();
        Container.Bind<ItemPlacingHandler>().FromComponentInNewPrefab(itemPreviewPrefab).AsSingle().NonLazy();
        Container.Bind<PauseMenu>().FromComponentInNewPrefab(pauseMenuPrefab).AsSingle().NonLazy();
        Container.Bind<TooltipHandler>().FromComponentInNewPrefab(toolTipPrefab).AsSingle().NonLazy();
        Container.Bind<EventSystem>().FromComponentInNewPrefab(eventSystemPrefab).AsSingle().NonLazy();
        Container.Bind<InWorldCanvas>().FromComponentInNewPrefab(inWorldCanvasPrefab).AsSingle().NonLazy();
        Container.Bind(typeof(CameraController), typeof(TransitionEffectHandler)).FromComponentInNewPrefab(cameraPrefab).AsSingle().NonLazy();
        Container.Bind<InventoryManager>().FromComponentInNewPrefab(inventoryManagerPrefab).AsSingle().NonLazy();
        Container.Bind<SaveHandler>().FromComponentInNewPrefab(saveHandlerPrefab).AsSingle().NonLazy();
        Container.Bind<GameState>().FromComponentInNewPrefab(gameStatePrefab).AsSingle().NonLazy();
        Container.Bind<PlayerInventoryOpener>().FromComponentInNewPrefab(playerInventoryOpenerUIPrefab).AsSingle().NonLazy();

        if (DebugMode.DEBUG_POSSIBLE)
        {
            Container.Bind<DebugMode>().FromComponentInNewPrefab(debugModePrefab).AsSingle().NonLazy();
        }

        //Factories
        Container.BindFactory<GameObject, InventoryVisualizer, InventoryVisualizer.Factory>().FromFactory<PrefabFactory<InventoryVisualizer>>();
        Container.BindFactory<UnityEngine.Object, InventorySlotVisualizer, InventorySlotVisualizer.Factory>().FromFactory<PrefabFactory<InventorySlotVisualizer>>();
        Container.BindFactory<UnityEngine.Object, ReadableItemVisualizer, ReadableItemVisualizer.Factory>().FromFactory<PrefabFactory<ReadableItemVisualizer>>();
        Container.BindFactory<GameObject, Transform, PrefabFactory>().FromFactory<NormalPrefabFactory>();
    }
}

public class PrefabFactory : PlaceholderFactory<GameObject, Transform>
{
    public Transform Create(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var t = Create(prefab);
        t.SetParent(parent, worldPositionStays: false);
        t.position = position;
        t.rotation = rotation;
        return t;
    }
    public Transform Create(GameObject prefab, Transform parent)
    {
        var t = Create(prefab);
        t.SetParent(parent, worldPositionStays: false);
        return t;
    }

}

public class NormalPrefabFactory : IFactory<GameObject, Transform>
{
    [Inject]
    readonly DiContainer _container = null;

    public DiContainer Container
    {
        get { return _container; }
    }

    public Transform Create(GameObject prefab)
    {
        Assert.That(prefab != null, "Null prefab given to factory create method");

        return _container.InstantiatePrefab(prefab).transform;
    }
}