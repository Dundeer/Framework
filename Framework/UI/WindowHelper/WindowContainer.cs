using System.Text.RegularExpressions;
using System;
using FairyGUI;

public class WindowContainer {
    public string name;
    public GComponent root;

    public static void CreateWindowContainer(WindowGroupConfig config) {
        var container = new WindowContainer();
        container.Init(config);
    }

    public void Init(WindowGroupConfig config) {
        root = new GComponent();
        name = root.name = root.gameObjectName = config.name;
        root.opaque = config.swallowTouch;

        root.SetSize(UIContainer.designWidth, UIContainer.designHeight, true);

        GRoot.inst.AddChild(root);
        WindowHelper.AddWindowContainer(this);
    }
}