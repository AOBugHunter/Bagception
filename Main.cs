using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core.GameData;
using AOSharp.Core.UI.Options;
using AOSharp.Core.IPC;
using AOSharp.Common.Unmanaged.Imports;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System.Threading;
using AOSharp.Common.Unmanaged.DataTypes;
using Zoltu.IO;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using System.Runtime.InteropServices;
using System.Drawing;
using AOSharp.Common.SharedEventArgs;
using SmokeLounge.AOtomation.Messaging.GameData;

namespace Bag
{
    public class Main : AOPluginEntry
    {
        public static bool Bagception = false;

        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("Bagception loaded");

                Chat.WriteLine("type /bagception then shift + right click a bag onto another bag");
                Chat.WriteLine("70 stims split in bank, if issues empty bank and repeat");

                Chat.RegisterCommand("bagception", (string command, string[] param, ChatWindow chatWindow) =>
                {
                    Bagception = !Bagception;
                    Chat.WriteLine($"Bagception : {Bagception}");
                });

                Network.N3MessageSent += Network_N3MessageSent;
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private void Network_N3MessageSent(object s, N3Message n3Msg)
        {
            //Only the active window will issue commands
            if (n3Msg.Identity != DynelManager.LocalPlayer.Identity)
                return;

            //Chat.WriteLine($"{n3Msg.N3MessageType}");

            if (n3Msg.N3MessageType == N3MessageType.CharacterAction)
            {
                CharacterActionMessage charActionMsg = (CharacterActionMessage)n3Msg;

                if (charActionMsg.Action == CharacterActionType.UseItemOnItem)
                {
                    if (Bagception)
                    {
                        Item FirstBag = Inventory.Items.Where(c => c.Slot.Instance == charActionMsg.Target.Instance).FirstOrDefault();
                        Item SecondID = Inventory.Items.Where(c => c.Slot.Instance == charActionMsg.Parameter2).FirstOrDefault();
                        Container SecondBag = Inventory.Backpacks.Where(x => x.Slot.Instance == SecondID.Slot.Instance).FirstOrDefault();

                        Identity bagInBagId = SecondBag.Identity;
                        Identity bank = new Identity();
                        bank.Type = IdentityType.BankByRef;
                        bank.Instance = FirstBag.Slot.Instance;

                        StripItem(bank, SecondBag);
                    }
                }
            }
        }

        private static void StripItem(Identity bank, Container bag)
        {
            Network.Send(new ClientContainerAddItem()
            {
                Target = bag.Identity,
                Source = bank
            });
        }

        public override void Teardown()
        {
        }
    }
}
