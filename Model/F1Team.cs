using System.ComponentModel;

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
        private bool _isSelected;

        [System.Xml.Serialization.XmlIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
