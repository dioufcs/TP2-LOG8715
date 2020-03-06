using System;
using System.Collections.Generic;
using UnityEngine;

public class SequentialPool : IEquatable<SequentialPool>
{
    public uint hashCode;
    public int?[] IndirectionTable;
    public int index;
    public List<IComponent> sequentialPool;

    //public int?[] IndirectionTable
    //{
    //    get => IndirectionTable;
    //}

    //public List<IComponent> sequentialPool
    //{
    //    get => sequentialPool;
    //}

    public SequentialPool(uint hashCode, int index)
    {
        this.hashCode = hashCode;
        this.index = index;
        IndirectionTable = new int?[ComponentsManager.maxEntities];
        sequentialPool = new List<IComponent>();
    }

    public void SetComponent(EntityComponent entityID, IComponent component)
    {
        if (IndirectionTable[entityID.id] == null)
        {
            IndirectionTable[entityID.id] = (int)sequentialPool.Count;
            sequentialPool.Add(component);
        }
        else
        {
            int componentIndex = (int)IndirectionTable[entityID.id];
            sequentialPool[componentIndex] = component;

        }
    }

    public void RemoveComponent(EntityComponent entityID)
    {
        int componentIndex = (int)IndirectionTable[entityID.id];
        int componentsCount = sequentialPool.Count;
        int toReplace = Array.IndexOf(IndirectionTable, componentsCount - 1);


        IndirectionTable[entityID.id] = null;


        if (componentIndex != componentsCount - 1)
        {
            sequentialPool[componentIndex] = sequentialPool[componentsCount - 1];
            IndirectionTable[toReplace] = componentIndex;
        }
        sequentialPool.RemoveAt(componentsCount - 1);
    }

    public IComponent GetComponent(EntityComponent entityID)
    {
        int componentIndex = (int)IndirectionTable[entityID.id];
        //Debug.Log( componentIndex.ToString() + " " + sequentialPool.Count.ToString());
        //Debug.Log(sequentialPool[0].GetType().Name);
        return sequentialPool[componentIndex];

    }

    public void Clear()
    {
        Array.Clear(IndirectionTable, 0, 1100);
        //IndirectionTable = new int?[ComponentsManager.maxEntities];
        sequentialPool.Clear();
    }

    public bool Equals(SequentialPool other)
    {
        return this.hashCode.Equals(other.hashCode);
    }
}
