using UnityEngine;
using Zenject;

public class RuntimeMapInstaller : MonoInstaller
{
    [SerializeField] RuntimeProceduralMap map;
    [SerializeField] SceneAdder sceneAdder;
    public override void InstallBindings()
    {
        Container.Bind<RuntimeProceduralMap>().FromInstance(map).AsSingle().NonLazy();
        Container.Bind<SceneAdder>().FromInstance(sceneAdder).AsSingle().NonLazy();
    }
}