using System;

namespace ConsoulLibrary.Test.Views
{
    public abstract class Weapon : Item
    {
        protected Random random { get; set; } = new Random(3);
        public int BaseDamage { get; set; }

        public Weapon(string name, int damage) : base(name)
        {
            BaseDamage = damage;
        }

        public int Hit()
        {
            return random.Next(0, BaseDamage);
        }
    }
}
