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
    static void Main()
    {
        // For Windows plugins
        List<BuildCommand> windows = new List<BuildCommand>();
        windows.Add(BuildCommand.Create(new WindowsToolchain(WindowsSdk.Locatorx86.UserDefaultOrLatest), "windows", "x86"));
        windows.Add(BuildCommand.Create(new WindowsToolchain(WindowsSdk.Locatorx64.UserDefaultOrLatest), "windows", "x86_64"));

        // For Android plugins
        List<BuildCommand> android = new List<BuildCommand>();
        android.Add(BuildCommand.Create(new AndroidNdkToolchain(AndroidNdk.LocatorArmv7.UserDefaultOrLatest), "android", "Android/armeabi-v7a"));
        android.Add(BuildCommand.Create(new AndroidNdkToolchain(AndroidNdk.LocatorArm64.UserDefaultOrLatest), "android", "Android/arm64-v8a"));
        android.Add(BuildCommand.Create(new AndroidNdkToolchain(AndroidNdk.Locatorx86.UserDefaultOrLatest), "android", "Android/x86"));


        NativeProgram androidStudioPlugin = new NativeProgram("libandroidstudio");
        androidStudioPlugin.Sources.Add("src/ProfilerPlugin.cpp");
        androidStudioPlugin.Sources.Add("src/AndroidSystrace.cpp");
        androidStudioPlugin.Sources.Add("src/AndroidSystraceProfiler.cpp");
        androidStudioPlugin.PrebuiltLibraries.Add(new SystemLibrary("log"));
        ProcessProgram(androidStudioPlugin, "../com.unity.nativeprofilers.androidsystrace/Plugins", android);

        NativeProgram streamlineAnalyzerPlugin = new NativeProgram("libstreamlineanalyzer");
        streamlineAnalyzerPlugin.Sources.Add("src/ProfilerPlugin.cpp");
        streamlineAnalyzerPlugin.Sources.Add("src/StreamlineAnalyzerProfiler.cpp");
        streamlineAnalyzerPlugin.Sources.Add("src/Arm");
        streamlineAnalyzerPlugin.PrebuiltLibraries.Add(new SystemLibrary("log"));
        ProcessProgram(streamlineAnalyzerPlugin, "../com.unity.nativeprofilers.streamlineanalyzer/Plugins", android);

        NativeProgram vtuneAmplifierPlugin = new NativeProgram("vtuneamplifier");
        vtuneAmplifierPlugin.Sources.Add("src/ProfilerPlugin.cpp");
        vtuneAmplifierPlugin.Sources.Add("src/VTuneAmplifierProfiler.cpp");
        vtuneAmplifierPlugin.DynamicLinkerSettingsForWindows().Add(s => s.WithDefinitionFile("src/ProfilerPlugin.def"));
        vtuneAmplifierPlugin.PrebuiltLibraries.Add((program) => { return program.ToolChain.Architecture == Architecture.x64 ? true : false; }, new StaticLibrary("src/Intel/libittnotify64.lib"));
        vtuneAmplifierPlugin.PrebuiltLibraries.Add((program) => { return program.ToolChain.Architecture == Architecture.x86 ? true : false; }, new StaticLibrary("src/Intel/libittnotify32.lib"));
        ProcessProgram(vtuneAmplifierPlugin, "../com.unity.nativeprofilers.vtune/Plugins", windows);
    }

    private static void ProcessProgram(NativeProgram plugin, string targetDir, List<BuildCommand> commands)
    {
        foreach (var command in commands)
        {
            var toolchain = command.ToolChain;

            var config = new NativeProgramConfiguration(CodeGen.Release, toolchain, false);
            var builtProgram = plugin.SetupSpecificConfiguration(config, toolchain.DynamicLibraryFormat);
            var artefact = builtProgram.Path;
            if (command.PostProcess != null)
                artefact = command.PostProcess(artefact, toolchain, targetDir, command.PluginSubFolder);
            Backend.Current.AddAliasDependency(command.Alias, artefact);
        }
    }

    class BuildCommand
    {
        public ToolChain ToolChain;
        public string Alias;
        public string PluginSubFolder;
        public Func<NPath, ToolChain, string, string, NPath> PostProcess = PostProcessDefault;
        public static BuildCommand Create (ToolChain chain, string alias, string pluginSubFolder = "")
        {
            return new BuildCommand() { Alias = alias, ToolChain = chain, PluginSubFolder = pluginSubFolder };
        }
    }

    private static NPath PostProcessDefault(NPath builtProgram, ToolChain toolchain, string pluginDir, string subFolderDir)
    {
        return Copy(builtProgram, builtProgram, toolchain, pluginDir, subFolderDir);
    }

    private static NPath Copy(NPath from, NPath to, ToolChain toolchain, string pluginDir, string subFolderDir)
    {
        if (subFolderDir != string.Empty)
            to = new NPath($"{pluginDir}/{subFolderDir}").Combine(to.FileName);
        else
            to = new NPath($"{pluginDir}/{toolchain.LegacyPlatformIdentifier}").Combine(to.FileName);
        CopyTool.Instance().Setup(to, from);
        return to;
    }

}
