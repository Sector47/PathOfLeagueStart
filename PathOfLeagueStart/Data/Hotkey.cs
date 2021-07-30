using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PathOfLeagueStart.Data
{
    public class Hotkey
    {
        
        public Hotkey(string name, Key key)
        {
            Key = key;
            Name = name;
        }

        public Key Key { get; set; }

        public string Name { get; set; }




    }
}
