using BotSharp.AspNetCore.Interfaces;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

namespace BotSharp.AspNetCore.lmplements
{
    public class PluginControllerManager : IPluginControllerManager
    {
        private readonly ApplicationPartManager _applicationPartManager;

        public PluginControllerManager(ApplicationPartManager applicationPartManager)
        {
            _applicationPartManager = applicationPartManager;
        }

        /// <summary>
        /// Get the Controller from the specified <see cref="Assembly"/> and add it
        /// </summary>
        /// <param name="assembly"></param>
        public void AddControllers(Assembly assembly)
        {
            AssemblyPart assemblyPart = new AssemblyPart(assembly);
            _applicationPartManager.ApplicationParts.Add(assemblyPart);

            ResetControllActions();
        }

        public void RemoveControllers(string pluginId)
        {
            ApplicationPart last = _applicationPartManager.ApplicationParts.First(m => m.Name == pluginId);
            _applicationPartManager.ApplicationParts.Remove(last);

            ResetControllActions();
        }

        /// <summary>
        /// Notify the application (main program) that Controller.Action has changed
        /// </summary>
        private void ResetControllActions()
        {
            PluginActionDescriptorChangeProvider.Instance.HasChanged = true;
            // TokenSource 为 null
            // Note: When the program is just started, IActionDescriptorChangeProvider.GetChangeToken() will not be triggered when the controller is not reached,
            // which will also cause the TokenSource to be null, and at the same time, at the same time, the plugin Controller.Action and the main program will be added together,
            // so there is no need to notify the change
            if (PluginActionDescriptorChangeProvider.Instance.TokenSource != null)
            {
                PluginActionDescriptorChangeProvider.Instance.TokenSource.Cancel();
            }
        }
    }
}
