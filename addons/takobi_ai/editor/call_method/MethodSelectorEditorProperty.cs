#if TOOLS
using Godot;
using System.Collections.Generic;
using TakobiAI.Leaves;

namespace TakobiAI.Editor;

/// <summary>
/// Custom inspector property for CallMethod.Method.
/// Displays a dropdown of user-authored methods on the Source node,
/// with return-type prefix labels and full signature tooltips.
/// </summary>
public partial class MethodSelectorEditorProperty : EditorProperty
{
    #region Fields

    private readonly CallMethod _targetNode;

    private VBoxContainer _root;
    private OptionButton _optionButton;

    private readonly List<MethodEntry> _allMethods = new();
    private static readonly HashSet<string> EngineBaseMethods = new();

    static MethodSelectorEditorProperty()
    {
        using var baseline = new Node();
        foreach (var m in baseline.GetMethodList())
            EngineBaseMethods.Add(m["name"].AsString());

        EngineBaseMethods.UnionWith(new[] { "Show", "Hide", "QueueRedraw", "UpdateConfigurationWarnings" });
    }

    #endregion

    #region Constructor

    public MethodSelectorEditorProperty(CallMethod targetNode)
    {
        _targetNode = targetNode;

        _root = new VBoxContainer();
        AddChild(_root);

        _optionButton = new OptionButton { ClipText = true };
        AddFocusable(_optionButton);
        _optionButton.ItemSelected += OnMethodSelected;
        _root.AddChild(_optionButton);
    }

    #endregion

    #region EditorProperty Overrides

    public override void _UpdateProperty()
    {
        CollectMethods();
        RebuildList();

        string current = GetEditedObject().Get(GetEditedProperty()).AsString();
        SelectCurrent(current);
    }

    #endregion

    #region Data Collection

    private void CollectMethods()
    {
        _allMethods.Clear();

        Node sourceNode = _targetNode.Source
            ?? _targetNode.Owner
            ?? _targetNode.GetTree()?.EditedSceneRoot;

        if (sourceNode == null) return;

        foreach (var methodData in sourceNode.GetMethodList())
        {
            string name = methodData["name"].AsString();

            if (name.StartsWith('_')) continue;
            if (EngineBaseMethods.Contains(name)) continue;

            string returnType = GetReturnLabel(methodData);
            string signature  = BuildSignature(name, methodData);

            _allMethods.Add(new MethodEntry(name, returnType, signature));
        }

        _allMethods.Sort((a, b) => string.Compare(a.Name, b.Name, System.StringComparison.Ordinal));
    }

    #endregion

    #region List Population

    private void RebuildList()
    {
        _optionButton.Clear();
        _optionButton.AddItem("[None]");
        _optionButton.SetItemTooltip(0, string.Empty);

        foreach (var entry in _allMethods)
        {
            string label = string.IsNullOrEmpty(entry.ReturnLabel)
                ? entry.Name
                : $"{entry.ReturnLabel} {entry.Name}";

            _optionButton.AddItem(label);
            _optionButton.SetItemTooltip(_optionButton.ItemCount - 1, entry.Signature);
        }

        string current = GetEditedObject()?.Get(GetEditedProperty()).AsString() ?? string.Empty;
        SelectCurrent(current);
    }

    private void SelectCurrent(string current)
    {
        int index = _allMethods.FindIndex(e => e.Name == current);

        if (index >= 0)
        {
            _optionButton.Selected = index + 1; // +1 for [None]
            return;
        }

        _optionButton.Selected = -1;
        _optionButton.Text = string.IsNullOrEmpty(current) ? "[Select Method]" : current;
    }

    #endregion

    #region Selection Handler

    private void OnMethodSelected(long index)
    {
        // index 0 is [None]
        string value = index == 0 ? string.Empty : _allMethods[(int)index - 1].Name;
        EmitChanged(GetEditedProperty(), value);
    }

    #endregion

    #region Signature Helpers

    private static string GetReturnLabel(Godot.Collections.Dictionary methodData)
    {
        if (!methodData.ContainsKey("return")) return string.Empty;

        var ret = methodData["return"].AsGodotDictionary();
        if (ret == null || !ret.ContainsKey("type")) return string.Empty;

        var vtype = (Variant.Type)(int)ret["type"];
        return vtype switch
        {
            Variant.Type.Nil    => "[void]",
            Variant.Type.Bool   => "[bool]",
            Variant.Type.Int    => "[int]",
            Variant.Type.Float  => "[float]",
            Variant.Type.String => "[String]",
            _                   => $"[{vtype}]"
        };
    }

    private static string BuildSignature(string methodName, Godot.Collections.Dictionary methodData)
    {
        if (!methodData.ContainsKey("args")) return methodName + "()";

        var args  = methodData["args"].AsGodotArray();
        var parts = new List<string>();

        foreach (var arg in args)
        {
            var argDict = arg.AsGodotDictionary();
            string argName = argDict.ContainsKey("name") ? argDict["name"].AsString() : "?";
            var vtype = argDict.ContainsKey("type") ? (Variant.Type)(int)argDict["type"] : Variant.Type.Nil;
            parts.Add($"{vtype} {argName}");
        }

        return $"{methodName}({string.Join(", ", parts)})";
    }

    #endregion

    #region MethodEntry

    private sealed record MethodEntry(string Name, string ReturnLabel, string Signature);

    #endregion
}
#endif