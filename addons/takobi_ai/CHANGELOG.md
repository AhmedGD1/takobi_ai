1. Used Export property hint range to clamp service interval value between [0.05, 10]
2. Behavior tree node now guard UnregisterMetrics() method with #if DEBUG to make sure it won't be called in release builds
3. Name mismatch fix (CheckBlackboardValueInspectorPlugin -> BlackboardCompareInspectorPlugin)
4. fix: debugger now shows node name instead of its type.
