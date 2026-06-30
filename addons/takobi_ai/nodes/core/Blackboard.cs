using Godot;
using Godot.Collections;

namespace TakobiAI;

[Tool, GlobalClass, Icon("uid://omghl31iydmf")]
public partial class Blackboard : Resource
{
    [Export]
    public Dictionary<StringName, Variant> Data { get; private set; } = [];

    public void SetValue(StringName key, Variant value) => Data[key] = value;

    public void SetValue<[MustBeVariant] T>(StringName key, T value) =>
        Data[key] = Variant.From(value);

    public Variant GetValue(StringName key, Variant fallback = default) => 
        Data.TryGetValue(key, out var value) ? value : fallback;

    public T GetValue<[MustBeVariant] T>(StringName key, T fallback = default!) =>
        Data.TryGetValue(key, out var value) ? value.As<T>() : fallback;
    
    public bool TryGetValue(StringName key, out Variant result) =>
        Data.TryGetValue(key, out result);
    
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
    
    public bool Has(StringName key) => Data.ContainsKey(key);
    public bool Erase(StringName key) => Data.Remove(key);
    public void Clear() => Data.Clear();
}

