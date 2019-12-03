namespace ConsoulLibrary.Test.Views
{
    public abstract class Item
    {
        public string Name { get; set; }

        public Item(string name)
        {
            Name = name;
        }
    }
}
