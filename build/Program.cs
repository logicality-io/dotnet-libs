﻿using System;
using System.IO;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace Build
{
    class Program
    {
        private const string ArtifactsDir = "artifacts";
        private const string Clean = "clean";
        private const string Build = "build";
        private const string Test = "test";
        private const string Pack = "pack";
        private const string Publish = "publish";

        static void Main(string[] args)
        {
            Target(Clean, () =>
            {
                if (Directory.Exists(ArtifactsDir))
                {
                    Directory.Delete(ArtifactsDir, true);
                }
                Directory.CreateDirectory(ArtifactsDir);
            });

            Target(Build, () => Run("dotnet", "build Hosting.sln -c Release"));

            Target(
                Test,
                DependsOn(Build),
                () => Run("dotnet", $"test test/Hosting.Tests/Hosting.Tests.csproj -c Release -r {ArtifactsDir} --no-build -l trx;LogFileName=Hosting.Tests.xml --verbosity=normal"));

            Target(
                Pack,
                DependsOn(Build),
                new[] { "Hosting", "Hosting.Docker", "Hosting.SerilogConsoleLogging" },
                project => Run("dotnet", $"pack src/{project}/{project}.csproj -c Release -o {ArtifactsDir} --no-build"));

            Target(Publish, DependsOn(Pack), () =>
            {
                var packagesToPush = Directory.GetFiles(ArtifactsDir, "*.nupkg", SearchOption.TopDirectoryOnly);
                Console.WriteLine($"Found packages to publish: {string.Join("; ", packagesToPush)}");

                var apiKey = Environment.GetEnvironmentVariable("FEEDZ_LOGICALITY_API_KEY");
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    Console.WriteLine("Feedz API Key not available. No packages will be pushed.");
                    return;
                }
                Console.WriteLine($"Feedz API Key ({apiKey.Substring(0, 5)}) available. Pushing packages to Feedz...");
                foreach (var packageToPush in packagesToPush)
                {
                    Run("dotnet", $"nuget push {packageToPush} -s https://f.feedz.io/logicality/public/nuget/index.json -k {apiKey} --skip-duplicate", noEcho: true);
                }
            });

            Target("default", DependsOn(Clean, Test, Publish));

            RunTargetsAndExit(args);
        }
    }
}
