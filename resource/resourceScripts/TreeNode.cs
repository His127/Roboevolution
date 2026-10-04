using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class TreeNode : Resource {
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public UpgradeEffect[] Effects { get; set; } = [];
    public int MaxLevel { get; set; } = 1;
    public BigNumber BasePrice { get; set; } = new();
    public BigNumber PriceIncrease { get; set; } = new();
    public int MinLevelForUnlock { get; set; } = 2;
    public List<TreeNode> Children { get; set; } = new();

    public event Action<NodeState, NodeState> StateChanged;

    private NodeState _state = NodeState.INVISIBLE;
    public NodeState State {
        get => _state;
        set {
            if (_state == value)
                return;

            NodeState oldState = _state;
            _state = value;
            StateChanged?.Invoke(oldState, _state);

            // Kinder auf Zustandsänderung des Parents reagieren lassen
            foreach (TreeNode child in Children)
                child.OnParentStateChanged(_state);
        }
    }

    private int _currentLevel = 0;
    public int CurrentLevel {
        get => _currentLevel;
        set {
            if (value > MaxLevel || value == _currentLevel)
                return;

            _currentLevel = value;

            // Weg 1: Level-Schwelle erreicht -> Kinder verfügbar
            if (_currentLevel >= MinLevelForUnlock)
                UnlockChildren();

            // Weg 2: Maximallevel erreicht -> gekauft (schaltet Kinder ebenfalls frei)
            if (_currentLevel >= MaxLevel)
                State = NodeState.PURCHASED;
        }
    }

    // Reaktion auf den Zustand des Parents
    private void OnParentStateChanged(NodeState parentState) {
        switch (parentState) {
            case NodeState.AVAILABLE:
                // Parent wird sichtbar/kaufbar -> Kind wird LOCKED (aber nie herabstufen)
                if (_state == NodeState.INVISIBLE)
                    State = NodeState.LOCKED;
                break;

            case NodeState.PURCHASED:
                // Parent gekauft -> Kind wird AVAILABLE
                Unlock();
                break;
        }
    }

    private void UnlockChildren() {
        foreach (TreeNode child in Children)
            child.Unlock();
    }

    // Hebt nur an, nie herabstufen. Gekaufte Nodes bleiben gekauft.
    private void Unlock() {
        if (_state == NodeState.INVISIBLE || _state == NodeState.LOCKED)
            State = NodeState.AVAILABLE;
    }
}