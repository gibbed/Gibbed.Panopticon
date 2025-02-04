/* Copyright (c) 2025 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using Gibbed.Buffers;
using Gibbed.Memory;
using Gibbed.Panopticon.Common;

namespace Gibbed.Panopticon.FileFormats.ItemSpecs
{
    using IItemLabeler = ILabeler<StringPool>;

    internal class FileHeader
    {
        public const int Size = 224;

        public const uint Signature = 0x50534923u; // '#ISP'

        public const int WeaponTypeCount = 8;

        public Endian Endian;
        public FileVersion Version;
        public readonly TableInfo<Item> Items = new();
        public readonly TableInfo<CraftRecipe> WeaponFacilityRecipes = new();
        public readonly TableInfo<ProductionRecipe> MunitionFacilityRecipes = new();
        public readonly TableInfo<ProductionRecipe> MedicalFacilityRecipes = new();
        public readonly TableInfo<FieldItemLotTable> FieldItemLotTables = new();
        public readonly TableInfo<UpgradeRecipe>[] WeaponUpgradeRecipesByType;
        public readonly TableInfo<Unknown70> Unknown70s = new();
        public readonly TableInfo<RewardCitizenLot> RewardCitizenLots = new();
        public readonly TableInfo<Unknown80> Unknown80s = new();
        public readonly TableInfo<Unknown88> Unknown88s = new();
        public readonly TableInfo<Unknown90> Unknown90s = new();
        public readonly TableInfo<Unknown98> Unknown98s = new();
        public readonly TableInfo<UnknownA0> UnknownA0s = new();
        public readonly TableInfo<UnknownA8> UnknownA8s = new();
        public readonly TableInfo<CitizenFirstName> CitizenFirstNames = new();
        public readonly TableInfo<CitizenLastName> CitizenLastNames = new();
        public readonly TableInfo<UnknownC8> UnknownC8s = new();
        public readonly TableInfo<UnknownD0> UnknownD0s = new();
        public readonly TableInfo<UnknownD8> UnknownD8s = new();

        public FileHeader()
        {
            this.WeaponUpgradeRecipesByType = new TableInfo<UpgradeRecipe>[WeaponTypeCount];
            for (int i = 0; i < WeaponTypeCount; i++)
            {
                this.WeaponUpgradeRecipesByType[i] = new();
            }
        }

        public static FileHeader Read(ReadOnlySpan<byte> span, ref int index)
        {
            if (span.Length < Size)
            {
                throw new ArgumentOutOfRangeException(nameof(span), "span is too small");
            }

            var magic = span.ReadValueU32(ref index, Endian.Little);
            if (magic != Signature)
            {
                throw new FormatException("unexpected signature");
            }

            var bom = span.ReadValueU16(ref index, Endian.Little);
            if (bom != 0xFFFE && bom != 0xFEFF)
            {
                throw new FormatException("unexpected bom");
            }
            var endian = bom == 0xFFFE ? Endian.Little : Endian.Big;

            span.SkipPadding(ref index, 2);

            FileHeader instance = new();
            instance.Endian = endian;
            instance.Items.Read(span, ref index, endian);
            var version = instance.Version = instance.Items.Header.Offset switch
            {
                0xA0 => FileVersion.Vita,
                0xE0 => FileVersion.Remaster,
                _ => throw new FormatException(),
            };
            instance.WeaponFacilityRecipes.Read(span, ref index, endian);
            instance.MunitionFacilityRecipes.Read(span, ref index, endian);
            instance.MedicalFacilityRecipes.Read(span, ref index, endian);
            instance.FieldItemLotTables.Read(span, ref index, endian);
            {
                int countIndex = index;
                int offsetIndex = index + WeaponTypeCount * 4;
                for (int i = 0; i < WeaponTypeCount; i++)
                {
                    var weaponUpgradeRecipeTable = instance.WeaponUpgradeRecipesByType[i];
                    weaponUpgradeRecipeTable.Header.Count = span.ReadValueS32(ref countIndex, endian);
                    weaponUpgradeRecipeTable.Header.Offset = span.ReadValueS32(ref offsetIndex, endian);
                }
                index = offsetIndex;
            }
            instance.Unknown70s.Read(span, ref index, endian);
            instance.RewardCitizenLots.Read(span, ref index, endian);
            instance.Unknown80s.Read(span, ref index, endian);
            instance.Unknown88s.Read(span, ref index, endian);
            instance.Unknown90s.Read(span, ref index, endian);
            instance.Unknown98s.Read(span, ref index, endian);
            if (version == FileVersion.Remaster)
            {
                instance.UnknownA0s.Read(span, ref index, endian);
                instance.UnknownA8s.Read(span, ref index, endian);
                instance.CitizenFirstNames.Read(span, ref index, endian);
                instance.CitizenLastNames.Read(span, ref index, endian);
                span.SkipPadding(ref index, 8);
                instance.UnknownC8s.Read(span, ref index, endian);
                instance.UnknownD0s.Read(span, ref index, endian);
                instance.UnknownD8s.Read(span, ref index, endian);
            }
            return instance;
        }

        internal static void Write(FileHeader instance, IArrayBufferWriter<byte> writer, IItemLabeler labeler)
        {
            var endian = instance.Endian;
            writer.WriteValueU32(Signature, endian);
            writer.WriteValueU16((ushort)0xFFFEu, endian);
            writer.WriteValueU16(0, endian);
            instance.Items.Write(writer, labeler, endian);
            instance.WeaponFacilityRecipes.Write(writer, labeler, endian);
            instance.MunitionFacilityRecipes.Write(writer, labeler, endian);
            instance.MedicalFacilityRecipes.Write(writer, labeler, endian);
            instance.FieldItemLotTables.Write(writer, labeler, endian);
            for (int i = 0; i < WeaponTypeCount; i++)
            {
                instance.WeaponUpgradeRecipesByType[i].Header.CountLabel = writer.WritePointer(labeler);
            }
            for (int i = 0; i < WeaponTypeCount; i++)
            {
                instance.WeaponUpgradeRecipesByType[i].Header.OffsetLabel = writer.WritePointer(labeler);
            }
            instance.Unknown70s.Write(writer, labeler, endian);
            instance.RewardCitizenLots.Write(writer, labeler, endian);
            instance.Unknown80s.Write(writer, labeler, endian);
            instance.Unknown88s.Write(writer, labeler, endian);
            instance.Unknown90s.Write(writer, labeler, endian);
            instance.Unknown98s.Write(writer, labeler, endian);
            instance.UnknownA0s.Write(writer, labeler, endian);
            instance.UnknownA8s.Write(writer, labeler, endian);
            instance.CitizenFirstNames.Write(writer, labeler, endian);
            instance.CitizenLastNames.Write(writer, labeler, endian);
            writer.SkipPadding(8);
            instance.UnknownC8s.Write(writer, labeler, endian);
            instance.UnknownD0s.Write(writer, labeler, endian);
            instance.UnknownD8s.Write(writer, labeler, endian);
        }

        internal void Write(IArrayBufferWriter<byte> writer, IItemLabeler labeler)
        {
            Write(this, writer, labeler);
        }
    }
}
