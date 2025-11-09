using System;

// Define all resource types in your game
public enum ResourceType
{
    Wood,
    Stone,
    Coins
}

[Serializable]
public class ResourceCost
{
    public ResourceType resource; 
    public int amount;            

    public ResourceCost(ResourceType resource, int amount)
    {
        this.resource = resource;
        this.amount = amount;
    }

    public override string ToString()
    {
        return $"{amount} {resource}";
    }
}