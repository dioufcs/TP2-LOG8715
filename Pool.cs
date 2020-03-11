using System;
using System.Collections.Generic;

public class Pool
{
    //public List<IComponent> pool;
    public IComponent[] pool;


    public Pool()
    {
       pool = new IComponent[ComponentsManager.maxEntities];
    }

    public void SetComponent(EntityComponent entityID, IComponent component)
    {
        pool[entityID.id] = component;
    }
public void RemoveComponent(EntityComponent entityID)
    {
        pool[entityID.id] = null;
    }

    public IComponent GetComponent(EntityComponent entityID)
    {
        //Debug.Log( componentIndex.ToString() + " " + sequentialPool.Count.ToString());
        //Debug.Log(sequentialPool[0].GetType().Name);
        return pool[entityID.id];

    }

    public void Clear()
    {
        Array.Clear(pool, 0, 1100);
        //IndirectionTable = new int?[ComponentsManager.maxEntities];
        //pool.Clear();
    }


}
