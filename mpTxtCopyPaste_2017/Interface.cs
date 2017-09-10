using mpPInterface;

namespace mpTxtCopyPaste
{
    public class Interface : IPluginInterface
    {
        public string Name => "mpTxtCopyPaste";
        public string AvailCad => "2017";
        public string LName => "Быстрая копия текста";
        public string Description => "Быстрое копирование содержимого однострочного или многострочного текста";
        public string Author => "Пекшев Александр aka Modis";
        public string Price => "0";
    }
}
