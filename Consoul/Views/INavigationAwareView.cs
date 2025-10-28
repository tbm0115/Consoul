namespace ConsoulLibrary.Views
{
    internal interface INavigationAwareView
    {
        ViewNavigationContext NavigationContext { get; set; }
    }
}
