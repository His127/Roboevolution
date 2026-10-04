using Godot;
using System;
using System.Collections.Generic;

public partial class TreeManager : Node
{
	[Export]
	UpgradeTree[] upgradeTrees;

	[Export]
	ShaderMaterial NodeMaterial;

	[Export]
	ShaderMaterial LineMaterial;

	[Export]
	PackedScene NodeScene;


	private Dictionary<int, TreeCreator> trees = new();

	private RandomNumberGenerator random = new();

	public List<CanvasLayer> canvasLayers = new();
	// Called when the node enters the scene tree for the first time.
	
	public void LoadTree(int index){
		GD.Print(Time.GetTimeStringFromSystem());
		TreeCreator creator;
		if(!trees.ContainsKey(index)){
			creator = TreeCreator.Create(upgradeTrees[index], canvasLayers[index]);
			trees[index] = creator;
		}else{
			creator = trees[index];
		}

		LoadTreeIntoCanvas(creator);
		GD.Print(Time.GetTimeStringFromSystem());
	}

	public void AddCanvas(CanvasLayer canvas){
		canvasLayers.Add(canvas);
	}

	private void LoadTreeIntoCanvas(TreeCreator creator){
		TreeControl treeControl = new(){
			MouseFilter = Control.MouseFilterEnum.Pass
		};
		Control baseControl = new(){
			Position = new(600, 350),
			MouseFilter = Control.MouseFilterEnum.Pass
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

			ShaderMaterial material = LineMaterial.Duplicate() as ShaderMaterial;

			material.SetShaderParameter("start_position", start);
			material.SetShaderParameter("end_position", end);
			material.SetShaderParameter("animation_offset", random.RandfRange(0f, 5f));
			material.SetShaderParameter("travel_time", random.RandfRange(10f, 20f));

			line.Material = material;

			Lines.AddChild(line);
		}
		baseControl.AddChild(Lines);
	}

	private void LoadNodes(Control baseControl, Dictionary<TreeNode, Vector2> positions){
		Node2D nodes = new();

		foreach((TreeNode node, Vector2 position) in positions){
			// GD.Print($"{node.Title}: {node.Effects.Length} effects");
			UpgradeTreeNode scene = NodeScene.Instantiate<UpgradeTreeNode>();

			scene.Position = position;
			scene.node = node;

			nodes.AddChild(scene);
		}

		baseControl.AddChild(nodes);
	}
}
