using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScratchGenerator
{

    // Program.cs
    class Program
    {
        static void Main()
        {
            int totalTickets = 15000;
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var generator = new LotteryTicketGenerator(totalTickets);
            string outputFile = "lottery_tickets.txt";
            generator.GenerateTickets(outputFile);

            stopwatch.Stop();
            TimeSpan totalProcessingTime = stopwatch.Elapsed;

            Console.WriteLine("\nĐã tạo và ghi vé ra file txt!");
            Console.WriteLine("\n--- Đo lường Performance ---");
            Console.WriteLine($"Tổng thời gian xử lý: {totalProcessingTime.TotalSeconds:F2} seconds");

            // Detailed timing breakdown
            Console.WriteLine($"Detailed Time: {(int)totalProcessingTime.TotalHours} hours, " +
                             $"{totalProcessingTime.Minutes} minutes, " +
                             $"{totalProcessingTime.Seconds + totalProcessingTime.Milliseconds / 1000.0:F2} seconds");

            // Calculate processing rate
            double ticketsPerSecond = totalTickets / totalProcessingTime.TotalSeconds;
            Console.WriteLine($"Tốc độ xử lý: {ticketsPerSecond:F2} vé/s");

            // Verify distribution
            generator.VerifyDistribution(outputFile);

            // Prevent console window from closing immediately
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();  // This will wait for a key press before closing
            Console.ReadKey();  // This will wait for a key press before closing
        }
    }
}
