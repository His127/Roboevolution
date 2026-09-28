using Godot;
using System;

public partial class JunkPice : CharacterBody2D
{
	public Texture2D Texture {get; set;}
	public BigNumber value {get;set;}
	public Vector2 Direction {get; set;}
	public double Lifetime {get; set;} = 2;
	public float Speed {get; set;} = 800;
	public float NormalSpeed = 50;
	public float DecayRate = 0.08f;

	TextureButton button;

	[Signal]
	public delegate void trashCollectedEventHandler(string value);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		button = GetNode<TextureButton>("TextureButton");
		button.TextureNormal = Texture;
		button.Size = Texture.GetSize();
		button.Position = new Vector2(-Texture.GetSize().X / 2, -Texture.GetSize().Y / 2);

		GetTree().CreateTimer(Lifetime).Timeout += ()=> Speed = 0;
		button.MouseFilter = Control.MouseFilterEnum.Ignore;
		GetTree().CreateTimer(Lifetime / 5).Timeout += ()=> button.MouseFilter = Control.MouseFilterEnum.Stop;

	}

    public override void _PhysicsProcess(double delta) {
        Velocity = Direction * Speed;
		if(Speed > NormalSpeed){
			float difference = Speed - NormalSpeed;
			Speed -= difference * DecayRate; 
		}
		MoveAndSlide();
    }

	public void Collect(){
		EmitSignal(SignalName.trashCollected, value.ToString());
		QueueFree();
	}

}
