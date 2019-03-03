using System;
using System.Linq;
using System.Collections.Generic;
using NiceIO;
using Unity.BuildSystem.CSharpSupport;
using Unity.BuildSystem.MacSDKSupport;
using Unity.BuildSystem.NativeProgramSupport;
using Unity.BuildSystem.VisualStudio.MsvcVersions;
using Unity.BuildTools;
using Bee.Core;
using Bee.Toolchain.Android;
using Bee.Toolchain.IOS;
using Bee.Toolchain.Windows;
using Bee.Toolchain.VisualStudio;
using Bee.Toolchain.GNU;

class BuildProgram
{
    static string pluginDir = "../com.unity.androidstudio/Plugins";

    static void Main()
    {
        // Building AndroidStudio plugin
        List<BuildCommand> commands = new List<BuildCommand>();
        commands.Add(BuildCommand.Create(new AndroidNdkToolchain(AndroidNdk.LocatorArmv7.UserDefaultOrLatest), "android", "Android/armeabi-v7a"));
        commands.Add(BuildCommand.Create(new AndroidNdkToolchain(AndroidNdk.LocatorArm64.UserDefaultOrLatest), "android", "Android/armeabi"));
        commands.Add(BuildCommand.Create(new AndroidNdkToolchain(AndroidNdk.Locatorx86.UserDefaultOrLatest), "android", "Android/x86"));

        NativeProgram plugin = new NativeProgram("libandroidstudio")
        {
            Sources = { "src" }
        };

        foreach (var command in commands)
        {
            var toolchain = command.ToolChain;

            var config = new NativeProgramConfiguration(CodeGen.Release, toolchain, false);
            var builtProgram = plugin.SetupSpecificConfiguration(config, toolchain.DynamicLibraryFormat);
            var artefact = builtProgram.Path;
            if (command.PostProcess != null)
                artefact = command.PostProcess(artefact, toolchain, command.PluginSubFolder);
            Backend.Current.AddAliasDependency(command.Alias, artefact);
        }
    }

    class BuildCommand
    {
        public ToolChain ToolChain;
        public string Alias;
        public string PluginSubFolder;
        public Func<NPath, ToolChain, string, NPath> PostProcess = PostProcessDefault;
        public static BuildCommand Create (ToolChain chain, string alias, string pluginSubFolder = "")
        {
            return new BuildCommand() { Alias = alias, ToolChain = chain, PluginSubFolder = pluginSubFolder };
        }
    }

    private static NPath PostProcessDefault(NPath builtProgram, ToolChain toolchain, string subFolderDir)
    {
        return Copy(builtProgram, builtProgram, toolchain, subFolderDir);
    }

    private static NPath Copy(NPath from, NPath to, ToolChain toolchain, string subFolderDir)
    {
        if (subFolderDir != string.Empty)
            to = new NPath($"{pluginDir}/{subFolderDir}").Combine(to.FileName);
        else
            to = new NPath($"{pluginDir}/{toolchain.LegacyPlatformIdentifier}").Combine(to.FileName);
        CopyTool.Instance().Setup(to, from);
        return to;
    }

}
