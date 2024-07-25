using System;
using System.Linq;
using System.Reflection;

namespace ConsoulLibrary.Views
{
    /// <summary>
    /// Creates a dynamic view to edit the properties of the given object.
    /// </summary>
    public class EditObjectView : DynamicView<object>
    {
        public EditObjectView(object entity, System.Reflection.BindingFlags bindingAttr = System.Reflection.BindingFlags.Public) : base()
        {
            Source = entity;

            var entityType = entity.GetType();

            var simpleTypes = new Type[]
            {
                typeof(bool),
                typeof(byte),
                typeof(sbyte),
                typeof(char),
                typeof(decimal),
                typeof(double),
                typeof(float),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(short),
                typeof(ushort),
                typeof(string),
                typeof(DateTime),
                typeof(Guid)
            };

            var properties = entityType.GetProperties(bindingAttr);
            foreach ( var property in properties )
            {
                var propertyType = property.PropertyType;
                if (simpleTypes.Contains(propertyType))
                {
                    this.Options.Add(new DynamicOption<object>(
                        () => $"Edit {property.Name}: " + property.GetValue(Source)?.ToString() ?? "<N/A>",
                        () =>
                        {
                            var input = Consoul.Input("Enter new " +  property.Name);
                            var newValue = Convert.ChangeType(input, propertyType);
                            property.SetValue(Source, newValue);
                        }
                    ));
                } else
                {
                    this.Options.Add(new DynamicOption<object>(
                        () => $"Edit {property.Name}",
                        () =>
                        {
                            var recursiveView = new EditObjectView(property.GetValue(Source));
                            recursiveView.Run();
                        },
                        () => ConsoleColor.DarkYellow
                    ));
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EditObjectView"/> for the provided <paramref name="entity"/>
        /// </summary>
        /// <param name="entity">Entity to create an <see cref="EditObjectView"/></param>
        /// <param name="bindingAttr">Scope the properties that should be editable based on accessibility</param>
        /// <returns>Dynamic view of the entity editor</returns>
        public static EditObjectView CreateView(object entity, BindingFlags bindingAttr)
        {
            var view = new EditObjectView(entity, bindingAttr);
            return view;
        }
    }
}
