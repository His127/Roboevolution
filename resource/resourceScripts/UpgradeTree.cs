using Godot;
using System;

[GlobalClass]
public partial class UpgradeTree : Resource
{
	[Export]
	public TreeVariable[] Variables;

	[Export(PropertyHint.MultilineText,"no_wrap")]
	public string TreeStructure {get; set;}

	[Export]
	public string Title {get; set;} = "";

}
