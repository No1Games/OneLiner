using Zenject;

public class LoggerInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container
            .Bind<ILogger>()
            .WithId("RuntimeTMP")
            .To<RuntimeTMPLogger>()
            .FromNew()
            .AsSingle();
    }
}
