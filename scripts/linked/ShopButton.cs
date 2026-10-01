using Godot;
using System;

public partial class ShopButton : Button
{
	
	public void on_pressed()
	{
		
		GameManager.Instance.screenManager.SwitchScreen("ShopScreen");

	}


}
