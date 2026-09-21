using Godot;
using Godot.Collections;

namespace TakobiAI;

[Tool, GlobalClass, Icon("uid://omghl31iydmf")]
public partial class Blackboard : Resource
{
    [Signal] public delegate void KeyAddedEventHandler(StringName key);
    [Signal] public delegate void KeyRemovedEventHandler(StringName key);
    [Signal] public delegate void ValueChangedEventHandler(StringName key);
    
    [Export] public Dictionary<StringName, Variant> Data { get; private set; } = [];

    public void SetValue(StringName key, Variant value)
    {
        SetValue<Variant>(key, value);
    }

    public void SetValue<[MustBeVariant] T>(StringName key, T value)
    {
        if (!Has(key))
            EmitSignalKeyAdded(key);
            
        Data[key] = Variant.From(value);
        EmitSignalValueChanged(key);
    }

    public Variant GetValue(StringName key, Variant fallback = default)
    {
        return Data.TryGetValue(key, out var value) ? value : fallback;
    }

    public T GetValue<[MustBeVariant] T>(StringName key, T fallback = default)
    {
        return Data.TryGetValue(key, out var value) ? value.As<T>() : fallback;
    }
    
    public bool TryGetValue(StringName key, out Variant result)
    {
        return Data.TryGetValue(key, out result);
    }
    
    public bool TryGetValue<[MustBeVariant] T>(StringName key, out T result)
    {
        if (!Data.TryGetValue(key, out var value))
        {
            result = default!;
            return false;
        }

        result = value.As<T>();
        return true;
    }
    
    public bool Erase(StringName key)
    {
        if (Data.Remove(key))
        {
            EmitSignalKeyRemoved(key);
            return true;
        }
        
        return false;
    }
    
    public bool Has(StringName key) => Data.ContainsKey(key);
    public void Clear() => Data.Clear();
}
