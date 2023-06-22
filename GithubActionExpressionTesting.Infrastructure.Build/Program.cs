// ---------------------------------------------------------------
// Copyright (c) Christo du Toit. All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using ADotNet.Models.Pipelines.GithubPipelines.DotNets;
using GithubActionExpressionTesting.Infrastructure.Build.Services;

namespace GithubActionExpressionTesting.Infrastructure.Build
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var scriptGenerationService = new ScriptGenerationService();

            scriptGenerationService.GenerateBuildScript(
                branchName: "main",
                projectRelativePath: "GithubActionExpressionTesting/GithubActionExpressionTesting.csproj",
                yamlFile: "build.yml");

            scriptGenerationService.GenerateOsSpecificBuildScript(
                buildName: "Windows Build",
                branchName: "main",
                projectRelativePath: "GithubActionExpressionTesting/GithubActionExpressionTesting.csproj",
                yamlFile: "build-windows.yml",
                BuildMachines.WindowsLatest);

            scriptGenerationService.GenerateOsSpecificBuildScript(
                buildName: "Ubuntu Build",
                branchName: "main",
                projectRelativePath: "GithubActionExpressionTesting/GithubActionExpressionTesting.csproj",
                yamlFile: "build-ubuntu.yml",
                BuildMachines.UbuntuLatest);
        }
    }
}