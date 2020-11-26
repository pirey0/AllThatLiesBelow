using UnityEngine;
using Zenject;

public class RuntimeMapInstaller : MonoInstaller
{
    [SerializeField] RuntimeProceduralMap map;
    public override void InstallBindings()
    {
        Container.Bind<RuntimeProceduralMap>().FromInstance(map).AsSingle().NonLazy();
    }
}