using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "DefaultProjectInstaller", menuName = "Installers/DefaultProjectInstaller")]
public class DefaultProjectInstaller : ScriptableObjectInstaller<DefaultProjectInstaller>
{
    public override void InstallBindings()
    {
    }
}