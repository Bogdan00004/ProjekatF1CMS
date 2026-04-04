using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjekatF1CMS.Model
{
    [Serializable]
    public class F1Team
    {
        public string TeamName { get; set; }
        public string Headquarters { get; set; }
        public string EngineSupplier { get; set; }
        public int YearFounded { get; set; }
        public int NumberOfChampionships { get; set; }
        public string LogoPath { get; set; }
        public string DescriptionFilePath { get; set; }
        public DateTime DateAdded { get; set; }
        public F1Team() { }
    }
}
