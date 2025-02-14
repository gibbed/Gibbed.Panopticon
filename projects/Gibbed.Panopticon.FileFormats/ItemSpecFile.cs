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
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Gibbed.Buffers;
using Gibbed.Memory;
using Gibbed.Panopticon.FileFormats.ItemSpecs;

namespace Gibbed.Panopticon.FileFormats
{
    using IItemLabeler = ILabeler<StringPool>;

    public class ItemSpecFile : BaseSpecFile
    {
        public const uint Signature = FileHeader.Signature;

        public override SpecFileType Type => SpecFileType.Item;

        [JsonIgnore]
        public override string FileExtension => SpecFileExtensions.Item;

        public const int WeaponTypeCount = FileHeader.WeaponTypeCount;

        private readonly List<UpgradeRecipeSpec>[] _WeaponUpgradeRecipesByType;

        public ItemSpecFile()
        {
            this._WeaponUpgradeRecipesByType = new List<UpgradeRecipeSpec>[WeaponTypeCount];
            for (int i = 0; i < WeaponTypeCount; i++)
            {
                this._WeaponUpgradeRecipesByType[i] = new();
            }
        }

        [JsonConstructor]
#pragma warning disable IDE0051 // Remove unused private members
        private ItemSpecFile(List<UpgradeRecipeSpec>[] weaponUpgradeRecipesByType)
#pragma warning restore IDE0051 // Remove unused private members
            : this()
        {
            if (weaponUpgradeRecipesByType == null)
            {
                throw new ArgumentNullException(nameof(weaponUpgradeRecipesByType));
            }
            if (weaponUpgradeRecipesByType.Length != WeaponTypeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(weaponUpgradeRecipesByType));
            }
            for (int i = 0; i < WeaponTypeCount; i++)
            {
                this._WeaponUpgradeRecipesByType[i].AddRange(weaponUpgradeRecipesByType[i]);
            }
        }

        [JsonPropertyName("endian")]
        public Endian Endian { get; set; }

        [JsonPropertyName("version")]
        public GameVersion Version { get; set; }

        [JsonPropertyName("items")]
        public List<ItemSpec> Items { get; set; }

        [JsonPropertyName("weapon_facility_recipes")]
        public List<CraftRecipeSpec> WeaponFacilityRecipes { get; set; }

        [JsonPropertyName("munition_facility_recipes")]
        public List<ProductionRecipeSpec> MunitionFacilityRecipes { get; set; }

        [JsonPropertyName("medical_facility_recipes")]
        public List<ProductionRecipeSpec> MedicalFacilityRecipes { get; set; }

        [JsonPropertyName("field_item_lot_tables")]
        public List<FieldItemLotTableSpec> FieldItemLotTables { get; set; }

        [JsonPropertyName("weapon_upgrade_recipes_by_type")]
        public List<UpgradeRecipeSpec>[] WeaponUpgradeRecipesByType => this._WeaponUpgradeRecipesByType;

        [JsonPropertyName("unknown70")]
        public List<Unknown70Spec> Unknown70s { get; set; }

        [JsonPropertyName("reward_citizen_lots")]
        public List<RewardCitizenLotSpec> RewardCitizenLots { get; set; }

        [JsonPropertyName("unknown80")]
        public List<Unknown80Spec> Unknown80s { get; set; }

        [JsonPropertyName("unknown88")]
        public List<Unknown88Spec> Unknown88s { get; set; }

        [JsonPropertyName("unknown90")]
        public List<Unknown90Spec> Unknown90s { get; set; }

        [JsonPropertyName("unknown98")]
        public List<Unknown98Spec> Unknown98s { get; set; }

        [JsonPropertyName("unknownA0")]
        public List<UnknownA0Spec> UnknownA0s { get; set; }

        [JsonPropertyName("unknownA8")]
        public List<UnknownA8Spec> UnknownA8s { get; set; }

        [JsonPropertyName("citizen_first_names")]
        public List<CitizenFirstNameSpec> CitizenFirstNames { get; set; }
        
        [JsonPropertyName("citizen_last_names")]
        public List<CitizenLastNameSpec> CitizenLastNames { get; set; }

        [JsonPropertyName("unknownC8")]
        public List<UnknownC8Spec> UnknownC8s { get; set; }

        [JsonPropertyName("unknownD0")]
        public List<UnknownD0Spec> UnknownD0s { get; set; }

        [JsonPropertyName("unknownD8")]
        public List<UnknownD8Spec> UnknownD8s { get; set; }

