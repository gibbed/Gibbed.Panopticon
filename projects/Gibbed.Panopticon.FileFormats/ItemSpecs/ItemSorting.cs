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

namespace Gibbed.Panopticon.FileFormats.ItemSpecs
{
    internal static class ItemSorting
    {
        public static int Sort(ItemSpec x, ItemSpec y)
        {
            foreach (var result in SortInternal(x, y))
            {
                if (result != 0)
                {
                    return result;
                }
            }
            return 0;
        }

        private static IEnumerable<int> SortInternal(ItemSpec x, ItemSpec y)
        {
            yield return TypePriority(x, y);
            yield return MainWeaponPriority(x, y);
            yield return BoosterPriority(x, y);
            yield return CitizenPriority(x, y);
            yield return x.Id.CompareTo(y.Id);
        }

        private static int TypePriority(ItemSpec x, ItemSpec y)
        {
            return TypePriority(x.Type).CompareTo(TypePriority(y.Type));
        }

        private static int TypePriority(ItemType type) => type switch
        {
            ItemType.MainWeapon => 0,
            ItemType.SubWeapon => 1,
            ItemType.ResourceLocal => 2,
            ItemType.Booster => 3,
            ItemType.ResourceField => 4,
            ItemType.ResourceArtificial => 5,
            ItemType.ResourceHeaven => 5,
            ItemType.ResourceSimple => 6,
            ItemType.Citizen => 7,
            ItemType.Key => 8,
            ItemType.Modular => 9,
            _ => int.MaxValue,
        };

        private static int MainWeaponPriority(ItemSpec x, ItemSpec y)
        {
            if (x.Type != ItemType.MainWeapon || y.Type != ItemType.MainWeapon)
            {
                return 0;
            }
            if (x.Id.Length < 5 || y.Id.Length < 5)
            {
                return 0;
            }
            var xPrefix = x.Id.Substring(0, 5);
            var yPrefix = y.Id.Substring(0, 5);
            var xIndex = Array.IndexOf(MainWeaponPrefixOrder, xPrefix);
            var yIndex = Array.IndexOf(MainWeaponPrefixOrder, yPrefix);
            if (xIndex < 0 || yIndex < 0)
            {
                return 0;
            }
            return xIndex.CompareTo(yIndex);
        }

        private static readonly string[] MainWeaponPrefixOrder = new string[]
        {
            "WG1_0",
            "WG2_0",
            "WG3_0",
            "WG4_0",
            "WS1_0",
            "WS2_0",
            "WS3_0",
            "WS4_0",
            "WG1_1",
            "WG2_1",
            "WG3_1",
            "WG4_1",
            "WS1_1",
            "WS2_1",
            "WS3_1",
        };

        private static int BoosterPriority(ItemSpec x, ItemSpec y)
        {
            if (x.Type != ItemType.Booster || y.Type != ItemType.Booster)
            {
                return 0;
            }
            var xIndex = Array.IndexOf(BoosterOrder, x.Id);
            var yIndex = Array.IndexOf(BoosterOrder, y.Id);
            if (xIndex < 0 || yIndex < 0)
            {
                return 0;
            }
            return xIndex.CompareTo(yIndex);
        }

