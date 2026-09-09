#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace CrazyBirdLady.FolderIcons
{
    // Static manager class for folder icon logic and menu integration
    [InitializeOnLoad]
    public static class FolderIconManager
    {
        // Stores loaded folder icons by color name
        private static Dictionary<string, Texture2D> folderIcons = new Dictionary<string, Texture2D>();
        // Prefix for EditorPrefs keys to store folder color selection
        private static readonly string iconKeyPrefix = "FolderColor_";

        // Static constructor: loads icons and hooks into the project window GUI
        static FolderIconManager()
        {
            LoadIcons();
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }

        // Loads all folder icon textures from Editor Default Resources
        public static void LoadIcons()
        {
            folderIcons.Clear();
            folderIcons["CharcoalGray"] = LoadIcon("charcoalGray");
            folderIcons["LightGray"] = LoadIcon("lightGray");
            folderIcons["Snow"] = LoadIcon("snow");
            folderIcons["Orange"] = LoadIcon("orange");
            folderIcons["Red"] = LoadIcon("red");
            folderIcons["Brown"] = LoadIcon("brown");
            folderIcons["Yellow"] = LoadIcon("yellow");
            folderIcons["Lemon"] = LoadIcon("lemon");
            folderIcons["Olive"] = LoadIcon("olive");
            folderIcons["GrassGreen"] = LoadIcon("grassGreen");
            folderIcons["Lime"] = LoadIcon("lime");
            folderIcons["Teal"] = LoadIcon("teal");
            folderIcons["Turquoise"] = LoadIcon("turquoise");
            folderIcons["NavyBlue"] = LoadIcon("navyBlue");
            folderIcons["SkyBlue"] = LoadIcon("skyBlue");
            folderIcons["Purple"] = LoadIcon("purple");
            folderIcons["Lavender"] = LoadIcon("lavender");
            folderIcons["Rose"] = LoadIcon("rose");
            folderIcons["Coral"] = LoadIcon("coral");
            folderIcons["Blush"] = LoadIcon("blush");
            folderIcons["Bubblegum"] = LoadIcon("bubblegum");
            folderIcons["Apricot"] = LoadIcon("apricot");
            folderIcons["Sand"] = LoadIcon("sand");
        }

        // Loads a single icon texture by name from Editor Default Resources
        private static Texture2D LoadIcon(string name)
        {
            // Loads from: Assets/Editor Default Resources/Handcrafted Pixel Folders/name.png
            return EditorGUIUtility.Load($"Handcrafted Pixel Folders/{name}.png") as Texture2D;
        }

        // Draws the custom folder icon in the Project window
        private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.IsValidFolder(path))
                return;

            // Get the selected color for this folder
            string iconName = EditorPrefs.GetString(iconKeyPrefix + path, "");
            if (!folderIcons.TryGetValue(iconName, out Texture2D baseIcon) || baseIcon == null)
                return;

            Texture2D iconToUse = baseIcon;
            bool isListView = selectionRect.height < 25f;

            // Use alternate icon for list view if available
            if (isListView)
            {
                Texture2D listIcon = LoadIcon(iconName + "_list");
                if (listIcon != null)
                    iconToUse = listIcon;
            }

            // Calculate icon rectangle for drawing
            Rect iconRect = isListView
                ? new Rect(selectionRect.x, selectionRect.y + 1f, 16f, 16f)
                : new Rect(
                    selectionRect.x + (selectionRect.width - Mathf.Min(selectionRect.width, selectionRect.height - 12f)) / 2f,
                    selectionRect.y,
                    Mathf.Min(selectionRect.width, selectionRect.height - 12f),
                    Mathf.Min(selectionRect.width, selectionRect.height - 12f)
                  );

            // Draw the icon
            GUI.DrawTexture(iconRect, iconToUse, ScaleMode.ScaleToFit);
        }

        // --- Menu Items ---
        // Validation for all color menu items (only enabled for folders)
        [MenuItem("Assets/Set Folder Color/CharcoalGray", true)]
        [MenuItem("Assets/Set Folder Color/LightGray", true)]
        [MenuItem("Assets/Set Folder Color/Snow", true)]
        [MenuItem("Assets/Set Folder Color/Orange", true)]
        [MenuItem("Assets/Set Folder Color/Red", true)]
        [MenuItem("Assets/Set Folder Color/Brown", true)]
        [MenuItem("Assets/Set Folder Color/Yellow", true)]
        [MenuItem("Assets/Set Folder Color/Lemon", true)]
        [MenuItem("Assets/Set Folder Color/Olive", true)]
        [MenuItem("Assets/Set Folder Color/GrassGreen", true)]
        [MenuItem("Assets/Set Folder Color/Lime", true)]
        [MenuItem("Assets/Set Folder Color/Teal", true)]
        [MenuItem("Assets/Set Folder Color/Turquoise", true)]
        [MenuItem("Assets/Set Folder Color/NavyBlue", true)]
        [MenuItem("Assets/Set Folder Color/SkyBlue", true)]
        [MenuItem("Assets/Set Folder Color/Purple", true)]
        [MenuItem("Assets/Set Folder Color/Lavender", true)]
        [MenuItem("Assets/Set Folder Color/Rose", true)]
        [MenuItem("Assets/Set Folder Color/Coral", true)]
        [MenuItem("Assets/Set Folder Color/Blush", true)]
        [MenuItem("Assets/Set Folder Color/Bubblegum", true)]
        [MenuItem("Assets/Set Folder Color/Apricot", true)]
        [MenuItem("Assets/Set Folder Color/Sand", true)]

        // Returns true if a folder is selected for color menu items
        public static bool ValidateSetColor()
        {
            return Selection.activeObject != null &&
                   AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        // --- Color Setters ---
        // Each menu item sets the color for the selected folder
        [MenuItem("Assets/Set Folder Color/CharcoalGray")]
        public static void SetCharcoalGray() => SetColor("CharcoalGray");
        [MenuItem("Assets/Set Folder Color/LightGray")]
        public static void SetLightGray() => SetColor("LightGray");
        [MenuItem("Assets/Set Folder Color/Snow")]
        public static void SetSnow() => SetColor("Snow");
        [MenuItem("Assets/Set Folder Color/Orange")]
        public static void SetOrange() => SetColor("Orange");
        [MenuItem("Assets/Set Folder Color/Red")]
        public static void SetRed() => SetColor("Red");
        [MenuItem("Assets/Set Folder Color/Brown")]
        public static void SetBrown() => SetColor("Brown");
        [MenuItem("Assets/Set Folder Color/Yellow")]
        public static void SetYellow() => SetColor("Yellow");
        [MenuItem("Assets/Set Folder Color/Lemon")]
        public static void SetLemon() => SetColor("Lemon");
        [MenuItem("Assets/Set Folder Color/Olive")]
        public static void SetOlive() => SetColor("Olive");
        [MenuItem("Assets/Set Folder Color/GrassGreen")]
        public static void SetGrassGreen() => SetColor("GrassGreen");
        [MenuItem("Assets/Set Folder Color/Lime")]
        public static void SetLime() => SetColor("Lime");
        [MenuItem("Assets/Set Folder Color/Teal")]
        public static void SetTeal() => SetColor("Teal");
        [MenuItem("Assets/Set Folder Color/Turquoise")]
        public static void SetTurquoise() => SetColor("Turquoise");
        [MenuItem("Assets/Set Folder Color/NavyBlue")]
        public static void SetNavyBlue() => SetColor("NavyBlue");
        [MenuItem("Assets/Set Folder Color/SkyBlue")]
        public static void SetSkyBlue() => SetColor("SkyBlue");
        [MenuItem("Assets/Set Folder Color/Purple")]
        public static void SetPurple() => SetColor("Purple");
        [MenuItem("Assets/Set Folder Color/Lavender")]
        public static void SetLavender() => SetColor("Lavender");
        [MenuItem("Assets/Set Folder Color/Rose")]
        public static void SetRose() => SetColor("Rose");
        [MenuItem("Assets/Set Folder Color/Coral")]
        public static void SetCoral() => SetColor("Coral");
        [MenuItem("Assets/Set Folder Color/Blush")]
        public static void SetBlush() => SetColor("Blush");
        [MenuItem("Assets/Set Folder Color/Bubblegum")]
        public static void SetBubblegum() => SetColor("Bubblegum");
        [MenuItem("Assets/Set Folder Color/Apricot")]
        public static void SetApricot() => SetColor("Apricot");
        [MenuItem("Assets/Set Folder Color/Sand")]
        public static void SetSand() => SetColor("Sand");

        // Sets the color for the selected folder and refreshes the Project window
        public static void SetColor(string color)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (AssetDatabase.IsValidFolder(path))
            {
                EditorPrefs.SetString(iconKeyPrefix + path, color);
                EditorApplication.RepaintProjectWindow();
            }
        }

        // Menu item to reset folder color to default
        [MenuItem("Assets/Set Folder Color/Reset")]
        public static void ResetFolderColor()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (AssetDatabase.IsValidFolder(path))
            {
                EditorPrefs.DeleteKey(iconKeyPrefix + path);
                EditorApplication.RepaintProjectWindow();
            }
        }

        // Validation for reset menu item (only enabled if folder has a color set)
        [MenuItem("Assets/Set Folder Color/Reset", true)]
        public static bool ValidateResetFolderColor()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return AssetDatabase.IsValidFolder(path) && EditorPrefs.HasKey(iconKeyPrefix + path);
        }
    }

    // AssetPostprocessor class to refresh icons when icon assets are imported
    public class FolderIconAssetPostprocessor : AssetPostprocessor
    {
        // Called by Unity when assets are imported, deleted, or moved
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                // If a Handcrafted Pixel Folders asset was imported, reload icons and refresh Project window
                if (asset.Contains("Handcrafted Pixel Folders"))
                {
                    FolderIconManager.LoadIcons();
                    EditorApplication.RepaintProjectWindow();
                    break;
                }
            }
        }
    }
}
#endif




                    
