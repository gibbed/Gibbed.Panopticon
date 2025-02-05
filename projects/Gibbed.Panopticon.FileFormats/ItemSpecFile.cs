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
using Gibbed.Panopticon.FileFormats.ItemSpecs;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gibbed.Panopticon.FileFormats
{
    using IItemLabeler = ILabeler<StringPool>;

    public class ItemSpecFile
    {
        public const int WeaponTypeCount = FileHeader.WeaponTypeCount;

        private readonly UpgradeRecipeSpec[][] _WeaponUpgradeRecipesByType;

        public ItemSpecFile()
        {
            this._WeaponUpgradeRecipesByType = new UpgradeRecipeSpec[WeaponTypeCount][];
        }

        [JsonConstructor]
        private ItemSpecFile(UpgradeRecipeSpec[][] weaponUpgradeRecipesByType)
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
            Array.Copy(weaponUpgradeRecipesByType, this._WeaponUpgradeRecipesByType, WeaponTypeCount);
        }

        [JsonProperty("endian")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Endian Endian { get; set; }

        [JsonProperty("version")]
        [JsonConverter(typeof(StringEnumConverter))]
        public GameVersion Version { get; set; }

        [JsonProperty("items")]
        public ItemSpec[] Items { get; set; }

        [JsonProperty("weapon_facility_recipes")]
        public CraftRecipeSpec[] WeaponFacilityRecipes { get; set; }

        [JsonProperty("munition_facility_recipes")]
        public ProductionRecipeSpec[] MunitionFacilityRecipes { get; set; }

        [JsonProperty("medical_facility_recipes")]
        public ProductionRecipeSpec[] MedicalFacilityRecipes { get; set; }

        [JsonProperty("field_item_lot_tables")]
        public FieldItemLotTableSpec[] FieldItemLotTables { get; set; }

        [JsonProperty("weapon_upgrade_recipes_by_type", ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public UpgradeRecipeSpec[][] WeaponUpgradeRecipesByType => this._WeaponUpgradeRecipesByType;

        [JsonProperty("unknown70")]
        public Unknown70Spec[] Unknown70s { get; set; }

        [JsonProperty("reward_citizen_lots")]
        public RewardCitizenLotSpec[] RewardCitizenLots { get; set; }

        [JsonProperty("unknown80")]
        public Unknown80Spec[] Unknown80s { get; set; }

        [JsonProperty("unknown88")]
        public Unknown88Spec[] Unknown88s { get; set; }

        [JsonProperty("unknown90")]
        public Unknown90Spec[] Unknown90s { get; set; }

        [JsonProperty("unknown98")]
        public Unknown98Spec[] Unknown98s { get; set; }

        [JsonProperty("unknownA0")]
        public UnknownA0Spec[] UnknownA0s { get; set; }

        [JsonProperty("unknownA8")]
        public UnknownA8Spec[] UnknownA8s { get; set; }

        [JsonProperty("citizen_first_names")]
        public CitizenFirstNameSpec[] CitizenFirstNames { get; set; }
        
        [JsonProperty("citizen_last_names")]
        public CitizenLastNameSpec[] CitizenLastNames { get; set; }

        [JsonProperty("unknownC8")]
        public UnknownC8Spec[] UnknownC8s { get; set; }

        [JsonProperty("unknownD0")]
        public UnknownD0Spec[] UnknownD0s { get; set; }

        [JsonProperty("unknownD8")]
        public UnknownD8Spec[] UnknownD8s { get; set; }

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

        public void Save(IArrayBufferWriter<byte> writer)
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
