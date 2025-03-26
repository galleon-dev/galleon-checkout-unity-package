using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout
{
    public class EditorExplorer : EditorWindow
    {
        [MenuItem("Tools/Galleon/Explorer")]
        public static void ShowExample()
        {
            EditorExplorer wnd = GetWindow<EditorExplorer>();
            wnd.titleContent = new GUIContent("Explorer");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Add(new Label("Editor Explorer"));
            root.Add(new Divider());
            root.Add(new Explorer());
        }
    }
}
