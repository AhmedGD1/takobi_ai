#if TOOLS
using Godot;
using System.Collections.Generic;
using TakobiAI.Conditions;

namespace TakobiAI.Editor;

public partial class BlackboardCompareEditor : EditorProperty
{
    private static readonly (string Label, Variant.Type Type)[] SupportedTypes =
    [
        ("Bool",       Variant.Type.Bool),
        ("Int",        Variant.Type.Int),
        ("Float",      Variant.Type.Float),
        ("String",     Variant.Type.String),
        ("Blackboard", Variant.Type.String),
    ];

    private static readonly string[] OrderedOps   = ["==", "≠", "<", ">", "≤", "≥"];
    private static readonly string[] EquatableOps = ["==", "≠"];

    private static readonly Dictionary<string, BlackboardCompare.CompareMode> LabelToMode = new()
    {
        ["=="] = BlackboardCompare.CompareMode.Equal,
        ["≠"]  = BlackboardCompare.CompareMode.NotEqual,
        ["<"]  = BlackboardCompare.CompareMode.Less,
        [">"]  = BlackboardCompare.CompareMode.Greater,
        ["≤"]  = BlackboardCompare.CompareMode.LessOrEqual,
        ["≥"]  = BlackboardCompare.CompareMode.GreaterOrEqual,
    };

    private static readonly Dictionary<BlackboardCompare.CompareMode, string> ModeToLabel = new()
    {
        [BlackboardCompare.CompareMode.Equal]          = "==",
        [BlackboardCompare.CompareMode.NotEqual]       = "≠",
        [BlackboardCompare.CompareMode.Less]           = "<",
        [BlackboardCompare.CompareMode.Greater]        = ">",
        [BlackboardCompare.CompareMode.LessOrEqual]    = "≤",
        [BlackboardCompare.CompareMode.GreaterOrEqual] = "≥",
    };

    private readonly BlackboardCompare _node;

    private OptionButton  _typeDropdown;
    private OptionButton  _opDropdown;
    private Control       _valueControl;
    private HBoxContainer _row;

    private bool _updating;

    public BlackboardCompareEditor(BlackboardCompare node)
    {
        _node = node;
        Label = "Comparison";

        _row = new HBoxContainer();
        _row.AddThemeConstantOverride("separation", 4);
        _row.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddChild(_row);
        SetBottomEditor(_row);

        _typeDropdown = new OptionButton { CustomMinimumSize = new Vector2(72, 0) };
        foreach (var (label, _) in SupportedTypes)
            _typeDropdown.AddItem(label);
        _typeDropdown.ItemSelected += _ => OnTypeChanged();
        _row.AddChild(_typeDropdown);

        _opDropdown = new OptionButton { CustomMinimumSize = new Vector2(56, 0) };
        _opDropdown.ItemSelected += _ => OnOpChanged();
        _row.AddChild(_opDropdown);

        _valueControl = new Label();
        _row.AddChild(_valueControl);
    }

    public override void _UpdateProperty()
    {
        _updating = true;

        Variant.Type currentType = _node.Value.VariantType;
        bool isBlackboardRef = currentType == Variant.Type.String
            && _node.Value.AsString().StartsWith('$');

        int typeIdx = 0;
        for (int i = 0; i < SupportedTypes.Length; i++)
        {
            if (SupportedTypes[i].Type != currentType) continue;
            if (SupportedTypes[i].Label == "Blackboard" && !isBlackboardRef) continue;
            if (SupportedTypes[i].Label == "String" && isBlackboardRef) continue;
            typeIdx = i;
            break;
        }
        _typeDropdown.Selected = typeIdx;

        var (label, type) = SupportedTypes[typeIdx];
        RefreshOpDropdown(label, type);
        ReplaceValueControl(label, type);

        _updating = false;
    }

