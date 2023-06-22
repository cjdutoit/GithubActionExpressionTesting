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

namespace GithubActionExpressionTesting.Infrastructure.Build.Services
{
    public class ScriptGenerationService
    {
        private readonly ADotNetClient adotNetClient;

        public ScriptGenerationService() =>
            this.adotNetClient = new ADotNetClient();

        public void GenerateBuildScript(string branchName, string projectRelativePath, string yamlFile)
        {
            var aDotNetClient = new ADotNetClient();

            var githubPipeline = new GithubPipeline
            {
                Name = "Build",

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
                                    name: "Extract Version",
                                    id: "extract_version",
                                    projectRelativePath,
                                    propertyName: "Version",
                                    stepVariableName: "version_number"),

                                new GithubTask()
                                {
                                    Name = "Display Version",
                                    Run = "echo \"Version number: ${{ steps.extract_version.outputs.version_number }}\""
                                },

                                new ExtractProjectPropertyTask(
                                    name: $"Extract Package Release Notes",
                                    id: "extract_package_release_notes",
                                    projectRelativePath,
                                    propertyName: "PackageReleaseNotes",
                                    stepVariableName: "package_release_notes"),

                                new GithubTask()
                                {
                                    Name = "Display Package Release Notes",
                                    Run =
                                        "echo \"Package Release Notes: "
                                        + "${{ steps.extract_package_release_notes.outputs.package_release_notes }}\""
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

            string buildScriptPath = $"../../../../.github/workflows/{yamlFile}";
            string directoryPath = Path.GetDirectoryName(buildScriptPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            aDotNetClient.SerializeAndWriteToFile(githubPipeline, path: buildScriptPath);
        }

        public void GenerateOsSpecificBuildScript(
            string buildName,
            string branchName,
            string projectRelativePath,
            string yamlFile,
            string buildMachine)
        {
            var aDotNetClient = new ADotNetClient();

            var githubPipeline = new GithubPipeline
            {
                Name = buildName,

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
                            RunsOn = buildMachine,

                            Steps = new List<GithubTask>
                            {
                                new CheckoutTaskV3
                                {
                                    Name = "Check out"
                                },

                                new ExtractProjectPropertyTask(
                                    name: "Extract Version",
                                    id: "extract_version",
                                    projectRelativePath,
                                    propertyName: "Version",
                                    stepVariableName: "version_number"),

                                new GithubTask()
                                {
                                    Name = "Display Version",
                                    Run = "echo \"Version number: ${{ steps.extract_version.outputs.version_number }}\""
                                },

                                new ExtractProjectPropertyTask(
                                    name: $"Extract Package Release Notes",
                                    id: "extract_package_release_notes",
                                    projectRelativePath,
                                    propertyName: "PackageReleaseNotes",
                                    stepVariableName: "package_release_notes"),

                                new GithubTask()
                                {
                                    Name = "Display Package Release Notes",
                                    Run =
                                        "echo \"Package Release Notes: "
                                        + "${{ steps.extract_package_release_notes.outputs.package_release_notes }}\""
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

            string buildScriptPath = $"../../../../.github/workflows/{yamlFile}";
            string directoryPath = Path.GetDirectoryName(buildScriptPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            aDotNetClient.SerializeAndWriteToFile(githubPipeline, path: buildScriptPath);
        }

    }
}
