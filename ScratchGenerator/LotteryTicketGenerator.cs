// LotteryTicketGenerator.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

public class LotteryTicketGenerator
{
    private readonly int totalTickets;
    private readonly int total10kTickets;
    private readonly int total20kTickets;
    private readonly int total50kTickets;
    private readonly int noPrizeTickets;
    private readonly Dictionary<string, int> ticketDistribution;
    private readonly Random random;

    public LotteryTicketGenerator(int totalTickets = 150000)
    {
        this.totalTickets = totalTickets;
        this.total10kTickets = totalTickets / 6;       // 250,000 / 1,500,000 in reality
        this.total20kTickets = totalTickets * 3 / 40;  // 112,500 / 1,500,000 in reality
        this.total50kTickets = totalTickets / 50;      // 30,000 / 1,500,000 in reality

        this.ticketDistribution = new Dictionary<string, int>
        {
            { "10K", total10kTickets },
            { "20K", total20kTickets },
            { "50K", total50kTickets }
        };

        // Validate ticket count
        int prizeTicketCount = ticketDistribution.Values.Sum();
        if (prizeTicketCount > totalTickets)
        {
            throw new ArgumentException("Prize tickets exceed total tickets");
        }

        // Remaining tickets are no-prize
        this.noPrizeTickets = totalTickets - prizeTicketCount;
        this.random = new Random(42); // Fixed seed for reproducibility
    }

    private List<int> GenerateUniqueNumbers(int count, int minVal = 1, int maxVal = 60)
    {
        return Enumerable.Range(minVal, maxVal - minVal + 1)
                        .OrderBy(x => random.Next())
                        .Take(count)
                        .ToList();
    }

    private List<string> FormatNumbers(List<int> numbers)
    {
        return numbers.Select(num => num.ToString("D2")).ToList();
    }

    private bool CheckWinningCondition(List<int> first4, List<int> last16)
    {
        return first4.Any(num => last16.Contains(num));
    }

    public void GenerateTickets(string outputFile = "lottery_tickets.txt")
    {
        var generatedTickets = new HashSet<string>();
        var listGeneratedTickets = new List<string>();
        var ticketTypeCounters = new Dictionary<string, int>
        {
            { "10K", 0 },
            { "20K", 0 },
            { "50K", 0 },
            { "NO_PRIZE", 0 }
        };

        var sw = new Stopwatch();
        sw.Start();

        for (int ticketId = 0; ticketId < totalTickets; ticketId++)
        {
            while (true)
            {
                var first4 = GenerateUniqueNumbers(4);
                var last16 = GenerateUniqueNumbers(16);
                var first4Formatted = FormatNumbers(first4);
                var last16Formatted = FormatNumbers(last16);

                bool isWinning = CheckWinningCondition(first4, last16);
                var ticketNumbersList = first4Formatted.Concat(last16Formatted).ToList();
                string ticketKey = string.Join(" ", ticketNumbersList);

                if (!generatedTickets.Contains(ticketKey))
                {
                    generatedTickets.Add(ticketKey);
                    string ticketType = DetermineTicketType(isWinning, ticketTypeCounters);

                    if (ticketType != null)
                    {
                        string currentTicketStr = $"{string.Join(" ", ticketNumbersList)} {ticketType}";
                        listGeneratedTickets.Add(currentTicketStr);
                        break;
                    }
                }
            }
        }

        // Shuffle tickets
        var shuffleTime = new Stopwatch();
        shuffleTime.Start();
        listGeneratedTickets = listGeneratedTickets.OrderBy(x => random.Next()).ToList();
        shuffleTime.Stop();
        Console.WriteLine($"\nThời gian xáo thứ tự vé: {shuffleTime.Elapsed.TotalSeconds:F2} seconds");

        // Write to file
        using (StreamWriter file = new StreamWriter(outputFile))
        {
            for (int idx = 0; idx < listGeneratedTickets.Count; idx++)
            {
                file.WriteLine($"ID {(idx + 1):D7}: {listGeneratedTickets[idx]}");
            }
        }

        // Print distribution
        Console.WriteLine("Cơ cấu giải thưởng sau khi tạo vé:");
        foreach (var kvp in ticketTypeCounters)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value} tickets");
        }
    }

    private string DetermineTicketType(bool isWinning, Dictionary<string, int> ticketTypeCounters)
    {
        if (isWinning)
        {
            if (ticketTypeCounters["10K"] < ticketDistribution["10K"])
            {
                ticketTypeCounters["10K"]++;
                return "10K";
            }
            if (ticketTypeCounters["20K"] < ticketDistribution["20K"])
            {
                ticketTypeCounters["20K"]++;
                return "20K";
            }
            if (ticketTypeCounters["50K"] < ticketDistribution["50K"])
            {
                ticketTypeCounters["50K"]++;
                return "50K";
            }
        }

        if (ticketTypeCounters["NO_PRIZE"] < noPrizeTickets)
        {
            ticketTypeCounters["NO_PRIZE"]++;
            return "NO_PRIZE";
        }

        return null;
    }

    public void VerifyDistribution(string outputFile = "lottery_tickets.txt")
    {
        var ticketTypeCounts = new Dictionary<string, int>
        {
            { "10K", 0 },
            { "20K", 0 },
            { "50K", 0 },
            { "NO_PRIZE", 0 }
        };

        foreach (string line in File.ReadLines(outputFile))
        {
            string ticketType = line.Split(' ').Last();
            ticketTypeCounts[ticketType]++;
        }

        Console.WriteLine("\nKết quả đếm kiểm thử:");
        Console.WriteLine($"Số lượng vé 10000 VND: {ticketTypeCounts["10K"]} (Kỳ vọng: {total10kTickets})");
        Console.WriteLine($"Số lượng vé 20000 VND: {ticketTypeCounts["20K"]} (Kỳ vọng: {total20kTickets})");
        Console.WriteLine($"Số lượng vé 50000 VND: {ticketTypeCounts["50K"]} (Kỳ vọng: {total50kTickets})");
        Console.WriteLine($"Số lượng vé không trúng: {ticketTypeCounts["NO_PRIZE"]} (Kỳ vọng: {noPrizeTickets})");

        // Verify distribution
        Debug.Assert(ticketTypeCounts["10K"] == total10kTickets, "10K ticket count incorrect");
        Debug.Assert(ticketTypeCounts["20K"] == total20kTickets, "20K ticket count incorrect");
        Debug.Assert(ticketTypeCounts["50K"] == total50kTickets, "50K ticket count incorrect");
        Debug.Assert(ticketTypeCounts["NO_PRIZE"] == noPrizeTickets, "No-prize ticket count incorrect");
    }
}
