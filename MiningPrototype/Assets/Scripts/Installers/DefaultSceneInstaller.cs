using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

[CreateAssetMenu(fileName = "DefaultSceneInstaller", menuName = "Installers/DefaultSceneInstaller")]
public class DefaultSceneInstaller : ScriptableObjectInstaller<DefaultSceneInstaller>
{
    [SerializeField] GameObject playerPrefab, progressionPrefab, readableItemPrefab, cameraPannerPrefab, cameraPrefab, itemPreviewPrefab;
    [SerializeField] GameObject pauseMenuPrefab, toolTipPrefab, eventSystemPrefab, inWorldCanvasPrefab;
    [SerializeField] GameObject debugModePrefab, nonPersistantSaveManager;
    
    public override void InstallBindings()
    {
        Container.Bind(typeof(PlayerStateMachine), typeof(PlayerInteractionHandler), typeof(OverworldEffectHandler)).FromComponentInNewPrefab(playerPrefab).AsSingle().NonLazy();
        Container.Bind<ProgressionHandler>().FromComponentInNewPrefab(progressionPrefab).AsSingle().NonLazy();
        Container.Bind<ReadableItemHandler>().FromComponentInNewPrefab(readableItemPrefab).AsSingle().NonLazy();
        Container.Bind<CameraPanner>().FromComponentInNewPrefab(cameraPannerPrefab).AsSingle().NonLazy();
        Container.Bind<ItemPlacingHandler>().FromComponentInNewPrefab(itemPreviewPrefab).AsSingle().NonLazy();
        Container.Bind<PauseMenu>().FromComponentInNewPrefab(pauseMenuPrefab).AsSingle().NonLazy();
        Container.Bind<TooltipHandler>().FromComponentInNewPrefab(toolTipPrefab).AsSingle().NonLazy();
        Container.Bind<EventSystem>().FromComponentInNewPrefab(eventSystemPrefab).AsSingle().NonLazy();
        Container.Bind<InWorldCanvas>().FromComponentInNewPrefab(inWorldCanvasPrefab).AsSingle().NonLazy();
        Container.Bind<NonPersistantSaveManager>().FromComponentInNewPrefab(nonPersistantSaveManager).AsSingle().NonLazy();

        Container.Bind(typeof(CameraController), typeof(TransitionEffectHandler)).FromComponentInNewPrefab(cameraPrefab).AsSingle().NonLazy();

        if (DebugMode.DEBUG_POSSIBLE)
        {
            Container.Bind<DebugMode>().FromComponentInNewPrefab(debugModePrefab).AsSingle().NonLazy();
        }

        //Factories
        Container.BindFactory<GameObject, InventoryVisualizer, InventoryVisualizer.Factory>().FromFactory<PrefabFactory<InventoryVisualizer>>();
        Container.BindFactory<UnityEngine.Object, InventorySlotVisualizer, InventorySlotVisualizer.Factory>().FromFactory<PrefabFactory<InventorySlotVisualizer>>();
        Container.BindFactory<UnityEngine.Object, ReadableItemVisualizer, ReadableItemVisualizer.Factory>().FromFactory<PrefabFactory<ReadableItemVisualizer>>();
        
    }
}