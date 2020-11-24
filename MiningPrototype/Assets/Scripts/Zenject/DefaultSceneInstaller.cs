using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

[CreateAssetMenu(fileName = "DefaultSceneInstaller", menuName = "Installers/DefaultSceneInstaller")]
public class DefaultSceneInstaller : ScriptableObjectInstaller<DefaultSceneInstaller>
{
    [SerializeField] GameObject playerPrefab, progressionPrefab, readableItemPrefab, cameraPannerPrefab, cameraPrefab, itemPreviewPrefab;
    [SerializeField] GameObject pauseMenuPrefab, toolTipPrefab, eventSystemPrefab, inWorldCanvasPrefab;


    public override void InstallBindings()
    {
        Container.Bind<PlayerStateMachine>().FromComponentInNewPrefab(playerPrefab).AsSingle().NonLazy();
        Container.Bind<ProgressionHandler>().FromComponentInNewPrefab(progressionPrefab).AsSingle().NonLazy();
        Container.Bind<ReadableItemHandler>().FromComponentInNewPrefab(readableItemPrefab).AsSingle().NonLazy();
        Container.Bind<CameraPanner>().FromComponentInNewPrefab(cameraPannerPrefab).AsSingle().NonLazy();

        Container.Bind<ItemPlacingHandler>().FromComponentInNewPrefab(itemPreviewPrefab).AsSingle().NonLazy();

        Container.Bind<PauseMenu>().FromComponentInNewPrefab(pauseMenuPrefab).AsSingle().NonLazy();
        Container.Bind<TooltipHandler>().FromComponentInNewPrefab(toolTipPrefab).AsSingle().NonLazy();
        Container.Bind<EventSystem>().FromComponentInNewPrefab(eventSystemPrefab).AsSingle().NonLazy();
        Container.Bind<InWorldCanvas>().FromComponentInNewPrefab(inWorldCanvasPrefab).AsSingle().NonLazy();

        Container.Bind(typeof(CameraController), typeof(TransitionEffectHandler)).FromComponentInNewPrefab(cameraPrefab).AsSingle().NonLazy();
    }
}