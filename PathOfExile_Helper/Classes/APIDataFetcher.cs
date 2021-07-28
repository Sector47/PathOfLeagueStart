using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfExile_Helper.Classes
{
    /// <summary>
    /// This class will request data from the pathofexile gamepedia api once per league or when manually initiated.
    /// The data will be converted to an sqlite database locally.
    /// An api call will confirm that the current league matches the league stored locally, if not then it will refetch the data.
    /// </summary>
    class APIDataFetcher
    {

    }
}
