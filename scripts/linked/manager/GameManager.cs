using Godot;
using System;

public partial class GameManager : Node{
	
	public static GameManager Instance {get; private set;}
	public ScreenManager screenManager {get; private set;}
	public TreeManager treeManager {get; private set;}

	[Export] private Node ScreenNode;

    public override void _Ready(){
        Instance = this;

		screenManager = GetNode<ScreenManager>("ScreenManager");
		screenManager.ScreenNode = ScreenNode;

		treeManager = GetNode<TreeManager>("TreeManager");
    }


}
