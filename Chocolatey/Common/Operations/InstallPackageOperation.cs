﻿using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Inedo.Agents;
using Inedo.Diagnostics;
using Inedo.Documentation;
using Inedo.Serialization;
#if Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Operations;
#elif BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Operations;
#endif

namespace Inedo.Extensions.Chocolatey.Operations
{
    [DisplayName("Install Chocolatey Package")]
    [Description("Installs a Chocolatey package on a server.")]
    [ScriptNamespace("Chocolatey")]
    [ScriptAlias("Install-Package")]
    [DefaultProperty(nameof(PackageName))]
    [Tag("chocolatey")]
    public sealed class InstallPackageOperation : ExecuteOperation
    {
        [Required]
        [Persistent]
        [ScriptAlias("Name")]
        [DisplayName("Package name")]
        public string PackageName { get; set; }

        [Persistent]
        [ScriptAlias("Version")]
        [DisplayName("Version")]
        [Description("The version number of the package to install. Leave blank for the latest version.")]
        public string Version { get; set; }

        public override async Task ExecuteAsync(IOperationExecutionContext context)
        {
            var buffer = new StringBuilder("upgrade --yes --fail-on-unfound ", 200);

            if (context.Simulation)
                buffer.Append("--what-if ");

            if (!string.IsNullOrEmpty(this.Version))
            {
                buffer.Append("--version \"");
                buffer.Append(this.Version);
                buffer.Append("\" ");
            }

            buffer.Append('\"');
            buffer.Append(this.PackageName);
            buffer.Append('\"');

            int exitCode = await this.ExecuteCommandLineAsync(
                context,
                new RemoteProcessStartInfo
                {
                    FileName = "choco",
                    Arguments = buffer.ToString()
                }
            ).ConfigureAwait(false);

            if (exitCode != 0)
                this.LogError("Process exited with code " + exitCode);
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            if (string.IsNullOrEmpty(this.Version))
            {
                return new ExtendedRichDescription(
                    new RichDescription(
                        "Install latest version of ",
                        new Hilite(config[nameof(this.PackageName)]),
                        " from Chocolatey"
                    )
                );
            }

            return new ExtendedRichDescription(
                new RichDescription(
                    "Install version ",
                    new Hilite(config[nameof(this.Version)]),
                    " of ",
                    new Hilite(config[nameof(this.PackageName)]),
                    " from Chocolatey"
                )
            );
        }
    }
}