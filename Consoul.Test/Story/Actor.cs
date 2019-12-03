namespace ConsoulLibrary.Test.Views
{
    public abstract class Actor
    {
        public string Name { get; set; }
        public int HitPoints { get; set; } = 1;
        public Inventory Inventory { get; set; } = new Inventory();

        public Actor(string name, int hitpoints)
        {
            Name = name;
            HitPoints = hitpoints;
        }
    }
}
