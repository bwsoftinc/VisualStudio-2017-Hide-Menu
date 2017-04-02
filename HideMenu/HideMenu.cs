using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace BWSoftInc
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideAutoLoad("{ADFC4E64-0397-11D1-9F4E-00A0C911004F}")]
    [ProvideAutoLoad("{F1536EF8-92EC-443C-9ED7-FDADF150DA82}")]
    [Guid("026205ad-7a73-4eca-9d9c-9093d455d3ec")]
    public sealed class HideMenu : Package
    {
        private FrameworkElement menu;
        private bool visible;
        
        private void OnMenuContainerFocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var focus = HasFocus();
            if(visible != focus)
            {
                if(visible = focus)
                    menu.ClearValue(FrameworkElement.HeightProperty);
                else
                    menu.Height = 0.0;
            }
        }

        private void PopupLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(visible && !HasFocus())
            {
                visible = false;
                menu.Height = 0.0;
            }
        }

        private bool HasFocus()
        {
            if (menu.IsKeyboardFocusWithin)
                return true;

            var dependencyObject = (DependencyObject)Keyboard.FocusedElement;
            while(dependencyObject != null)
            {
                if (dependencyObject == menu)
                    return true;

                dependencyObject = ExtensionMethods.GetVisualOrLogicalParent(dependencyObject);
            }

            return false;
        }

        protected override void Initialize()
        {
            base.Initialize();
            EventManager.RegisterClassHandler(typeof(UIElement), UIElement.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(this.PopupLostKeyboardFocus));

            var mainWindow = Application.Current.MainWindow;
            EventHandler layoutUpdated = null;
            layoutUpdated = (sender, e) =>
            {
                var descendant = ExtensionMethods.FindDescendants<Menu>(mainWindow).FirstOrDefault(m => AutomationProperties.GetAutomationId(m) == "MenuBar");
                var parent = ExtensionMethods.GetVisualOrLogicalParent(descendant);
                menu = ExtensionMethods.GetVisualOrLogicalParent(parent) as DockPanel;
                menu.Height = 0.0;

                menu.IsKeyboardFocusWithinChanged += new DependencyPropertyChangedEventHandler(OnMenuContainerFocusChanged);
                mainWindow.LayoutUpdated -= layoutUpdated;
            };

            mainWindow.LayoutUpdated += layoutUpdated;
        }
    }
}
