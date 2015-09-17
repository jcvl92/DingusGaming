using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Rocket.Unturned.Player;
using Steamworks;
using UnityEngine;

namespace DingusGaming.Party
{
    public class Party
    {
        private readonly List<CSteamID> members;
        private CSteamID leader;
        private static readonly Color chatColor = Color.cyan;

        public Party(UnturnedPlayer leader)
        {
            this.leader = leader.CSteamID;
            members = new List<CSteamID>();
            members.Add(leader.CSteamID);
        }

        public ReadOnlyCollection<CSteamID> getMembers()
        {
            return members.AsReadOnly();
        }

        public void disband()
        {
            members.Clear();
        }

        public bool isMember(UnturnedPlayer player)
        {
            return members.Contains(player.CSteamID);
        }

        public bool isLeader(UnturnedPlayer player)
        {
            return leader.Equals(player.CSteamID);
        }

        public string getInfo()
        {
            var info = "";

            foreach (var member in members)
            {
                var player = DGPlugin.getPlayer(member);
                info += player.CharacterName + (isLeader(player) ? "[L](" : "(") +
                        (player.Dead ? "dead" : player.Health + "/100") + "), ";
            }

            return info.Substring(0, info.Length - 2);
        }

        public UnturnedPlayer getLeader()
        {
            return DGPlugin.getPlayer(leader);
        }

        public void tellParty(string text)
        {
            foreach (var member in members)
                DGPlugin.messagePlayer(DGPlugin.getPlayer(member), text, chatColor);
        }

        public void tellParty(string text, UnturnedPlayer skipPlayer)
        {
            foreach (var member in members)
                if(!member.Equals(skipPlayer.CSteamID))
                    DGPlugin.messagePlayer(DGPlugin.getPlayer(member), text, chatColor);
        }

        public void chat(UnturnedPlayer caller, string text)
        {
            if (isMember(caller))
                tellParty(caller.CharacterName + (isLeader(caller) ? "[L]: " : "[P]: ") + text);
            else
                DGPlugin.messagePlayer(caller, "Error, you are not in this party.");
        }

        public void addMember(UnturnedPlayer player)
        {
            members.Add(player.CSteamID);
            tellParty(player.CharacterName + " has joined the party!", player);
            DGPlugin.messagePlayer(player, "You have joined the party!", chatColor);
        }

        public void kickMember(UnturnedPlayer caller, UnturnedPlayer player)
        {
            if (!isMember(player))
            {
                DGPlugin.messagePlayer(caller, player.CharacterName + " is not in your party.");
                return;
            }

            if (leader.Equals(caller.CSteamID))
            {
                removeMember(player);
                DGPlugin.messagePlayer(player, "You were removed from the party.");
            }
            else
                DGPlugin.messagePlayer(caller,
                    "Only the party leader(" + DGPlugin.getPlayer(leader).CharacterName + ") can kick party members.");
        }

        public void removeMember(UnturnedPlayer player)
        {
            members.RemoveAt(members.FindIndex(0, x => x.Equals(player.CSteamID)));

            Parties.toggleChat(player, false);

            //promote a new leader if the leader was removed
            if (members.Count > 1)
            {
                if (leader.Equals(player.CSteamID))
                {
                    leader = members.First();
                    tellParty(DGPlugin.getPlayer(leader).CharacterName + " has been made party leader!");
                }
            }
            else
            {
                Parties.disbandParty(this);
            }
        }

        public void makeLeader(UnturnedPlayer caller, UnturnedPlayer player)
        {
            if (caller.Equals(player))
            {
                DGPlugin.messagePlayer(caller, "You are already the party leader.");
                return;
            }
            if (isMember(player))
            {
                if (isLeader(caller))
                {
                    leader = player.CSteamID;
                    tellParty(player.CharacterName + " has been made party leader!");
                }
                else
                {
                    DGPlugin.messagePlayer(caller,
                        "Only the party leader(" + DGPlugin.getPlayer(leader).CharacterName + ") switch leaders.");
                }
            }
            else
                DGPlugin.messagePlayer(caller, "Could not find " + player.CharacterName + " in your party.");
        }
    }
}