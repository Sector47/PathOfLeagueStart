namespace PathOfLeagueStart.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Class to hold quest data
    /// </summary>
    public class Quest
    {
        [JsonProperty("quest")]
        public string questName { get; set; }

        [JsonProperty("reward")]
        public string reward { get; set; }

        [JsonProperty("classes")]
        public string classes { get; set; }

        public string initialZone { get; set; }

        public string finishZone { get; set; }

        public string prerequisiteQuest { get; set; }

        public bool isCompleted { get; set; }

        public bool isStarted { get; set; }

        // constructor for a quest object without rewards
        public Quest(string questName, string intInitialZone, string finishZone,
            string prerequisiteQuest)
        {
            this.questName = questName;
            this.initialZone = intInitialZone;
            this.finishZone = finishZone;
            this.prerequisiteQuest = prerequisiteQuest;
        }
    }
}
/*
This file uses the Newtonsoft.Json library available under the MIT license as follows:

The MIT License (MIT)

Copyright (c) 2007 James Newton-King

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
