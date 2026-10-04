using Godot;
using System;
using System.Linq;

public partial class UpgradeTreeNode : Node2D
{
	public TreeNode node;
	
	[Export]
	private Area2D area;
	
	[Export]
	NinePatchRect rect;

	[Export]
	Texture2D FallbackTexture;

	[Export]
	Sprite2D insideSprite;

	[Export]
	ShaderMaterial ShadowShader;

	public Texture2D InsideTexture;

	private RichTextLabel text;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		area.InputEvent += GotInputEvent;
		area.MouseEntered += () => { rect.Visible = true; };
		area.MouseExited += () => { rect.Visible = false; };

		InsideTexture = node.Effects.Length > 0 && node.Effects[0].Texture != null
			? node.Effects[0].Texture
			: FallbackTexture;
		
		insideSprite.Texture = InsideTexture;
		rect.Visible = false;
		text = GetNode<RichTextLabel>("Background/RichTextLabel");

		replacePlaceholderString();
		Material = ShadowShader;
		node.StateChanged += OnStateChange;

		Visible = false;
		OnStateChange(NodeState.INVISIBLE, node.State);
	}

	private void GotInputEvent(Node viewport, InputEvent @event, long shapeIdx){
		if(@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed){
			GD.Print($"Node Bevor: lvl={node.CurrentLevel}, maxLvl={node.MaxLevel}, minLevelForUnlock={node.MinLevelForUnlock}");
			node.CurrentLevel++;
			GD.Print($"Node After: lvl={node.CurrentLevel}, maxLvl={node.MaxLevel}, minLevelForUnlock={node.MinLevelForUnlock}");
		}
	}

	private void OnStateChange(NodeState oldState, NodeState newState){
		if(newState == NodeState.LOCKED){
			Visible = true;
			Material = ShadowShader;
			return;
		}
		if(newState == NodeState.AVAILABLE){
			Visible = true;
			Material = null;
			return;
		}
		//TODO Add Case for Purchased

	}

	private void replacePlaceholderString(){
		string placeholder = text.Text;

		if(node.Effects == null || node.Effects.Length == 0){
			placeholder = placeholder.Replace("{title}", "The start of your journey");
			placeholder = placeholder.Replace("{description}", "At this point your journey of upgrades starts.");
			placeholder = placeholder.Replace("{effect}", "");
			text.Text = placeholder;
			return;
		}

		placeholder = placeholder.Replace("{title}", node.Title);
		placeholder = placeholder.Replace("{description}", node.Description);

		for(int i = 0; i < node.Effects.Length; i++){
			UpgradeEffect effect = node.Effects[i];
			int index = placeholder.IndexOf("{effect}");

			if(index >= 0){
				placeholder = placeholder
					.Remove(index, "{effect}".Length)
					.Insert(index, $"{effect.Variable}:{effect.Increase.ToDisplayFormat()}");
			}
		}
		placeholder = placeholder.Replace("{effect}", "");

		text.Text = placeholder;
	}
}
