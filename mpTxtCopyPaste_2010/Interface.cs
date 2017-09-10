using mpPInterface;

namespace mpTxtCopyPaste
{
    public class Interface : IPluginInterface
    {
        private const string _Name = "mpTxtCopyPaste";
        private const string _AvailCad = "2010";
        private const string _LName = "Быстрая копия текста";
        private const string _Description = "Быстрое копирование содержимого однострочного или многострочного текста";
        private const string _Author = "Пекшев Александр aka Modis";
        private const string _Price = "0";
        public string Name => _Name;
        public string AvailCad => _AvailCad;
        public string LName => _LName;
        public string Description => _Description;
        public string Author => _Author;
        public string Price => _Price;
    }
}
