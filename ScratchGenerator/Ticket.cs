using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScratchGenerator
{
    public class Ticket
    {
        //public int ID { get; set; }
        /// <summary>
        /// mã vé
        /// </summary>
        public string TicketCode { get; set; }
        public string TicketCodeWithoutDash { get; set; }

        /// <summary>
        /// Các số trên vé dạng số gốc
        /// </summary>
        public string StringOfRawNumbers { get; set; }

        /// <summary>
        /// Số trên vé dạng mã hoá
        /// </summary>
        //public List<string> EncodedNumbers { get; set; }
        public string AVC { get; set; }
        public string VIRN { get; set; }
        /// <summary>
        /// Các giải in trên vé (16 giải)
        /// </summary>
        //public string ListOfPrizes { get; set; }

        /// <summary>
        /// Giải thưởng mà vé trúng
        /// </summary>
        public string PrizeTag { get; set; }
    }
}
