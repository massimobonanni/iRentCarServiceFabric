using System;
using System.Collections.Generic;
using System.Text;

namespace System.Fabric
{
    public static class FabricExtensions
    {
        public static bool GetPartitionRange(this IStatefulServicePartition partition, out long lowKey, out long highKey)
        {
            var result = false;
            lowKey = long.MinValue;
            highKey = long.MaxValue;
            if (partition?.PartitionInfo is Int64RangePartitionInformation)
            {
                var partitionInfo = (Int64RangePartitionInformation)partition.PartitionInfo;
                lowKey = partitionInfo.LowKey;
                highKey = partitionInfo.HighKey;
                result = true;
            }
            return result;
        }
    }
}
