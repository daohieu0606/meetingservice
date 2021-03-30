using System;
using System.Collections.Generic;
using System.Text;

namespace meetingservice
{
    public class GlobalData
    {
        public ulong HistoryId { get; set; }

        private GlobalData()
        {
        }

        private static readonly object padlock = new object();
        private static GlobalData instance = null;
        public static GlobalData Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new GlobalData();
                        }
                    }
                }
                return instance;
            }
        }
    }
}
