using Godot;
using System;

public class TreeLayoutSettings {

    public float NodeDiameter { get; set; } = 50f;
    public float AveragePathLength { get; set; } = 150f;
    public float NodePathClearance { get; set; } = 30f;

    public int SimulationIterations { get; set; } = 50;

    public float AttractionStrength { get; set; } = 1f;
    public float RepulsionStrength { get; set; } = 1f;

    public float MovementStrength { get; set; } = 0.2f;
    public float MinimumNodeDistance { get; set; } = 80f;
}
