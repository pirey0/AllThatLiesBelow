using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "DefaultMainMenuInstaller", menuName = "Installers/DefaultMainMenuInstaller")]
public class DefaultMainMenuInstaller : ScriptableObjectInstaller<DefaultMainMenuInstaller>
{
    public override void InstallBindings()
    {
    }
}