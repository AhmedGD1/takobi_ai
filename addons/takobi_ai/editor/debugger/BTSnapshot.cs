namespace TakobiAI.Editor;

public readonly struct BTNodeSnapshot
{
    public long NodeId { get; init; }
    public long ParentId { get; init; }
    public string TypeName { get; init; }
    public Status Status { get; init; }
    public bool IsRunning { get; init; }
    public int TickCount { get; init; }
}

public readonly struct BTTreeSnapshot
{
    public long TreeId { get; init; }
    public string TreeName { get; init; }
    public double Timestamp { get; init; }
    public BTNodeSnapshot[] Nodes { get; init; }
    public long BlackboardId { get; init; }
}

public readonly struct BTBlackboardSnapshot
{
    public long BlackboardId { get; init; }
    public (string Key, string ValuePreview)[] Entries { get; init; }
}