// LotteryTicketGenerator.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace ScratchGenerator
{

    public class LotteryTicketGenerator
    {
        //private readonly string[] _groupKyTuTrungRieng = { "t", "m", "w", "y", "j", "a" };
        //private readonly string[] _groupKyTuTrungChung = { "c", "b", "n", "r", "s" };
        //private readonly string[] _groupKyTuKhongTrung = { "z", "x", "p", "i", "f", "h" };
        private readonly List<char> _groupKyTuTrungRieng = new List<char> { 't', 'm', 'w', 'y', 'j', 'a' };
        private readonly List<char> _groupKyTuTrungChung = new List<char> { 'c', 'b', 'n', 'r', 's' };
        private readonly List<char> _groupKyTuKhongTrung = new List<char> { 'z', 'x', 'p', 'i', 'f', 'h' };

        private readonly Dictionary<string, string> _prizeToPrefix = new Dictionary<string, string>
        {
            { "10K", "t" },
            { "20K", "m" },
            { "50K", "w" },
            { "100K", "y" },
            { "200K", "j" },
            { "500K", "a" }
        };
        private readonly string gameCode = "186";
        private readonly int TongSLQuyen;
        private readonly int SLVeTrongTo = 15;
        private readonly int SLVeTrongQuyen = 150;
        private readonly int SLQuyenTrongThung = 20;
        private readonly int TongSLVe;
        private readonly int TongSLVe10K;
        private readonly int TongSLVe20K;
        private readonly int TongSLVe50K;
        private readonly int TongSLVe100K;
        private readonly int TongSLVe200K;
        private readonly int TongSLVe500K;
        private readonly int TongSLVeTrung;
        private readonly int TongSLVeKhongTrung;
        private readonly Dictionary<string, int> ticketDistribution;
        private readonly Random random;

        public LotteryTicketGenerator(int totalTickets = 150000)
        {
            // tỉ lệ này lấy theo bảng, mặc định thực tế sẽ lấy số chia hết
            this.TongSLQuyen = totalTickets / SLVeTrongQuyen;
            this.TongSLVe = totalTickets;
            this.TongSLVe10K = totalTickets / 6;       // 250,000 / 1,500,000 in reality
            this.TongSLVe20K = totalTickets * 3/40;  // 112,500 / 1,500,000 in reality
            this.TongSLVe50K = totalTickets / 50;      // 30,000 / 1,500,000 in reality
            this.TongSLVe100K = totalTickets / 300;
            this.TongSLVe200K = totalTickets / 1500;
            this.TongSLVe500K = totalTickets / 2500;
            this.TongSLVeTrung = TongSLVe10K + TongSLVe20K + TongSLVe50K + TongSLVe100K + TongSLVe200K + TongSLVe500K;

            this.ticketDistribution = new Dictionary<string, int>
        {
            { "10K", TongSLVe10K },
            { "20K", TongSLVe20K },
            { "50K", TongSLVe50K },
            { "100K", TongSLVe100K },
            { "200K", TongSLVe200K },
            { "500K", TongSLVe500K }
        };

            // Validate ticket count
            int prizeTicketCount = ticketDistribution.Values.Sum();
            if (prizeTicketCount > totalTickets)
            {
                throw new ArgumentException("Prize tickets exceed total tickets");
            }

            // Remaining tickets are no-prize
            this.TongSLVeKhongTrung = totalTickets - prizeTicketCount;
            this.random = new Random(42); // Fixed seed for reproducibility
        }


        private string GenerateWinningTicket(int count1, int count2, int min, int max)
        {
            List<int> firstFour = GenerateUniqueNumbers(count1, min, max);
            List<int> lastSixteen = GenerateUniqueNumbers(count2, min, max);

            // Đảm bảo có ít nhất một số trùng nhau
            if (!firstFour.Intersect(lastSixteen).Any())
            {
                //int randomIndex = random.Next(count1);
                lastSixteen[random.Next(count2)] = firstFour[random.Next(count1)];
            }

            List<int> allNumbers = new List<int>();
            allNumbers.AddRange(firstFour);
            allNumbers.AddRange(lastSixteen);

            return string.Join(" ", allNumbers.Select(n => n.ToString("D2")));
        }

        /// <summary>
        /// Tạo dãy số ngẫu nhiên không trùng lặp
        /// </summary>
        /// <param name="count"></param>
        /// <param name="minVal"></param>
        /// <param name="maxVal"></param>
        /// <returns></returns>
        private List<int> GenerateUniqueNumbers(int count, int minVal = 1, int maxVal = 60)
        {
            return Enumerable.Range(minVal, maxVal - minVal + 1)
                            .OrderBy(x => random.Next())
                            .Take(count)
                            .ToList();
        }

        private List<string> FormatNumbers(List<int> numbers, string formatType)
        {
            return numbers.Select(num => num.ToString(formatType)).ToList();
        }

        private bool CheckWinningCondition(List<int> first4, List<int> last16)
        {
            return first4.Any(num => last16.Contains(num));
        }

        private static int GenerateRandomNumber(int max)
        {
            Random random = new Random();
            return random.Next(0, max); //.ToString("D2");
        }

        /// <summary>
        /// Hàm tạo số cho vé trúng (trùng 1 cặp số)
        /// </summary>
        /// <returns></returns>
        public List<int> GenerateWinningRawNumbers(int group1count, int group2count)
        {
            Random random = new Random();
            List<int> ticket = new List<int>();

            // Generate 4 unique numbers for the first group
            HashSet<int> firstGroup = new HashSet<int>();
            while (firstGroup.Count < group1count)
            {
                firstGroup.Add(GenerateRandomNumber(60));
            }

            ticket.AddRange(firstGroup);

            // Generate 16 unique numbers for the second group
            HashSet<int> secondGroup = new HashSet<int>();
            while (secondGroup.Count < group2count)
            {
                secondGroup.Add(GenerateRandomNumber(60));
            }

            ticket.AddRange(secondGroup);

            // Pick one random number from the first group to duplicate in the second group
            int duplicateNumber = firstGroup.ElementAt(random.Next(4));
            //int duplicatePosition1 = ticket.IndexOf(duplicateNumber);

            // Replace a random number in the second group with the duplicate number
            int duplicatePosition2;
            do
            {
                duplicatePosition2 = random.Next(group1count, group2count);
            } while (ticket[duplicatePosition2] == duplicateNumber);

            ticket[duplicatePosition2] = duplicateNumber;

            return ticket;
        }



        /// <summary>
        /// Tạo số cho vé không trúng
        /// </summary>
        /// <param name="currentPos"></param>
        /// <param name="currentValue"></param>
        /// <param name="maxPosInBlock"></param>
        /// <param name="minPosInBlock"></param>
        /// <param name="allPos"></param>
        /// 
        public List<int> GenerateNonWinningRawNumbers()
        {
            var randon20 = GenerateUniqueNumbers(20);
            return randon20;
        }


        


        /// <summary>
        /// Tạo list chứa danh sách các số random không lặp
        /// </summary>
        /// <param name="total"></param>
        /// <param name="neededQuantity"></param>
        /// <returns></returns>
        private List<int> CreateListUniqueRandom(int total, int neededQuantity)
        {
            Random random = new Random();
            HashSet<int> uniqueNumbers = new HashSet<int>();

            while (uniqueNumbers.Count < neededQuantity)
            {
                uniqueNumbers.Add(random.Next(1, total + 1));
            }

            List<int> numbersList = uniqueNumbers.ToList();
            return numbersList;
        }

        /// <summary>
        /// Phân bổ vé 
        /// quyển, thùng và các vé trúng được phân đều nhất có thể
        /// </summary>
        /// <param name="nonWinningTickets">List các vé không trúng</param>
        /// <returns></returns>
        public List<Ticket> DistributeAndShuffleTickets(int TongSoLuongTickets, int TongSoLuongVe10K, int TongSoLuongVe20K, int TongSoLuongVe50K, int TongSoLuongVe100K, int TongSoLuongVe200K, int TongSoLuongVe500K)
        {

            // Khởi tạo List<string> tất cả các vé, chỉ có trường prize tag
            List<string> allPrizeTags = new List<string>();
            for (int i = 1; i <= TongSoLuongTickets; i++)
            {
                // add tạm tag mặc định là không trúng thưởng vào mỗi item trong list
                allPrizeTags.Add("NO_PRIZE");     // đại diện cho slot không trúng
            }

            Random random = new Random();
            int IDCurrentTicketInAll = 0; //STT vé hiện tại trong tất cả vé
            int IDCurrentBook = 0; //STT quyển hiện tại trong tất cả quyển
            int IDCurrentPack = 0; //STT thùng hiện tại trong tất cả thùng

            // Dành cho vé 10k, 20K, 50K, vé cần đảm bảo phân bố theo quyển
            // Chia tổng số vé (1500 000) thành các quyển 150 vé, tính số quyển cần thiết để phân bổ
            int TongSoLuongBooks = (int)Math.Ceiling(TongSoLuongTickets / 150.0);
            // Xác định 2500 quyển ngẫu nhiên sẽ có 12 vé thay vì 11 vé, tạo danh sách đó
            int SoLuongQuyenMaxVe20K = TongSoLuongBooks / 4; // trung bình 1 quyển 11.25 vé
                                                             //2500 chỉ chạy trong trường hợp 10000 quyển
            List<int> listSoQuyenCoMaxSoVe20K = CreateListUniqueRandom(TongSoLuongBooks, SoLuongQuyenMaxVe20K);

            // Dành cho vé 100k, 200K, 500K, vé cần đảm bảo phân bố theo thùng
            // Mỗi thùng có 3000 vé, ta tính tổng số thùng dựa trên tổng số vé
            int TongSoLuongPack = (int)Math.Ceiling(TongSoLuongTickets / (float)SLVeTrongQuyen / SLQuyenTrongThung);
            int SoLuongThungMaxVe500K = TongSoLuongPack / 5; // trung bình 1 thùng 1.5 vé
                                                                //2500 chỉ chạy trong trường hợp 10000 quyển
            List<int> listSoThungCoMaxSoVe500K = CreateListUniqueRandom(TongSoLuongPack, SoLuongThungMaxVe500K);


            // Hàm Điền vào slot không trúng giải gần nhất
            void FillTheNearestEmpptySlot(int currentID, string currentTag, int maxIDinBlock, int minIDinBlock, List<string> allPos, string nonWinningTag)
            {
                bool isFilled = false;
                if (isFilled == false)
                {
                    //đếm từ vị trí gần nhất sau nó
                    for (int x = currentID + 1; x <= maxIDinBlock; x++)
                    {
                        // Nếu slot đó trống thì điền vào
                        if (allPos[x - 1].Equals(nonWinningTag))
                        {
                            allPos[x - 1] = currentTag;
                            isFilled = true;
                            break; //không cần dò nữa, đã điền chỗ xong
                        }
                        else
                        {
                            // không thì quay lại dò lại
                            continue;
                        }
                    }
                }

                if (isFilled == false)
                {   
                    if (currentID > minIDinBlock)
                    {
                        // đếm từ vị trí gần nhất trước đó
                        for (int x = currentID - 1; x >= minIDinBlock; x--)
                        {
                            // Nếu slot đó trống thì điền vào
                            if (allPos[x - 1].Equals(nonWinningTag))
                            {
                                allPos[x - 1] = currentTag;
                                isFilled = true;
                                break; //không cần dò nữa, đã điền chỗ xong
                            }
                            else
                            {
                                // không thì quay lại dò lại
                                continue;
                            }
                        }
                    }

                }
            }

            void ChiaVeVaoBlock (List<string> allID, int TongSoVeCanChia, int currentBlockID, int currentTicketID, int blockSize, int SoVeChiaChoMoiBlock, string prizeTag)
            {
                for (int i = 1; i <= SoVeChiaChoMoiBlock; i++)
                {
                    // currentTicketID sẽ đếm từ 1
                    currentTicketID = (currentBlockID - 1) * blockSize + i;
                    // Nếu vé ở vị trí đó chưa trúng thì điền vé trúng hiện tại vào vị trí 
                    if (allID[currentTicketID - 1].Equals("NO_PRIZE"))
                    {
                        allID[currentTicketID - 1] = prizeTag;
                    }
                    else //nếu không điền vào vị trí gần nhất trước đó hoặc sau đó
                    {
                        // Dò tìm slot trống gần nhất sau và trước vé đó trong quyển
                        FillTheNearestEmpptySlot(currentTicketID, prizeTag, currentBlockID * blockSize, (IDCurrentBook - 1) * 150, allID, "NO_PRIZE");
                    }
                }
            }

            #region cho vé 10K, 20K, 50K
            int sizeOfBlock = 150;
            var currentBookInfo = new List<string>(); //lưu trữ thông tin các vé trong 1 quyển
            // Với mỗi block dạng quyển 150 vé, sẽ có 10 000 quyển = vòng lặp, ta điền hết vé 10K vào các quyển
            for (IDCurrentBook = 1; IDCurrentBook < TongSoLuongBooks + 1; IDCurrentBook++)
            {
                // 3. đảm bảo 25 vé trúng trong 150 vé, trường hợp này là chia hết
                //int qty10kTicNeedInBook = (int)Math.Ceiling(250000.0 / totalBooks);
                if (TongSoLuongVe10K > 0)
                {
                    ChiaVeVaoBlock(allPrizeTags, TongSLVe10K, IDCurrentBook, IDCurrentTicketInAll, sizeOfBlock, 25, "10K");
                    TongSoLuongVe10K--;
                }
            }

            for (IDCurrentBook = 1; IDCurrentBook <= TongSoLuongBooks; IDCurrentBook++)
            {
                // 3. đảm bảo 11,12 vé trúng trong 150 vé, trường hợp này chia dư 112500/500/20=11.25 ==> tối thiểu 11 vé, tối đa 12 vé trong 1 quyển 150 vé
                int maxQuantityInBook = 11;

                if (listSoQuyenCoMaxSoVe20K.Contains(IDCurrentBook))
                {
                    maxQuantityInBook = 12;
                }
                if (TongSoLuongVe20K > 0)
                {
                    ChiaVeVaoBlock(allPrizeTags, TongSLVe20K, IDCurrentBook, IDCurrentTicketInAll, sizeOfBlock, maxQuantityInBook, "20K");
                    TongSoLuongVe20K--;
                }
            }

            // 3. đảm bảo 3 vé trúng trong quyển 150 vé, trường hợp này là chia hết
            //int qty10kTicNeedInBook = (int)Math.Ceiling(250000.0 / totalBooks);
            for (IDCurrentBook = 1; IDCurrentBook < TongSoLuongBooks + 1; IDCurrentBook++)
            {
                if (TongSoLuongVe50K > 0)
                {
                    ChiaVeVaoBlock(allPrizeTags, TongSLVe50K, IDCurrentBook, IDCurrentTicketInAll, sizeOfBlock, 3, "50K");
                    TongSoLuongVe50K--;
                }
            }
            #endregion


            #region cho vé 100K, 200k, 500k
            sizeOfBlock = 3000;

            // Chia 5000 vé 100K vào 1500 000 vé, mỗi thùng (3000 vé) trúng 10 vé 
            // Xếp 10 vé 100K đầu tiên vào mỗi thùng
            for (IDCurrentPack = 1; IDCurrentPack < TongSoLuongPack + 1; IDCurrentPack++)
            {

                if (TongSoLuongVe100K > 0)
                {
                    ChiaVeVaoBlock(allPrizeTags, TongSoLuongVe100K, IDCurrentPack, IDCurrentTicketInAll, sizeOfBlock, 10, "100K");
                    TongSoLuongVe100K--;
                }
            }

            // tương tự cho vé 200K
            for (IDCurrentPack = 1; IDCurrentPack < TongSoLuongPack + 1; IDCurrentPack++)
            {

                if (TongSoLuongVe200K > 0)
                {
                    ChiaVeVaoBlock(allPrizeTags, TongSoLuongVe200K, IDCurrentPack, IDCurrentTicketInAll, sizeOfBlock, 2, "200K");
                    TongSoLuongVe200K--;
                }

            }

            // cho vé 500K
            for (IDCurrentPack = 1; IDCurrentPack < TongSoLuongPack + 1; IDCurrentPack++)
            {
                // 3. đảm bảo 11,12 vé trúng trong 150 vé, trường hợp này chia dư 112500/500/20=11.25 ==> tối thiểu 11 vé, tối đa 12 vé trong 1 quyển 150 vé
                int QuantityInPack = 1;
                if (listSoThungCoMaxSoVe500K.Contains(IDCurrentPack))
                {
                    QuantityInPack = 2;
                }

                if (TongSoLuongVe500K > 0)
                {
                    ChiaVeVaoBlock(allPrizeTags, TongSLVe20K, IDCurrentPack, IDCurrentTicketInAll, sizeOfBlock, QuantityInPack, "500K");
                    TongSoLuongVe500K--;
                }
            }

            #endregion
            

            #region xáo vé
            var stopwatch1 = new System.Diagnostics.Stopwatch();
            stopwatch1.Start();
            IDCurrentBook = 0;
            for (IDCurrentBook = 1; IDCurrentBook <= TongSoLuongBooks; IDCurrentBook++)
            {

                int startIndex = (IDCurrentBook - 1) * 150;
                int endIndex = IDCurrentBook * 150;

                // Tách phần cần xáo trộn và xáo phần đã tách đó
                List<string> rangeToShuffle = allPrizeTags.GetRange(startIndex, 150);
                var shuffledRange = rangeToShuffle.OrderBy(x => random.Next()).ToList();

                // Thay thế phần cũ bằng phần đã xáo trộn
                allPrizeTags.RemoveRange(startIndex, 150); //151
                allPrizeTags.InsertRange(startIndex, shuffledRange);
            }


            stopwatch1.Stop();
            TimeSpan totalProcessingTime = stopwatch1.Elapsed;
            Console.WriteLine($"Tổng thời gian xáo vé: {totalProcessingTime.TotalSeconds:F2} seconds");
            //for (int count =1; count <= 150; count++)
            //{
                //Console.WriteLine($"{allPrizeTags[count]}");
//            }
            
            #endregion



            #region gán các giá trị còn lại của class Ticket vào vé
            var stopwatch2 = new System.Diagnostics.Stopwatch();
            stopwatch2.Start();

            List<string> Codes = new List<string>();
            List<Ticket> ShuffledTickets = new List<Ticket>();
            int idx = 0;
            for (IDCurrentPack = 1; IDCurrentPack < TongSoLuongPack + 1; IDCurrentPack++)
            {
                for (int IDBookInPack = 1; IDBookInPack <= SLQuyenTrongThung; IDBookInPack++)
                {
                    for (int IDTicketInBook = 1; IDTicketInBook <= SLVeTrongQuyen; IDTicketInBook++)
                    {
                        IDCurrentTicketInAll = (IDCurrentPack - 1) * 3000 + (IDBookInPack - 1) * 150 + IDTicketInBook - 1;
                        string tag = allPrizeTags[IDCurrentTicketInAll];
                        Ticket newTicket = new Ticket();
                        /*
                        if (tag == "NO_PRIZE")
                        {
                            newTicket.StringOfRawNumbers = string.Join(" ", GenerateUniqueNumbers(20));
                        }
                        else
                        {
                            newTicket.StringOfRawNumbers = string.Join(" ", GenerateWinningRawNumbers(4,16));
                        }
                        Codes = CreateCodesFromID(IDCurrentPack, IDBookInPack, IDTicketInBook, gameCode);
                        newTicket.VIRN = Codes[0];
                        newTicket.TicketCode = Codes[1];
                        newTicket.TicketCodeWithoutDash = Codes[2];
                        newTicket.AVC = GenerateAVC(newTicket.PrizeTag, NhomKyTuDungChung);
                        
                        */
                        if (tag == "NO_PRIZE")
                        {
                            newTicket.StringOfRawNumbers = string.Join(" ", GenerateUniqueNumbers(20));
                        }
                        else
                        {
                            newTicket.StringOfRawNumbers = GenerateWinningTicket(4,16,1,60);
                        }
                        Codes = CreateCodesFromID(IDCurrentPack, IDBookInPack, IDTicketInBook, gameCode);
                        newTicket.VIRN = Codes[0];
                        newTicket.TicketCode = Codes[1];
                        newTicket.TicketCodeWithoutDash = Codes[2];
                        newTicket.PrizeTag = tag;
                        newTicket.AVC = string.Join(",",GenerateAVC(newTicket.PrizeTag, _groupKyTuTrungChung, _groupKyTuKhongTrung, 3));
                        ShuffledTickets.Add(newTicket);
                        /*
                        if (idx % 10000 == 0)
                        {
                            Console.WriteLine($"Số vé đã tạo {idx}.");
                            //Console.WriteLine(newTicket.StringOfRawNumbers + $"{newTicket.VIRN} {newTicket.TicketCode} {newTicket.AVC}   {newTicket.PrizeTag}");
                        }
                        */
                        idx++;
                    }
                    
                }    
            }
            stopwatch2.Stop();
            TimeSpan totalProcessingTime2 = stopwatch2.Elapsed;
            Console.WriteLine($"Tổng thời gian điền thông tin vé: {totalProcessingTime2.TotalSeconds:F2} seconds");

            /*
            foreach (string s in idAll)
            {
                int IDTicketInBook = 0;   // STT vé trong quyển hiện tại, do vòng lặp duyệt qua từng quyển
                                          // Xáo các quyển sau mỗi vòng lặp, do các slot đầu là vé trúng
                // ID hiện tại là idx, ta tự suy ra currentThung, currentQuyen, currentTicketIDinBook                
                IDCurrentBook = (int)Math.Ceiling(idx / 150.0);
                IDTicketInBook = idx - (IDCurrentBook - 1) * 150;
                IDCurrentPack = (int)Math.Ceiling(IDCurrentBook / 20.0);
                //Codes = CreateCodesFromID(IDCurrentPack, IDCurrentBook, IDTicketInBook, gameCode);

                Ticket newTicket = new Ticket();
                string tag = idAll[idx];
                if (tag == "NO_PRIZE")
                {
                    newTicket.RawNumbers = GenerateUniqueNumbers(20);
                }
                else
                {
                    newTicket.RawNumbers = GenerateWinningRawNumbers();
                }
                //newTicket.VIRN = Codes[0];
                //newTicket.TicketCode = Codes[1];
                //newTicket.TicketCodeWithoutDash = Codes[2];
                newTicket.TypeOfWinningPrize = tag;
                //newTicket.AVC = GenerateAVC(newTicket.TypeOfWinningPrize, NhomKyTuDungChung);
                ShuffledTickets.Add(newTicket);
                if (idx%100==0)
                {
                    Console.WriteLine($"Số vé đã tạo {idx}.");
                }    
                idx++;
            }
            stopwatch2.Stop();
            TimeSpan totalProcessingTime2 = stopwatch2.Elapsed;
            Console.WriteLine($"Tổng thời gian điền thông tin vé: {totalProcessingTime2.TotalSeconds:F2} seconds");
            */
            #endregion

            return ShuffledTickets;
        }


        

        /// <summary>
        /// Chọn các ký tự từ nhóm ký tự file XML
        /// </summary>
        /// <param name="a"></param>
        /// <param name="listChar"></param>
        /// <returns></returns>
        private List<string> GetRandomCharsFromCharSet (int a, List<char> listChar)
        {
            List<string> randomListChar = new List<string>();
            Random rand = new Random();
            for (int i=1; i<=a; i++)
            {
                randomListChar.Add(listChar[rand.Next(listChar.Count)].ToString());
            }
            return randomListChar;
        }


        private static readonly Dictionary<string, string> PrizeFirstCharMap = new Dictionary<string, string>
        {
            { "10K", "t" },
            { "20K", "m" },
            { "50K", "w" },
            { "100K", "i" },
            { "200K", "j" },
            { "500K", "a" }
        };
        private string GenerateAVC(string typeOfWinningPrize,  List<char> sharedWinningCharSet, List<char> sharedNonWinningCharSet,  int numberOfCharsToBePrinted = 3)
        {
            var random = new Random();

            // Generate sorted random positions
            /*
            var randomPos = Enumerable.Range(0, numberOfCharsToBePrinted)
                .Select(_ => random.Next(1, 20))
                .OrderBy(x => x)
                .Select(x => x.ToString())
                .ToList();
            */
            var randomPosInt = GenerateUniqueNumbers(3, 1, 20);
            List<string> randomPos = randomPosInt.ConvertAll(x => x.ToString());


            // Get random characters
            var randomCharsTrung = GetRandomCharsFromCharSet(2, sharedWinningCharSet);
            var randomCharsKhongTrung = GetRandomCharsFromCharSet(3, sharedNonWinningCharSet);

            // Build result
            var result = "";
            if (typeOfWinningPrize == "NO_PRIZE")
            {
                result = string.Join(",", randomCharsKhongTrung[0], randomPos[0], randomCharsKhongTrung[1], randomPos[1], randomCharsKhongTrung[2], randomPos[2]);
            }
            else
            {
                string firstChar;
                if (!PrizeFirstCharMap.TryGetValue(typeOfWinningPrize, out firstChar))
                {
                    firstChar = string.Empty;
                }
                result = string.Join(",", firstChar, randomPos[0], randomCharsTrung[0], randomPos[1], randomCharsTrung[1], randomPos[2]);
            }

            return result;
        }


        /// <summary>
        /// Tạo mã AVC
        /// </summary>
        /// <param name="TypeOfWinningPrize"></param>
        /// <param name="sharedWinningCharSet">tập ký tự dùng chung</param>
        /// /// <param name="numbOfCharsToBePrinted">số ký tự AVC</param>
        /// <returns></returns>
        private List<string> GenerateAVC1(string TypeOfWinningPrize, List<char> sharedWinningCharSet, int numbOfCharsToBePrinted =3)
        {
            List<string> AVC;
            string FirstChar = "";
            Random random = new Random();

            // danh sách 3 vị trí ngẫu nhiên của 3 ký tự AVC
            List<int> randomPos = new List<int>();
            for (int i = 1; i <= numbOfCharsToBePrinted; i++)
            {
                int randNumb = random.Next(1, 20);
                randomPos.Add(randNumb);
            }
            randomPos.Sort();

            if (TypeOfWinningPrize == "NO_PRIZE")
            {
                List<string> rd = GetRandomCharsFromCharSet(3, _groupKyTuKhongTrung);
                AVC = new List<string> { rd[0], randomPos[0].ToString(), rd[1], randomPos[1].ToString(), rd[2], randomPos[2].ToString() };
            }
            else
            {
                switch (TypeOfWinningPrize)
                {
                    case "10K":
                        FirstChar = "t";
                        break;
                    case "20K":
                        FirstChar = "m";
                        break;
                    case "50K":
                        FirstChar = "w";
                        break;
                    case "100K":
                        FirstChar = "i";
                        break;
                    case "200K":
                        FirstChar = "j";
                        break;
                    case "500K":
                        FirstChar = "a";
                        break;
                }
                List<string> rd = GetRandomCharsFromCharSet(2, _groupKyTuTrungChung);
                AVC = new List<string> { FirstChar, randomPos[0].ToString(), rd[0], randomPos[1].ToString(), rd[1], randomPos[2].ToString() };
            }

            return AVC;
        }
            

        /// <summary>
        /// Sinh lần lượt các mã VIRN, mã vé có gạch và không gạch từ ID vé
        /// </summary>
        /// <param name="currentThung"></param>
        /// <param name="currentQuyenInThung"></param>
        /// <param name="currentVeTrongQuyen"></param>
        /// <param name="gameCode"></param>
        /// <returns></returns>
        private List<string> CreateCodesFromID(int currentThung, int currentQuyenInThung, int currentVeTrongQuyen, string gameCode)
        {
            string S = "1"; //Số lần in
                            // Mã vé G G G - P P P P B B - T T T
            string PPPP = currentThung.ToString("D4");
            string BB = currentQuyenInThung.ToString("D2");
            string TTT = currentVeTrongQuyen.ToString("D3");
            string TicketCode = gameCode + "-" + PPPP + BB + "-" + TTT;
            string TicketCodeKhongGach = gameCode + PPPP + BB + TTT;
            // Mã VIRN 
            // 7 ký tự số ngẫu nhiên
            Random rand = new Random();
            string random7symbols = rand.Next(0,10000000).ToString("D7");
            /*
            for (int i = 0; i < 7; i++)
            {
                random7symbols += rand.Next(0, 10).ToString();
            }
            */
            string VIRN = gameCode.ToString() + PPPP + BB + TTT + S + random7symbols;
            List<string> Codes = new List<string> { VIRN, TicketCode, TicketCodeKhongGach };
            return Codes;

        }

        /// <summary>
        /// Xác định loại vé trúng thưởng giải gì hoặc không trúng
        /// </summary>
        /// <param name="isWinning"></param>
        /// <param name="ticketTypeCounters"></param>
        /// <returns></returns>
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
                if (ticketTypeCounters["100K"] < ticketDistribution["100K"])
                {
                    ticketTypeCounters["100K"]++;
                    return "100K";
                }
                if (ticketTypeCounters["200K"] < ticketDistribution["200K"])
                {
                    ticketTypeCounters["200K"]++;
                    return "200K";
                }
                if (ticketTypeCounters["500K"] < ticketDistribution["500K"])
                {
                    ticketTypeCounters["500K"]++;
                    return "500K";
                }
            }

            if (ticketTypeCounters["NO_PRIZE"] < TongSLVeKhongTrung)
            {
                ticketTypeCounters["NO_PRIZE"]++;
                return "NO_PRIZE";
            }

            return null;
        }

        /// <summary>
        /// Sinh vé trúng theo số lượng đã cho, cân nhắc chạy đa luồng
        /// </summary>
        /// <param name="AllTickest"></param>
        /// <param name="ListThisTypeTicket"></param>
        /// <param name="quantity"></param>
        /// <param name="winningPrize"></param>
        private void SinhVeTrungTheoSoLuong(HashSet<string> AllTickest, List<Ticket> ListThisTypeTicket, int quantity, string winningPrize)
        {
            for (int ticketId = 0; ticketId < quantity; ticketId++)
            {
                string ticketRawNumer = GenerateWinningRawNumbers(4,16).ToString();

                AllTickest.Add(ticketRawNumer);
                Ticket ticket = new Ticket { StringOfRawNumbers = ticketRawNumer, PrizeTag = winningPrize };
                ListThisTypeTicket.Add(ticket);

                /*
                if (!AllTickest.Contains(ticketRawNumer))
                {
                    AllTickest.Add(ticketRawNumer);
                    Ticket ticket = new Ticket {RawNumbers = ticketRawNumer, TypeOfWinningPrize = winningPrize };
                    ListThisTypeTicket.Add(ticket);
                }
                */
            }
        }

        public void GenerateTickets(string outputFile = "lottery_tickets.txt")
        {
            var generatedTickets = new HashSet<List<int>>(); //danh sách vé đã tạo dùng để kiểm tra tính trùng
            var finalTicketsList = new List<Ticket>();  // danh sách vé đã tạo dùng để ghi vào CSDL
            
            // danh sách vé trúng và không trúng, dùng để xáo vé bước sau
            var list10KTickets = new List<Ticket>();
            var list20KTickets = new List<Ticket>();
            var list50KTickets = new List<Ticket>();
            var list200KTickets = new List<Ticket>();
            var list100KTickets = new List<Ticket>();
            var list500KTickets = new List<Ticket>();
            var listNoWinningTickets = new List<Ticket>();  

            var ticketTypeCounters = new Dictionary<string, int>
            {
                { "10K", 0 },
                { "20K", 0 },
                { "50K", 0 },
                { "100K", 0 },
                { "200K", 0 },
                { "500K", 0 },
                { "NO_PRIZE", 0 }
            };

            var sw = new Stopwatch();
            sw.Start();

            /*
            SinhVeTrungTheoSoLuong(generatedTickets, list10KTickets, TongSoLuong10kTickets, "10K");
            SinhVeTrungTheoSoLuong(generatedTickets, list20KTickets, TongSoLuong20kTickets, "20K");
            SinhVeTrungTheoSoLuong(generatedTickets, list50KTickets, TongSoLuong50kTickets, "50K");
            SinhVeTrungTheoSoLuong(generatedTickets, list100KTickets, TongSoLuong100kTickets, "100K");
            SinhVeTrungTheoSoLuong(generatedTickets, list200KTickets, TongSoLuong200kTickets, "200K");
            SinhVeTrungTheoSoLuong(generatedTickets, list500KTickets, TongSoLuong500kTickets, "500K");
            */

            #region archived
            /*
            for (int ticketId = 0; ticketId < TongSoLuongVe; ticketId++)
            {
                while (true)
                {
                    // tạo các vé trúng 10K
                    // tạo vé trúng
                    // gán mã ACV

                    // tạo các vé trúng 20K

                    // tạo các vé không trúng

                    //
                    var first4 = GenerateUniqueNumbers(4);
                    var last16 = GenerateUniqueNumbers(16);
                    var first4Formatted = FormatNumbers(first4, "D2");
                    var last16Formatted = FormatNumbers(last16, "D2");

                    bool isWinning = CheckWinningCondition(first4, last16);
                    var ticketNumbersList = first4Formatted.Concat(last16Formatted).ToList();
                    string ticketNumbersStr = string.Join(" ", ticketNumbersList);

                    // nếu vé đã tạo không nằm trong list các vé đã tạo
                    if (!generatedTickets.Contains(ticketNumbersStr))
                    {
                        generatedTickets.Add(ticketNumbersStr);
                        string ticketType = DetermineTicketType(isWinning, ticketTypeCounters);

                        if (ticketType != null)
                        {
                            string currentTicketStr = $"{string.Join(" ", ticketNumbersList)} {ticketType}";
                            listGeneratedTickets.Add(currentTicketStr);
                            switch (ticketType)
                            {
                                case "NO_PRIZE":
                                    listNoWinningTickets.Add(currentTicketStr);
                                    break;
                                case "10K":
                                    list10KTickets.Add(currentTicketStr);
                                    break;
                                case "20K":
                                    list20KTickets.Add(currentTicketStr);
                                    break;
                                case "50K":
                                    list50KTickets.Add(currentTicketStr);
                                    break;
                                case "100K":
                                    list100KTickets.Add(currentTicketStr);
                                    break;
                                case "200K":
                                    list200KTickets.Add(currentTicketStr);
                                    break;
                                case "500K":
                                    list500KTickets.Add(currentTicketStr);
                                    break;
                            }
                            break;
                        }
                        // debug cho thấy không chạy vào dòng này --> các vé đều được phân loại hết
                        else
                            break;
                    }
                }
            }
            */
            #endregion 

            // xáo thứ tự vé theo thuật toán sao cho 15 vé liền nhau sẽ có 1 vé trúng
            var shuffleTime = new Stopwatch();
            shuffleTime.Start();
            // listGeneratedTickets = listGeneratedTickets.OrderBy(x => random.Next()).ToList();
            finalTicketsList = DistributeAndShuffleTickets(TongSLVe, TongSLVe10K, TongSLVe20K, TongSLVe50K, TongSLVe100K, TongSLVe200K, TongSLVe500K);
            shuffleTime.Stop();
            Console.WriteLine($"\nThời gian phân phối và xáo vé theo cơ cấu đã cho: {shuffleTime.Elapsed.TotalSeconds:F2} seconds");

            // Write to txt file
            using (StreamWriter file = new StreamWriter(outputFile))
            {
                for (int idx = 0; idx < finalTicketsList.Count; idx++)
                {
                    // chèn mã vé vào
                    //file.WriteLine($"ID {(idx + 1):D7}: {finalTicketsList[idx].VIRN } {finalTicketsList[idx].TicketCode}    {finalTicketsList[idx].RawNumbers} {finalTicketsList[idx].AVC}  {finalTicketsList[idx].TypeOfWinningPrize}");
                    file.WriteLine($"{idx +1:D7} {finalTicketsList[idx].StringOfRawNumbers}   {finalTicketsList[idx].VIRN}          {finalTicketsList[idx].TicketCode}   {finalTicketsList[idx].AVC}   {finalTicketsList[idx].PrizeTag}");
                }
            }

            /*
            // Print distribution
            Console.WriteLine("Cơ cấu giải thưởng sau khi tạo vé (đếm danh sách đối tượng List vừa tạo:");
            Console.WriteLine($"Tổng số vé:{TongSLVe}");
            Console.WriteLine($"Tổng số quyển:{TongSLQuyen}");
            Console.WriteLine($"Tổng số thùng:{TongSLQuyen / 20}");
            foreach (var kvp in ticketTypeCounters)
            {
                Console.WriteLine($"{kvp.Key}: {kvp.Value} tickets");
            }
            */
        }

        /// <summary>
        /// Đọc 1 khoảng các dòng trong file đã chọn
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="groupNumber"></param>
        /// <param name="groupSize"></param>
        /// <returns></returns>
        public List<string> ReadLinesRange(string filePath, int startLine, int groupSize = 150)
        {
            try
            {
                // Calculate start and end line numbers
                int endLine = startLine + groupSize;

                // Read the specified range of lines
                return File.ReadLines(filePath)
                          .Skip(startLine)
                          .Take(groupSize)
                          .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Kiểm thử từ file txt
        /// </summary>
        /// <param name="outputFile"></param>
        /// 
        public void VerifyDistribution(string outputFile = "lottery_tickets.txt")
        {
            var ticketTypeCounts = new Dictionary<string, int>
        {
            { "10K", 0 },
            { "20K", 0 },
            { "50K", 0 },
            { "100K", 0 },
            { "200K", 0 },
            { "500K", 0 },
            { "NO_PRIZE", 0 }
        };

            int totalLine = 0;
            int totalTypeCount = 0;
            foreach (string line in File.ReadLines(outputFile))
            {
                totalLine++;
                // tách chuỗi và lấy phần tử cuối cùng chính là tên giải thưởng
                string ticketType = line.Split(' ').Last();
                ticketTypeCounts[ticketType]++;
                totalTypeCount++;
            }

            Console.WriteLine("\nKết quả đếm kiểm thử khi đọc file txt:");
            Console.WriteLine($"Số lượng vé 10000 VND: {ticketTypeCounts["10K"]} (Kỳ vọng: {TongSLVe10K})");
            Console.WriteLine($"Số lượng vé 20000 VND: {ticketTypeCounts["20K"]} (Kỳ vọng: {TongSLVe20K})");
            Console.WriteLine($"Số lượng vé 50000 VND: {ticketTypeCounts["50K"]} (Kỳ vọng: {TongSLVe50K})");
            Console.WriteLine($"Số lượng vé 100000 VND: {ticketTypeCounts["100K"]} (Kỳ vọng: {TongSLVe100K})");
            Console.WriteLine($"Số lượng vé 200000 VND: {ticketTypeCounts["200K"]} (Kỳ vọng: {TongSLVe200K})");
            Console.WriteLine($"Số lượng vé 500000 VND: {ticketTypeCounts["500K"]} (Kỳ vọng: {TongSLVe500K})");
            Console.WriteLine($"Số lượng vé không trúng: {ticketTypeCounts["NO_PRIZE"]} (Kỳ vọng: {TongSLVeKhongTrung})");

            // Verify distribution
            Debug.Assert(ticketTypeCounts["10K"] == TongSLVe10K, "Số vé 10K không khớp");
            Debug.Assert(ticketTypeCounts["20K"] == TongSLVe20K, "Số vé 20K không khớp");
            Debug.Assert(ticketTypeCounts["50K"] == TongSLVe50K, "Số vé 50K không khớp");
            Debug.Assert(ticketTypeCounts["100K"] == TongSLVe100K, "Số vé 100K không khớp");
            Debug.Assert(ticketTypeCounts["200K"] == TongSLVe200K, "Số vé 200K không khớp");
            Debug.Assert(ticketTypeCounts["500K"] == TongSLVe500K, "Số vé 500K không khớp");
            Debug.Assert(ticketTypeCounts["NO_PRIZE"] == TongSLVeKhongTrung, "Số vé không trúng thưởng không khớp cơ cấu");


            // Kiểm thử cho 1 quyển = 150 dòng bất kỳ trong file text
            int blocksize = 150; // nếu là thùng thì sẽ là 3000
            int coef = TongSLVe / blocksize;
            totalLine = 0;
            totalTypeCount = 0;
            int newTotal10kTickets = TongSLVe10K / coef;
            float newTotal20kTickets = (float)TongSLVe20K / (float)coef;
            int newTotal50kTickets = TongSLVe50K / coef;
            List<string> prizes = new List<string> { "10K", "20K", "50K", "100K", "200K", "500K", "NO_PRIZE" };
            foreach (string prize in prizes)
            {
                ticketTypeCounts[prize] = 0;
            }

            Random random = new Random();
            int rand = random.Next(1, TongSLQuyen);
            int startLine = rand * 150;
            foreach (string line in ReadLinesRange(outputFile, startLine, blocksize))
            {
                totalLine++;
                // tách chuỗi và lấy phần tử cuối cùng chính là tên giải thưởng
                string ticketType = line.Split(' ').Last();
                ticketTypeCounts[ticketType]++;
                totalTypeCount++;
            }
            Console.WriteLine($"\nKết quả đếm kiểm thử 1 quyển bất kỳ (150 vé) bắt đầu từ dòng ngẫu nhiên thứ {startLine} trong file txt:");
            Console.WriteLine($"Số lượng vé 10000 VND: {ticketTypeCounts["10K"]} (Kỳ vọng: {newTotal10kTickets})");
            Console.WriteLine($"Số lượng vé 20000 VND: {ticketTypeCounts["20K"]} (Kỳ vọng: {newTotal20kTickets})");
            Console.WriteLine($"Số lượng vé 50000 VND: {ticketTypeCounts["50K"]} (Kỳ vọng: {newTotal50kTickets})");

            // Kiểm thử cho 1 thùng bất kỳ
            blocksize = 3000; // nếu là thùng thì sẽ là 3000
            coef = TongSLVe / blocksize;
            int newTotal100kTickets = TongSLVe100K / coef;
            int newTotal200kTickets = TongSLVe200K / coef;
            float newTotal500kTickets = (float)TongSLVe500K / (float)coef;
            foreach (string prize in prizes)
            {
                ticketTypeCounts[prize] = 0;
            }

            rand = random.Next(0, TongSLQuyen / 20);
            startLine = rand * 3000;
            foreach (string line in ReadLinesRange(outputFile, startLine, blocksize))
            {
                // tách chuỗi và lấy phần tử cuối cùng chính là tên giải thưởng
                string ticketType = line.Split(' ').Last();
                ticketTypeCounts[ticketType]++;
                totalTypeCount++;
            }
            Console.WriteLine($"\nKết quả đếm kiểm thử 1 thùng bất kỳ (3000 vé) bắt đầu từ dòng ngẫu nhiên thứ {startLine} trong file txt:");
            Console.WriteLine($"Số lượng vé 100K VND: {ticketTypeCounts["100K"]} (Kỳ vọng: {newTotal100kTickets})");
            Console.WriteLine($"Số lượng vé 200K VND: {ticketTypeCounts["200K"]} (Kỳ vọng: {newTotal200kTickets})");
            Console.WriteLine($"Số lượng vé 500K VND: {ticketTypeCounts["500K"]} (Kỳ vọng: {newTotal500kTickets})");

        }
    }
}