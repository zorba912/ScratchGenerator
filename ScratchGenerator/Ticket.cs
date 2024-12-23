using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScratchGenerator
{
    class Ticket
    {
        public int ID { get; set; }
        /// <summary>
        /// mã vé
        /// </summary>
        public int TicketCode { get; set; }
        /// <summary>
        /// Số trên vé dạng số gốc
        /// </summary>
        public List<int> RawNumber { get; set; }
        /// <summary>
        /// Số trên vé dạng mã hoá
        /// </summary>
        public List<string> EncodedNumbers { get; set; }
        public string AVC { get; set; }
        public string VIRN { get; set; }
        /// <summary>
        /// Các giải in trên vé (16 giải)
        /// </summary>
        public string ListOfPrizes { get; set; }

        /// <summary>
        /// Giải thưởng mà vé trúng
        /// </summary>
        public string WinningPrize { get; set; }
    }
}
