#if TOOLS
using Godot;
using System;
using System.Collections.Generic;

namespace TakobiAI.Editor;

public partial class BTDebuggerPanel : VBoxContainer
{
    private readonly Dictionary<long, BTTreeSnapshot> latestTreeSnapshots = [];
    private readonly Dictionary<long, BTBlackboardSnapshot> latestBlackboardSnapshots = [];

    private OptionButton treeSelector;
    private LineEdit nodeFilter;
    private Tree nodeTree;
    private VBoxContainer blackboardContainer;
    private LineEdit blackboardFilter;
    private Label blackboardEmptyLabel;
    private Tree blackboardTree;

    private Label statusBar;
    private HBoxContainer statusBadgesRow;
    private Label runningBadge;
    private Label successBadge;
    private Label failureBadge;

    private long selectedTreeId = -1;

    private static readonly Color SuccessColor = new("4cae7d");
    private static readonly Color FailureColor = new("e5534b");
    private static readonly Color RunningColor = new("e3b341");
    private static readonly Color IdleColor = new("8b949e");
    private static readonly Color ValueAccentColor = new("79c0ff");

    public override void _Ready()
    {
        var marginRoot = new MarginContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        marginRoot.AddThemeConstantOverride("margin_top", 4);
        marginRoot.AddThemeConstantOverride("margin_bottom", 4);
        marginRoot.AddThemeConstantOverride("margin_left", 8);
        marginRoot.AddThemeConstantOverride("margin_right", 8);
        AddChild(marginRoot);

        var mainLayout = new VBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        mainLayout.AddThemeConstantOverride("separation", 8);
        marginRoot.AddChild(mainLayout);

        var headerPanel = new PanelContainer();
        headerPanel.AddThemeStyleboxOverride("panel", GetThemeStylebox("panel", "TabContainer"));
        mainLayout.AddChild(headerPanel);

        var headerMargin = new MarginContainer();
        headerMargin.AddThemeConstantOverride("margin_top", 4);
        headerMargin.AddThemeConstantOverride("margin_bottom", 4);
        headerMargin.AddThemeConstantOverride("margin_left", 6);
        headerMargin.AddThemeConstantOverride("margin_right", 6);
        headerPanel.AddChild(headerMargin);

        var header = new HBoxContainer();
        header.AddThemeConstantOverride("separation", 10);
        headerMargin.AddChild(header);

        treeSelector = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        treeSelector.ItemSelected += OnTreeSelected;
        header.AddChild(treeSelector);

        statusBar = new Label
        {
            Text = "Waiting for a tree to start ticking…",
            HorizontalAlignment = HorizontalAlignment.Right,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        statusBar.AddThemeColorOverride("font_color", IdleColor);
        header.AddChild(statusBar);

        statusBadgesRow = new HBoxContainer { Visible = false };
        statusBadgesRow.AddThemeConstantOverride("separation", 8);
        header.AddChild(statusBadgesRow);

        runningBadge = CreateStatusBadge("Play", RunningColor, out var runningWrapper);
        successBadge = CreateStatusBadge("StatusSuccess", SuccessColor, out var successWrapper);
        failureBadge = CreateStatusBadge("StatusError", FailureColor, out var failureWrapper);
        statusBadgesRow.AddChild(runningWrapper);
        statusBadgesRow.AddChild(successWrapper);
        statusBadgesRow.AddChild(failureWrapper);

        var split = new HSplitContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        mainLayout.AddChild(split);

        var nodeTreeContainer = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = 3
        };
        nodeTreeContainer.AddThemeConstantOverride("separation", 4);
        split.AddChild(nodeTreeContainer);

        nodeFilter = new LineEdit
        {
            PlaceholderText = "Filter nodes…",
            ClearButtonEnabled = true
        };
        nodeFilter.TextChanged += OnNodeFilterChanged;
        nodeTreeContainer.AddChild(nodeFilter);

        nodeTree = new Tree
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            Columns = 3,
            ColumnTitlesVisible = true,
            HideRoot = true,
            SelectMode = Tree.SelectModeEnum.Row
        };
        nodeTree.AddThemeConstantOverride("draw_guides", 1);
        nodeTree.SetColumnTitle(0, "Node");
        nodeTree.SetColumnTitle(1, "Status");
        nodeTree.SetColumnTitle(2, "Ticks");
        nodeTree.SetColumnExpandRatio(0, 4);
        nodeTree.SetColumnExpandRatio(1, 1);
        nodeTree.SetColumnExpandRatio(2, 1);
        nodeTreeContainer.AddChild(nodeTree);