    private void OnTypeChanged()
    {
        if (_updating) return;

        var (label, type) = SupportedTypes[_typeDropdown.Selected];

        Variant defaultVal = label switch
        {
            "Bool"       => Variant.From(false),
            "Int"        => Variant.From(0L),
            "Float"      => Variant.From(0.0f),
            "String"     => Variant.From(string.Empty),
            "Blackboard" => Variant.From("$"),
            _            => default
        };
        EmitChanged("Value", defaultVal);

        if (label is "Bool" && _node.Mode is not (
            BlackboardCompare.CompareMode.Equal or
            BlackboardCompare.CompareMode.NotEqual))
        {
            EmitChanged(GetEditedProperty(), (int)BlackboardCompare.CompareMode.Equal);
        }

        RefreshOpDropdown(label, type);
        ReplaceValueControl(label, type);
    }

    private void RefreshOpDropdown(string label, Variant.Type type)
    {
        bool ordered = type is Variant.Type.Int or Variant.Type.Float
            || label == "Blackboard";

        string[] ops = ordered ? OrderedOps : EquatableOps;
        string current = ModeToLabel.GetValueOrDefault(_node.Mode, "==");

        _opDropdown.Clear();
        int selectIdx = 0;
        for (int i = 0; i < ops.Length; i++)
        {
            _opDropdown.AddItem(ops[i]);
            if (ops[i] == current) selectIdx = i;
        }
        _opDropdown.Selected = selectIdx;
    }

    private void OnOpChanged()
    {
        if (_updating) return;
        string label = _opDropdown.GetItemText(_opDropdown.Selected);
        if (LabelToMode.TryGetValue(label, out var mode))
            EmitChanged(GetEditedProperty(), (int)mode);
    }

    private void ReplaceValueControl(string label, Variant.Type type)
    {
        _row.RemoveChild(_valueControl);
        _valueControl.QueueFree();

        _valueControl = label switch
        {
            "Bool"       => MakeBool(),
            "Int"        => MakeInt(),
            "Float"      => MakeFloat(),
            "String"     => MakeString(),
            "Blackboard" => MakeBlackboardKey(),
            _            => new Label { Text = "—" }
        };
        _valueControl.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _row.AddChild(_valueControl);
    }

    private CheckBox MakeBool()
    {
        bool cur = _node.Value.VariantType == Variant.Type.Bool && _node.Value.AsBool();
        var cb = new CheckBox { ButtonPressed = cur, Text = cur ? "true" : "false" };
        cb.Toggled += on =>
        {
            if (_updating) return;
            cb.Text = on ? "true" : "false";
            EmitChanged("Value", Variant.From(on));
        };
        return cb;
    }

    private SpinBox MakeInt()
    {
        long cur = _node.Value.VariantType == Variant.Type.Int ? _node.Value.AsInt64() : 0L;
        var sb = new SpinBox { Step = 1, Value = cur, UpdateOnTextChanged = false };
        sb.ValueChanged += v => { if (!_updating) EmitChanged("Value", Variant.From((long)v)); };
        return sb;
    }

    private SpinBox MakeFloat()
    {
        double cur = _node.Value.VariantType == Variant.Type.Float ? _node.Value.AsDouble() : 0.0;
        var sb = new SpinBox { Step = 0.001, Value = cur, UpdateOnTextChanged = false };
        sb.ValueChanged += v => { if (!_updating) EmitChanged("Value", Variant.From((float)v)); };
        return sb;
    }

    private LineEdit MakeString()
    {
        string cur = _node.Value.VariantType == Variant.Type.String ? _node.Value.AsString() : "";
        var le = new LineEdit { Text = cur, PlaceholderText = "value…" };
        le.FocusExited   += () => { if (!_updating) EmitChanged("Value", Variant.From(le.Text)); };
        le.TextSubmitted += t  => { if (!_updating) EmitChanged("Value", Variant.From(t)); };
        return le;
    }

    private Control MakeBlackboardKey()
    {
        string cur = _node.Value.VariantType == Variant.Type.String
            ? _node.Value.AsString().TrimPrefix("$")
            : "";

        var container = new HBoxContainer();

        var prefix = new Label { Text = "$" };
        container.AddChild(prefix);

        var le = new LineEdit { Text = cur, PlaceholderText = "key name…" };
        le.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        le.FocusExited   += () => { if (!_updating) EmitChanged("Value", Variant.From("$" + le.Text)); };
        le.TextSubmitted += t  => { if (!_updating) EmitChanged("Value", Variant.From("$" + t)); };
        container.AddChild(le);

        return container;
    }
}
#endif