#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Galleon.Checkout.ExplorerTests
{
    public class TestExplorerWindow : EditorWindow
    {
        private ExplorerItem explorerItem;

        [MenuItem("Tools/Galleon/TestExplorer")]
        public static void ShowWindow()
        {
            TestExplorerWindow wnd = GetWindow<TestExplorerWindow>();
            wnd.titleContent = new GUIContent("TestExplorer");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            explorerItem = new ExplorerItem
            {
                Target = Root.Instance
            };

            explorerItem.BuildUI(root);
        }
    }
}

#endif
