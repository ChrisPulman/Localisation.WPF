// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Localisation.Building;

/// <summary>Defines the repository build pipeline.</summary>
public sealed class Build : NukeBuild
{
    private static readonly AbsolutePath SolutionFile = RootDirectory / "src" / "Localisation.slnx";

    private readonly Solution _solution = SolutionFile.ReadSolution();

    /// <summary>Gets or sets the build configuration.</summary>
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public Configuration Configuration { get; set; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    private static AbsolutePath PackagesDirectory => RootDirectory / "output";

    private Target Print => target => target
        .Executes(() =>
        {
            Log.Information("Configuration = {Configuration}", Configuration);
            Log.Information("MinVerVersionOverride = {Value}", Environment.GetEnvironmentVariable("MinVerVersionOverride") ?? "<auto>");
        });

    private Target Clean => target => target
        .Before(Restore)
        .Executes(() =>
        {
            if (IsLocalBuild)
            {
                return;
            }

            _ = PackagesDirectory.CreateOrCleanDirectory();
        });

    private Target Restore => target => target
        .DependsOn(Clean)
        .Executes(() => DotNetRestore(s => s.SetProjectFile(_solution)));

    private Target Compile => target => target
        .DependsOn(Restore, Print)
        .Executes(() => DotNetBuild(s => s
                .SetProjectFile(_solution)
                .SetConfiguration(Configuration)
                .SetNoRestore(true)));

    /// <summary>Runs the requested NUKE targets.</summary>
    /// <returns>The process exit code.</returns>
    public static int Main() => Execute<Build>(x => x.Compile);
}
