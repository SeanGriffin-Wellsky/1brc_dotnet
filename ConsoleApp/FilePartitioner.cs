namespace ConsoleApp;

public readonly record struct Partition(long Pos, int Length);

public static class FilePartitioner
{
    public static List<Partition> PartitionFile(FileStream stream, int partitionSize)
    {
        var fileLength = stream.Length;
        var numPartitions = (int) (fileLength / partitionSize);
        var partitions = new List<Partition>(numPartitions + 1);

        for (var i = 1L; i <= numPartitions; ++i)
        {
            var startPos = stream.Position;
            stream.Position = i * partitionSize;

            int b;
            do
            {
                b = stream.ReadByte();
            } while (b != -1 && b != Constants.NewLine);

            partitions.Add(new Partition(startPos, (int) (stream.Position - startPos)));
        }

        if (stream.Position < fileLength)
        {
            partitions.Add(new Partition(stream.Position, (int) (fileLength - stream.Position)));
        }

        return partitions;
    }
}