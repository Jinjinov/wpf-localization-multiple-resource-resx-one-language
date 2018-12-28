# WPF localization with multiple Resource.resx files (resource managers) used for one language.

If you only want to localize text and you find the [WPF Localization Extension](https://github.com/XAMLMarkupExtensions/WPFLocalizationExtension) too large for your needs, you can use this as a "one file" alternative that can localize strings (but not images).

It supports dynamic changes of culture, that means: it allows changing language at run-time, without restarting the application.

Useful for localizing multiple Prism modules:

    ModuleA\Properties\Resources.resx
    ModuleA\Properties\Resources.de.resx
    ModuleA\Properties\Resources.es.resx
    
    ModuleB\Properties\Resources.resx
    ModuleB\Properties\Resources.de.resx
    ModuleB\Properties\Resources.es.resx

This solution is based on the excellent [simple approach by Jakub Fijałkowski](https://codinginfinity.me/post/2015-05-10/localization_of_a_wpf_app_the_simple_approach)

His solution can be found on [GitHub](https://gist.github.com/jakubfijalkowski/0771bfbd26ce68456d3e)

The hard coded reference to the ResourceManager was replaced with a Dictionary.

The markup extension is still used in the same way:

    <TextBlock Text="{loc:Loc MyText}"/>

A new attached property Translation.ResourceManager is used with the root control of the visual tree (e.g. UserControl) to set the ResourceManager for the localization markup extension:

    <UserControl xmlns:resx="clr-namespace:MyProject.Properties"
                 xmlns:loc="clr-namespace:WpfLocalizationWithMultipleResourceManagers"
                 loc:Translation.ResourceManager="{x:Static resx:Resources.ResourceManager}">
        <TextBlock Text="{loc:Loc MyText}"/>
    </UserControl>

If you want to use a different ResourceManager with a specific control you can override the root setting at the control level:

    <TextBox loc:Translation.ResourceManager="{x:Static resx:Resources.ResourceManager}" Text="{loc:Loc MyText}"/>

If you want to use the markup extension inside a control template you have to set the Translation.ResourceManager attached property on the TemplatedParent.

## If you use this solution in a separate class library

- Visual Studio designer displays the FallbackValue that was added to LocExtension.

- You have to change Resources access modifier from "internal" to "public".

You can do this by setting the Custom Tool property in the Property Window for the Resx file: use the PublicResXFileCodeGenerator instead of the ResXFileCodeGenerator.

Alternatetively you can set the Access Modifier to public when you open the resx file in Visual Studio. The Access Modifier dropdown box can be found at the top of the form.
