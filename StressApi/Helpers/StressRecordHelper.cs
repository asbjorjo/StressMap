using StressData.Model;
using System;
using System.Collections.Generic;
using static StressData.Types.StressTypes;

namespace StressApi.Helpers
{
    public class StressRecordHelper
    {
        public static readonly Dictionary<RecordType, string> mapTypeForStyle = new Dictionary<RecordType, string>() 
        {
            { RecordType.BO,  "BO"  },
            { RecordType.BOC, "BO"  },
            { RecordType.BOT, "BO"  },
            { RecordType.BS,  "BS"  },
            { RecordType.DIF, "DIF" },
            { RecordType.FMA, "FMS" },
            { RecordType.FMF, "FMS" },
            { RecordType.FMS, "FMS" },
            { RecordType.GFI, "GI"  },
            { RecordType.GFM, "GI"  },
            { RecordType.GFS, "GI"  },
            { RecordType.GVA, "GI"  },
            { RecordType.HF,  "HF"  },
            { RecordType.HFG, "HF"  },
            { RecordType.HFM, "HF"  },
            { RecordType.HFP, "HF"  },
            { RecordType.OC,  "OC"  },
            { RecordType.PC,  "GI"  }
        };
        public static readonly Dictionary<RegimeType, string> mapRegimeForStyle = new Dictionary<RegimeType, string>()
        {
            { RegimeType.NF, "NF" },
            { RegimeType.NS, "NF" },
            { RegimeType.SS, "SS" },
            { RegimeType.TF, "TF" },
            { RegimeType.TS, "TF" },
            { RegimeType.U,  "U"  }
        };

        internal static object StressRecordStyle(StressRecord record)
        {
            string styleType = "";
            string styleRegime = "";
            if (! mapTypeForStyle.TryGetValue(record.Type, out styleType))
            {
                styleType = record.Type.ToString();
            }
            if (! mapRegimeForStyle.TryGetValue(record.Regime, out styleRegime))
            {
                styleRegime = record.Regime.ToString();
            }

            return styleType + "-" + styleRegime;
        }
    }
}
