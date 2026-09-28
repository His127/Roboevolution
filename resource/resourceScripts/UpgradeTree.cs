using Godot;
using System;

[GlobalClass]
public partial class UpgradeTree : Resource
{
	[Export(PropertyHint.MultilineText,"no_wrap")]
	public string Variables {get; set;}

	[Export(PropertyHint.MultilineText,"no_wrap")]
	public string TreeStructure {get; set;}

	[Export]
	public string Title {get; set;} = "";

}
