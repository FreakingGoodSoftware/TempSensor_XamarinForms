using MvvmCross.Platform;
using MvvmCross.Platform.Plugins;

namespace TempSensor.Xamarin.UWP.Bootstrap
{
    public class UserInteractionPluginBootstrap
        : MvxLoaderPluginBootstrapAction<Chance.MvvmCross.Plugins.UserInteraction.PluginLoader, Chance.MvvmCross.Plugins.UserInteraction.WindowsUWP.Plugin>
    {
    }
}