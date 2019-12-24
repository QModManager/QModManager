﻿namespace QModManager.API.ModLoading
{
    using System.Collections.Generic;
    using QModManager.Patching;
    using QModManager.Utility;

    internal class Initializer
    {
        private readonly QModGame currentGame;

        internal Initializer(QModGame currentlyRunningGame)
        {
            currentGame = currentlyRunningGame;
        }

        internal void InitializeMods(List<QMod> modsToInitialize)
        {
            InitializeMods(modsToInitialize, PatchingOrder.PreInitialize);
            InitializeMods(modsToInitialize, PatchingOrder.NormalInitialize);
            InitializeMods(modsToInitialize, PatchingOrder.PostInitialize);
        }

        private void InitializeMods(List<QMod> modsToInitialize, PatchingOrder order)
        {
            foreach (QMod mod in modsToInitialize)
            {
                if (mod.Status != ModStatus.Success)
                    continue;

                if ((mod.SupportedGame & currentGame) == QModGame.None)
                {
                    mod.PatchMethods.Clear(); // Do not attempt any other patch methods
                    mod.Status = ModStatus.CurrentGameNotSupported;
                    continue;
                }

                if (mod.PatchMethods.Count == 0 || !mod.PatchMethods.TryGetValue(order, out QModPatchMethod patchMethod))
                    continue;

                if (patchMethod.IsPatched)
                {
                    mod.Status = ModStatus.DuplicatePatchAttemptDetected;
                    continue;
                }

                Logger.Debug($"Starting patch method for mod \"{mod.Id}\" at {order}");

                if (!patchMethod.TryInvoke())
                {
                    mod.PatchMethods.Clear(); // Do not attempt any other patch methods
                    mod.Status = ModStatus.PatchMethodFailed;
                    continue;
                }

                Logger.Debug($"Completed patch method for mod \"{mod.Id}\" at {order}");
            }
        }
    }
}
