//#define BAD_PERF // TODO CHANGEZ MOI. Mettre en commentaire pour utiliser votre propre structure

using System;
using UnityEngine;
using System.Collections.Generic;

#if BAD_PERF
using InnerType = System.Collections.Generic.Dictionary<uint, IComponent>;
using AllComponents = System.Collections.Generic.Dictionary<uint, System.Collections.Generic.Dictionary<uint, IComponent>>;
#else
using InnerType = Pool;
using AllComponents = System.Collections.Generic.List<Pool>;
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


    private int count = 0;

    public const int maxEntities = 1100;


    public void DebugPrint()
    {
        string toPrint = "";
        var allComponents = Instance.DebugGetAllComponents();
        foreach (var type in allComponents)
        {
            toPrint += $"{type}: \n";
#if !BAD_PERF
            //foreach (var component in type)
#else
            foreach (var component in type.Value)
#endif
            {
#if BAD_PERF
                toPrint += $"\t{component.Key}: {component.Value}\n";
#else
             //   toPrint += $"\t{component}: {component}\n";
#endif
            }
            toPrint += "\n";
        }
        Debug.Log(toPrint);
    }

    // CRUD
    public void SetComponent<T>(EntityComponent entityID, IComponent component) where T : IComponent
    {
        Pool a = new Pool();
        if (!hashCodes.Contains(TypeRegistry<T>.typeID))
        {
            //_allComponents[TypeRegistry<T>.typeID] = new Dictionary<uint, IComponent>();
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
            if (_allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].pool[entityID.id] != null)
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
        if (_allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].pool[entity.id] != null)
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
        var allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].pool;
        count = allEntities.Length;
        for (var i = 0; i < count; i++)
        {
            if (_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].pool[i] == null)
            {
                continue;
            }
            lambda((EntityComponent)allEntities[i], (T1)_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].GetComponent((EntityComponent)allEntities[i]));
        }
    }

    public void ForEach<T1, T2>(Action<EntityComponent, T1, T2> lambda) where T1 : IComponent where T2 : IComponent
    {
        var allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].pool;
        count = allEntities.Length;

        for (var i = 0; i < count; i++)
        {
            if (_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].pool[i] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T2>.typeID)].pool[i] == null
                )
            {
                continue;
            }
            lambda((EntityComponent)allEntities[i], (T1)_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].GetComponent((EntityComponent)allEntities[i]), (T2)_allComponents[hashCodes.IndexOf(TypeRegistry<T2>.typeID)].GetComponent((EntityComponent)allEntities[i]));
        }

    }

    public void ForEach<T1, T2, T3>(Action<EntityComponent, T1, T2, T3> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {
        var allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].pool;
        count = allEntities.Length;

        for (var i = 0; i < count; i++)
        {

            if (_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].pool[i] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T2>.typeID)].pool[i] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T3>.typeID)].pool[i] == null
                )
            {
                continue;
            }
            lambda(((EntityComponent)allEntities[i]), (T1)_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].GetComponent(((EntityComponent)allEntities[i])), (T2)_allComponents[hashCodes.IndexOf(TypeRegistry<T2>.typeID)].GetComponent(((EntityComponent)allEntities[i])), (T3)_allComponents[hashCodes.IndexOf(TypeRegistry<T3>.typeID)].GetComponent(((EntityComponent)allEntities[i])));
        }

    }

    public void ForEach<T1, T2, T3, T4>(Action<EntityComponent, T1, T2, T3, T4> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {
        var allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].pool;
        count = allEntities.Length;

        for (var i = 0; i < count; i++)
        {
            if (_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].pool[i] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T2>.typeID)].pool[i] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T3>.typeID)].pool[i] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T4>.typeID)].pool[i] == null
                )
            {
                continue;
            }
            lambda(((EntityComponent)allEntities[i]), (T1)_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].GetComponent(((EntityComponent)allEntities[i])), (T2)_allComponents[hashCodes.IndexOf(TypeRegistry<T2>.typeID)].GetComponent(((EntityComponent)allEntities[i])), (T3)_allComponents[hashCodes.IndexOf(TypeRegistry<T3>.typeID)].GetComponent(((EntityComponent)allEntities[i])), (T4)_allComponents[hashCodes.IndexOf(TypeRegistry<T4>.typeID)].GetComponent(((EntityComponent)allEntities[i])));
        }
    }

    public AllComponents DebugGetAllComponents()
    {
        return _allComponents;
    }
}
