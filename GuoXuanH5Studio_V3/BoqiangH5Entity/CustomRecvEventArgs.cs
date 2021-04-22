using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoqiangH5
{
    public class CustomRecvDataEventArgs
    {
        public CustomRecvDataEventArgs(List<byte> lsByte,string idstr)
        {
            recvMsg = lsByte;
            idStr = idstr;
        }
        private List<byte> recvMsg;
        private string idStr;
        public List<byte> RecvMsg
        {
            get { return recvMsg; }
            set { recvMsg = value; }
        }

        public string IDStr
        {
            get { return idStr; }
            set { idStr = value; }
        }
    }
}
