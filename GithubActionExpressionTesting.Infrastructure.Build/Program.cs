// ---------------------------------------------------------------
// Copyright (c) Christo du Toit. All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using ADotNet.Clients;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets.Tasks;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets.Tasks.SetupDotNetTaskV1s;
using ADotNet.Models.Pipelines.GithubPipelines.DotNets.Tasks.SetupDotNetTaskV3s;

namespace GithubActionExpressionTesting.Infrastructure.Build
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var aDotNetClient = new ADotNetClient();

            var githubPipeline = new GithubPipeline
            {
                Name = ".Net",

                OnEvents = new Events
                {
                    Push = new PushEvent
                    {
                        Branches = new string[] { "main" }
                    },

                    PullRequest = new PullRequestEvent
                    {
                        Types = new string[] { "opened", "synchronize", "reopened", "closed" },
                        Branches = new string[] { "main" }
                    }
                },

                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "IS_RELEASE_CANDIDATE", EnvironmentVariables.IsGitHubReleaseCandidate() }
                },

                Jobs = new Dictionary<string, Job>
                {
                    {
                        "build-ubuntu",
                        new Job
                        {
                            RunsOn = BuildMachines.UbuntuLatest,

                            Steps = new List<GithubTask>
                            {
                                new CheckoutTaskV3
                                {
                                    Name = "Check out"
                                },

                                new ConfigureGitTask()
                                {
                                    Name = "Configure Git",
                                },

                                new ExtractProjectPropertyTask(
                                    projectRelativePath: "GithubActionExpressionTesting/GithubActionExpressionTesting.csproj",
                                    propertyName: "Version",
                                    environmentVariableName: "version_number")
                                {
                                    Name = $"Extract Version"
                                },

                                new ExtractProjectPropertyTask(
                                    projectRelativePath: "GithubActionExpressionTesting/GithubActionExpressionTesting.csproj",
                                    propertyName: "PackageReleaseNotes",
                                    environmentVariableName: "package_release_notes")
                                {
                                    Name = $"Extract Package Release Notes"
                                },

                                new SetupDotNetTaskV3
                                {
                                    Name = "Setup .Net",

                                    With = new TargetDotNetVersionV3
                                    {
                                        DotNetVersion = "7.0.201"
                                    }
                                },

                                new RestoreTask
                                {
                                    Name = "Restore"
                                },

                                new DotNetBuildTask
                                {
                                    Name = "Build"
                                },

                                new TestTask
                                {
                                    Name = "Test"
                                }
                            }
                        }
                    },
                    {
                        "build-windows",
                        new Job
                        {
                            RunsOn = BuildMachines.WindowsLatest,

                            Steps = new List<GithubTask>
                            {
                                new CheckoutTaskV3
                                {
                                    Name = "Check out"
                                },

                                new ConfigureGitTask()
                                {
                                    Name = "Configure Git",
                                },

                                new ExtractProjectPropertyTask(
                                    projectRelativePath: "GithubActionExpressionTesting/GithubActionExpressionTesting.csproj",
                                    propertyName: "Version",
                                    environmentVariableName: "version_number")
                                {
                                    Name = $"Extract Version"
                                },

                                new ExtractProjectPropertyTask(
                                    projectRelativePath: "GithubActionExpressionTesting/GithubActionExpressionTesting.csproj",
                                    propertyName: "PackageReleaseNotes",
                                    environmentVariableName: "package_release_notes")
                                {
                                    Name = $"Extract Package Release Notes"
                                },

                                new SetupDotNetTaskV3
                                {
                                    Name = "Setup .Net",

                                    With = new TargetDotNetVersionV3
                                    {
                                        DotNetVersion = "7.0.201"
                                    }
                                },

                                new RestoreTask
                                {
                                    Name = "Restore"
                                },

                                new DotNetBuildTask
                                {
                                    Name = "Build"
                                },

                                new TestTask
                                {
                                    Name = "Test"
                                }
                            }
                        }
                    },
                }
            };

            string buildScriptPath = "../../../../.github/workflows/dotnet.yml";
            string directoryPath = Path.GetDirectoryName(buildScriptPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            aDotNetClient.SerializeAndWriteToFile(githubPipeline, path: buildScriptPath);
        }
    }
}