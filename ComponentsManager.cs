//#define BAD_PERF // TODO CHANGEZ MOI. Mettre en commentaire pour utiliser votre propre structure

using System;
using UnityEngine;
using System.Collections.Generic;

#if BAD_PERF
using InnerType = System.Collections.Generic.Dictionary<uint, IComponent>;
using AllComponents = System.Collections.Generic.Dictionary<uint, System.Collections.Generic.Dictionary<uint, IComponent>>;
#else
using InnerType = SequentialPool;
using AllComponents = System.Collections.Generic.List<SequentialPool>;
#endif

// Appeler GetHashCode sur un Type est couteux. Cette classe sert a precalculer le hashcode
public static class TypeRegistry<T> where T : IComponent
{
    public static uint typeID = (uint)Mathf.Abs(default(T).GetRandomNumber()) % ComponentsManager.maxEntities;
}

public class Singleton<V> where V : new()
{
    private static bool isInitiated = false;
    private static V _instance;
    public static V Instance
    {
        get
        {
            if (!isInitiated)
            {
                isInitiated = true;
                _instance = new V();
            }
            return _instance;
        }
    }
    protected Singleton() { }
}

internal class ComponentsManager : Singleton<ComponentsManager>
{
    private AllComponents _allComponents = new AllComponents();

    private List<uint> hashCodes = new List<uint>();

    private List<IComponent> allEntities;

    public const int maxEntities = 2000;

    public void DebugPrint()
    {
        string toPrint = "";
        var allComponents = Instance.DebugGetAllComponents();
        foreach (var type in allComponents)
        {
            toPrint += $"{type}: \n";
#if !BAD_PERF
            var count  = _allComponents.Count;
            for (var i = 0; i < count ; i++)
            {
#else
            foreach (var component in type.Value)
#endif
                {
#if BAD_PERF
                toPrint += $"\t{component.Key}: {component.Value}\n";
#else
                    toPrint += $"\t{_allComponents[i]}: {_allComponents[i].sequentialPool}\n";
#endif
                }
                toPrint += "\n";
            }
            Debug.Log(toPrint);
        }
    }

    // CRUD
    public void SetComponent<T>(EntityComponent entityID, IComponent component) where T : IComponent
    {
        if (!hashCodes.Contains(TypeRegistry<T>.typeID))
        {
            _allComponents.Add(new InnerType());
            hashCodes.Add(TypeRegistry<T>.typeID);
        }
        _allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].SetComponent(entityID, component);   
    }
    public void RemoveComponent<T>(EntityComponent entityID) where T : IComponent
    {
        _allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].RemoveComponent(entityID);
    }
    public T GetComponent<T>(EntityComponent entityID) where T : IComponent
    {
        return (T)_allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].GetComponent(entityID);
    }
    public bool TryGetComponent<T>(EntityComponent entityID, out T component) where T : IComponent
    {
        if (hashCodes.Contains(TypeRegistry<T>.typeID))
        {
            if (_allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].IndirectionTable[entityID.id] != null)
            {
                component = (T)_allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].GetComponent(entityID);
                return true;
            }
        }
        component = default;
        return false;
    }

    public bool EntityContains<T>(EntityComponent entity) where T : IComponent
    {
        if (_allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].IndirectionTable[entity.id] != null)
            return true;
        return false;
    }

    public void ClearComponents<T>() where T : IComponent
    {
        if (!hashCodes.Contains(TypeRegistry<T>.typeID))
        {
            _allComponents.Add(new InnerType());
            hashCodes.Add(TypeRegistry<T>.typeID);
        }
        else
        {
           _allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].Clear();
        }
    }

    public void ForEach<T1>(Action<EntityComponent, T1> lambda) where T1 : IComponent
    {
        allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].sequentialPool;

        int IndexT1 = hashCodes.IndexOf(TypeRegistry<T1>.typeID);

        foreach(EntityComponent entity in allEntities)
        {
            if (_allComponents[IndexT1].IndirectionTable[entity.id] == null)
            {
                continue;
            }
            lambda(entity, (T1)_allComponents[IndexT1].GetComponent(entity));
        }
    }

    public void ForEach<T1, T2>(Action<EntityComponent, T1, T2> lambda) where T1 : IComponent where T2 : IComponent
    {
        allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].sequentialPool;

        int IndexT1 = hashCodes.IndexOf(TypeRegistry<T1>.typeID);
        int IndexT2 = hashCodes.IndexOf(TypeRegistry<T2>.typeID);


        foreach (EntityComponent entity in allEntities)
        {
            if (_allComponents[IndexT1].IndirectionTable[entity.id] == null ||
                _allComponents[IndexT2].IndirectionTable[entity.id] == null
                )
            {
                continue;
            }
            lambda(entity, (T1)_allComponents[IndexT1].GetComponent(entity), (T2)_allComponents[IndexT2].GetComponent(entity));
        }

    }

    public void ForEach<T1, T2, T3>(Action<EntityComponent, T1, T2, T3> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {
        allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].sequentialPool;

        int IndexT1 = hashCodes.IndexOf(TypeRegistry<T1>.typeID);
        int IndexT2 = hashCodes.IndexOf(TypeRegistry<T2>.typeID);
        int IndexT3 = hashCodes.IndexOf(TypeRegistry<T3>.typeID);

        foreach (EntityComponent entity in allEntities)
        {

            if (_allComponents[IndexT1].IndirectionTable[entity.id] == null ||
                _allComponents[IndexT2].IndirectionTable[entity.id] == null ||
                _allComponents[IndexT3].IndirectionTable[entity.id] == null
                )
            {
                continue;
            }
            lambda(entity, (T1)_allComponents[IndexT1].GetComponent(entity), (T2)_allComponents[IndexT2].GetComponent(entity), (T3)_allComponents[IndexT3].GetComponent(entity));
        }

    }

    public void ForEach<T1, T2, T3, T4>(Action<EntityComponent, T1, T2, T3, T4> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {
        allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].sequentialPool;

        int IndexT1 = hashCodes.IndexOf(TypeRegistry<T1>.typeID);
        int IndexT2 = hashCodes.IndexOf(TypeRegistry<T2>.typeID);
        int IndexT3 = hashCodes.IndexOf(TypeRegistry<T3>.typeID);
        int IndexT4 = hashCodes.IndexOf(TypeRegistry<T4>.typeID);

        foreach (EntityComponent entity in allEntities)
        {
            if (_allComponents[IndexT1].IndirectionTable[entity.id] == null ||
                _allComponents[IndexT2].IndirectionTable[entity.id] == null ||
                _allComponents[IndexT3].IndirectionTable[entity.id] == null ||
                _allComponents[IndexT4].IndirectionTable[entity.id] == null
                )
            {
                continue;
            }
            lambda(entity, (T1)_allComponents[IndexT1].GetComponent(entity), (T2)_allComponents[IndexT2].GetComponent(entity), (T3)_allComponents[IndexT3].GetComponent(entity), (T4)_allComponents[IndexT4].GetComponent(entity));
        }

    }

    public AllComponents DebugGetAllComponents()
    {
        return _allComponents;
    }
}
