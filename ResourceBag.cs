using System;

public class ResourceBag
{
    /**
     * Maps resource type to the amount of that resource at this province
     * Indexed by the ResourceType enum
     */
    private int[] resourceCounts;

    public ResourceBag()
    {
        this.resourceCounts = new int[Enum.GetNames(typeof(ResourceType)).Length];
    }

    /**
     * Returns amount of resource type requested in this bag
     */
    public int GetAmountOf(ResourceType resourceType)
    {
        return this.resourceCounts[(int)resourceType];
    }

    /**
     * Sets the amount of the specified resource in this bag
     */
    public void SetAmountOf(ResourceType resourceType, int amount)
    {
        this.resourceCounts[(int)resourceType] = amount;
    }

    /**
     * Attempts to use the specified amount of the given resource, subtracting amount from the total count
     * Returns true on success, and false if not enough of that resource is available or if a negative value is passed
     */
    public bool Use(ResourceType resourceType, int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException("Amount to use must be greater than or equal to zero");
        }

        if (this.GetAmountOf(resourceType) < amount)
        {
            return false;
        }

        this.SetAmountOf(resourceType, this.GetAmountOf(resourceType) - amount);
        return true;
    }

    public void Add(ResourceType resourceType, int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException("Amount to add must be greater than or equal to zero");
        }

        this.SetAmountOf(resourceType, this.GetAmountOf(resourceType) + amount);
    }

    public void Add(ResourceBag other)
    {
        foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
        {
            this.SetAmountOf(resourceType, this.GetAmountOf(resourceType) + other.GetAmountOf(resourceType));
        }
    }

    public override string ToString()
    {
        return "Food: " + this.GetAmountOf(ResourceType.Food) + " Weapons: " + this.GetAmountOf(ResourceType.Weapons);
    }
}