using System;
using OpenTK.Graphics.OpenGL;

public class Province
{
    /**
     * Rate at which this province generates each resource
     * Indexed by the ResourceType enum
     */
    private ResourceBag passiveResources;
    private ResourceBag activeResources;

    public Province()
        : this(null, null)
    {
    }

    public Province(City city, Player owner)
    {
        City = city;
        Owner = owner;

        var numResources = Enum.GetNames(typeof(ResourceType)).Length;
        this.passiveResources = new ResourceBag();
        this.activeResources = new ResourceBag();

        // for now, default regen rates = 10 for food and 5 for weapons if there is a city, otherwise 0
        // it may be changed in the future (need to discuss design) s.t. cities generate resources and they permeate upwards or tweak interface
        if (City != null)
        {
            this.passiveResources.SetAmountOf(ResourceType.Food, 0);
            this.passiveResources.SetAmountOf(ResourceType.Weapons, 1);

            this.activeResources.SetAmountOf(ResourceType.Food, 10);
            this.activeResources.SetAmountOf(ResourceType.Weapons, 5);
        }
        else
        {
            this.passiveResources.SetAmountOf(ResourceType.Food, 0);
            this.passiveResources.SetAmountOf(ResourceType.Weapons, 0);

            this.activeResources.SetAmountOf(ResourceType.Food, 2);
            this.activeResources.SetAmountOf(ResourceType.Weapons, 0);
        }
    }

    /**
     * The city contained in this province
     * If the province doesn't have a city then null
     */
    public City City { get; set; }

    /**
     * The player that owns or occupies this province
     * If the province is neutral then null
     */
    public Player Owner { get; set; }

    /**
     * Tick method - updates state of Province after each round of turns completes
     * Right now, merely generates new resources.
     */
    public void Tick()
    {
        Owner?.Resources.Add(this.passiveResources);
    }

    /**
     * Called by a Player to gather all of the resources of a given type available at this province.
     * Returns amount gathered
     */
    public void Gather(Player player)
    {
        player.Resources.Add(this.activeResources);
    }

    public void Render()
    {
        Color c = Owner?.Color ?? new Color(0.5f, 0.5f, 0.5f);

        c.Use();
        GL.Rect(0.0f, 0.0f, 1.0f, 1.0f);

        City?.Render();
    }
}
