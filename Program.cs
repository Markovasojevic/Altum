using System.Text;

internal class Program
{
	//Open in terminal and run the following command: dotnet run
	//this will generate a file with 1 million lines and then sort it
	//the sorted file will be saved as sorted_output.txt
	//the original file will be saved as output.txt
	//the file will contain random numbers and strings
	//the strings will be sorted in ascending order
	static void Main(string[] args)
	{
		GenerateTestFile("output.txt", 1000000);
		SortLargeFile("output.txt", "sorted_output.txt");
	}

	static void GenerateTestFile(string outputFilePath, int numberOfLines)
	{
		string[] sampleStrings = [
			"Apple",
		"Cherry is the best",
		"Banana is yellow",
		"Something something something"
		];

		var random = new Random();
		var sb = new StringBuilder();

		using (StreamWriter writer = new(outputFilePath))
		{
			for (int i = 0; i < numberOfLines; i++)
			{
				var number = random.Next(1, 100000);
				var text = sampleStrings[random.Next(sampleStrings.Length)];
				sb.AppendLine($"{number}. {text}");
			}
			writer.Write(sb.ToString());
		}

		Console.WriteLine($"File generated at {outputFilePath} with {numberOfLines} lines.");
	}

	static void SortLargeFile(string inputFilePath, string outputFilePath)
	{
		var chunkSize = 100000; // Number of lines per chunk
		var tempFiles = new List<string>();

		// Step 1: Split the large file into smaller sorted chunks
		using (StreamReader reader = new(inputFilePath))
		{
			var lines = new List<(string Text, int Number)>();
			var line = string.Empty;
			var chunkIndex = 0;

			while ((line = reader.ReadLine()) != null)
			{
				var dotIndex = line.IndexOf('.');
				if (dotIndex > 0)
				{
					var number = int.Parse(line[..dotIndex]);
					var text = line[(dotIndex + 2)..];
					lines.Add((text, number));
				}

				if (lines.Count >= chunkSize)
				{
					var tempFilePath = $"temp_chunk_{chunkIndex++}.txt";
					WriteSortedChunk(lines, tempFilePath);
					tempFiles.Add(tempFilePath);
					lines.Clear();
				}
			}

			// Write the remaining lines
			if (lines.Count > 0)
			{
				var tempFilePath = $"temp_chunk_{chunkIndex++}.txt";
				WriteSortedChunk(lines, tempFilePath);
				tempFiles.Add(tempFilePath);
			}
		}

		// Step 2: Merge the sorted chunks
		MergeSortedChunks(tempFiles, outputFilePath);

		// Clean up temporary files
		foreach (var tempFile in tempFiles)
		{
			File.Delete(tempFile);
		}

		Console.WriteLine($"File sorted and saved to {outputFilePath}");
	}

	static void WriteSortedChunk(List<(string Text, int Number)> lines, string filePath)
	{
		var sortedLines = lines.OrderBy(x => x.Text).ThenBy(x => x.Number);
		using StreamWriter writer = new(filePath);
		foreach (var line in sortedLines)
		{
			writer.WriteLine($"{line.Number}. {line.Text}");
		}
	}

	static void MergeSortedChunks(List<string> tempFiles, string outputFilePath)
	{
		var readers = tempFiles.Select(file => new StreamReader(file)).ToList();
		var priorityQueue = new SortedDictionary<(string Text, int Number), int>();

		// Initialize the priority queue with the first line from each file
		for (int i = 0; i < readers.Count; i++)
		{
			if (readers[i].Peek() >= 0)
			{
				var line = readers[i].ReadLine();
				if (!string.IsNullOrEmpty(line))
				{
					var dotIndex = line.IndexOf('.');
					var number = int.Parse(line[..dotIndex]);
					var text = line[(dotIndex + 2)..];
					if (!priorityQueue.Any(s => s.Key == (text, number)))
					{
						priorityQueue.Add((text, number), i);
					}
				}
			}
		}

		using (StreamWriter writer = new(outputFilePath))
		{
			while (priorityQueue.Count > 0)
			{
				var minEntry = priorityQueue.First();
				priorityQueue.Remove(minEntry.Key);

				writer.WriteLine($"{minEntry.Key.Number}. {minEntry.Key.Text}");

				var fileIndex = minEntry.Value;
				if (readers[fileIndex].Peek() >= 0)
				{
					var line = readers[fileIndex]?.ReadLine();
					if (!string.IsNullOrEmpty(line))
					{
						var dotIndex = line.IndexOf('.');
						var number = int.Parse(line[..dotIndex]);
						var text = line[(dotIndex + 2)..];
						if (!priorityQueue.Any(s => s.Key == (text, number)))
						{
							priorityQueue.Add((text, number), fileIndex);
						}
					}
				}
			}
		}

		foreach (var reader in readers)
		{
			reader.Dispose();
		}
	}
}