        blackboardContainer = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(260, 0),
            SizeFlagsStretchRatio = 1
        };
        blackboardContainer.AddThemeConstantOverride("separation", 4);
        split.AddChild(blackboardContainer);

        var blackboardTitle = new Label { Text = "Blackboard View" };
        blackboardTitle.AddThemeFontOverride("font", GetThemeFont("bold", "EditorFonts"));
        blackboardTitle.AddThemeColorOverride("font_color", GetThemeColor("accent_color", "Editor"));
        blackboardContainer.AddChild(blackboardTitle);

        var separator = new HSeparator();
        blackboardContainer.AddChild(separator);

        blackboardFilter = new LineEdit
        {
            PlaceholderText = "Filter keys…",
            ClearButtonEnabled = true
        };
        blackboardFilter.TextChanged += OnBlackboardFilterChanged;
        blackboardContainer.AddChild(blackboardFilter);

        blackboardEmptyLabel = new Label
        {
            Text = "No blackboard data yet.",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            Visible = true,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        blackboardEmptyLabel.AddThemeColorOverride("font_color", IdleColor);
        blackboardContainer.AddChild(blackboardEmptyLabel);

        blackboardTree = new Tree
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            Visible = false,
            Columns = 2,
            ColumnTitlesVisible = true,
            HideRoot = true,
            SelectMode = Tree.SelectModeEnum.Row
        };
        blackboardTree.AddThemeConstantOverride("draw_guides", 1);
        blackboardTree.SetColumnTitle(0, "Key");
        blackboardTree.SetColumnTitle(1, "Value");
        blackboardTree.SetColumnExpandRatio(0, 1);
        blackboardTree.SetColumnExpandRatio(1, 2);
        blackboardContainer.AddChild(blackboardTree);
    }

    public void OnTreeSnapshot(BTTreeSnapshot snapshot)
    {
        bool isNewTree = !latestTreeSnapshots.ContainsKey(snapshot.TreeId);
        latestTreeSnapshots[snapshot.TreeId] = snapshot;

        if (isNewTree)
            RefreshTreeSelector();

        if (snapshot.TreeId == selectedTreeId)
        {
            RenderNodeTree(snapshot);
            UpdateStatusBar(snapshot);
        }
    }

    public void OnBlackboardSnapshot(BTBlackboardSnapshot snapshot)
    {
        latestBlackboardSnapshots[snapshot.BlackboardId] = snapshot;

        if (latestTreeSnapshots.TryGetValue(selectedTreeId, out var tree)
            && tree.BlackboardId == snapshot.BlackboardId)
            RenderBlackboard(snapshot);
    }

    private void RefreshTreeSelector()
    {
        long previouslySelected = selectedTreeId;
        treeSelector.Clear();

        int i = 0;
        int indexToSelect = -1;
        var icon = GetIconOrFallback("Tree", "Node");

        foreach (var kvp in latestTreeSnapshots)
        {
            long id = kvp.Key;
            string name = kvp.Value.TreeName;

            treeSelector.AddItem($"{name} (#{id})", i);
            treeSelector.SetItemIcon(i, icon);
            treeSelector.SetItemMetadata(i, id);

            if (id == previouslySelected)
                indexToSelect = i;

            i++;
        }

        if (indexToSelect == -1 && treeSelector.ItemCount > 0)
            indexToSelect = 0;

        if (indexToSelect != -1)
        {
            treeSelector.Select(indexToSelect);
            SelectTree((long)treeSelector.GetItemMetadata(indexToSelect));
        }
    }

    private void OnTreeSelected(long index) => SelectTree((long)treeSelector.GetItemMetadata((int)index));

    private void SelectTree(long treeId)
    {
        selectedTreeId = treeId;

        if (!latestTreeSnapshots.TryGetValue(selectedTreeId, out var snapshot))
            return;

        RenderNodeTree(snapshot);
        UpdateStatusBar(snapshot);

        if (latestBlackboardSnapshots.TryGetValue(snapshot.BlackboardId, out var bbSnapshot))
            RenderBlackboard(bbSnapshot);
        else
            ShowBlackboardEmpty("No blackboard data attached to this tree.");
    }

    private void RenderNodeTree(BTTreeSnapshot snapshot)
    {
        nodeTree.Clear();
        var root = nodeTree.CreateItem();
        var itemsByNodeId = new Dictionary<long, TreeItem>();

        foreach (var node in snapshot.Nodes)
        {
            var parentItem = node.ParentId != 0 && itemsByNodeId.TryGetValue(node.ParentId, out var p)
                ? p
                : root;

            var item = nodeTree.CreateItem(parentItem);
            string label = string.IsNullOrEmpty(node.TypeName) ? $"#{node.NodeId}" : node.TypeName;

            item.SetText(0, label);

            item.SetText(1, StatusLabel(node));
            item.SetIcon(1, ResolveStatusIcon(node));
            var statusColor = StatusColor(node);
            item.SetCustomColor(1, statusColor);

            var rowTint = new Color(statusColor, node.IsRunning ? 0.12f : 0.06f);
            if (node.IsRunning)
            {
                item.SetCustomBgColor(0, rowTint);
                item.SetCustomBgColor(1, rowTint);
                item.SetCustomBgColor(2, rowTint);
            }
            else
            {
                item.SetCustomBgColor(1, rowTint);
            }

            item.SetText(2, node.TickCount.ToString());
            item.SetTextAlignment(2, HorizontalAlignment.Right);
            item.SetCustomColor(2, new Color(1, 1, 1, 0.5f));

            itemsByNodeId[node.NodeId] = item;
        }

        ApplyNodeFilter(nodeFilter.Text);
    }

    private void UpdateStatusBar(BTTreeSnapshot snapshot)
    {
        int running = 0, success = 0, failure = 0;
        foreach (var n in snapshot.Nodes)
        {
            if (n.IsRunning) running++;
            else if (n.Status == Status.Success) success++;
            else if (n.Status == Status.Failure) failure++;
        }

        statusBar.Visible = false;
        statusBadgesRow.Visible = true;

        runningBadge.Text = $"{running} running";
        successBadge.Text = $"{success} success";
        failureBadge.Text = $"{failure} failure";
    }

    private void RenderBlackboard(BTBlackboardSnapshot snapshot)
    {
        if (snapshot.Entries.Length == 0)
        {
            ShowBlackboardEmpty("Blackboard is empty.");
            return;
        }

        blackboardEmptyLabel.Visible = false;
        blackboardTree.Visible = true;
        blackboardTree.Clear();

        var root = blackboardTree.CreateItem();

        foreach (var (key, value) in snapshot.Entries)
        {
            var item = blackboardTree.CreateItem(root);

            item.SetText(0, key);
            item.SetIcon(0, ResolveValueIcon(value));

            item.SetText(1, value);
            item.SetCustomColor(1, ValueAccentColor);
        }

        ApplyBlackboardFilter(blackboardFilter.Text);
    }

    private void ShowBlackboardEmpty(string message)
    {
        blackboardEmptyLabel.Text = message;
        blackboardEmptyLabel.Visible = true;
        blackboardTree.Visible = false;
    }

    private void OnNodeFilterChanged(string newText) => ApplyNodeFilter(newText);

    private void OnBlackboardFilterChanged(string newText) => ApplyBlackboardFilter(newText);

    private void ApplyNodeFilter(string filter)
    {
        var root = nodeTree.GetRoot();
        if (root is null)
            return;

        filter = (filter ?? string.Empty).Trim();
        foreach (TreeItem child in root.GetChildren())
            ApplyNodeFilterRecursive(child, filter);
    }

    private static bool ApplyNodeFilterRecursive(TreeItem item, string filter)
    {
        bool selfMatch = filter.Length == 0 || item.GetText(0).Contains(filter, StringComparison.OrdinalIgnoreCase);

        bool childMatch = false;
        foreach (TreeItem child in item.GetChildren())
            childMatch |= ApplyNodeFilterRecursive(child, filter);

        bool visible = selfMatch || childMatch;
        item.Visible = visible;
        return visible;
    }

    private void ApplyBlackboardFilter(string filter)
    {
        var root = blackboardTree.GetRoot();
        if (root is null)
            return;

        filter = (filter ?? string.Empty).Trim();
        foreach (TreeItem child in root.GetChildren())
            child.Visible = filter.Length == 0 || child.GetText(0).Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    private Label CreateStatusBadge(string iconName, Color color, out Control wrapper)
    {
        var stylebox = new StyleBoxFlat
        {
            BgColor = new Color(color, 0.2f),
            BorderColor = new Color(color, 0.35f),
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            BorderWidthBottom = 1,
            ContentMarginLeft = 10,
            ContentMarginRight = 10,
            ContentMarginTop = 3,
            ContentMarginBottom = 3,
            CornerRadiusTopLeft = 10,
            CornerRadiusTopRight = 10,
            CornerRadiusBottomLeft = 10,
            CornerRadiusBottomRight = 10
        };

        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride("panel", stylebox);

        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 5);
        panel.AddChild(row);

        row.AddChild(new TextureRect
        {
            Texture = GetIconOrFallback(iconName, "Node"),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(14, 14)
        });

        var label = new Label();
        label.AddThemeColorOverride("font_color", color);
        row.AddChild(label);

        wrapper = panel;
        return label;
    }

    private Texture2D ResolveStatusIcon(BTNodeSnapshot node) =>
        node.IsRunning
            ? GetIconOrFallback("Play", "Node")
            : node.Status switch
            {
                Status.Success => GetIconOrFallback("StatusSuccess", "Node"),
                Status.Failure => GetIconOrFallback("StatusError", "Node"),
                _ => GetIconOrFallback("StatusWarning", "Node")
            };

    private Texture2D ResolveValueIcon(string valuePreview)
    {
        string iconName = valuePreview switch
        {
            "true" or "True" or "false" or "False" => "bool",
            _ when double.TryParse(valuePreview, out _) => "float",
            _ when valuePreview.StartsWith('(') && valuePreview.EndsWith(')') => "Vector2",
            _ when valuePreview.StartsWith('[') && valuePreview.EndsWith(']') => "Array",
            _ when valuePreview.StartsWith('{') && valuePreview.EndsWith('}') => "Dictionary",
            _ => "MemberProperty"
        };

        return GetIconOrFallback(iconName, "MemberProperty");
    }

    private Texture2D GetIconOrFallback(string iconName, string fallbackIconName) =>
        HasThemeIcon(iconName, "EditorIcons")
            ? GetThemeIcon(iconName, "EditorIcons")
            : GetThemeIcon(fallbackIconName, "EditorIcons");

    private static string StatusLabel(BTNodeSnapshot node) =>
        node.IsRunning ? "Running" : node.Status switch
        {
            Status.Success => "Success",
            Status.Failure => "Failure",
            _ => node.Status.ToString()
        };

    private static Color StatusColor(BTNodeSnapshot node) =>
        node.IsRunning ? RunningColor : node.Status switch
        {
            Status.Success => SuccessColor,
            Status.Failure => FailureColor,
            _ => IdleColor
        };
}
#endif