        private static readonly string[] BoosterOrder = new string[]
        {
            "B_HLT00",
            "B_HLT01",
            "B_HLT02",
            "B_HLR00",
            "B_HLR02",
            "B_HLR03",
            "B_HLR01",
            "B_POW00",
            "B_POW01",
            "B_POW06",
            "B_POW02",
            "B_POW03",
            "B_POW04",
            "B_POW05",
            "B_PHU00",
            "B_PAC00",
            "B_PAB00",
            "B_PCS00",
            "B_PPX00",
            "B_PDO00",
            "B_PAL00",
            "B_PEL00",
            "B_PEL01",
            "B_PEL05",
            "B_PEL02",
            "B_PEL03",
            "B_PEL04",
            "B_DEF00",
            "B_DEF01",
            "B_DEF05",
            "B_DEF02",
            "B_DEF03",
            "B_DEF04",
            "B_DML00",
            "B_DRG00",
            "B_DHU00",
            "B_DAC00",
            "B_DAB00",
            "B_DCS00",
            "B_DPX00",
            "B_DDO00",
            "B_DAL00",
            "B_IPR00",
            "B_IBI00",
            "B_IFB00",
            "B_IJM00",
            "B_IPS00",
            "B_IAL00",
            "B_IAL01",
            "B_ITP00",
            "B_IHW00",
            "B_DOD00",
            "B_SUM00",
            "B_SUM01",
            "B_SUM02",
            "B_SUC00",
            "B_SUJ00",
            "B_SUH00",
            "B_TLN00",
            "B_TLN01",
            "B_TLN02",
            "B_TRC00",
            "B_TRC01",
            "B_TRC02",
            "B_TCS00",
            "B_TCS01",
            "B_TCS02",
            "B_RSE00",
            "B_RDE00",
            "B_RRE00",
            "B_PDN00",
            "B_PDN01",
            "B_PDN02",
            "B_EDT00",
            "B_TRD00",
            "B_NDL00",
            "B_NDL01",
            "B_ABH00",
            "B_ABH07",
            "B_ABH01",
            "B_ABH02",
            "B_ABH03",
            "B_ABH09",
            "B_ABH04",
            "B_ABH05",
            "B_ABH06",
            "B_SZF00",
            "B_SZF01",
            "B_SZF02",
            "B_DSK00",
            "B_RDP00",
        };

        private static int CitizenPriority(ItemSpec x, ItemSpec y)
        {
            if (x.Type != ItemType.Citizen || y.Type != ItemType.Citizen)
            {
                return 0;
            }
            var xIndex = Array.IndexOf(CitizenOrder, x.Id);
            var yIndex = Array.IndexOf(CitizenOrder, y.Id);
            if (xIndex < 0 || yIndex < 0)
            {
                return 0;
            }
            return xIndex.CompareTo(yIndex);
        }

        private static readonly string[] CitizenOrder = new string[]
        {
            "CIT_0000",
            "CIT_0001",
            "CIT_0002",
            "CIT_0003",
            "CIT_0004",
            "CIT_0005",
            "CIT_0006",
            "CIT_0007",
            "CIT_0008",
            "CIT_0009",
            "CIT_0010",
            "CIT_0011",
            "CIT_0012",
            "CIT_0013",
            "CIT_0014",
            "CIT_0015",
            "CIT_0016",
            "CIT_0017",
            "CIT_0018",
            "CIT_0019",
            "CIT_0020",
            "CIT_0021",
            "CIT_0022",
            "CIT_0023",
            "CIT_0024",
            "CIT_0025",
            "CIT_0026",
            "CIT_0027",
            "CIT_0028",
            "CIT_0029",
            "CIT_0030",
            "CIT_0031",
            "CIT_0032",
            "CIT_0033",
            "CIT_0034",
            "CIT_0035",
            "CIT_0036",
            "CIT_0037",
            "CIT_0038",
            "CIT_0039",
            "CIT_0040",
            "CIT_0041",
            "CIT_0042",
            "CIT_0043",
            "CIT_0044",
            "CIT_0045",
            "CIT_0046",
            "CIT_0047",
            "CIT_0048",
            "CIT_0049",
            "CIT_1000",
            "CIT_1001",
            "CIT_2000",
            "CIT_2001",
            "CIT_3000",
            "CIT_3001",
            "CIT_4000",
            "CIT_4001",
            "CIT_5000",
            "CIT_5001",
            "CIT_6000",
            "CIT_6001",
            "CIT_7000",
            "CIT_7001",
            "CIT_8000",
            "CIT_8001",
            "CIT_4100",
            "CIT_9000",
            "CIT_9001",
            "CIT_9002",
            "CIT_9003",
            "CIT_9999",
        };
    }
}