        public static ItemSpecFile Load(ReadOnlySpan<byte> span)
        {
            int index = 0;
            var header = FileHeader.Read(span, ref index);
            var endian = header.Endian;
            var version = header.Version;
            ItemSpecFile instance = new();
            instance.Endian = endian;
            instance.Version = version;
            instance.Items = header.Items.LoadTable(span, version, endian);
            instance.WeaponFacilityRecipes = header.WeaponFacilityRecipes.LoadTable(span, version, endian);
            instance.MunitionFacilityRecipes = header.MunitionFacilityRecipes.LoadTable(span, version, endian);
            instance.MedicalFacilityRecipes = header.MedicalFacilityRecipes.LoadTable(span, version, endian);
            instance.FieldItemLotTables = header.FieldItemLotTables.LoadTable(span, version, endian);
            for (int i = 0; i < FileHeader.WeaponTypeCount; i++)
            {
                instance._WeaponUpgradeRecipesByType[i] = header.WeaponUpgradeRecipesByType[i].LoadTable(span, version, endian);
            }
            instance.Unknown70s = header.Unknown70s.LoadTable(span, version, endian);
            instance.RewardCitizenLots = header.RewardCitizenLots.LoadTable(span, version, endian);
            instance.Unknown80s = header.Unknown80s.LoadTable(span, version, endian);
            instance.Unknown88s = header.Unknown88s.LoadTable(span, version, endian);
            instance.Unknown90s = header.Unknown90s.LoadTable(span, version, endian);
            instance.Unknown98s = header.Unknown98s.LoadTable(span, version, endian);
            if (version == GameVersion.Remaster)
            {
                instance.UnknownA0s = header.UnknownA0s.LoadTable(span, version, endian);
                instance.UnknownA8s = header.UnknownA8s.LoadTable(span, version, endian);
                instance.CitizenFirstNames = header.CitizenFirstNames.LoadTable(span, version, endian);
                instance.CitizenLastNames = header.CitizenLastNames.LoadTable(span, version, endian);
                instance.UnknownC8s = header.UnknownC8s.LoadTable(span, version, endian);
                instance.UnknownD0s = header.UnknownD0s.LoadTable(span, version, endian);
                instance.UnknownD8s = header.UnknownD8s.LoadTable(span, version, endian);
            }
            return instance;
        }

        private void Save(IArrayBufferWriter<byte> writer, IItemLabeler labeler)
        {
            FileHeader header = new();
            var endian = header.Endian = this.Endian;
            var version = header.Version = this.Version;
            header.Write(writer, labeler);

            header.Items.SaveTable(this.Items, writer, labeler, version, endian);
            header.WeaponFacilityRecipes.SaveTable(this.WeaponFacilityRecipes, writer, labeler, version, endian);
            header.MunitionFacilityRecipes.SaveTable(this.MunitionFacilityRecipes, writer, labeler, version, endian);
            header.MedicalFacilityRecipes.SaveTable(this.MedicalFacilityRecipes, writer, labeler, version, endian);
            for (int i = 0; i < FileHeader.WeaponTypeCount; i++)
            {
                header.WeaponUpgradeRecipesByType[i].SaveTable(this.WeaponUpgradeRecipesByType[i], writer, labeler, version, endian);
            }
            header.FieldItemLotTables.SaveTable(this.FieldItemLotTables, writer, labeler, version, endian);
            header.Unknown70s.SaveTable(this.Unknown70s, writer, labeler, version, endian);
            header.RewardCitizenLots.SaveTable(this.RewardCitizenLots, writer, labeler, version, endian);
            header.Unknown80s.SaveTable(this.Unknown80s, writer, labeler, version, endian);
            header.Unknown88s.SaveTable(this.Unknown88s, writer, labeler, version, endian);
            header.Unknown90s.SaveTable(this.Unknown90s, writer, labeler, version, endian);
            header.Unknown98s.SaveTable(this.Unknown98s, writer, labeler, version, endian);
            if (version == GameVersion.Remaster)
            {
                header.UnknownA0s.SaveTable(this.UnknownA0s, writer, labeler, version, endian);
                header.UnknownA8s.SaveTable(this.UnknownA8s, writer, labeler, version, endian);
                header.CitizenFirstNames.SaveTable(this.CitizenFirstNames, writer, labeler, version, endian);
                header.CitizenLastNames.SaveTable(this.CitizenLastNames, writer, labeler, version, endian);
                header.UnknownC8s.SaveTable(this.UnknownC8s, writer, labeler, version, endian);
                header.UnknownD0s.SaveTable(this.UnknownD0s, writer, labeler, version, endian);
                header.UnknownD8s.SaveTable(this.UnknownD8s, writer, labeler, version, endian);
            }
        }

        public override void Save(IArrayBufferWriter<byte> writer)
        {
            var endian = this.Endian;
            PooledArrayBufferWriter<byte> buffer = new();
            Labeler labeler = new();
            Save(buffer, labeler);
            var bytes = buffer.WrittenSpan.ToArray();
            buffer.Clear();
            labeler.Fixup(bytes, out var stringBytes, endian);
            writer.WriteBytes(bytes);
            writer.WriteBytes(stringBytes);
        }
    }
}
