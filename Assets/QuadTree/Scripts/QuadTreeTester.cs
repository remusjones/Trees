using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace Trees.Scripts
{
    public class QuadTreeTester : MonoBehaviour
    {
        [SerializeField] public Vector2 size;
        public int quadTreeItemPerQuad = 1;
        [SerializeField] private int itemCount = 50;
        [HideInInspector] public QuadTree<IQuadTreeItem> quadTree;
        [HideInInspector] public List<QuadTreeGameObject> items = new List<QuadTreeGameObject>();
        [HideInInspector] public Rect rect;

        private GameObject rootStorageGameObject;
        private void Start()
        {
            rect  = new Rect(this.transform.position, size);
            quadTree = new QuadTree<IQuadTreeItem>(quadTreeItemPerQuad, rect);
            rootStorageGameObject = new GameObject("QuadTree Objects");

            AddItems();
        }

        [ContextMenu("Add Items")]
        public void AddItems()
        {
            for (int i = 0; i < itemCount; i++)
            {
                GameObject t = new GameObject($"{items.Count}");
                QuadTreeGameObject item = t.AddComponent<QuadTreeGameObject>();
                Transform itemTransform = item.transform;
                itemTransform.parent = rootStorageGameObject.transform;
                itemTransform.position = new Vector3(rect.center.x, 0f, rect.center.y);
                items.Add(item);
                quadTree.Insert(item);
            }

        }
        private void Update()
        {
            foreach (QuadTreeGameObject item in items)
            {
                Profiler.BeginSample("Remove Item");
                item.GetLeaf<IQuadTreeItem>().Remove(item);
                Profiler.EndSample();
                
                Profiler.BeginSample("Mono");
                Vector3 t = Random.insideUnitCircle;
                t = new Vector3(t.x, 0f, t.y);
       
                item.Steer(t);
                item.ManualUpdate();
                if (!rect.Contains(item.Position))
                    item.transform.position = new Vector3(rect.center.x, 0f, rect.center.y);
                Profiler.EndSample();
                Profiler.BeginSample("Insert Item");
           
                quadTree.Insert(item);
                Profiler.EndSample();
            }
        }
#if UNITY_EDITOR
        [CustomEditor(typeof(QuadTreeTester))]
        public class QuadTreeManagerEditor : Editor
        {
            private void OnSceneGUI()
            {
                QuadTreeTester self = target as QuadTreeTester;
                Handles.color = Color.red;
                Vector3 center = new Vector3(self.rect.center.x + self.transform.position.x, 0f, self.rect.center.y + self.transform.position.z);
                Handles.DrawWireCube(center, new Vector3(self.size.x, 0.05f, self.size.y));
              
                Handles.color = new Color(1,0.5f,0,0.3f);
               
                float halfWidth = self.size.x / 2f;
                float halfHeight = self.size.y / 2f;
                Vector3[] polygonCorners = new Vector3[4];
                polygonCorners[0] = center + new Vector3(-halfWidth ,0, -halfHeight);
                polygonCorners[1] = center + new Vector3(-halfWidth, 0, halfHeight);
                polygonCorners[2] = center + new Vector3(halfWidth, 0, halfHeight);
                polygonCorners[3] = center + new Vector3(halfWidth,0 , -halfHeight);
                Handles.DrawAAConvexPolygon(polygonCorners);
                
                
                Handles.color = new Color(0,0,0,1f);
                foreach (QuadTreeGameObject item in self.items)
                {
                    Handles.DrawSolidDisc(new Vector3(item.Position.x, 0f, item.Position.y), Vector3.up, 0.05f );
                }
                Handles.color = Color.black;
                self.quadTree?.DrawHandles();
            }
        }
#endif
    }

}