using Godot;
using System.Text.RegularExpressions;

namespace TakobiAI.Leaves;

[Tool, GlobalClass, Icon("uid://fqxrs3re7mpd")]
public partial class Log : BTAction
{
    [Export(PropertyHint.MultilineText)]
    public string Message { get; set; } = string.Empty;

    [GeneratedRegex(@"\$([A-Za-z0-9_.]+)")]
    private static partial Regex BlackboardRegex();
    
    protected override void OnEnter(BTContext ctx)
    {
        if (string.IsNullOrEmpty(Message))
            return;

        string result = BlackboardRegex().Replace(Message, match =>
        {
            string key = match.Groups[1].Value;

            if (!ctx.Blackboard.Has(key))
                return $"<missing:{key}>";

            return ctx.Blackboard.GetValue(key).ToString();
        });

        GD.PrintRich(result);
    }
}

