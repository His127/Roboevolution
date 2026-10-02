using Godot;
using System;
using System.Collections.Generic;

public partial class TreeManager : Node
{
	[Export]
	UpgradeTree[] upgradeTrees;

	[Export]
	Material material;

	public List<CanvasLayer> canvasLayers = new();
	// Called when the node enters the scene tree for the first time.
	
	public void LoadTree(int index){
		TreeCreator creator = TreeCreator.Create(upgradeTrees[index], canvasLayers[index]);
		LoadTreeIntoCanvas(creator);
	}

	public void AddCanvas(CanvasLayer canvas){
		canvasLayers.Add(canvas);
	}

	private void LoadTreeIntoCanvas(TreeCreator creator){
		TreeControl treeControl = new(){};
		Control baseControl = new(){
			Position = new(600, 350),
		};
		LoadConnections(baseControl, creator.layout.connections);
		LoadNodes(baseControl, creator.layout.positions);

		baseControl.Scale = new(0.3f, 0.3f);
		treeControl.AddChild(baseControl);
		creator.canvas.AddChild(treeControl);
		treeControl.movableControl = baseControl;
	}

	private void LoadConnections(Control baseControl, List<(Vector2 start, Vector2 end)> connections){
		Node2D Lines = new();
		foreach((Vector2 start, Vector2 end) in connections){
			Line2D line = new(){
				Points = [start, end],
				DefaultColor = Colors.White,
				BeginCapMode = Line2D.LineCapMode.Round,
				EndCapMode = Line2D.LineCapMode.Round
			};
			Lines.AddChild(line);
		}
		baseControl.AddChild(Lines);
	}

	private void LoadNodes(Control baseControl, Dictionary<TreeNode, Vector2> positions){
		Node2D nodes = new();
		foreach(Vector2 vec in positions.Values){
			nodes.AddChild(new ColorRect(){
				Color = Colors.Red,
				Size = new(30,30),
				Material = material,
				Position = vec - new Vector2(15, 15),
			});
		}
		baseControl.AddChild(nodes);
	}
}
