using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
   public  class E_KeyWord
    {

        public int ID
        {
            get;
            set;
        }
        public string KeyWord
        {
            get;
            set;
        }
        public int Hit
        {
            get;
            set;
        }
        public bool IsSearch
        {
            get;
            set;
        }
        public DateTime UpdateTime
        {
            get;
            set;
        }
    }
}
