# Consoul
Provides GUI-like extensions for interacting with Console.

# The Basics
Almost every Console application will have some form of methods that simplify the ability to prompt for input or write to the screen. Consoul provides a few of these basic methods:

 - **Write**: A spin on the typical `Console.WriteLine()` that allows you to specify a color for the specific content you're writing
 - **Wait**: Synchronously wait for the user to acknowledge continuation of the process with a more human-readable command
 - **Input**: Prompt the user to respond with a raw string value using the same methods as the `Consoul.Write()` method. Optionally provide a safety net for yourself by not allowing null or empty responses, for those users that just try to blow past prompts
 - **Ask**: Similar to `Consoul.Input()`, but prompts the user to safely provide a Yes or No response that results in a boolean. Optionally allow users to blow past the prompt by allowing null or empty responses instead of the Yes or No input.
 - **Prompt**: A basic version of a *View* (see below) that prompts the user to select from a list of options by choosing the corresponding number.
 - **Read**: Waits for the user to input text and press enter to submit their response, yielding a string of their response.

# Views
A Console application will typically consist of a few inputs, some messages, all primarily driven from a series of cascading methods and user choices. So, Consoul focuses on allowing you to build "views" with the premise that there will be one or more "choices" that the user must make in order to perform an action. For example, in a text-based RPG game, you will have a high-level "view" structure:

 - Main Menu
 - First Dungeon
   - First Room
   - Second Room
   - Hidden Room
   - Boss Room
 - Last Dungeon
   - {Insert lots of rooms}
 - You Win Screen
 - You Lose Screen

Taking a look at the "Main Menu", you may have two options you want to present to the user:

 - New Game
 - Load Game

Now for the "Second Room" in the "First Dungeon" you may want to present to the user:

 - Turn Back (Goes to First Room)
 - Inspect Lever (Goes to Hidden Room)
 - Proceed through double doors (Goes to Boss Room)

Each of the "choices" presented you may want to change the "view" and/or alter some mechanic in the game such as changing hit points, adjusting inventory, whatever you need to do.

## What is a View
The Consoul library allows you to organize views in terms of classes. So, you can implement either Static or Dynamic "Views" depending on your needs. This should allow you to manage any state of the application independently across views. For example, you may need to persist a running timer as a `System.Diagnostics.Stopwatch` in the "Hidden Room" view because you don't want the user spending too much time in the room.

## User Choices
A View isn't much without a list of choices. You have two options for adding options to your view:

 - Populate the list of `base.Options` in your constructor
 - Decorate methods in your class as an `Option` using `DynamicOptionAttribute` or `OptionAttribute`
 
Utilizing attributes is the preferred method as it provides a more natural workflow of cause and effect within your application.

## StaticView
A StaticView displays options in the rawest form available, not worrying about the state of your view and just simply displaying the raw option text you specify in your code. For example:

``` csharp
public class HelloWorld : StaticView {

  public HelloWorld() : base() {
    // Initialize
  }
  
  [ViewOption("Say Your Name")]
  public void SayYourName() {
    Consoul.Consoul.Write("I'm a computer, so I do not have a name!");
  }
  
}
```

## DynamicView
A DynamicView focuses on dynamically building options depending on the state of your view. Options that are defined in a dynamic view will execute methods to dynamically build text and/or change coloring. For example: 

``` csharp
public class HelloWorld : DynamicView {
  
  private string UserName { get; set; }
  
  public HelloWorld() : base() {
    // Initialize
    UserName = string.Empty;
  }
  
  private string _sayNameMessage() => string.IsNullOrEmpty(UserName) ? "Enter Name..." : "Say my name";
  
  private ConsoleColor _changeOptionColor() => string.IsNullOrEmpty(UserName) ? ConsoleColor.Red : ConsoleColor.Green;
  
  [DynamicViewOption(nameof(_sayNameMessage), nameof(_changeOptionColor))]
  public void SayMyName() {
    if (string.IsNullOrEmpty(UserName)) {
      UserName = Consoul.Consoul.Input("What IS your name?");
    } else {
      Consoul.Consoul.Write(UserName, ConsoleColor.Green);
    }
  }
}
```
