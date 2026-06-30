#if TOOLS
using Godot;
using System.Collections.Generic;
using Godot.Collections;

namespace TakobiAI.Editor;

public partial class SignalSelectorEditorProperty : EditorProperty
{
    #region Fields

    private readonly Node targetNode;

    private VBoxContainer root;
    private OptionButton optionButton;

    private readonly List<SignalEntry> allSignals = new();
    private static readonly HashSet<string> EngineBaseSignals = new();

    static SignalSelectorEditorProperty()
    {
        using var baseline = new Node();
        foreach (var s in baseline.GetSignalList())
            EngineBaseSignals.Add(s["name"].AsString());
    }

    #endregion

    #region Constructor

    public SignalSelectorEditorProperty(Node targetNode)
    {
        this.targetNode = targetNode;

        root = new VBoxContainer();
        AddChild(root);

        optionButton = new OptionButton { ClipText = true };
        AddFocusable(optionButton);
        optionButton.ItemSelected += OnSignalSelected;
        root.AddChild(optionButton);
    }

    #endregion

    #region EditorProperty Overrides

    public override void _UpdateProperty()
    {
        CollectSignals();
        RebuildList();

        string current = GetEditedObject().Get(GetEditedProperty()).AsString();
        SelectCurrent(current);
    }

    #endregion

    #region Data Collection

    private void CollectSignals()
    {
        allSignals.Clear();

        if (targetNode == null) return;

        foreach (var sigData in targetNode.GetSignalList())
        {
            string name = sigData["name"].AsString();

            if (name.StartsWith('_')) continue;
            if (EngineBaseSignals.Contains(name)) continue;

            var args     = sigData["args"].AsGodotArray();
            string sig   = BuildSignature(name, args);
            string badge = $"[{args.Count}]";

            allSignals.Add(new SignalEntry(name, badge, sig));
        }

        allSignals.Sort((a, b) => string.Compare(a.Name, b.Name, System.StringComparison.Ordinal));
    }

    #endregion

    #region List Population

    private void RebuildList()
    {
        optionButton.Clear();
        optionButton.AddItem("[None]");
        optionButton.SetItemTooltip(0, string.Empty);

        foreach (var entry in allSignals)
        {
            string label = $"{entry.Badge} {entry.Name}";
            optionButton.AddItem(label);
            optionButton.SetItemTooltip(optionButton.ItemCount - 1, entry.Signature);
        }

        string current = GetEditedObject()?.Get(GetEditedProperty()).AsString() ?? string.Empty;
        SelectCurrent(current);
    }

    private void SelectCurrent(string current)
    {
        int index = allSignals.FindIndex(e => e.Name == current);

        if (index >= 0)
        {
            optionButton.Selected = index + 1; // +1 for [None]
            return;
        }

        optionButton.Selected = -1;
        optionButton.Text = string.IsNullOrEmpty(current) ? "[Select Signal]" : current;
    }

    #endregion

    #region Selection Handler

    private void OnSignalSelected(long index)
    {
        // index 0 is [None]
        string value = index == 0 ? string.Empty : allSignals[(int)index - 1].Name;
        EmitChanged(GetEditedProperty(), value);
    }

    #endregion

    #region Signature Helpers

    private static string BuildSignature(string signalName, Array args)
    {
        var parts = new List<string>();
        foreach (var arg in args)
        {
            var argDict = arg.AsGodotDictionary();
            var argName = argDict.ContainsKey("name") ? argDict["name"].AsString() : "?";
            var vtype   = argDict.ContainsKey("type") ? (Variant.Type)(int)argDict["type"] : Variant.Type.Nil;
            parts.Add($"{vtype} {argName}");
        }
        return $"{signalName}({string.Join(", ", parts)})";
    }

    #endregion

    #region SignalEntry

    private sealed record SignalEntry(string Name, string Badge, string Signature);

    #endregion
}
#endif