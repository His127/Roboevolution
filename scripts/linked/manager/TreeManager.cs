using Godot;
using System;

public partial class TreeManager : Node
{
	[Export]
	UpgradeTree[] upgradeTrees;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TreeParser.ParseTree(upgradeTrees[0]);
	}
}
