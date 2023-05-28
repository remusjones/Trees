using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Pool;

public interface IQuadTreeItem
{
    public void SetLeaf<T>(QuadTree<T> leaf) where T : IQuadTreeItem;
    public QuadTree<T> GetLeaf<T>() where T : IQuadTreeItem;
    Vector2 Position { get; }
}



public class QuadTree<T> where T : IQuadTreeItem
{
    private List<T> storedQuadTreeItems;
    private Rect bounds;
    private readonly QuadTree<T>[] cells = new QuadTree<T>[4];
    private readonly int maxItems;
    private bool listPoolHasReleased = false;
    public QuadTree(int size, Rect bounds)
    {
        storedQuadTreeItems = ListPool<T>.Get();
        maxItems = size;
        this.bounds = bounds;
    }

    public void Insert(T item)
    {
        bool cellsInitialized = cells[0] != null;
        if (cellsInitialized)
        {
            int res = GetAvailableCell(item.Position);
            if (res != -1)
            {
                cells[res].Insert(item);
                return;
            }
        }

        if (listPoolHasReleased)
        {
            storedQuadTreeItems = ListPool<T>.Get();
            listPoolHasReleased = false;
        }

        storedQuadTreeItems.Add(item);
        item.SetLeaf(this);
        if (storedQuadTreeItems.Count > maxItems)
        {
            if (!cellsInitialized)
            {
                float w = (bounds.width / 2f);
                float h = (bounds.height / 2f);
                float x = bounds.x;
                float y = bounds.y;
                
                Rect[] rects = new Rect[4];
                rects[0] = new Rect(x + w, y, w, h);
                rects[1] = new Rect(x, y, w, h);
                rects[2] = new Rect(x, y + h, w, h);
                rects[3] = new Rect(x + w, y + h, w, h);
                cells[0] = new QuadTree<T>(maxItems, rects[0]);
                cells[1] = new QuadTree<T>(maxItems, rects[1]);
                cells[2] = new QuadTree<T>(maxItems,  rects[2]);
                cells[3] = new QuadTree<T>(maxItems, rects[3]);
               
            }
            
            // Redistribute existing items into cells 
            for(int i = storedQuadTreeItems.Count-1; i >= 0; i--)
            {
                int res = GetAvailableCell(storedQuadTreeItems[i].Position);
                if(res != -1)
                    cells[res].Insert(storedQuadTreeItems[i]);             
                storedQuadTreeItems.RemoveAt(i);
                ValidateListPool();
            }
        }
    }
    private void ValidateListPool()
    {
        if (storedQuadTreeItems.Count == 0 && !listPoolHasReleased)
        {
            ListPool<T>.Release(storedQuadTreeItems);
            listPoolHasReleased = true;
        }
    }
    public void Remove(T item)
    {
        if (item.GetLeaf<T>() != this && !Contains(item.Position))
        {
            return;
        }

        if (storedQuadTreeItems.Remove(item))
        {
            ValidateListPool();
        }

        if (cells[0] == null) return;
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].Remove(item);
        }
    }
    public void Clear()
    {
        for(int i  = 0; i < cells.Length; i++)
        {
            if(cells[i] != null)
            {
                cells[i].Clear();
                cells[i] = null;
            }
        }
    }
    public int RemainingItemsInCell()
    {
        int sum = listPoolHasReleased ? 0 : storedQuadTreeItems.Count;
        if (cells[0] == null)
            return sum;
        for(int i = 0; i < cells.Length; i++)
        {
            sum += cells[i].RemainingItemsInCell();
        }
        return sum;
    }
    private int GetAvailableCell(Vector2 itemPosition)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].Contains(itemPosition))
                return i;
        }
        return -1;
    }
    public bool Contains(Vector2 position)
    {
        return bounds.Contains(position);
    }
    
    
#if UNITY_EDITOR
    private static int highestDepth = 1;
    public void DrawHandles(int depth = 0)
    {
        if (depth > highestDepth)
            highestDepth = depth;

        if (RemainingItemsInCell() == 0)
            return;
        Handles.color = Color.HSVToRGB((depth / (float) highestDepth), 1f, 1f);
        if (!listPoolHasReleased)
        {
            foreach (T item in storedQuadTreeItems)
            {
                Handles.DrawWireDisc(new Vector3(item.Position.x, 0f, item.Position.y), Vector3.up, 0.05f);
            }
        }

        Handles.DrawLine(new Vector3(bounds.x, 0, bounds.y), new Vector3(bounds.x, 0, bounds.y + bounds.height));
        Handles.DrawLine(new Vector3(bounds.x, 0, bounds.y), new Vector3(bounds.x + bounds.width, 0, bounds.y));
        Handles.DrawLine(new Vector3(bounds.x + bounds.width, 0, bounds.y),
            new Vector3(bounds.x + bounds.width, 0, bounds.y + bounds.height));
        Handles.DrawLine(new Vector3(bounds.x, 0, bounds.y + bounds.height),
            new Vector3(bounds.x + bounds.width, 0, bounds.y + bounds.height));
        if (cells[0] == null) return;
        
        foreach (QuadTree<T> t in cells)
        {
            t?.DrawHandles(depth + 1);
        }
        

        if (highestDepth == depth && highestDepth > 0)
            highestDepth--;

    }
#endif

}
