using Godot;
using System;
using System.Dynamic;


[GlobalClass]
public partial class ScreenEntry : Resource{

[Export] public string Key {get; set;} = "";

[Export] public PackedScene Screen {get; set;}


}
