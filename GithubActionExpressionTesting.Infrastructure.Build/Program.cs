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
            string branchName = "main";
            string projectRelativePath = "GithubActionExpressionTesting/GithubActionExpressionTesting.csproj";
            string versionEnvironmentVariableName = "version_number";
            string packageReleaseNotesEnvironmentVariable = "package_release_notes";

            var aDotNetClient = new ADotNetClient();

            var githubPipeline = new GithubPipeline
            {
                Name = ".Net",

                OnEvents = new Events
                {
                    Push = new PushEvent
                    {
                        Branches = new string[] { branchName }
                    },

                    PullRequest = new PullRequestEvent
                    {
                        Types = new string[] { "opened", "synchronize", "reopened", "closed" },
                        Branches = new string[] { branchName }
                    }
                },

                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "IS_RELEASE_CANDIDATE", EnvironmentVariables.IsGitHubReleaseCandidate() }
                },

                Jobs = new Dictionary<string, Job>
                {
                    {
                        "build",
                        new Job
                        {
                            RunsOn = BuildMachines.UbuntuLatest,

                            Steps = new List<GithubTask>
                            {
                                new CheckoutTaskV3
                                {
                                    Name = "Check out"
                                },

                                new ExtractProjectPropertyTask(
                                    projectRelativePath,
                                    propertyName: "Version",
                                    environmentVariableName: versionEnvironmentVariableName)
                                {
                                    Name = $"Extract Version"
                                },

                                new GithubTask()
                                {
                                    Name = "Display Version Found",
                                    Run = "echo '" + versionEnvironmentVariableName + ": ${{ env.$(" + versionEnvironmentVariableName + ") }}'"
                                },

                                new ExtractProjectPropertyTask(
                                    projectRelativePath,
                                    propertyName: "PackageReleaseNotes",
                                    environmentVariableName: packageReleaseNotesEnvironmentVariable)
                                {
                                    Name = $"Extract Package Release Notes"
                                },

                                new GithubTask()
                                {
                                    Name = "Display Package Release Notes",
                                    Run = "echo '" + packageReleaseNotesEnvironmentVariable + ": ${{ env.$(" + packageReleaseNotesEnvironmentVariable + ") }}'"
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
                        "add_tag",
                        new TagJob(
                            runsOn: BuildMachines.UbuntuLatest,
                            dependsOn: "build",
                            projectRelativePath,
                            githubToken: "${{ secrets.PAT_FOR_TAGGING }}",
                            versionEnvironmentVariableName,
                            packageReleaseNotesEnvironmentVariable,
                            branchName)
                    },
                    {
                        "publish",
                        new PublishJob(
                            runsOn: BuildMachines.UbuntuLatest,
                            dependsOn: "add_tag",
                            nugetApiKey: "${{ secrets.NUGET_ACCESS }}")
                    }
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