using System.Collections.Generic;
using UnityEngine;

namespace Support.GraphicsGroup
{
    public class GraphicsSwitchGroup : MonoBehaviour
    {
        [SerializeField] private string _id;
        [SerializeField] private List<GraphicsSwitchElement> _elements;

        public string Id => _id;

        public void Switch()
        {
            CommonActions();

            foreach (var element in _elements)
                element.Execute();
        }

        public bool Contains(GraphicsSwitchElement target)
        {
            CommonActions();

            var targetId = GetId(target);

            foreach (var element in _elements)
            {
                var elementId = GetId(element);

                if (targetId == elementId)
                    return true;
            }

            return false;
        }

        public void Add(GraphicsSwitchElement target)
        {
            CommonActions();

            if (!Contains(target))
                _elements.Add(target);

            MakeDirty();
        }

        private void MakeDirty()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
        }

        public void Remove(GraphicsSwitchElement target)
        {
            CommonActions();

            var targetId = GetId(target);

            _elements.RemoveAll(item => GetId(item) == targetId);

            MakeDirty();
        }

        private string GetId(GraphicsSwitchElement element)
        {
            return element.GetInstanceID().ToString();
        }

        private void CommonActions()
        {
            if (_elements == null)
                _elements = new List<GraphicsSwitchElement>();

            var count = _elements.RemoveAll(element => element == null);

            if (count > 0)
                MakeDirty();
        }
    }
}