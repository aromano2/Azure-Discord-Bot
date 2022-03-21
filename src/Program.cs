using DSharpPlus;
using DSharpPlus.EventArgs;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.ResourceManager;
using Azure.Security.KeyVault.Secrets;
using System.Net.Sockets;

namespace AzureDiscordBot
{
    public class Program
    {
        public static async Task ManageVm(string Task)
        {
            ResourceIdentifier vmId = new ResourceIdentifier("/subscriptions/716c5ff9-7dff-4a96-aae5-91cd70e3df8a/resourceGroups/Sandbox/providers/Microsoft.Compute/virtualMachines/App");
            ArmClient armClient = new ArmClient(new DefaultAzureCredential());
            VirtualMachine vm = armClient.GetVirtualMachine(vmId);
            
            switch (Task)
            {
                case "Start":
                    await vm.PowerOnAsync(false);

                    break;
                case "Stop":
                    await vm.DeallocateAsync(false);

                    break;
            }
        }

        public static async Task Main(string[] args)
        {
            var credentials = new DefaultAzureCredential();

            
            //Key vault service URL
            var kvUri = "https://azurediscord.vault.azure.net/";
            var credential = new ChainedTokenCredential(
                new ManagedIdentityCredential());

            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            KeyVaultSecret secret = client.GetSecret("DiscordToken");

            var discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = secret.Value,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            });
            
            discordClient.MessageCreated += OnMessageCreated;

            await discordClient.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task OnMessageCreated(object sender, MessageCreateEventArgs e)
        {
            switch (e.Message.Content.ToLower())
            {
                case "!valheimlist":
                    await e.Message.RespondAsync(
                        "!valheimstart - Start Valheim\n" +
                        "!valheimstop - Stop Valheim");
                    break;
                case "!valheimstart":
                    await ManageVm("Start");
                    await e.Message.RespondAsync(e.Message.Author.Username + " started Valheim.");

                    break;
                case "!valheimstop":
                    await ManageVm("Stop");
                    await e.Message.RespondAsync(e.Message.Author.Username + " stopped Valheim.");

                    break;
            }
        }
    }
}