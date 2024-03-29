using System.Drawing;
using Backend.Tibia7;
using System.Collections.Generic;
using Backend.Tibia11;
using System;

namespace Backend
{
    public interface GameData : IDisposable
    {
        Bitmap? GetSpriteBitmap(uint spriteId);
        Bitmap? GetItemTypeBitmap(Appearance appearance);
        VersionData Version { get; }

        void ComputeSpriteHash(Appearance appearance);

        uint LastItemTypeClientId { get; }
        uint LastItemTypeServerId { get; }

        void Load(Progress<int>? reporter);

        void CreateNewOtb();
        void LoadOtb(string path);
        void WriteOtb(string path);

        void WriteClientData(string path);

        OtbData? OtbData { get; }

        
        /// <summary>
        /// Creates missing items in the game data.
        /// </summary>
        /// <returns>The number of items created.</returns>
        uint CreateMissingItems();

        IEnumerable<Appearance> Objects { get; }


        Appearance? GetItemTypeByServerId(uint serverId);
        Appearance? GetItemTypeByClientId(uint clientId);

        Appearance GetOrCreateItemTypeByClientId(uint clientId);
    }
}