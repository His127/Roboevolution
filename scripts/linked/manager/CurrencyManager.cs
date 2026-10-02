using Godot;
using System;
public partial class CurrencyManager : Node{
	
	public BigNumber Geld{get;  set;} = new(); 
	public BigNumber Schrott{get;  set;} = new();
	public BigNumber R{get;  set;} = new();
	public BigNumber E{get;  set;} = new(); 

	[Signal] public delegate void currencyChangeEventHandler();

	public void update()
	{
		
		EmitSignal(SignalName.currencyChange);

	}

    public override void _Process(double delta)
    {
   		update();
    }



}
