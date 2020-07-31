namespace PathOfLeagueStart.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Class to hold gem data
    /// </summary>
    public class Gem
    {
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("level requirement")]
        public string level_requirement { get; set; }

        [JsonProperty("tags")]
        public string tags { get; set; }

        [JsonProperty("item class id restriction")]
        public string item_class_id_restriction { get; set; }

        [JsonProperty("gem tags")]
        public string gem_tags { get; set; }

        [JsonProperty("stat text")]
        public string stat_text { get; set; }
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
