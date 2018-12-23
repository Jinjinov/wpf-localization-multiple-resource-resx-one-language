WPF localization with multiple Resource.resx files used for one language. Useful for localizing multiple Prism modules.

This solution is based on the simple approach by Jakub Fijałkowski:

https://codinginfinity.me/post/2015-05-10/localization_of_a_wpf_app_the_simple_approach

The hard coded reference to the ResourceManager was replaced with a Dictionary.

The markup extension is still used in the same way:

    <TextBlock Text="{loc:Loc MyText}"/>

A new attached property Translation.ResourceManager is used with the root control of the visual tree (e.g. UserControl) to set the ResourceManager for the localization markup extension:

    <UserControl xmlns:resx="clr-namespace:MyProject.Properties"
                 xmlns:loc="clr-namespace:WpfLocalizationMultipleResourceResxOneLanguage"
                 loc:Translation.ResourceManager="{x:Static resx:Resources.ResourceManager}">
        <TextBlock Text="{loc:Loc MyText}"/>
    </UserControl>

If you want to use a different ResourceManager with a specific control, you can override the root setting at the control level:

    <TextBox loc:Translation.ResourceManager="{x:Static resx:Resources.ResourceManager}" Text="{loc:Loc MyText}"/>

You have to change Resources access modifier from "internal" to "public". You can do this by setting the Custom Tool property in the Property Window for the Resx file: use the PublicResXFileCodeGenerator instead of the ResXFileCodeGenerator. Alternatetively you can set the Access Modifier to public when you open the resx file in Visual Studio. The Access Modifier dropdown box can be found at the top of the form.
