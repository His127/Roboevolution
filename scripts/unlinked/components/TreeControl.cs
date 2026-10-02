using Godot;

public partial class TreeControl : Control
{
    private bool isDragging = false;
    private Vector2 offset = Vector2.Zero;

    public Control movableControl;

    private const float ZoomStep = 0.1f;
    private const float MinZoom = 0.1f;
    private const float MaxZoom = 2.0f;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton) {
            if(mouseButton.ButtonIndex == MouseButton.Middle) {
                if(mouseButton.Pressed) {
                    isDragging = true;
                    offset = GetGlobalMousePosition() - movableControl.GlobalPosition;
                    AcceptEvent();
                } else {
                    isDragging = false;
                }
            }

            if(mouseButton.ButtonIndex == MouseButton.WheelUp) {
                Zoom(ZoomStep);
                AcceptEvent();
            }

            if(mouseButton.ButtonIndex == MouseButton.WheelDown) {
                Zoom(-ZoomStep);
                AcceptEvent();
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseButton mouseButton &&
           mouseButton.ButtonIndex == MouseButton.Middle &&
           !mouseButton.Pressed) {

            isDragging = false;
        }

        if(isDragging && @event is InputEventMouseMotion) {
            movableControl.GlobalPosition = GetGlobalMousePosition() - offset;
        }
    }

    private void Zoom(float amount) {
		float oldZoom = movableControl.Scale.X;
		float newZoom = Mathf.Clamp(oldZoom + amount, MinZoom, MaxZoom);

		Vector2 mousePosition = GetGlobalMousePosition();

		Vector2 mouseOffset = mousePosition - movableControl.GlobalPosition;
		mouseOffset /= oldZoom;

		movableControl.Scale = new Vector2(newZoom, newZoom);

		movableControl.GlobalPosition = mousePosition - mouseOffset * newZoom;
	}
}