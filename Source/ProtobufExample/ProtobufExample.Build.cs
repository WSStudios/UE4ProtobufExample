// Fill out your copyright notice in the Description page of Project Settings.

using UnrealBuildTool;
using System.IO;

public class ProtobufExample : ModuleRules
{
	public ProtobufExample(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
	
		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore" });

		PrivateDependencyModuleNames.AddRange(new string[] { "protobuf" });
        //PublicDelayLoadDLLs.Add("libprotobuf-lite.dll");
        //RuntimeDependencies.Add("ThirdParty/protobuf/bin/libprotobuf-lite.dll");
        RuntimeDependencies.Add("$(TargetOutputDir)/libprotobuf.dll",
            "$(ProjectDir)/Source/ThirdParty/protobuf/tools/protobuf/libprotobuf.dll");

        // Uncomment if you are using Slate UI
        // PrivateDependencyModuleNames.AddRange(new string[] { "Slate", "SlateCore" });

        // Uncomment if you are using online features
        // PrivateDependencyModuleNames.Add("OnlineSubsystem");

        // To include OnlineSubsystemSteam, add it to the plugins section in your uproject file with the Enabled attribute set to true
    }
}
