using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomEditors
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset VisualTree;
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
        
            VisualTree.CloneTree(root);
        
            return root;
        }
    }
}