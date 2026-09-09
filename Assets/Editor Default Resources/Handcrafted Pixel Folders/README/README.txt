Handcrafted Pixel Folders for Unity

Make your Unity project easier to navigate - and a lot more fun - with colorful custom folder icons!
This simple Editor extension lets you assign vibrant, pixel-art inspired folder icons in just a couple of clicks.

Features

* 23 handcrafted folder icons in a variety of colors and styles
* Works in both grid view and list view in the Unity Project window
* Lightweight, editor-only system that does not affect builds
* Automatically draws custom icons over Unity's default folder icons
* Easy to extend with your own custom artwork

How to Install

1. Import the .unitypackage into your Unity project.
2. Ensure this folder structure exisit:
Assets/
  ├── Editor Default Resources/
  │     └── Handcrafted Pixel Folders/
   
3. You’ll now see new context menu options when right-clicking folders in the Project window. Set Folder Color > Color Name

Tip: To reset a folder's icon, right-click it again and choose "Reset".

Customization

* Add your own PNG icons to the "Editor Default Resources/Handcrafted Pixel Folders" folder. (The custom pixel folders included are 16x16.)
* Update the script dictionary and menu to recognize your new icon.

Script Reference

The tool uses a single Editor script: Assets/Editor/FolderIconPostprocessor.cs
It uses EditorPrefs to save icon assignments per folder path.
Icons are drawn over Unity's default icons using OnProjectWindowItemGUI().

License

Free to use in commercial or personal projects.
Attribution is not required, but appreciated if you feel like it!

Color palette "Resurrect64" created by Kerrie Lake. Available on Lospec.

Created by CrazyBirdLady
Tool concept and development with help from ChatGPT
https://crazybirdlady.net/
