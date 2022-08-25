# Mono Script Tools

This package contains a collection of utilities for quickly creating scripts in Unity. It is designed to save time by opening files immediately after they are created, as opposed to waiting for C# assemblies to recompile, and by giving users some more flexibility with script templates.

## Installation

Go to `Window > Package Manager > Add package from git URL...` and paste the following URL: `https://github.com/ovaworkt/mono-script-tools.git#release`. Alternatively, clone the repository and add it to your project, whichever works best.

## Usage

This package has some default keyboard shortcuts setup for optimal use, which you are free to change as you wish. `Alt+C` - Creates a single MonoScript asset, allowing the user to name it first. `Alt+Shift+C` opens a popup window allowing users to create several MonoScript assets at once, allowing some more customisation. This can also be accessed via `Assets > Create > Multiple C# Scripts`. Once open, you can either use the buttons provided, or you can press `Tab` to advance to the next field, `Shift+Tab` to retreat to the previous field, `Escape+Escape` to cancel or `Enter+Enter` to create the scripts, provided there are no naming violations. You can change the icon of the script by clicking on its icon and using the popup menu, or you can hover over the icon instead, to see the C# type that the script will derive from in tooltip form.

The MonoScriptTemplate system works similarly to Unity's default templates. They are stored as .json files and replaceable words are wrapped in # symbols. The system uses regular expressions to match and omit parts of script names. For instance: `ItemSO` would be detected as a ScriptableObject due to the `SO` suffix and will be removed from the resulting filename.

## Customisation

You can create custom templates and other types of templates to suit your needs. `MonoScriptTemplateWriter.cs` is what handles replacing specific keywords, so editing that allows you some more flexibility with what you can create. Its relatively crude at the moment, but it works.
