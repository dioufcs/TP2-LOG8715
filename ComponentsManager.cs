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
    private SequentialPool _sequentialPool = new InnerType(0, 0);
    //private SequentialPool _sequentialPool1 = new InnerType(0,0);
    //private SequentialPool _sequentialPool2 = new InnerType(0,0);
    //private SequentialPool _sequentialPool3 = new InnerType(0,0);
    //private SequentialPool _sequentialPool4 = new InnerType(0,0);
    //private List<IComponent> allEntities;
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
            count = _allComponents.Count;
            for (var i = 0; i < count; i++)
            {
#else
            foreach (var component in type.Value)
#endif
                {
#if BAD_PERF
                toPrint += $"\t{component.Key}: {component.Value}\n";
#else
                    toPrint += $"\t{_allComponents[i].hashCode}: {_allComponents[i].sequentialPool}\n";
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
        _sequentialPool.hashCode = TypeRegistry<T>.typeID;
        if (!hashCodes.Contains(TypeRegistry<T>.typeID))
        {
            //_allComponents[TypeRegistry<T>.typeID] = new Dictionary<uint, IComponent>();
            _allComponents.Add(new InnerType(TypeRegistry<T>.typeID, _allComponents.Count));
            hashCodes.Add(TypeRegistry<T>.typeID);
        }
        _allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].SetComponent(entityID, component);   
    }
    public void RemoveComponent<T>(EntityComponent entityID) where T : IComponent
    {
        _sequentialPool.hashCode = TypeRegistry<T>.typeID;
        _allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].RemoveComponent(entityID);
    }
    public T GetComponent<T>(EntityComponent entityID) where T : IComponent
    {
        //_sequentialPool.hashCode = TypeRegistry<T>.typeID;
        //this.DebugPrint();
        return (T)_allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].GetComponent(entityID);
    }
    public bool TryGetComponent<T>(EntityComponent entityID, out T component) where T : IComponent
    {
        _sequentialPool.hashCode = TypeRegistry<T>.typeID;
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
        _sequentialPool.hashCode = TypeRegistry<T>.typeID;
        if (_allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].IndirectionTable[entity.id] != null)
            return true;
        return false;
    }

    public void ClearComponents<T>() where T : IComponent
    {
        _sequentialPool.hashCode = TypeRegistry<T>.typeID;
        if (!hashCodes.Contains(TypeRegistry<T>.typeID))
        {
            _allComponents.Add(new InnerType(TypeRegistry<T>.typeID, _allComponents.Count));
            hashCodes.Add(TypeRegistry<T>.typeID);
        }
        else
        {
           _allComponents[hashCodes.IndexOf(TypeRegistry<T>.typeID)].Clear();
        }
    }

    public void ForEach<T1>(Action<EntityComponent, T1> lambda) where T1 : IComponent
    {
        //// Debug.Log("T1");

        //_sequentialPool.hashCode = TypeRegistry<EntityComponent>.typeID;
        //_sequentialPool1.hashCode = TypeRegistry<T1>.typeID;

        var allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].sequentialPool;
        count = allEntities.Count;
        for(var i=0; i<count; i++)
        {
            if (_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].IndirectionTable[((EntityComponent)allEntities[i]).id] == null)
            {
                continue;
            }
            lambda((EntityComponent)allEntities[i], (T1)_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].GetComponent((EntityComponent)allEntities[i]));
        }


    }

    public void ForEach<T1, T2>(Action<EntityComponent, T1, T2> lambda) where T1 : IComponent where T2 : IComponent
    {
        ////  Debug.Log("T12");
        //_sequentialPool.hashCode = TypeRegistry<EntityComponent>.typeID;
        //_sequentialPool1.hashCode = TypeRegistry<T1>.typeID;
        //_sequentialPool2.hashCode = TypeRegistry<T2>.typeID;

        var allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].sequentialPool;
        count = allEntities.Count;

        for (var i = 0; i < count; i++)
        {
            if (_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].IndirectionTable[((EntityComponent)allEntities[i]).id] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T2>.typeID)].IndirectionTable[((EntityComponent)allEntities[i]).id] == null
                )
            {
                continue;
            }
            lambda((EntityComponent)allEntities[i], (T1)_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].GetComponent((EntityComponent)allEntities[i]), (T2)_allComponents[hashCodes.IndexOf(TypeRegistry<T2>.typeID)].GetComponent((EntityComponent)allEntities[i]));
        }

    }

    public void ForEach<T1, T2, T3>(Action<EntityComponent, T1, T2, T3> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {
        //// Debug.Log("T123");
        //_sequentialPool.hashCode = TypeRegistry<EntityComponent>.typeID;
        //_sequentialPool1.hashCode = TypeRegistry<T1>.typeID;
        //_sequentialPool2.hashCode = TypeRegistry<T2>.typeID;
        //_sequentialPool3.hashCode = TypeRegistry<T3>.typeID;

        var allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].sequentialPool;
        count = allEntities.Count;

        for (var i = 0; i < count; i++)
        {

            if (_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].IndirectionTable[((EntityComponent)allEntities[i]).id] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T2>.typeID)].IndirectionTable[((EntityComponent)allEntities[i]).id] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T3>.typeID)].IndirectionTable[((EntityComponent)allEntities[i]).id] == null
                )
            {
                continue;
            }
            lambda(((EntityComponent)allEntities[i]), (T1)_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].GetComponent(((EntityComponent)allEntities[i])), (T2)_allComponents[hashCodes.IndexOf(TypeRegistry<T2>.typeID)].GetComponent(((EntityComponent)allEntities[i])), (T3)_allComponents[hashCodes.IndexOf(TypeRegistry<T3>.typeID)].GetComponent(((EntityComponent)allEntities[i])));
        }

    }

    public void ForEach<T1, T2, T3, T4>(Action<EntityComponent, T1, T2, T3, T4> lambda) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {
        // Debug.Log("T1234");
        //_sequentialPool.hashCode = TypeRegistry<EntityComponent>.typeID;
        //_sequentialPool1.hashCode = TypeRegistry<T1>.typeID;
        //_sequentialPool2.hashCode = TypeRegistry<T2>.typeID;
        //_sequentialPool3.hashCode = TypeRegistry<T3>.typeID;
        //_sequentialPool4.hashCode = TypeRegistry<T4>.typeID;

        var allEntities = _allComponents[hashCodes.IndexOf(TypeRegistry<EntityComponent>.typeID)].sequentialPool;
        count = allEntities.Count;

        for (var i = 0; i < count; i++)
        {
            if (_allComponents[hashCodes.IndexOf(TypeRegistry<T1>.typeID)].IndirectionTable[((EntityComponent)allEntities[i]).id] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T2>.typeID)].IndirectionTable[((EntityComponent)allEntities[i]).id] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T3>.typeID)].IndirectionTable[((EntityComponent)allEntities[i]).id] == null ||
                _allComponents[hashCodes.IndexOf(TypeRegistry<T4>.typeID)].IndirectionTable[((EntityComponent)allEntities[i]).id] == null
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
