﻿namespace QModManager.API.SMLHelper.Patchers
{
    using Harmony;
    using QModManager.Utility;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class DevConsolePatcher
    {
        public static List<CommandInfo> commands = new List<CommandInfo>();

        public static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(DevConsole), "Submit"),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(DevConsolePatcher), "Postfix")));

            Logger.Debug("DevConsolePatcher is done.");
        }

        internal static void Postfix(bool __result, string value)
        {
            char[] separator = new char[]
            {
                ' ',
                '\t'
            };

            string text = value.Trim();
            string[] args = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            if (args.Length != 0)
            {
                foreach (CommandInfo command in commands)
                {
                    if (command.Name.Contains(args[0]))
                    {
                        string[] argsList = args.ToList().GetRange(1, args.Length - 1).ToArray();

                        command.CommandHandler.Invoke(null, new object[] { argsList });

                        __result = true;
                        return;
                    }
                }
            }

            __result = false;
        }
    }

    internal class CommandInfo
    {
        public MethodInfo CommandHandler;
        public string Name;
        public bool CaseSensitive;
        public bool CombineArgs;
    }
}