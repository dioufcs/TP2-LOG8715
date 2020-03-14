using System;
using System.Collections.Generic;

/*
    Classe représentant un allocateur de type Sequential Pool
    La classe contient les attributs suivants :
    _ un array de int représentant la table d'indirection
    _ une List de Components représentant le Sequential Pool
*/

public class SequentialPool
{
    // Table d'indirection : contient la valeur de l'index de chaque component dans le Sequential Pool 
    public int?[] IndirectionTable;

    // Sequential Pool : Contient une liste de Components
    public List<IComponent> sequentialPool;

    public SequentialPool()
    {
        // Crée une nouvelle table d'indirection et un sequential pool

        // La table d'indirection sera de la taille du nombre d'entités maxmimal
        // Toutes les valeurs de la tables seront initialisées à null
        IndirectionTable = new int?[ComponentsManager.maxEntities];

        // Liste de Components vide à la création
        sequentialPool = new List<IComponent>();
    }

    public void SetComponent(EntityComponent entityID, IComponent component)
    {
        if (IndirectionTable[entityID.id] == null)
        {
            // Ajout d'un élément à la queue de la sequential pool
            IndirectionTable[entityID.id] = (int)sequentialPool.Count;
            sequentialPool.Add(component);
        }
        else // Modification d'un component
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
            // Retrait se fait en swappant avec le dernier élément
            sequentialPool[componentIndex] = sequentialPool[componentsCount - 1];
            IndirectionTable[toReplace] = componentIndex;
        }
        sequentialPool.RemoveAt(componentsCount - 1);
    }

    public IComponent GetComponent(EntityComponent entityID)
    {
        int componentIndex = (int)IndirectionTable[entityID.id];
        return sequentialPool[componentIndex];

    }

    public void Clear()
    {
        Array.Clear(IndirectionTable, 0, ComponentsManager.maxEntities);
        sequentialPool.Clear();
    }


}
