using Godot;
using System.Collections.Generic;

public partial class ScreenManager : Node {

    [Export]
    ScreenEntry[] screenEntries;

    public Node ScreenNode;

    private Dictionary<string, ScreenEntry> availableScreens = new();
    private Stack<ScreenHistoryEntry> screenHistory = new();

    public override void _Ready() {
        foreach(ScreenEntry screenEntry in screenEntries) {
            availableScreens[screenEntry.Key] = screenEntry;
        }
    }

    public void SwitchScreen(string name) {
        if(!TryGetScreenEntry(name, out ScreenEntry entry))
            return;

        if(entry.IsOverlay) {
            GD.PushError($"\"{name}\" is an overlay. Use ToggleOverlay(\"{name}\")");
            return;
        }

        SaveCurrentState();
        ChangeScreen(entry);
    }

    public void ToggleOverlay(string name) {
        if(!TryGetScreenEntry(name, out ScreenEntry entry))
            return;

        if(!entry.IsOverlay) {
            GD.PushError($"\"{name}\" isn't an overlay. To switch a screen use SwitchScreen(\"{name}\")");
            return;
        }

        Node overlays = ScreenNode.GetNode<Node>("Overlays");

        foreach(Node node in overlays.GetChildren()) {
            if(node.Name == name) {
                node.QueueFree();
                return;
            }
        }

        SaveCurrentState();

        Node overlay = entry.Screen.Instantiate();
        overlay.Name = entry.Key;

        overlays.AddChild(overlay);
    }

    public void GoBack() {
        if(!CanReturnFromCurrentState())
            return;

        if(screenHistory.Count == 0)
            return;

        ScreenHistoryEntry previousState = screenHistory.Pop();

        RestoreState(previousState);
    }

    private bool CanReturnFromCurrentState() {
        Node overlays = ScreenNode.GetNode<Node>("Overlays");

        foreach(Node overlay in overlays.GetChildren()) {
            if(availableScreens.TryGetValue(overlay.Name, out ScreenEntry entry)) {
                if(!entry.CanReturnToScreen)
                    return false;
            }
        }

        foreach(Node node in ScreenNode.GetChildren()) {
            if(node == overlays)
                continue;

            if(availableScreens.TryGetValue(node.Name, out ScreenEntry entry)) {
                return entry.CanReturnToScreen;
            }
        }

        return true;
    }

    private void SaveCurrentState() {
        Node overlays = ScreenNode.GetNode<Node>("Overlays");

        ScreenHistoryEntry state = new();

        foreach(Node node in ScreenNode.GetChildren()) {
            if(node == overlays)
                continue;

            state.screenName = node.Name;
            break;
        }

        foreach(Node overlay in overlays.GetChildren()) {
            state.overlayNames.Add(overlay.Name);
        }

        screenHistory.Push(state);
    }

    private void ChangeScreen(ScreenEntry entry) {
        Node overlays = ScreenNode.GetNode<Node>("Overlays");

        foreach(Node node in ScreenNode.GetChildren()) {
            if(node == overlays)
                continue;

            node.QueueFree();
        }

        foreach(Node overlay in overlays.GetChildren()) {
            overlay.QueueFree();
        }

        Node screen = entry.Screen.Instantiate();
        screen.Name = entry.Key;

        ScreenNode.AddChild(screen);

        GD.Print($"Switched screen to {entry.Key}");
    }

    private void RestoreState(ScreenHistoryEntry state) {
        Node overlays = ScreenNode.GetNode<Node>("Overlays");

        foreach(Node node in ScreenNode.GetChildren()) {
            if(node == overlays)
                continue;

            node.QueueFree();
        }

        foreach(Node overlay in overlays.GetChildren()) {
            overlay.QueueFree();
        }

        if(state.screenName != null &&
           availableScreens.TryGetValue(state.screenName, out ScreenEntry screenEntry)) {

            Node screen = screenEntry.Screen.Instantiate();
            screen.Name = screenEntry.Key;

            ScreenNode.AddChild(screen);
        }

        foreach(string overlayName in state.overlayNames) {
            if(!availableScreens.TryGetValue(overlayName, out ScreenEntry overlayEntry))
                continue;

            if(!overlayEntry.IsOverlay)
                continue;

            Node overlay = overlayEntry.Screen.Instantiate();
            overlay.Name = overlayEntry.Key;

            overlays.AddChild(overlay);
        }
    }

    private bool TryGetScreenEntry(string name, out ScreenEntry entry) {
        if(!availableScreens.TryGetValue(name, out entry)) {
            GD.PushError($"Screen \"{name}\" doesn't exist");
            return false;
        }

        return true;
    }

    private class ScreenHistoryEntry {

        public string screenName;

        public List<string> overlayNames = new();
    }
}
