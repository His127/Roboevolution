using Godot;
using System;

public partial class Button : Godot.Button
{
	
	public void on_pressed()
	{

		GameManager.Instance.screenManager.SwitchScreen("Test");

	}



}
