using System;
using UnityEngine;

namespace Trees.Scripts
{
    public class QuadTreeGameObject : MonoBehaviour, IQuadTreeItem
    {
        public QuadTree<IQuadTreeItem> leafRef;
        public void SetLeaf<T>(QuadTree<T> leaf) where T : IQuadTreeItem
        {
            leafRef = leaf as QuadTree<IQuadTreeItem>;
        }
        public QuadTree<T> GetLeaf<T>() where T : IQuadTreeItem
        {
            return leafRef as QuadTree<T>;
        }

        public Vector2 Position => new Vector2(this.transform.position.x, this.transform.position.z);
        private Vector3 heading;
        public void Steer(Vector3 direction)
        {
            heading += direction;
            heading.Normalize();
        }

        public void ManualUpdate()
        {
            this.transform.Translate(heading * Time.deltaTime);
        }
    }
}