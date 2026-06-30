using Godot;
using System;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TakobiAI;

public readonly record struct ArgInfo(bool IsBlackboardKey, string Key, Variant Value);

public static class ArgsResolver
{
    #region Cache

    public static ArgInfo[] BuildCache(Array args)
    {
        var cache = new ArgInfo[args.Count];
        for (int i = 0; i < args.Count; i++)
        {
            var arg = args[i];

            if (arg.VariantType == Variant.Type.String)
            {
                string s = arg.AsString();
                if (s.StartsWith('$'))
                {
                    cache[i] = new ArgInfo(true, s[1..], default);
                    continue;
                }
            }
            cache[i] = new ArgInfo(false, string.Empty, arg);
        }
        return cache;
    }

    #region Resolution

    public static void ResolveArgs(BTContext ctx, ReadOnlySpan<ArgInfo> cache, Array destArray)
    {
        destArray.Clear();
        for (int i = 0; i < cache.Length; i++)
        {
            ref readonly var arg = ref cache[i];
            Variant resolvedValue = arg.IsBlackboardKey ? ctx.Blackboard.GetValue(arg.Key) : arg.Value;
            destArray.Add(resolvedValue);
        }
    }

    public static void ResolveArgs(BTContext ctx, ReadOnlySpan<ArgInfo> cache, Span<Variant> destSpan)
    {
        for (int i = 0; i < cache.Length; i++)
        {
            ref readonly var arg = ref cache[i];
            
            destSpan[i] = arg.IsBlackboardKey 
                ? ctx.Blackboard.GetValue(arg.Key) 
                : arg.Value;
        }
    }

    #endregion

    #endregion

    #region Editor Configuration

    public static string[] GetMismatchWarning(string header, string name, Array<Dictionary> list, Array providedArgs)
    {
        foreach (var sigData in list)
        {
            if (sigData["name"].AsString() != name) 
                continue;

            var sigArgs = sigData["args"].AsGodotArray();
            int expected = sigArgs.Count;
            int provided = providedArgs.Count;

            if (provided != expected)
                return [$"Signal '{name}' expects {expected} arg(s) but {provided} provided."];

            for (int i = 0; i < provided; i++)
            {
                var expectedType = sigArgs[i].AsGodotDictionary()["type"].AsInt32();
                var providedType = providedArgs[i].VariantType;

                var isBlackboardKey = providedArgs[i].VariantType == Variant.Type.String 
                    && providedArgs[i].AsString().StartsWith('$');
                
                if (expectedType == (int)providedType) continue;
                if (isBlackboardKey) continue;

                return [$"{header} '{name}' param type '{(Variant.Type)expectedType}' doesn't match args '{providedType}' variant type."];
            }
        }

        return [];
    }

    public static int GetArgCount(string name, Array<Dictionary> list)
    {
        if (string.IsNullOrEmpty(name)) return 0;

        foreach (var data in list)
        {
            if (data["name"].AsString() != name) continue;
            return data["args"].AsGodotArray().Count;
        }

        return 0;
    }

    #endregion
}

