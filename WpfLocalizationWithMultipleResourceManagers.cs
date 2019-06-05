using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;

namespace WpfLocalizationWithMultipleResourceManagers
{
    public class TranslationSource : INotifyPropertyChanged
    {
        public static TranslationSource Instance { get; } = new TranslationSource();

        private readonly Dictionary<string, ResourceManager> resourceManagerDictionary = new Dictionary<string, ResourceManager>();

        public string this[string key]
        {
            get
            {
                var (baseName, stringName) = SplitName(key);
                string translation = null;
                if (resourceManagerDictionary.ContainsKey(baseName))
                    translation = resourceManagerDictionary[baseName].GetString(stringName, currentCulture);
                return translation ?? key;
            }
        }

        private CultureInfo currentCulture = null;
        public CultureInfo CurrentCulture
        {
            get { return currentCulture; }
            set
            {
                if (currentCulture != value)
                {
                    currentCulture = value;
                    // string.Empty/null indicates that all properties have changed
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                }
            }
        }

        // WPF bindings register PropertyChanged event if the object supports it and update themselves when it is raised
        public event PropertyChangedEventHandler PropertyChanged;

        public void AddResourceManager(ResourceManager resourceManager)
        {
            if (!resourceManagerDictionary.ContainsKey(resourceManager.BaseName))
            {
                resourceManagerDictionary.Add(resourceManager.BaseName, resourceManager);
            }
        }

        public static (string baseName, string stringName) SplitName(string name)
        {
            int idx = name.LastIndexOf('.');
            return (name.Substring(0, idx), name.Substring(idx + 1));
        }
    }

    public class Translation : DependencyObject
    {
        public static readonly DependencyProperty ResourceManagerProperty =
            DependencyProperty.RegisterAttached("ResourceManager", typeof(ResourceManager), typeof(Translation));

        public static ResourceManager GetResourceManager(DependencyObject dependencyObject)
        {
            return (ResourceManager)dependencyObject.GetValue(ResourceManagerProperty);
        }

        public static void SetResourceManager(DependencyObject dependencyObject, ResourceManager value)
        {
            dependencyObject.SetValue(ResourceManagerProperty, value);
        }
    }

    public class LocExtension : MarkupExtension
    {
        public string StringName { get; }

        public LocExtension(string stringName)
        {
            StringName = stringName;
        }

        private ResourceManager GetResourceManager(object control)
        {
            if (control is DependencyObject dependencyObject)
            {
                object localValue = dependencyObject.ReadLocalValue(Translation.ResourceManagerProperty);

                // does this control have a "Translation.ResourceManager" attached property with a set value?
                if (localValue != DependencyProperty.UnsetValue)
                {
                    if (localValue is ResourceManager resourceManager)
                    {
                        TranslationSource.Instance.AddResourceManager(resourceManager);

                        return resourceManager;
                    }
                }
            }

            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // targetObject is the control that is using the LocExtension
            object targetObject = (serviceProvider as IProvideValueTarget)?.TargetObject;

            if (targetObject?.GetType().Name == "SharedDp") // is extension used in a control template?
                return targetObject; // required for template re-binding

            string baseName = GetResourceManager(targetObject)?.BaseName ?? string.Empty;

            if (string.IsNullOrEmpty(baseName))
            {
                // rootObject is the root control of the visual tree (the top parent of targetObject)
                object rootObject = (serviceProvider as IRootObjectProvider)?.RootObject;
                baseName = GetResourceManager(rootObject)?.BaseName ?? string.Empty;
            }

            if (string.IsNullOrEmpty(baseName)) // template re-binding
            {
                if (targetObject is FrameworkElement frameworkElement)
                {
                    baseName = GetResourceManager(frameworkElement.TemplatedParent)?.BaseName ?? string.Empty;
                }
            }

            Binding binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Path = new PropertyPath($"[{baseName}.{StringName}]"),
                Source = TranslationSource.Instance,
                FallbackValue = StringName
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
