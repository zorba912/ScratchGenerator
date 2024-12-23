// LotteryTicketGenerator.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

public class LotteryTicketGenerator
{
    private readonly int totalTickets;
    private readonly int SLBook;
    private readonly int gameCode;
    private readonly int total10kTickets;
    private readonly int total20kTickets;
    private readonly int total50kTickets;
    private readonly int total100kTickets;
    private readonly int total200kTickets;
    private readonly int total500kTickets;
    private readonly int noPrizeTickets;
    private readonly Dictionary<string, int> ticketDistribution;
    private readonly Random random;

    public LotteryTicketGenerator(int totalTickets = 150000)
    {
        // tỉ lệ này lấy theo bảng, mặc định thực tế sẽ lấy số chia hết
        this.SLBook = totalTickets / 150; //150 là số vé 1 quyển
        this.gameCode = 186;        // mã trò chơi
        this.totalTickets = totalTickets;
        this.total10kTickets = totalTickets / 6;       // 250,000 / 1,500,000 in reality
        this.total20kTickets = totalTickets * 3 / 40;  // 112,500 / 1,500,000 in reality
        this.total50kTickets = totalTickets / 50;      // 30,000 / 1,500,000 in reality
        this.total100kTickets = totalTickets / 300;
        this.total200kTickets = totalTickets / 1500;
        this.total500kTickets = totalTickets / 2500;

        this.ticketDistribution = new Dictionary<string, int>
        {
            { "10K", total10kTickets },
            { "20K", total20kTickets },
            { "50K", total50kTickets },
            { "100K", total100kTickets },
            { "200K", total200kTickets },
            { "500K", total500kTickets }
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

    private void FillTheNearestSlot(int currentPos, string currentValue, int maxPosInBlock, int minPosInBlock, List<string> allPos)
    {
        bool isFilled = false;
        if (isFilled == false)
        {
            for (int x = currentPos + 1; x <= maxPosInBlock; x++)
            {
                // Nếu slot đó trống thì điền vào
                if (allPos[x - 1].Equals("0"))
                {
                    allPos[x - 1] = currentValue;
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
            for (int x = currentPos - 1; x >= minPosInBlock; x--)
            {
                // Nếu slot đó trống thì điền vào
                if (allPos[x - 1].Equals("0"))
                {
                    allPos[x - 1] = currentValue;
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

    /// <summary>
    /// Điền vé hiện tại vào slot trống gần nhất sau nó
    /// </summary>
    /// <param name="currentPos">slot hiện tại</param>
    /// <param name="maxPosInBlock">slot tối đa trong 1 block</param>
    /// <param name="allPos">Danh sách chứa tất thông tin cả các slot</param>
    private void FillTheNextEmpty(int currentPos, string currentValue, int maxPosInBlock,  List<string> allPos)
    {
        for (int x = currentPos + 1; x <= maxPosInBlock; x++)
        {
            // Nếu slot đó trống thì điền vào
            if (allPos[x-1].Equals("0"))
            {
                allPos[x-1] = currentValue;
                break; //không cần dò nữa, đã điền chỗ xong
            }
            else
            {
                // không thì quay lại dò lại
                continue;
            }
        }
    }

    /// <summary>
    /// Điền vé vào vị trí trống gần nhất trước nó
    /// </summary>
    /// <param name="currentPos"></param>
    /// <param name="currentValue"></param>
    /// <param name="maxPosInBlock"></param>
    /// <param name="allPos"></param>
    private void FillTheLastEmpty(int currentPos, string currentValue, int minPosInBlock, List<string> allPos)
    {
        for (int x = currentPos-1; x >= minPosInBlock; x--)
        {
            // Nếu slot đó trống thì điền vào
            if (allPos[x-1].Equals("0"))
            {
                allPos[x-1] = currentValue;
                break; //không cần dò nữa, đã điền chỗ xong
            }
            else
            {
                // không thì quay lại dò lại
                continue;
            }
        }
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
            uniqueNumbers.Add(random.Next(1, total+1));
        }

        List<int> numbersList = uniqueNumbers.ToList();
        return numbersList;
    }

    /// <summary>
    /// Xáo trộn STT của 1 khoảng cho trước trong 1 list tổng thể đã cho
    /// </summary>
    /// <param name="tickets"></param>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <returns></returns>


    /// <summary>
    /// Phân bổ vé sao cho 15 vé sẽ có 1 vé trúng, các vé trúng được phân đều nhất có thể
    /// </summary>
    /// <param name="nonWinningTickets">List các vé không trúng</param>
    /// <returns></returns>
    public List<string> ShuffleTickets(int SoLuongTickets, List<string> list10K, List<string> list20K, List<string> list50K, List<string> list100K, List<string> list200K, List<string> list500K, List<string> listNoPrize)
    {
        // Khởi tạo List tất cả các vé, sẽ có thêm trường ID
        List<string> idAll = new List<string>();
        for (int i = 1; i <= SoLuongTickets; i++)
        {
            // add tạm ký tự trống (placeholder) vào mỗi item trong idAll
            idAll.Add("0");     // đại diện cho slot trống chưa có vé nào
        }
  
        List<string> id10K = new List<string>();
        List<string> id20K = new List<string>();
        List<string> id50K = new List<string>();
        List<string> id100K = new List<string>();
        List<string> id200K = new List<string>();
        List<string> id500K = new List<string>();

        Random random = new Random();
        List<string> finalDistribution = new List<string>();
        int IDCurrentTicketInAll = 0; //STT vé hiện tại trong tất cả vé
        int IDCurrentBook = 0; //STT quyển hiện tại trong tất cả quyển
        int IDCurrentPackage = 0; //STT thùng hiện tại trong tất cả thùng

        // Dành cho vé 10k, 20K, 50K, vé cần đảm bảo phân bố theo quyển
        // Chia tổng số vé (1500 000) thành các quyển 150 vé, tính số quyển cần thiết để phân bổ
        int TongSoLuongBooks = (int)Math.Ceiling(SoLuongTickets / 150.0);
        // Xác định 2500 quyển ngẫu nhiên sẽ có 12 vé thay vì 11 vé, tạo danh sách đó
        int SoLuongQuyenMaxVe20K = TongSoLuongBooks / 4; // trung bình 1 quyển 11.25 vé
        //2500 chỉ chạy trong trường hợp 10000 quyển
        List<int> listSoQuyenCoMaxSoVe20K = CreateListUniqueRandom(TongSoLuongBooks, SoLuongQuyenMaxVe20K);

        // Dành cho vé 100k, 200K, 500K, vé cần đảm bảo phân bố theo thùng
        // Mỗi thùng có 3000 vé, ta tính tổng số thùng dựa trên tổng số vé
        int TongSoLuongPackage = (int)Math.Ceiling(SoLuongTickets / 3000.0);
        int SoLuongThungMaxVe500K = TongSoLuongPackage / 5; // trung bình 1 thùng 1.5 vé
        //2500 chỉ chạy trong trường hợp 10000 quyển
        List<int> listSoThungCoMaxSoVe500K = CreateListUniqueRandom(TongSoLuongPackage, SoLuongThungMaxVe500K);

        #region cho vé 10K, 20K, 50K
        var currentBookInfo = new List<string>(); //lưu trữ thông tin các vé trong 1 quyển
        // 2. Với mỗi block dạng quyển 150 vé, sẽ có 10 000 quyển = vòng lặp, ta điền hết vé 10K vào các quyển
        for (IDCurrentBook = 1; IDCurrentBook < TongSoLuongBooks + 1; IDCurrentBook++)
        {
            // 3. đảm bảo 25 vé trúng trong 150 vé, trường hợp này là chia hết
            //int qty10kTicNeedInBook = (int)Math.Ceiling(250000.0 / totalBooks);

            if (list10K.Count > 0)
            {
                for (int i = 1; i <= 25; i++)
                {
                    var winning_item = list10K[0];
                    IDCurrentTicketInAll = (IDCurrentBook - 1) * 150 + i;
                    // Nếu chưa có vé nào ở vị trí đó thì điền vé hiện tại vào vị trí 
                    if (idAll[IDCurrentTicketInAll-1].Equals("0"))
                    {
                        idAll[IDCurrentTicketInAll-1] = winning_item;
                    }
                    else //nếu không điền vào vị trí gần nhất trước đó hoặc sau đó
                    {
                        // Dò tìm slot trống gần nhất sau và trước vé đó trong quyển
                        FillTheNearestSlot(IDCurrentTicketInAll, winning_item, IDCurrentBook * 150, (IDCurrentBook - 1) * 150, idAll);
                        //FillTheNextEmpty(IDCurrentTicketInAll, winning_item, IDCurrentBook * 150, idAll);
                        //FillTheLastEmpty(IDCurrentTicketInAll, winning_item, (IDCurrentBook - 1) * 150, idAll);
                    }
                    //id10K.Add(winning_item);
                    list10K.RemoveAt(0);
                }
            }
        }

        int count_book20K = 0;
        int count20K_2 = 0;
        for (IDCurrentBook = 1; IDCurrentBook < TongSoLuongBooks + 1; IDCurrentBook++)
        {

            // 3. đảm bảo 11,12 vé trúng trong 150 vé, trường hợp này chia dư 112500/500/20=11.25 ==> tối thiểu 11 vé, tối đa 12 vé trong 1 quyển 150 vé
            //int qty10kTicNeedInBook = (int)Math.Ceiling(250000.0 / totalBooks);
            count20K_2++;
            if (list20K.Count > 0)
            {
                int maxQuantityInBook = 11;
                
                if (listSoQuyenCoMaxSoVe20K.Contains(IDCurrentBook))
                {
                    maxQuantityInBook = 12;
                    count_book20K++;
                }

                // chia 112500 vé vào 10000 quyển
                for (int i = 1; i <= maxQuantityInBook; i++)
                {
                    var winning_item = list20K[0];
                    IDCurrentTicketInAll = (IDCurrentBook - 1) * 150 + i;
                    // Nếu chưa có vé nào ở vị trí đó thì điền vé hiện tại vào vị trí 
                    if (idAll[IDCurrentTicketInAll - 1].Equals("0"))
                    {
                        idAll[IDCurrentTicketInAll - 1] = winning_item;
                    }
                    else //nếu không điền vào vị trí gần nhất trước đó hoặc sau đó
                    {
                        FillTheNearestSlot(IDCurrentTicketInAll, winning_item, IDCurrentBook * 150, (IDCurrentBook - 1) * 150, idAll);
                    }
                    //id20K.Add(winning_item);
                    list20K.RemoveAt(0);
                }
            }
        }

        for (IDCurrentBook = 1; IDCurrentBook < TongSoLuongBooks + 1; IDCurrentBook++)
        {
            // 3. đảm bảo 3 vé trúng trong quyển 150 vé, trường hợp này là chia hết
            //int qty10kTicNeedInBook = (int)Math.Ceiling(250000.0 / totalBooks);
            if (list50K.Count > 0)
            {
                for (int i = 1; i <= 3; i++)
                {
                    var winning_item = list50K[0];
                    IDCurrentTicketInAll = (IDCurrentBook - 1) * 150 + i;
                    // Nếu chưa có vé nào ở vị trí đó thì điền vé hiện tại vào vị trí 
                    if (idAll[IDCurrentTicketInAll - 1].Equals("0"))
                    {
                        idAll[IDCurrentTicketInAll - 1] = winning_item;
                    }
                    else //nếu không điền vào vị trí gần nhất trước đó hoặc sau đó
                    {
                        FillTheNearestSlot(IDCurrentTicketInAll, winning_item, IDCurrentBook * 150, (IDCurrentBook - 1) * 150, idAll);
                    }
                    //id50K.Add(winning_item);
                    list50K.RemoveAt(0);
                }
            }
        }
        #endregion


        #region cho vé 100K, 200k, 500k

        for (IDCurrentPackage = 1; IDCurrentPackage < TongSoLuongPackage + 1; IDCurrentPackage++)
        {
            // Chia 5000 vé 100K vào 1500 000 vé, mỗi thùng (3000 vé) trúng 10 vé 
            // Xếp 10 vé 100K đầu tiên vào mỗi thùng
            if (list100K.Count > 0)
            {
                for (int i = 1; i <= 10; i++)
                {
                    var winning_item = list100K[0];
                    IDCurrentTicketInAll = (IDCurrentPackage - 1) * 3000 + i;
                    // Nếu chưa có vé nào ở vị trí đó thì điền vé hiện tại vào vị trí 
                    if (idAll[IDCurrentTicketInAll - 1].Equals("0"))
                    {
                        idAll[IDCurrentTicketInAll - 1] = winning_item;
                    }
                    else //nếu không điền vào vị trí gần nhất trước đó hoặc sau đó
                    {
                        FillTheNearestSlot(IDCurrentTicketInAll, winning_item, IDCurrentPackage * 3000, (IDCurrentPackage - 1) * 3000, idAll);
                        //FillTheNextEmpty(IDCurrentTicketInAll, winning_item, IDCurrentPackage * 3000, idAll);
                        //FillTheLastEmpty(IDCurrentTicketInAll, winning_item, (IDCurrentPackage - 1) * 3000, idAll);
                    }
                    //id100K.Add(winning_item);
                    list100K.RemoveAt(0);
                }
            }
        }

        for (IDCurrentPackage = 1; IDCurrentPackage < TongSoLuongPackage + 1; IDCurrentPackage++)
        {
            // Xếp 2 vé 200K vào mỗi thùng
            if (list200K.Count > 0)
            {
                for (int i = 1; i <= 2; i++)
                {
                    var winning_item = list200K[0];
                    IDCurrentTicketInAll = (IDCurrentPackage - 1) * 3000 + i;
                    // Nếu chưa có vé nào ở vị trí đó thì điền vé hiện tại vào vị trí 
                    if (idAll[IDCurrentTicketInAll - 1].Equals("0"))
                    {
                        idAll[IDCurrentTicketInAll - 1] = winning_item;
                    }
                    else //nếu không điền vào vị trí gần nhất trước đó hoặc sau đó
                    {
                        FillTheNearestSlot(IDCurrentTicketInAll, winning_item, IDCurrentPackage * 3000, (IDCurrentPackage - 1) * 3000, idAll);
                    }
                    //id200K.Add(winning_item);
                    list200K.RemoveAt(0);
                }
            }
        }

        int count4 = 0;
        for (IDCurrentPackage = 1; IDCurrentPackage < TongSoLuongPackage + 1; IDCurrentPackage++)
        {
            // 3. đảm bảo 600 vé trúng trong thùng 3000 vé, trường hợp này là chia không hết
            if (list500K.Count > 0)
            {
                int QuantityInBook = 1;
                if (listSoThungCoMaxSoVe500K.Contains(IDCurrentPackage))
                {
                    QuantityInBook = 2;
                    count4++; //kỳ vọng 100 thùng trên 500 thùng trúng
                }

                // chia hết 112500 vé vào 10000 quyển
                for (int i = 1; i <= QuantityInBook; i++)
                {
                    var winning_item = list500K[0];
                    IDCurrentTicketInAll = (IDCurrentPackage - 1) * 3000 + i;
                    // Nếu chưa có vé nào ở vị trí đó thì điền vé hiện tại vào vị trí 
                    if (idAll[IDCurrentTicketInAll - 1].Equals("0"))
                    {
                        idAll[IDCurrentTicketInAll - 1] = winning_item;
                    }
                    else //nếu không điền vào vị trí gần nhất trước đó hoặc sau đó trong thùng
                    {
                        FillTheNearestSlot(IDCurrentTicketInAll, winning_item, IDCurrentPackage * 3000, (IDCurrentPackage - 1) * 3000, idAll);
                    }
                    //id500K.Add(winning_item);
                    list500K.RemoveAt(0);
                }
            }
        }
        #endregion

        int count3 = 0; // đếm vé trống debug
        int IDTicketInBook = 0;   // STT vé trong quyển hiện tại
        // vòng lặp lướt qua các quyển
        for (IDCurrentBook = 1; IDCurrentBook <= TongSoLuongBooks; IDCurrentBook++)
        {

            // điền nốt các vé vào các quyển hiện tại
            // chạy 150 vòng lặp cho 1 quyển
            for (IDCurrentTicketInAll = (IDCurrentBook - 1) * 150; IDCurrentTicketInAll < IDCurrentBook * 150; IDCurrentTicketInAll++)
            {
                IDTicketInBook++;
                // kiểm tra slot hiện tại có trống không
                if (idAll[IDCurrentTicketInAll].Equals("0"))
                {   
                    // điền các vé trống vào ô còn lại
                    count3++;
                    var randon20 = GenerateUniqueNumbers(20);
                    var randon20Formatted = FormatNumbers(randon20, "D2");
                    string nonWinningTicketNumbersStr = string.Join(" ", randon20Formatted) + " NO_PRIZE"; // thêm tag No Prize


                    idAll[IDCurrentTicketInAll] = nonWinningTicketNumbersStr;
                }
                //mã vé

                //mã xác thực VIRN 23 chữ số

                // mã AVC
            }
            // Xáo các quyển sau mỗi vòng lặp, do các slot đầu là vé trúng
            int startIndex = (IDCurrentBook - 1) * 150;
            int endIndex = IDCurrentBook * 150;

            // Tách phần cần xáo trộn
            //List<string> rangeToShuffle = idAll.GetRange(startIndex, 151);
            List<string> rangeToShuffle = idAll.GetRange(startIndex, 150);

            // Xáo trộn phần đã tách
            var shuffledRange = rangeToShuffle.OrderBy(x => random.Next()).ToList();

            // Thay thế phần cũ bằng phần đã xáo trộn
            idAll.RemoveRange(startIndex, 150); //151
            idAll.InsertRange(startIndex, shuffledRange);

  

        }

        IDCurrentBook = 0;

        List<string> Codes = new List<string>();
        // Thêm mã vào đầu mỗi dòng vé
        for (int idx = 0; idx < totalTickets; idx++)
        {
            //ID hiện tại là idx
            // tự suy ra currentThung, currentQuyen, currentTicketIDinBook                
            IDCurrentBook = (int)Math.Ceiling(idx / 150.0);
            IDTicketInBook = idx - (IDCurrentBook-1) * 150;
            IDCurrentPackage = (int)Math.Ceiling(IDCurrentBook / 20.0);
            Codes = CreateCodesFromID(IDCurrentPackage, IDCurrentBook, IDTicketInBook, gameCode);
            idAll[idx] = Codes[0] + " " + Codes[1] + " " + Codes[2] + idAll[idx];
        }

        return idAll;
    }

    /// <summary>
    /// Sinh các mã VIRN, mã vé cho vé
    /// </summary>
    /// <param name="currentThung"></param>
    /// <param name="currentQuyen"></param>
    /// <param name="currentVeTrongQuyen"></param>
    /// <param name="gameCode"></param>
    /// <returns></returns>
    private List<string> CreateCodesFromID(int currentThung, int currentQuyen, int currentVeTrongQuyen, int gameCode)
    {
        string S = "1"; //Số lần in
        // Mã vé G G G - P P P P B B - T T T
        string PPPP = currentThung.ToString("D4");
        string BB = currentQuyen.ToString("D2");
        string TTT = currentVeTrongQuyen.ToString("D3");
        string TicketCode = gameCode.ToString() + "-" + PPPP + BB + "-" + TTT;
        string TicketCodeKhongGach = gameCode.ToString() + PPPP + BB + TTT;
        // Mã VIRN 
        // 7 ký tự số ngẫu nhiên
        string random7symbols = "";
        Random rand = new Random();
        for (int i=0;  i<7; i++)
        {
            random7symbols += rand.Next(0, 10).ToString();
        }    
        string VIRN = gameCode.ToString() + PPPP + BB + TTT + S + random7symbols;
        List<string> Codes = new List<string> { VIRN, TicketCode, TicketCodeKhongGach};
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

        if (ticketTypeCounters["NO_PRIZE"] < noPrizeTickets)
        {
            ticketTypeCounters["NO_PRIZE"]++;
            return "NO_PRIZE";
        }

        return null;
    }


    public void GenerateTickets(string outputFile = "lottery_tickets.txt")
    {
        var generatedTickets = new HashSet<string>(); //danh sách vé đã tạo dùng để kiểm tra tính trùng
        var listGeneratedTickets = new List<string>();  // danh sách vé đã tạo dùng để ghi vào CSDL
        // danh sách vé trúng và không trúng, dùng để xáo vé
        var list10KTickets = new List<string>();
        var list20KTickets = new List<string>();
        var list50KTickets = new List<string>();
        var list100KTickets = new List<string>();
        var list200KTickets = new List<string>();
        var list500KTickets = new List<string>();
        var listNoWinningTickets = new List<string>();    // danh sách vé không trúng, dùng để xáo vé
        
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


        for (int ticketId = 0; ticketId < totalTickets; ticketId++)
        {
            while (true)
            {
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

        // xáo thứ tự vé theo thuật toán sao cho 15 vé liền nhau sẽ có 1 vé trúng
        var shuffleTime = new Stopwatch();
        shuffleTime.Start();
        // listGeneratedTickets = listGeneratedTickets.OrderBy(x => random.Next()).ToList();
        listGeneratedTickets = ShuffleTickets(totalTickets, list10KTickets, list20KTickets, list50KTickets, list100KTickets, list200KTickets, list500KTickets, listNoWinningTickets);
        shuffleTime.Stop();
        Console.WriteLine($"\nThời gian xáo thứ tự vé theo cơ cấu đã cho: {shuffleTime.Elapsed.TotalSeconds:F2} seconds");

        // Write to file
        using (StreamWriter file = new StreamWriter(outputFile))
        {
            for (int idx = 0; idx < listGeneratedTickets.Count; idx++)
            {
                // chèn mã vé vào
                file.WriteLine($"ID {(idx + 1):D7}: {listGeneratedTickets[idx]}");
            }
        }

        // Print distribution
        Console.WriteLine("Cơ cấu giải thưởng sau khi tạo vé (đếm danh sách đối tượng List vừa tạo:");
        Console.WriteLine($"Tổng số vé:{totalTickets}");
        Console.WriteLine($"Tổng số quyển:{SLBook}");
        Console.WriteLine($"Tổng số thùng:{SLBook/20}");
        foreach (var kvp in ticketTypeCounters)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value} tickets");
        }
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
        Console.WriteLine($"Số lượng vé 10000 VND: {ticketTypeCounts["10K"]} (Kỳ vọng: {total10kTickets})");
        Console.WriteLine($"Số lượng vé 20000 VND: {ticketTypeCounts["20K"]} (Kỳ vọng: {total20kTickets})");
        Console.WriteLine($"Số lượng vé 50000 VND: {ticketTypeCounts["50K"]} (Kỳ vọng: {total50kTickets})");
        Console.WriteLine($"Số lượng vé 100000 VND: {ticketTypeCounts["100K"]} (Kỳ vọng: {total100kTickets})");
        Console.WriteLine($"Số lượng vé 200000 VND: {ticketTypeCounts["200K"]} (Kỳ vọng: {total200kTickets})");
        Console.WriteLine($"Số lượng vé 500000 VND: {ticketTypeCounts["500K"]} (Kỳ vọng: {total500kTickets})");
        Console.WriteLine($"Số lượng vé không trúng: {ticketTypeCounts["NO_PRIZE"]} (Kỳ vọng: {noPrizeTickets})");

        // Verify distribution
        Debug.Assert(ticketTypeCounts["10K"] == total10kTickets, "Số vé 10K không khớp");
        Debug.Assert(ticketTypeCounts["20K"] == total20kTickets, "Số vé 20K không khớp");
        Debug.Assert(ticketTypeCounts["50K"] == total50kTickets, "Số vé 50K không khớp");
        Debug.Assert(ticketTypeCounts["100K"] == total100kTickets, "Số vé 100K không khớp");
        Debug.Assert(ticketTypeCounts["200K"] == total200kTickets, "Số vé 200K không khớp");
        Debug.Assert(ticketTypeCounts["500K"] == total500kTickets, "Số vé 500K không khớp");
        Debug.Assert(ticketTypeCounts["NO_PRIZE"] == noPrizeTickets, "Số vé không trúng thưởng không khớp cơ cấu");


        // Kiểm thử cho 1 quyển = 150 dòng bất kỳ trong file text
        int blocksize = 150; // nếu là thùng thì sẽ là 3000
        int coef = totalTickets / blocksize;
        totalLine = 0;
        totalTypeCount = 0;
        int newTotal10kTickets = total10kTickets / coef;
        float newTotal20kTickets = (float)total20kTickets/(float)coef;
        int newTotal50kTickets = total50kTickets / coef;
        List<string> prizes = new List<string> { "10K", "20K", "50K", "100K", "200K", "500K", "NO_PRIZE" };
        foreach (string prize in prizes)
        {
            ticketTypeCounts[prize] = 0;
        }

        Random random = new Random();
        int rand = random.Next(1, SLBook);
        int startLine = rand*150;
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
        coef = totalTickets / blocksize;
        int newTotal100kTickets = total100kTickets / coef;
        int newTotal200kTickets = total200kTickets / coef;
        float newTotal500kTickets = (float)total500kTickets / (float)coef;
        foreach (string prize in prizes)
        {
            ticketTypeCounts[prize] = 0;
        }

        rand = random.Next(1, SLBook/20);
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
